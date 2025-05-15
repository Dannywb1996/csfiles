using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using CI.QuickSave;
using CI.QuickSave.Core.Storage;
using GraphicsConfigurator.API.URP;
using LimWorks.Rendering.URP.ScreenSpaceReflections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using VolumetricFogAndMist2;

public static class DewSave
{
	internal class SaveFile<T>
	{
		private static QuickSaveSettings Settings;

		private QuickSaveReader _reader;

		private QuickSaveWriter _writer;

		private string _root;

		public SaveFile(string path)
		{
			string dir = Path.Join(Application.persistentDataPath, "QuickSave");
			string root = (_root = Path.ChangeExtension(path, null).Substring(dir.Length + 1));
			_writer = QuickSaveWriter.Create(root, Settings);
			_reader = QuickSaveReader.Create(root, Settings);
		}

		public SaveFile(string path, T defaultValue)
		{
			string root = Path.GetFileNameWithoutExtension(path);
			_writer = QuickSaveWriter.Create(root, Settings);
			try
			{
				_reader = QuickSaveReader.Create(root, Settings);
				Read();
			}
			catch (Exception)
			{
				_writer.Write("root", defaultValue);
				_writer.Commit();
				_reader = QuickSaveReader.Create(root, Settings);
				Read();
			}
		}

		public bool Exists()
		{
			return _reader.Exists("root");
		}

		public T Read()
		{
			return _reader.Read<T>("root");
		}

		public bool TryRead(out T value)
		{
			return _reader.TryRead<T>("root", out value);
		}

		public void Write(T value)
		{
			_writer.Write("root", value);
			_writer.Commit();
			_reader.Reload();
		}

		public void Delete()
		{
			_writer = null;
			_reader = null;
			QuickSaveRaw.Delete(_root + ".json");
		}

		static SaveFile()
		{
			Settings = new QuickSaveSettings
			{
				CompressionMode = CI.QuickSave.CompressionMode.None,
				SecurityMode = SecurityMode.None
			};
		}
	}

	public static DewPlatformSettings platformSettings;

	public static DewProfile profile;

	private static SaveFile<DewPlatformSettings> _platformFile;

	private static SaveFile<DewProfile> _profileFile;

	private static SaveFile<DewItemsData> _itemsFile;

	public static DewItemsData items;

	public static string SavePrefix => DewBuildProfile.current.savePrefix;

	public static string ProfileFileNameTemplate => SavePrefix + "profile{0}.json";

	public static string PlatformTemplate => SavePrefix + "platform";

