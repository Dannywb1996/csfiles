using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public static class DewItem
{
	private static readonly (string, string)[] CouponServers = new(string, string)[6]
	{
		("ap-east-1", "https://oc4p6dokwt3f3bwtsvxg6xjexi0ffwjb.lambda-url.ap-east-1.on.aws/"),
		("ap-southeast-1", "https://eijmsuyq3jxj7uxsf3lzvhuu7q0mtyjz.lambda-url.ap-southeast-1.on.aws/"),
		("us-west-1", "https://gv2uo22rvgskhwtkvposdbbqr40xxxbu.lambda-url.us-west-1.on.aws/"),
		("eu-west-3", "https://voolxxeuk3vq32y3guf24ythwq0raobp.lambda-url.eu-west-3.on.aws/"),
		("ca-central-1", "https://5jm3dssl3wpa7zuozscmrqgjhq0zvuea.lambda-url.ca-central-1.on.aws/"),
		("ap-northeast-1", "https://3f4iuhshq6sw5peheqr5qwrnzq0lycqt.lambda-url.ap-northeast-1.on.aws/")
	};

	public static MonoBehaviour GetItemPrefab(string itemName)
	{
		if (itemName.StartsWith("Emote_"))
		{
			return DewResources.GetByName<Emote>(itemName);
		}
		if (itemName.StartsWith("Acc_"))
		{
			return DewResources.GetByName<Accessory>(itemName);
		}
		if (itemName.StartsWith("Nametag_"))
		{
			return DewResources.GetByName<Nametag>(itemName);
		}
		return null;
	}

	public static bool IsItemGeneratedFromServer(string itemName)
	{
		MonoBehaviour item = GetItemPrefab(itemName);
		if (item is Emote e)
		{
			return e.generatedFromServer;
		}
		if (item is Accessory a)
		{
			return a.generatedFromServer;
		}
		if (item is Nametag n)
		{
			return n.generatedFromServer;
		}
		return false;
	}

	public static async UniTask<bool> RedeemCode(string code)
	{
		if (code.Trim().Length <= 0)
		{
			return false;
		}
		try
		{
			ManagerBase<TransitionManager>.instance.SetBusy(value: true);
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForSteam);
			List<string> duplicateItemNames = new List<string>();
			List<string> newItemNames = new List<string>();
			using SteamTicketForWebApi ticket = await SteamManager.instance.GetTicketForWebApi();
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.RequestingItemFromServer);
			foreach (DecryptedItemData i in await RequestItemGeneration_Imp(new Dictionary<string, string> { { "code", code } }, ticket))
			{
				if (DewSave.items.Contains(i))
				{
					duplicateItemNames.Add(GetLocalizedNameWithType(i.item));
					continue;
				}
				newItemNames.Add(GetLocalizedNameWithType(i.item));
				DewSave.AddServerGeneratedItem(i);
			}
			duplicateItemNames.Sort();
			newItemNames.Sort();
			if (newItemNames.Count > 0)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = string.Format(DewLocalization.GetUIValue("Redeem_ReceivedItem"), newItemNames.JoinToString("\n")),
					buttons = DewMessageSettings.ButtonType.Ok
				});
			}
			else if (code == "*")
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = DewLocalization.GetUIValue("Redeem_NoGiftsToRestore"),
					buttons = DewMessageSettings.ButtonType.Ok
				});
			}
			if (code != "*" && duplicateItemNames.Count > 0)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = string.Format(DewLocalization.GetUIValue("Redeem_YouAlreadyHaveItem"), duplicateItemNames.JoinToString("\n")),
					buttons = DewMessageSettings.ButtonType.Ok
				});
			}
			Debug.Log(string.Format("New items({0}): {1}", newItemNames.Count, newItemNames.JoinToString(", ")));
			Debug.Log(string.Format("Duplicate items({0}): {1}", duplicateItemNames.Count, duplicateItemNames.JoinToString(", ")));
			DewSave.SaveItems();
			return true;
		}
		catch (Exception e)
		{
			DewSessionError.ShowError(e, isFatal: false, isGame: false);
		}
		finally
		{
			if (ManagerBase<TransitionManager>.instance != null)
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
				ManagerBase<TransitionManager>.instance.SetBusy(value: false);
			}
		}
		return false;
	}

	public static async UniTask<bool> GenerateItem(string itemName)
	{
		return await GenerateItems(new List<string> { itemName });
	}

	public static async UniTask<bool> GenerateItems(List<string> itemNames)
	{
		_ = 1;
		try
		{
			ManagerBase<TransitionManager>.instance.SetBusy(value: true);
			List<string> localAccs = new List<string>();
			List<string> localNametags = new List<string>();
			List<string> localEmotes = new List<string>();
			List<string> serverItems = new List<string>();
			foreach (string g in itemNames)
			{
				if (GetItemPrefab(g) == null)
				{
					throw new DewException(DewExceptionType.UnknownItem, g);
				}
				if (IsItemGeneratedFromServer(g))
				{
					serverItems.Add(g);
					continue;
				}
				if (g.StartsWith("Acc_"))
				{
					localAccs.Add(g);
				}
				if (g.StartsWith("Nametag_"))
				{
					localNametags.Add(g);
				}
				if (g.StartsWith("Emote_"))
				{
					localEmotes.Add(g);
				}
			}
			List<string> duplicateItemNames = new List<string>();
			List<string> newItemNames = new List<string>();
			List<DecryptedItemData> generated = new List<DecryptedItemData>();
			if (serverItems.Count > 0)
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.WaitingForSteam);
				SteamTicketForWebApi ticket = await SteamManager.instance.GetTicketForWebApi();
				try
				{
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.RequestingItemFromServer);
					List<DecryptedItemData>[] array = await UniTask.WhenAll(serverItems.Select((string value) => RequestItemGeneration_Imp(new Dictionary<string, string> { { "item", value } }, ticket)).ToArray());
					foreach (List<DecryptedItemData> r in array)
					{
						generated.AddRange(r);
					}
					foreach (DecryptedItemData i in generated)
					{
						if (DewSave.items.Contains(i))
						{
							duplicateItemNames.Add(GetLocalizedNameWithType(i.item));
							continue;
						}
						newItemNames.Add(GetLocalizedNameWithType(i.item));
						DewSave.AddServerGeneratedItem(i);
					}
				}
				finally
				{
					if (ticket != null)
					{
						((IDisposable)ticket).Dispose();
					}
				}
			}
			foreach (string o in localAccs)
			{
				if (DewSave.profile.accessories.TryGetValue(o, out var data) && data.isUnlocked)
				{
					duplicateItemNames.Add(GetLocalizedNameWithType(o));
					continue;
				}
				DewSave.profile.UnlockAccessory(o, null);
				newItemNames.Add(GetLocalizedNameWithType(o));
			}
			foreach (string o2 in localEmotes)
			{
				if (DewSave.profile.emotes.TryGetValue(o2, out var data2) && data2.isUnlocked)
				{
					duplicateItemNames.Add(GetLocalizedNameWithType(o2));
					continue;
				}
				DewSave.profile.UnlockEmote(o2, null);
				newItemNames.Add(GetLocalizedNameWithType(o2));
			}
			foreach (string o3 in localNametags)
			{
				if (DewSave.profile.nametags.TryGetValue(o3, out var data3) && data3.isUnlocked)
				{
					duplicateItemNames.Add(GetLocalizedNameWithType(o3));
					continue;
				}
				DewSave.profile.UnlockNametag(o3, null);
				newItemNames.Add(GetLocalizedNameWithType(o3));
			}
			duplicateItemNames.Sort();
			newItemNames.Sort();
			if (newItemNames.Count > 0)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = string.Format(DewLocalization.GetUIValue("Redeem_ReceivedItem"), newItemNames.JoinToString("\n")),
					buttons = DewMessageSettings.ButtonType.Ok
				});
			}
			if (duplicateItemNames.Count > 0)
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = string.Format(DewLocalization.GetUIValue("Redeem_YouAlreadyHaveItem"), duplicateItemNames.JoinToString("\n")),
					buttons = DewMessageSettings.ButtonType.Ok
				});
			}
			Debug.Log($"{newItemNames.Count} new items, {duplicateItemNames.Count} duplicate items.");
			DewSave.SaveItems();
			DewSave.SaveProfile();
			return true;
		}
		catch (Exception e)
		{
			DewSessionError.ShowError(e, isFatal: false, isGame: false);
		}
		finally
		{
			if (ManagerBase<TransitionManager>.instance != null)
			{
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.Empty);
				ManagerBase<TransitionManager>.instance.SetBusy(value: false);
			}
		}
		return false;
	}

	private static async UniTask<List<DecryptedItemData>> RequestItemGeneration_Imp(Dictionary<string, string> body, SteamTicketForWebApi ticket)
	{
		body.Add("userId", SteamUser.GetSteamID().ToString());
		body.Add("appId", SteamUtils.GetAppID().ToString());
		body.Add("ticket", ticket.ticket);
		List<(string, string)> servers = CouponServers.ToList();
		servers.Shuffle();
		string response;
		while (true)
		{
			(string, string) server = servers[0];
			servers.RemoveAt(0);
			Debug.Log("Connecting to " + server.Item1 + "...");
			try
			{
				response = await PostRequestAsync(server.Item2, body);
			}
			catch (Exception)
			{
				Debug.Log("Connection to " + server.Item1 + " failed.");
				if (servers.Count == 0)
				{
					throw;
				}
				continue;
			}
			break;
		}
		if (!response.StartsWith("!"))
		{
			switch (response)
			{
			case "ITEM_NOT_ELIGIBLE":
				throw new DewException(DewExceptionType.ItemNotEligible);
			case "AUTH_FAILED":
				throw new DewException(DewExceptionType.SteamAuthFailed);
			case "INVALID_CODE":
				throw new DewException(DewExceptionType.InvalidGiftCode);
			case "USED_CODE":
				throw new DewException(DewExceptionType.UsedCode);
			case "ALREADY_HAVE":
				throw new DewException(DewExceptionType.AlreadyHaveThisGift);
			default:
				throw new DewException(DewExceptionType.FailedToGetItemGeneric, response);
			}
		}
		string[] array = response.Split("|");
		List<DecryptedItemData> list = new List<DecryptedItemData>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			DecryptedItemData dec = GetDecryptedItemData(array2[i]);
			if (dec == null)
			{
				throw new DewException(DewExceptionType.InvalidResponseFromItemServer);
			}
			list.Add(dec);
		}
		return list;
	}

	public static DecryptedItemData GetDecryptedItemData(string ownershipKey)
	{
		try
		{
			DecryptedItemData? decryptedItemData = JsonConvert.DeserializeObject<DecryptedItemData>(VerifyPgpMessage(Unshorten(ownershipKey)));
			decryptedItemData.ownershipKey = ownershipKey;
			return decryptedItemData;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static string GetLocalizedNameWithType(string name, bool colored = true)
	{
		string type = "";
		string typeColor = "#fff";
		string textColor = "#fff";
		if (name.StartsWith("Acc_"))
		{
			type = DewLocalization.GetUIValue("ItemType_Souvenir");
			typeColor = "#FF8EE0FF";
			textColor = "#FFCFEFFF";
		}
		if (name.StartsWith("Emote_"))
		{
			type = DewLocalization.GetUIValue("ItemType_Emote");
			typeColor = "#FFE084FF";
			textColor = "#FFF0C2FF";
		}
		if (name.StartsWith("Nametag_"))
		{
			type = DewLocalization.GetUIValue("ItemType_Emblem");
			typeColor = "#FF928EFF";
			textColor = "#FFD0CFFF";
		}
		if (colored)
		{
			return "<b><color=" + typeColor + ">" + type + "</color></b> <color=" + textColor + ">" + DewLocalization.GetUIValue(name + "_Name") + "</color>";
		}
		return type + " " + DewLocalization.GetUIValue(name + "_Name");
	}

	public static string Unshorten(string shortMessage)
	{
		return "-----BEGIN PGP MESSAGE-----\n\n" + shortMessage.Substring(1) + "\n-----END PGP MESSAGE-----";
	}

	public static string VerifyPgpMessage(string armoredMessage)
	{
		string publicKeyArmored = "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n\r\nxjMEZ4xvKhYJKwYBBAHaRw8BAQdA/Bv4dryZUdAZ63+svh685awHAWqMYHCj\r\ns+IrE5lMLK7NAMKMBBAWCgA+BYJnjG8qBAsJBwgJkFVJguqKN55VAxUICgQW\r\nAAIBAhkBApsDAh4BFiEEXaobhe0W0u9GValhVUmC6oo3nlUAABpDAPwPzyT9\r\noMq0pVNs64kS36NECdFELnIyYrciA9YuPIMHUgEAuEoxofkuCGtH3JcUj04r\r\nL3u/LW1FpIT/w3N6swCUpgTOOARnjG8qEgorBgEEAZdVAQUBAQdA8z7VA2DF\r\nveLUdHHOKkIuZ6j98ibpdTIETfme+Xwj4k4DAQgHwngEGBYKACoFgmeMbyoJ\r\nkFVJguqKN55VApsMFiEEXaobhe0W0u9GValhVUmC6oo3nlUAAMfvAP4joUMG\r\n6wEwCwmquuEEJ/epIBNjyL+1jOO1VZjwJIQhlwEAm3QMV/fAir/LG3Uvl/k/\r\nQz/UDkTP7cJddxRgadpt0g4=\r\n=A4fl\r\n-----END PGP PUBLIC KEY BLOCK-----";
		try
		{
			using MemoryStream messageStream = new MemoryStream(Encoding.ASCII.GetBytes(armoredMessage));
			using ArmoredInputStream armoredMessageStream = new ArmoredInputStream(messageStream);
			using MemoryStream bufferedStream = new MemoryStream(ReadFully(armoredMessageStream));
			PgpObjectFactory pgpFact = new PgpObjectFactory(bufferedStream);
			PgpObject pgpObject = pgpFact.NextPgpObject();
			PgpPublicKey publicKey;
			using (MemoryStream keyStream = new MemoryStream(Encoding.ASCII.GetBytes(publicKeyArmored)))
			{
				using ArmoredInputStream armoredKeyStream = new ArmoredInputStream(keyStream);
				using MemoryStream bufferedKeyStream = new MemoryStream(ReadFully(armoredKeyStream));
				publicKey = new PgpPublicKeyRing(bufferedKeyStream).GetPublicKey();
			}
			if (publicKey == null)
			{
				throw new Exception("Public key not found");
			}
			if (pgpObject is PgpCompressedData compressedData)
			{
				pgpFact = new PgpObjectFactory(compressedData.GetDataStream());
				pgpObject = pgpFact.NextPgpObject();
			}
			if (pgpObject is PgpOnePassSignatureList onePassSignatureList)
			{
				PgpOnePassSignature onePassSignature = onePassSignatureList[0];
				PgpLiteralData literalData = (PgpLiteralData)pgpFact.NextPgpObject();
				PgpSignature signature = ((PgpSignatureList)pgpFact.NextPgpObject())[0];
				onePassSignature.InitVerify(publicKey);
				StringBuilder messageContent = new StringBuilder();
				using (Stream literalDataStream = literalData.GetInputStream())
				{
					using StreamReader reader = new StreamReader(literalDataStream);
					int ch;
					while ((ch = reader.Read()) >= 0)
					{
						messageContent.Append((char)ch);
						onePassSignature.Update((byte)ch);
					}
				}
				if (onePassSignature.Verify(signature))
				{
					return messageContent.ToString();
				}
				Debug.Log("Signature verification failed");
				return null;
			}
			throw new Exception("Message is not signed or is in an unexpected format");
		}
		catch (Exception ex)
		{
			Debug.Log("Error verifying message: " + ex.Message);
			return null;
		}
	}

	private static byte[] ReadFully(Stream input)
	{
		using MemoryStream ms = new MemoryStream();
		input.CopyTo(ms);
		return ms.ToArray();
	}

	public static async UniTask<string> PostRequestAsync(string url, Dictionary<string, string> postData, float timeoutSeconds = 10f)
	{
		try
		{
			using CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
			WWWForm form = new WWWForm();
			foreach (KeyValuePair<string, string> kvp in postData)
			{
				form.AddField(kvp.Key, kvp.Value);
			}
			using UnityWebRequest request = UnityWebRequest.Post(url, form);
			await request.SendWebRequest().ToUniTask(null, PlayerLoopTiming.Update, cts.Token);
			if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.DataProcessingError)
			{
				throw new Exception("Request failed: " + request.error);
			}
			return request.downloadHandler.text;
		}
		catch (OperationCanceledException)
		{
			throw new TimeoutException($"Request timed out after {timeoutSeconds} seconds");
		}
		catch (Exception ex2)
		{
			throw new Exception("Request failed: " + ex2.Message);
		}
	}
}