	public static string profilePath { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitAll()
	{
		profile = null;
		platformSettings = null;
		_platformFile = null;
		_profileFile = null;
		SceneManager.sceneLoaded += delegate
		{
			try
			{
				ApplyPerSceneSettings();
			}
			catch (Exception exception5)
			{
				Debug.LogException(exception5);
			}
		};
		try
		{
			MigrateSave_DreamToDreams();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			ManageDailyBackups();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		try
		{
			ManageAutoRecoveryBackups();
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
		if (!LoadPlatformSettings())
		{
			Debug.LogWarning("Platform settings broken? Resetting platform settings...");
			try
			{
				string platformPath = Path.Join(Application.persistentDataPath, "QuickSave", PlatformTemplate + ".json");
				if (File.Exists(platformPath))
				{
					File.Delete(platformPath);
				}
			}
			catch (Exception exception4)
			{
				Debug.LogException(exception4);
			}
			if (!LoadPlatformSettings())
			{
				platformSettings = new DewPlatformSettings();
				platformSettings.Initialize();
				platformSettings.Validate();
				ShowSaveLoadErrorAndExit();
			}
		}
		LoadAndDoMaintenanceOfItems();
		LoadProfile();
	}

	public static void ShowSaveLoadErrorAndExit()
	{
		GlobalLogicPackage.CallOnReady(delegate
		{
			DewSessionError.ShowError(new DewException(DewExceptionType.SaveLoadFailed_NewSaveFailed), isFatal: true);
		});
	}

	public static bool LoadPlatformSettings()
	{
		try
		{
			_platformFile = new SaveFile<DewPlatformSettings>(PlatformTemplate, new DewPlatformSettings());
			platformSettings = _platformFile.Read();
			platformSettings.Validate();
			Debug.Log("Loaded platform settings.");
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	public static bool LoadProfile()
	{
		if (profilePath == null || !Exists(profilePath) || GetProfileState(profilePath, out var _) != DewProfileState.Normal)
		{
			List<DewProfileItem> profiles = GetNormalProfiles();
			if (profiles.Count == 1)
			{
				profilePath = profiles[0].path;
			}
			else if (!string.IsNullOrEmpty(platformSettings.lastProfilePath) && profiles.FindIndex((DewProfileItem p) => p.path == platformSettings.lastProfilePath) != -1)
			{
				profilePath = platformSettings.lastProfilePath;
			}
			else
			{
				profilePath = null;
			}
		}
		return LoadProfile(profilePath);
	}

	public static bool LoadProfile(string path)
	{
		try
		{
			_profileFile = null;
			profilePath = path;
			if (path == null)
			{
				profile = new DewProfile
				{
					name = "Transient"
				};
				profile.Initialize();
				profile.Validate();
				ApplySettings();
				Debug.Log("Using transient profile from now on.");
				return true;
			}
			DewProfile peek;
			DewProfileState state = GetProfileState(path, out peek);
			if (state != DewProfileState.Normal)
			{
				Debug.LogError($"Profile load failed, the profile state is invalid. ({state})");
				return false;
			}
			_profileFile = new SaveFile<DewProfile>(path);
			if (!_profileFile.TryRead(out profile))
			{
				Debug.LogError("Profile load failed for unknown reason.");
				return false;
			}
			profile.Validate();
			ApplySettings();
			Debug.Log("Loaded User Profile " + profile.name + ": " + path);
			if (platformSettings != null && platformSettings.lastProfilePath != path)
			{
				platformSettings.lastProfilePath = path;
				SavePlatformSettings();
			}
			if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				Debug.Log("Booth build. Unlocking everything...");
				ConsoleCommands.AchCompleteAll();
				ConsoleCommands.DiscoverAll();
			}
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	public static void CreateProfile(string name)
	{
		_profileFile = null;
		profilePath = Path.Combine(CI.QuickSave.Core.Storage.FileAccess.BasePath, string.Format(ProfileFileNameTemplate, Guid.NewGuid().ToString()));
		while (Exists(profilePath))
		{
			profilePath = Path.Combine(CI.QuickSave.Core.Storage.FileAccess.BasePath, string.Format(ProfileFileNameTemplate, Guid.NewGuid().ToString()));
		}
		string lang = profile.language;
		profile = new DewProfile
		{
			name = name,
			language = lang
		};
		profile.Initialize();
		profile.Validate();
		_profileFile = new SaveFile<DewProfile>(profilePath, profile);
		ApplySettings();
		Debug.Log("Created new profile " + name + ": " + profilePath);
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			Debug.Log("Booth build. Unlocking everything...");
			ConsoleCommands.AchCompleteAll();
			ConsoleCommands.DiscoverAll();
		}
	}

	public static void ConvertProfile(string path)
	{
		DewProfile peek;
		DewProfileState state = GetProfileState(path, out peek);
		if (state != DewProfileState.Convertible)
		{
			Debug.LogError("Save conversion failed, invalid state: " + state);
			return;
		}
		_profileFile = null;
		profilePath = Path.Combine(CI.QuickSave.Core.Storage.FileAccess.BasePath, string.Format(ProfileFileNameTemplate, Guid.NewGuid().ToString()));
		while (Exists(profilePath))
		{
			profilePath = Path.Combine(CI.QuickSave.Core.Storage.FileAccess.BasePath, string.Format(ProfileFileNameTemplate, Guid.NewGuid().ToString()));
		}
		profile = peek;
		profile.Validate();
		_profileFile = new SaveFile<DewProfile>(profilePath, profile);
		ApplySettings();
		DeleteProfile(path);
		Debug.Log("Converted profile " + profile.name + ": " + profilePath);
	}

	public static void SavePlatformSettings()
	{
		_platformFile.Write(platformSettings);
		Debug.Log("Platform settings saved.");
	}

	public static void ResetPlatformSettings()
	{
		platformSettings = new DewPlatformSettings();
		if (_platformFile == null)
		{
			_platformFile = new SaveFile<DewPlatformSettings>(PlatformTemplate, new DewPlatformSettings());
		}
		_platformFile.Write(platformSettings);
		Debug.Log("Platform settings reset.");
	}

	public static void DeleteProfile(string path)
	{
		if (path == null)
		{
			Debug.Log("DeleteProfile: Tried to delete transient profile.");
		}
		else
		{
			QuickSaveRaw.Delete(Path.GetFileName(path));
		}
	}

	public static void SaveProfile()
	{
		if (profilePath == null)
		{
			Debug.Log("SaveProfile: Currently using transient profile.");
			return;
		}
		_profileFile.Write(profile);
		try
		{
			CreateAutoRecoveryBackup();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		Debug.Log("Profile saved.");
	}

	public static bool Exists(string path)
	{
		if (path == null)
		{
			return true;
		}
		return File.Exists(path);
	}

	private static DewProfileState GetProfileState(string path, out DewProfile peek)
	{
		peek = null;
		if (!Exists(path))
		{
			return DewProfileState.Corrupted;
		}
		string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
		if (fileNameWithoutExt.StartsWith("err-"))
		{
			return DewProfileState.Corrupted;
		}
		try
		{
			if (!new SaveFile<DewProfile>(path).TryRead(out peek))
			{
				peek = null;
			}
			if (peek == null)
			{
				return DewProfileState.Corrupted;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return DewProfileState.Corrupted;
		}
		if (!fileNameWithoutExt.StartsWith(SavePrefix))
		{
			if (peek.saveVersion > 5)
			{
				return DewProfileState.UnsupportedEdition;
			}
			return DewProfileState.Convertible;
		}
		if (peek.saveVersion > 5)
		{
			return DewProfileState.UnsupportedVersion;
		}
		return DewProfileState.Normal;
	}

	public static List<DewProfileItem> GetProfiles()
	{
		List<DewProfileItem> list = new List<DewProfileItem>();
		string[] files = Directory.GetFiles(Path.Join(Application.persistentDataPath, "QuickSave"), "*_profile*.json");
		foreach (string path in files)
		{
			DewProfileItem item = new DewProfileItem
			{
				path = path
			};
			item.state = GetProfileState(path, out item.peek);
			list.Add(item);
		}
		return list;
	}

	public static List<DewProfileItem> GetNormalProfiles()
	{
		List<DewProfileItem> list = GetProfiles();
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (list[i].state != DewProfileState.Normal)
			{
				list.RemoveAt(i);
			}
		}
		return list;
	}

	internal static void ValidateEnumValues(object obj)
	{
		FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		List<long> possibleValues = new List<long>();
		FieldInfo[] array = fields;
		foreach (FieldInfo f in array)
		{
			if (!f.FieldType.IsEnum)
			{
				continue;
			}
			long current = Convert.ToInt64(f.GetValue(obj));
			foreach (object v in Enum.GetValues(f.FieldType))
			{
				possibleValues.Add(Convert.ToInt64(v));
			}
			if (possibleValues.Count > 0 && !possibleValues.Contains(current))
			{
				long bestValue = possibleValues[0];
				float bestDiff = Mathf.Abs(current - possibleValues[0]);
				for (int j = 1; j < possibleValues.Count; j++)
				{
					float diff = Mathf.Abs(current - possibleValues[j]);
					if (diff < bestDiff)
					{
						bestDiff = diff;
						bestValue = possibleValues[j];
					}
				}
				object enumValue = Enum.ToObject(f.FieldType, bestValue);
				f.SetValue(obj, enumValue);
				Debug.LogWarning($"Fixing out of range value: {obj.GetType().Name}.{f.Name} = {current} => {enumValue}({bestValue})");
			}
			possibleValues.Clear();
		}
	}

	private static void LoadAndDoMaintenanceOfItems()
	{
		string parentDir = Path.Join(Application.persistentDataPath, "QuickSave");
		_itemsFile = new SaveFile<DewItemsData>(SavePrefix + "items", new DewItemsData());
		if (!_itemsFile.TryRead(out items))
		{
			string path = Path.Join(parentDir, SavePrefix + "items.json");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			if (!_itemsFile.TryRead(out items))
			{
				ShowSaveLoadErrorAndExit();
				return;
			}
		}
		items.Validate();
		string[] files = Directory.GetFiles(parentDir, "*_items.json");
		for (int i = 0; i < files.Length; i++)
		{
			if (!new SaveFile<DewItemsData>(Path.GetFileNameWithoutExtension(files[i]), new DewItemsData()).TryRead(out var otherEdition))
			{
				continue;
			}
			otherEdition.Validate();
			foreach (string item in otherEdition.encryptedItems)
			{
				if (!items.encryptedItems.Contains(item))
				{
					items.encryptedItems.Add(item);
				}
			}
		}
		_itemsFile.Write(items);
		new SaveFile<DewItemsData>("backup_items", new DewItemsData()).Write(items);
	}

	public static void AddMissingServerGeneratedItemsToProfile(DewProfile p)
	{
		if (items == null)
		{
			return;
		}
		foreach (string encryptedItem in items.encryptedItems)
		{
			DecryptedItemData decrypted = DewItem.GetDecryptedItemData(encryptedItem);
			if (decrypted != null)
			{
				p.UnlockServerGeneratedItem(decrypted);
			}
		}
	}

	public static void AddServerGeneratedItem(DecryptedItemData item)
	{
		if (item == null || string.IsNullOrEmpty(item.ownershipKey))
		{
			return;
		}
		profile.UnlockServerGeneratedItem(item);
		if (!items.encryptedItems.Contains(item.ownershipKey) && !items.Contains(item))
		{
			items.encryptedItems.Add(item.ownershipKey);
			if (DewPlayer.local != null)
			{
				DewPlayer.local.CmdAuthorizeForUse(item.ownershipKey);
			}
		}
	}

	public static void AddServerGeneratedItemsWithMessageAndSave(List<DecryptedItemData> items)
	{
	}

	public static void ClearItemsFromItemStorage()
	{
		items = new DewItemsData();
		SaveItems();
	}

	public static void SaveItems()
	{
		_itemsFile.Write(items);
		new SaveFile<DewItemsData>("backup_items", new DewItemsData()).Write(items);
	}

	private static void ManageDailyBackups()
	{
		string zipName = DateTime.Now.ToString("yyyyMMdd") + ".zip";
		string backupTargetDir = Path.Join(Application.persistentDataPath, "QuickSave");
		string zipDir = Path.Join(Application.persistentDataPath, "QuickSave", "Backups");
		Directory.CreateDirectory(zipDir);
		string zipPath = Path.Join(zipDir, zipName);
		if (File.Exists(zipPath))
		{
			Debug.Log("Already made a backup today. Skipping...");
			return;
		}
		Debug.Log("Making a backup");
		CreateZipWithJsonFiles(backupTargetDir, zipPath);
		Debug.Log("Successfully created a backup at " + zipPath);
		List<FileInfo> backupFiles = (from f in Directory.GetFiles(zipDir, "*.zip")
			select new FileInfo(f) into f
			orderby f.CreationTime descending
			select f).ToList();
		int backupLimit = 30;
		if (backupFiles.Count <= backupLimit)
		{
			return;
		}
		Debug.Log($"Too many backups({backupFiles.Count}), purging...");
		foreach (FileInfo file in backupFiles.Skip(backupLimit))
		{
			try
			{
				file.Delete();
				Debug.Log("Deleted old backup: " + file.Name);
			}
			catch (Exception exception)
			{
				Debug.LogError("Error deleting backup " + file.Name);
				Debug.LogException(exception);
			}
		}
	}

	private static void ManageAutoRecoveryBackups()
	{
		string saveDir = Path.Join(Application.persistentDataPath, "QuickSave");
		string autoRecoveryDir = Path.Join(Application.persistentDataPath, "QuickSave", "AutoRecovery");
		Directory.CreateDirectory(autoRecoveryDir);
		try
		{
			List<string> profileNames = Directory.GetFiles(saveDir, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
			foreach (FileInfo b in (from f in Directory.GetFiles(autoRecoveryDir, "*.json")
				select new FileInfo(f)).ToList())
			{
				if (!profileNames.Any((string pName) => b.Name.StartsWith(pName)))
				{
					Debug.Log("Deleting stray auto-recovery backup: " + b.Name);
					b.Delete();
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		foreach (DewProfileItem p in GetProfiles())
		{
			try
			{
				if (p.state != DewProfileState.Corrupted)
				{
					continue;
				}
				string fileNameNoExt = Path.GetFileNameWithoutExtension(p.path);
				if (fileNameNoExt.StartsWith("err-"))
				{
					continue;
				}
				Debug.Log("Attempting auto-recovery for " + fileNameNoExt);
				List<FileInfo> list = (from f in Directory.GetFiles(autoRecoveryDir, fileNameNoExt + "_*.json")
					select new FileInfo(f)).OrderByDescending(delegate(FileInfo f)
				{
					try
					{
						return long.Parse(Path.GetFileNameWithoutExtension(f.FullName).Split("_")[^1]);
					}
					catch (Exception)
					{
						return long.MinValue;
					}
				}).ToList();
				string validCandidatePath = null;
				string profileName = null;
				foreach (FileInfo c in list)
				{
					if (GetProfileState(c.FullName, out var peek) != DewProfileState.Corrupted)
					{
						validCandidatePath = c.FullName;
						profileName = peek.name;
						break;
					}
				}
				if (validCandidatePath == null)
				{
					Debug.Log("No valid candidate found");
					continue;
				}
				File.Copy(validCandidatePath, p.path, overwrite: true);
				Debug.Log("Successfully recovered broken profile");
				GlobalLogicPackage.CallOnReady(delegate
				{
					ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
					{
						rawContent = string.Format(DewLocalization.GetUIValue("Title_Profile_Message_RecoveredCorruptSaveFile"), profileName)
					});
				});
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
	}

	private static void CreateAutoRecoveryBackup()
	{
		string dir = Path.Join(Application.persistentDataPath, "QuickSave", "AutoRecovery");
		Directory.CreateDirectory(dir);
		if (string.IsNullOrEmpty(profilePath))
		{
			return;
		}
		string fileNameNoExt = Path.GetFileNameWithoutExtension(profilePath);
		long nextIndex = 0L;
		List<FileInfo> existingBackups = (from fileName in Directory.GetFiles(dir, fileNameNoExt + "_*.json")
			select new FileInfo(fileName)).OrderBy(delegate(FileInfo fileInfo)
		{
			try
			{
				return long.Parse(Path.GetFileNameWithoutExtension(fileInfo.FullName).Split("_")[^1]);
			}
			catch (Exception)
			{
				return long.MinValue;
			}
		}).ToList();
		foreach (FileInfo f in existingBackups)
		{
			try
			{
				long curr = long.Parse(Path.GetFileNameWithoutExtension(f.FullName).Split("_")[^1]);
				if (curr + 1 > nextIndex)
				{
					nextIndex = curr + 1;
				}
			}
			catch (Exception)
			{
			}
		}
		try
		{
			while (existingBackups.Count > 9)
			{
				Debug.Log("Deleting old auto-recovery backup: " + existingBackups[0].Name);
				existingBackups[0].Delete();
				existingBackups.RemoveAt(0);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		File.Copy(profilePath, Path.Join(dir, $"{fileNameNoExt}_{nextIndex}.json"));
		Debug.Log($"Created auto-recovery backup {nextIndex} for {profile.name}");
	}

	public static void CreateZipWithJsonFiles(string sourceDirectory, string zipFilePath)
	{
		string[] jsonFiles = Directory.GetFiles(sourceDirectory, "*.json", SearchOption.TopDirectoryOnly);
		if (jsonFiles.Length == 0)
		{
			return;
		}
		using ZipArchive zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
		string[] array = jsonFiles;
		foreach (string jsonFile in array)
		{
			zipArchive.CreateEntryFromFile(jsonFile, Path.GetFileName(jsonFile));
		}
	}

	private static void MigrateSave_DreamToDreams()
	{
		string newPersistentPath = Application.persistentDataPath;
		DirectoryInfo parentDir = Directory.GetParent(newPersistentPath);
		if (parentDir == null)
		{
			return;
		}
		string oldPersistentPath = Path.Join(parentDir.FullName, "Shape of Dream");
		if (!Directory.Exists(oldPersistentPath))
		{
			Debug.Log("No pre-name-change persistent data found");
			return;
		}
		Debug.Log("Found pre-name-change persistent data");
		string newSavePath = Path.Join(newPersistentPath, "QuickSave");
		string oldSavePath = Path.Join(oldPersistentPath, "QuickSave");
		if (!Directory.Exists(oldSavePath))
		{
			Debug.Log("No pre-name-change save found. Probably been migrated already");
			return;
		}
		Debug.Log("Starting D2D migration");
		Directory.CreateDirectory(newSavePath);
		string[] files = Directory.GetFiles(oldSavePath, "*.json");
		foreach (string filePath in files)
		{
			string fileName = Path.GetFileName(filePath);
			string newFilePath = Path.Join(newSavePath, fileName);
			for (int j = 0; j < 100; j++)
			{
				if (File.Exists(newFilePath))
				{
					string newFileName = $"conf-{j}-d2d-{fileName}";
					newFilePath = Path.Join(newSavePath, newFileName);
				}
				if (!File.Exists(newFilePath))
				{
					break;
				}
			}
			File.Move(filePath, newFilePath);
		}
		Directory.Delete(oldSavePath, recursive: true);
		Debug.Log("D2D migration success");
	}

	private static void ApplyPerSceneSettings()
	{
		if (Application.isPlaying)
		{
			UpdateTerrainAndVegetationQuality();
			UpdateFogQuality();
			UpdateShadowQuality();
			if (ManagerBase<InputManager>.instance != null)
			{
				ManagerBase<InputManager>.instance.ResetInputDevices();
			}
		}
	}

	private static void UpdateShadowQuality()
	{
		switch (platformSettings.graphics.shadowQuality)
		{
		case QualityOff4Levels.Off:
			Configuring.CurrentURPA.MainLightShadowsCasting(state: false);
			Configuring.CurrentURPA.AdditionalLightsShadowsCasting(state: false);
			break;
		case QualityOff4Levels.Low:
			Configuring.CurrentURPA.MainLightShadowsCasting(state: true);
			Configuring.CurrentURPA.MainLightShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._1024);
			Configuring.CurrentURPA.AdditionalLightsShadowsCasting(state: false);
			break;
		case QualityOff4Levels.Medium:
			Configuring.CurrentURPA.MainLightShadowsCasting(state: true);
			Configuring.CurrentURPA.MainLightShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._1024);
			Configuring.CurrentURPA.AdditionalLightsShadowsCasting(state: true);
			Configuring.CurrentURPA.additionalLightsShadowResolutionTierHigh(256);
			Configuring.CurrentURPA.AdditionalLightsShadowResolutionTierMedium(256);
			Configuring.CurrentURPA.AdditionalLightsShadowResolutionTierLow(256);
			break;
		case QualityOff4Levels.High:
			Configuring.CurrentURPA.MainLightShadowsCasting(state: true);
			Configuring.CurrentURPA.MainLightShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._2048);
			Configuring.CurrentURPA.AdditionalLightsShadowsCasting(state: true);
			Configuring.CurrentURPA.additionalLightsShadowResolutionTierHigh(512);
			Configuring.CurrentURPA.AdditionalLightsShadowResolutionTierMedium(512);
			Configuring.CurrentURPA.AdditionalLightsShadowResolutionTierLow(512);
			break;
		case QualityOff4Levels.Ultra:
			Configuring.CurrentURPA.MainLightShadowsCasting(state: true);
			Configuring.CurrentURPA.MainLightShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._4096);
			Configuring.CurrentURPA.AdditionalLightsShadowsCasting(state: true);
			Configuring.CurrentURPA.additionalLightsShadowResolutionTierHigh(1024);
			Configuring.CurrentURPA.AdditionalLightsShadowResolutionTierMedium(1024);
			Configuring.CurrentURPA.AdditionalLightsShadowResolutionTierLow(1024);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void UpdateTerrainAndVegetationQuality()
	{
		int pixelError = platformSettings.graphics.terrainQuality switch
		{
			Quality3Levels.Low => 100, 
			Quality3Levels.Medium => 65, 
			Quality3Levels.High => 15, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		float detailDensity = platformSettings.graphics.vegetationQuality switch
		{
			Quality3Levels.Low => 0.25f, 
			Quality3Levels.Medium => 0.7f, 
			Quality3Levels.High => 1f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		Terrain[] array = UnityEngine.Object.FindObjectsOfType<Terrain>();
		foreach (Terrain obj in array)
		{
			obj.heightmapPixelError = pixelError;
			obj.detailObjectDensity = detailDensity;
			obj.shadowCastingMode = ((platformSettings.graphics.shadowQuality >= QualityOff4Levels.Medium) ? ShadowCastingMode.On : ShadowCastingMode.Off);
		}
	}

	private static void UpdateFogQuality()
	{
		VolumetricFog[] array = UnityEngine.Object.FindObjectsOfType<VolumetricFog>(includeInactive: true);
		foreach (VolumetricFog fog in array)
		{
			if (!fog.profile.name.Contains("(Clone)"))
			{
				fog.profile = UnityEngine.Object.Instantiate(fog.profile);
			}
			bool isFogOff = false;
			fog.enabled = !isFogOff;
			fog.GetComponent<MeshRenderer>().enabled = !isFogOff;
			switch (platformSettings.graphics.fogQuality)
			{
			case Quality3Levels.Low:
				fog.profile.raymarchQuality = 1;
				fog.profile.jittering = 5f;
				break;
			case Quality3Levels.Medium:
				fog.profile.raymarchQuality = 3;
				fog.profile.jittering = 1f;
				break;
			case Quality3Levels.High:
				fog.profile.raymarchQuality = 4;
				fog.profile.jittering = 0.5f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (fog.gameObject.activeSelf)
			{
				fog.gameObject.SetActive(value: false);
				fog.gameObject.SetActive(value: true);
			}
		}
		VolumetricFogManager[] array2 = UnityEngine.Object.FindObjectsOfType<VolumetricFogManager>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].downscaling = ((platformSettings.graphics.fogQuality >= Quality3Levels.High) ? 1 : 2);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void ApplySettings()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		try
		{
			if (profile != null && profile.gameplay != null)
			{
				profile.gameplay.uiScale = 0.8f;
			}
			if (ManagerBase<AudioManager>.instance != null)
			{
				ManagerBase<AudioManager>.instance.UpdateMixerAttenuations();
			}
			if (ManagerBase<InGameTutorialManager>.instance != null && ManagerBase<InGameTutorialManager>.instance.isTutorialActive && profile.gameplay.disableTutorial)
			{
				ManagerBase<InGameTutorialManager>.instance.StopTutorials();
			}
			if (ManagerBase<CursorManager>.instance != null)
			{
				ManagerBase<CursorManager>.instance.ResizeTextures();
			}
			Screen.SetResolution(platformSettings.graphics.resolutionWidth, platformSettings.graphics.resolutionHeight, platformSettings.graphics.fullScreenMode);
			LimSSR.Enabled = platformSettings.graphics.screenSpaceReflections > QualityOff3Levels.Off;
			switch (platformSettings.graphics.screenSpaceReflections)
			{
			case QualityOff3Levels.Off:
			case QualityOff3Levels.Low:
				LimSSR.SetSettings(new ScreenSpaceReflectionsSettings
				{
					Downsample = 0u,
					MaxSteps = 8f,
					MinSmoothness = 0.5f,
					StepStrideLength = 0.4f
				});
				break;
			case QualityOff3Levels.Medium:
				LimSSR.SetSettings(new ScreenSpaceReflectionsSettings
				{
					Downsample = 0u,
					MaxSteps = 40f,
					MinSmoothness = 0.5f,
					StepStrideLength = 0.1f
				});
				break;
			case QualityOff3Levels.High:
				LimSSR.SetSettings(new ScreenSpaceReflectionsSettings
				{
					Downsample = 0u,
					MaxSteps = 128f,
					MinSmoothness = 0.5f,
					StepStrideLength = 0.01f
				});
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (ManagerBase<GlobalUIManager>.instance != null)
			{
				ManagerBase<GlobalUIManager>.instance.EnforceFontFallbackOrder();
			}
			Transform[] array = UnityEngine.Object.FindObjectsOfType<Transform>(includeInactive: true);
			foreach (Transform transform in array)
			{
				ILangaugeChangedCallback[] components = transform.GetComponents<ILangaugeChangedCallback>();
				foreach (ILangaugeChangedCallback langaugeChangedCallback in components)
				{
					try
					{
						langaugeChangedCallback.OnLanguageChanged();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception, langaugeChangedCallback as UnityEngine.Object);
					}
				}
				ISettingsChangedCallback[] components2 = transform.GetComponents<ISettingsChangedCallback>();
				foreach (ISettingsChangedCallback settingsChangedCallback in components2)
				{
					try
					{
						settingsChangedCallback.OnSettingsChanged();
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2, settingsChangedCallback as UnityEngine.Object);
					}
				}
			}
			ApplyPerSceneSettings();
			CultureInfo obj = (CultureInfo)CultureInfo.CurrentUICulture.Clone();
			obj.NumberFormat.PercentSymbol = "%";
			CultureInfo.CurrentUICulture = obj;
			CultureInfo obj2 = (CultureInfo)CultureInfo.CurrentCulture.Clone();
			obj2.NumberFormat.PercentSymbol = "%";
			CultureInfo.CurrentCulture = obj2;
			if (ManagerBase<LobbyManager>.instance != null)
			{
				ManagerBase<LobbyManager>.instance.SwitchService();
			}
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
	}
}
