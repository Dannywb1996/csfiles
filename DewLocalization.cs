using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using DewInternal;
using UnityEngine;

public static class DewLocalization
{
	public struct EntityStats
	{
		public float attackDamage;

		public float abilityPower;

		public EntityStats(DewGameResult.PlayerData data)
		{
			attackDamage = data.attackDamage;
			abilityPower = data.abilityPower;
		}
	}

	public struct DescriptionSettings
	{
		public Entity contextEntity;

		public UnityEngine.Object contextObject;

		public int[] levels;

		public int? currentLevel;

		public int? previousLevel;

		public bool showLevelScaling;

		public bool isSkillStar;

		public EntityStats? stats;

		public Dictionary<string, string> capturedFields;
	}

	private const string MainLocalization = "MainLocalization";

	private static DewLocalizationBuildData _buildData;

	private static DataTable _calculator = new DataTable
	{
		Locale = CultureInfo.InvariantCulture
	};

	private static StringBuilder _expression = new StringBuilder();

	private static readonly StringBuilder _textSb = new StringBuilder();

	private static readonly StringBuilder _expSb = new StringBuilder();

	public static DewLocalizationBuildData buildData
	{
		get
		{
			if (_buildData == null)
			{
				_buildData = Resources.Load<DewLocalizationBuildData>("MainLocalization");
				if (_buildData == null)
				{
					throw new Exception("Could not load main DewLocalizationBuildData!");
				}
				_buildData.InitForRuntime();
			}
			return _buildData;
		}
	}

	public static PerLanguageLocalizationData data
	{
		get
		{
			if (DewSave.profile != null)
			{
				return buildData.dataByLanguage[DewSave.profile.language];
			}
			return buildData.dataByLanguage["ko-KR"];
		}
	}

	public static void Initialize()
	{
		Resources.UnloadUnusedAssets();
		_buildData = null;
		_ = buildData;
	}

	private static List<LocaleNode> DebugNodeList(string message)
	{
		return new List<LocaleNode>
		{
			new LocaleNode
			{
				type = LocaleNodeType.Text,
				textData = message
			}
		};
	}

	public static string ConvertDescriptionNodesToText(IReadOnlyList<LocaleNode> nodes, DescriptionSettings settings)
	{
		bool isInGame = NetworkedManagerBase<GameManager>.softInstance != null;
		bool isEquipped = false;
		if (isInGame)
		{
			if (settings.contextObject is SkillTrigger st && st.netIdentity != null && st.owner != null)
			{
				isEquipped = true;
			}
			else if (settings.contextObject is Gem g && g.netIdentity != null && g.owner != null)
			{
				isEquipped = true;
			}
		}
		StringBuilder finalText = new StringBuilder();
		bool shouldShowDetail = Application.isPlaying && DewInput.GetButton(DewSave.profile.controls.showDetails, checkGameAreaForMouse: false);
		if (shouldShowDetail)
		{
			settings.showLevelScaling = true;
		}
		bool isInsideTag_ingame = false;
		bool isInsideTag_equipped = false;
		foreach (LocaleNode n in nodes)
		{
			if (n.type == LocaleNodeType.Text)
			{
				if ((!isInsideTag_ingame || isInGame) && (!isInsideTag_equipped || isEquipped))
				{
					finalText.Append(n.textData);
				}
			}
			else if (n.type == LocaleNodeType.Tag)
			{
				if (n.textData == "equipped")
				{
					isInsideTag_equipped = true;
				}
				if (n.textData == "/equipped")
				{
					isInsideTag_equipped = false;
				}
				if (n.textData == "ingame")
				{
					isInsideTag_ingame = true;
				}
				if (n.textData == "/ingame")
				{
					isInsideTag_ingame = false;
				}
			}
			else if ((!isInsideTag_ingame || isInGame) && (!isInsideTag_equipped || isEquipped))
			{
				finalText.Append(EvaluateExpression(n.expressionData, settings, shouldShowDetail));
			}
		}
		return finalText.ToString();
	}

	public static void CaptureDescriptionExpressions(IReadOnlyList<LocaleNode> nodes, Dictionary<string, string> data, DescriptionSettings settings)
	{
		foreach (LocaleNode n in nodes)
		{
			if (n.type == LocaleNodeType.Expression)
			{
				data[n.expressionData.raw] = EvaluateExpression(n.expressionData, settings, shouldShowDetail: false);
			}
		}
	}

	public static string EvaluateExpression(ExpressionData exp, DescriptionSettings settings, bool shouldShowDetail)
	{
		if (settings.capturedFields != null)
		{
			return settings.capturedFields.GetValueOrDefault(exp.raw, "<color=grey>???</color>");
		}
		bool hasApFactor = false;
		bool hasAdFactor = false;
		bool hasLevelScaling = false;
		try
		{
			if (settings.levels == null || settings.levels.Length <= 1)
			{
				int targetLvl = 1;
				if (settings.contextObject is Gem gem)
				{
					targetLvl = gem.effectiveLevel;
				}
				else if (settings.contextObject is SkillTrigger skill)
				{
					targetLvl = skill.level;
				}
				if (settings.currentLevel.HasValue)
				{
					targetLvl = settings.currentLevel.Value;
				}
				return Render(targetLvl);
			}
			string val = Render(settings.levels[0]);
			bool hasDiff = false;
			for (int i = 1; i < settings.levels.Length; i++)
			{
				if (val != Render(settings.levels[i]))
				{
					hasDiff = true;
					break;
				}
			}
			if (!hasDiff)
			{
				return Render(settings.levels[0]);
			}
			string result = "";
			for (int j = 0; j < settings.levels.Length; j++)
			{
				settings.showLevelScaling = settings.levels.Length != 1 && j == settings.levels.Length - 1;
				result = ((!settings.currentLevel.HasValue) ? (result + Render(settings.levels[j])) : ((settings.currentLevel == settings.levels[j]) ? (result + Render(settings.levels[j])) : (result + "<color=grey>" + Render(settings.levels[j]) + "</color>")));
				if (j != settings.levels.Length - 1)
				{
					result += "<color=grey> / </color>";
				}
			}
			return result;
		}
		catch (DewExpressionException ex)
		{
			Debug.LogException(ex);
			return ex.Message;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return "!exp";
		}
		string Colorize(string input)
		{
			if (settings.previousLevel.HasValue && hasLevelScaling)
			{
				return "<color=#fffd5c>" + input + "</color>";
			}
			if (hasApFactor)
			{
				return "<color=#16D7FF>" + input + "</color>";
			}
			if (hasAdFactor)
			{
				return "<color=#FF8A2D>" + input + "</color>";
			}
			return input;
		}
		float Evaluate(int level, float attackDamage, float abilityPower)
		{
			_expression.Clear();
			foreach (ExpressionChildNode e in exp.nodes)
			{
				if (e.type == ExpressionChildNodeType.Expression)
				{
					_expression.Append(e.value);
				}
				else
				{
					if (!buildData.fieldInfos.TryGetValue(e.value, out var info))
					{
						throw new DewExpressionException("!noinfo(" + e.value + ")");
					}
					object value = null;
					if (info.requiresTarget)
					{
						string parentName = e.value.Split("::")[0];
						UnityEngine.Object fieldTarget = (parentName.StartsWith("Star_") ? null : ((!(settings.contextObject != null) || !(settings.contextObject.GetType().Name == parentName)) ? DewResources.GetByShortTypeName(parentName) : settings.contextObject));
						value = fieldTarget;
					}
					try
					{
						ScalingValue.levelOverride = level;
						Func<object, object>[] valueGetters = info.valueGetters;
						for (int k = 0; k < valueGetters.Length; k++)
						{
							value = valueGetters[k](value);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						throw new DewExpressionException("!field(" + e.value + ")");
					}
					finally
					{
						ScalingValue.levelOverride = null;
					}
					if (value is ScalingValue sv0)
					{
						_expression.Append(sv0.GetValue(level, attackDamage, abilityPower).ToString(CultureInfo.InvariantCulture));
					}
					else if (value is float f)
					{
						_expression.Append(f.ToString(CultureInfo.InvariantCulture));
					}
					else if (value is double d)
					{
						_expression.Append(d.ToString(CultureInfo.InvariantCulture));
					}
					else if (value is int integer)
					{
						_expression.Append(integer.ToString(CultureInfo.InvariantCulture));
					}
					else if (value is int[] iarr)
					{
						_expression.Append(iarr.GetClamped(level - 1).ToString(CultureInfo.InvariantCulture));
					}
					else if (value is float[] farr)
					{
						_expression.Append(farr.GetClamped(level - 1).ToString(CultureInfo.InvariantCulture));
					}
					else if (value is double[] darr)
					{
						_expression.Append(((float)darr.GetClamped(level - 1)).ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						_expression.Append(value);
					}
				}
			}
			return float.Parse(_calculator.Compute(_expression.ToString(), null).ToString());
		}
		string GetPrefix()
		{
			if (hasApFactor)
			{
				return "<sprite=1>";
			}
			if (hasAdFactor)
			{
				return "<sprite=2>";
			}
			return "";
		}
		string GetSuffix(bool hideLevelScaling)
		{
			if (!settings.showLevelScaling)
			{
				return "";
			}
			if (settings.isSkillStar)
			{
				return "";
			}
			if (hasLevelScaling && !hideLevelScaling)
			{
				return "<sprite=5>";
			}
			return "";
		}
		string Render(int num2)
		{
			float num = Evaluate(0, 0f, 0f);
			if (num != Evaluate(0, 100f, 0f))
			{
				hasAdFactor = true;
			}
			if (num != Evaluate(0, 0f, 100f))
			{
				hasApFactor = true;
			}
			if (Evaluate(num2, 1f, 1f) != Evaluate(num2 + 200, 1f, 1f))
			{
				hasLevelScaling = true;
			}
			if (!settings.stats.HasValue && settings.contextEntity != null)
			{
				settings.stats = new EntityStats
				{
					abilityPower = settings.contextEntity.Status.abilityPower,
					attackDamage = settings.contextEntity.Status.attackDamage
				};
			}
			if (settings.stats.HasValue)
			{
				EntityStats stats = settings.stats.Value;
				string formattedFinalValue = Evaluate(num2, stats.attackDamage, stats.abilityPower).ToString(exp.format);
				if (settings.previousLevel.HasValue)
				{
					string formattedFromVal = Evaluate(settings.previousLevel.Value, stats.attackDamage, stats.abilityPower).ToString(exp.format);
					if (formattedFinalValue != formattedFromVal)
					{
						if (shouldShowDetail)
						{
							if (hasAdFactor)
							{
								formattedFromVal = $"{Evaluate(settings.previousLevel.Value, 1f, 0f):#,##0%} ({formattedFromVal})";
								formattedFinalValue = $"{Evaluate(num2, 1f, 0f):#,##0%} ({formattedFinalValue})";
							}
							if (hasApFactor)
							{
								formattedFromVal = $"{Evaluate(settings.previousLevel.Value, 0f, 1f):#,##0%} ({formattedFromVal})";
								formattedFinalValue = $"{Evaluate(num2, 0f, 1f):#,##0%} ({formattedFinalValue})";
							}
						}
						return GetPrefix() + RenderChangedValue(formattedFromVal, formattedFinalValue) + GetSuffix(hideLevelScaling: false);
					}
				}
				if (shouldShowDetail)
				{
					if (hasAdFactor)
					{
						return GetPrefix() + Colorize($"{Evaluate(num2, 1f, 0f):#,##0%}") + " (" + formattedFinalValue + ")" + GetSuffix(hideLevelScaling: false);
					}
					if (hasApFactor)
					{
						return GetPrefix() + Colorize($"{Evaluate(num2, 0f, 1f):#,##0%}") + " (" + formattedFinalValue + ")" + GetSuffix(hideLevelScaling: false);
					}
				}
				return GetPrefix() + Colorize(formattedFinalValue) + GetSuffix(hideLevelScaling: false);
			}
			if (settings.previousLevel.HasValue)
			{
				string text = RenderRawFactorValue(colorize: false, settings.previousLevel.Value, hideLevelScaling: false);
				string next = RenderRawFactorValue(colorize: false, num2, hideLevelScaling: false);
				if (text != next)
				{
					return RenderChangedValue(RenderRawFactorValue(colorize: false, settings.previousLevel.Value, hideLevelScaling: true), RenderRawFactorValue(colorize: true, num2, hideLevelScaling: false));
				}
			}
			return RenderRawFactorValue(colorize: true, num2, hideLevelScaling: false);
		}
		string RenderRawFactorValue(bool colorize, int lvl, bool hideLevelScaling)
		{
			float baseVal = Evaluate(lvl, 0f, 0f);
			if (baseVal > 0f)
			{
				float factor = 0f;
				if (hasApFactor)
				{
					factor = Evaluate(lvl, 0f, 1f) - baseVal;
				}
				else if (hasAdFactor)
				{
					factor = Evaluate(lvl, 1f, 0f) - baseVal;
				}
				if (factor > 0f)
				{
					return string.Format("{0:#,##0}+{1}{2}{3}", baseVal, GetPrefix(), colorize ? Colorize(factor.ToString("#,##0%")) : factor.ToString("#,##0%"), GetSuffix(hideLevelScaling));
				}
				return baseVal.ToString(exp.format) + GetSuffix(hideLevelScaling);
			}
			float factor2 = 0f;
			if (hasApFactor)
			{
				factor2 = Evaluate(lvl, 0f, 1f);
			}
			else if (hasAdFactor)
			{
				factor2 = Evaluate(lvl, 1f, 0f);
			}
			if (factor2 > 0f)
			{
				return GetPrefix() + (colorize ? Colorize(factor2.ToString("#,##0%")) : factor2.ToString("#,##0%")) + GetSuffix(hideLevelScaling);
			}
			return "0" + GetSuffix(hideLevelScaling);
		}
	}

	public static string RenderChangedValue(string prev, string after)
	{
		return "<color=#888>" + prev + "</color><sprite=0><color=#fffd5c>" + after + "</color>";
	}

	public static string GetSkillLevelTemplate(int level, int? toLevel = null)
	{
		if (toLevel.HasValue)
		{
			return "{0}" + (GetSkillLevelSuffix(level).StartsWith(" ") ? " " : "") + RenderChangedValue(GetSkillLevelSuffix(level).Trim(), GetSkillLevelSuffix(toLevel.Value).Trim());
		}
		return "{0}" + GetSkillLevelSuffix(level);
	}

	public static string GetSkillLevelSuffix(int level)
	{
		level = Mathf.Max(level, 1);
		if (level < 5)
		{
			return new string('+', level - 1) ?? "";
		}
		return $" +{level - 1}";
	}

	public static string HighlightKeywords(string input)
	{
		return input.Replace("[", "<color=yellow>").Replace("]", "</color>");
	}

	private static bool TryGetConstValue(Type type, string fieldName, out string value)
	{
		if (type == null)
		{
			value = null;
			return false;
		}
		FieldInfo constField = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (constField == null || !constField.IsLiteral || constField.IsInitOnly)
		{
			value = null;
			return false;
		}
		value = Dew.ConvertToStringInvariantCulture(constField.GetRawConstantValue());
		return true;
	}

	public static string ProcessGenericBacktickedString(string text, object refObject)
	{
		_textSb.Clear();
		DewLocalizationNodeParser.ParseBacktickedString(text, delegate(string normal)
		{
			_textSb.Append(normal);
		}, delegate(string tag)
		{
			_textSb.Append("<" + tag + ">");
		}, delegate(string expression)
		{
			_expSb.Clear();
			string[] array = expression.Split("|");
			string format = DewLocalizationNodeParser.GetFormat((array.Length > 1) ? array[1] : null);
			DewLocalizationNodeParser.ParseExpression(array[0], delegate(string exp)
			{
				_expSb.Append(exp);
			}, delegate(string field)
			{
				string text3;
				FieldInfo field2;
				PropertyInfo property;
				Type type2;
				if (field.Contains("::"))
				{
					string[] array2 = field.Split("::");
					string text2 = array2[0];
					text3 = array2[1];
					if (!Dew.TryGetTypeFromShortName(text2, out var type))
					{
						Error("notype-" + text2);
						return;
					}
					field2 = type.GetField(text3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					property = type.GetProperty(text3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					type2 = type;
				}
				else
				{
					if (refObject == null)
					{
						Error("context-" + field);
						return;
					}
					text3 = field;
					type2 = ((refObject is Type) ? ((Type)refObject) : refObject.GetType());
					field2 = type2.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					property = type2.GetProperty(text3, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}
				if (TryGetConstValue(type2, text3, out var value))
				{
					_expSb.Append(value);
				}
				else if (field2 == null && property == null)
				{
					Error("field-" + field);
				}
				else if (refObject != null && type2.IsInstanceOfType(refObject))
				{
					object value2 = ((field2 == null) ? property.GetValue(refObject) : field2.GetValue(refObject));
					_expSb.Append(Dew.ConvertToStringInvariantCulture(value2));
				}
				else if (DewResources.GetByShortTypeName(type2.Name) != null)
				{
					object value3 = ((field2 == null) ? property.GetValue(refObject) : field2.GetValue(refObject));
					_expSb.Append(Dew.ConvertToStringInvariantCulture(value3));
				}
				else
				{
					Error("fail-" + field);
				}
			});
			try
			{
				object obj = _calculator.Compute(_expSb.ToString(), null);
				_textSb.Append(float.Parse(obj.ToString()).ToString(format));
			}
			catch
			{
				_textSb.Append(_expSb.ToString());
			}
		});
		return _textSb.ToString();
		static void Error(string err)
		{
			Debug.LogWarning("!" + err + "!");
			_expSb.Append("!" + err + "!");
		}
	}

	public static string GetRecommendedSupportedLanguage()
	{
		string[] supported = buildData.dataByLanguage.Keys.ToArray();
		string currentCulture = CultureInfo.CurrentUICulture.Name;
		if (supported.Contains(currentCulture))
		{
			return currentCulture;
		}
		if (currentCulture.Equals("zh-Hans", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-CN";
		}
		if (currentCulture.Equals("zh-SG", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-CN";
		}
		if (currentCulture.Equals("zh-Hant", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-TW";
		}
		if (currentCulture.Equals("zh-HK", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-TW";
		}
		if (currentCulture.Equals("zh-MO", StringComparison.InvariantCultureIgnoreCase))
		{
			return "zh-TW";
		}
		string currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		string recommendedCulture = supported.FirstOrDefault((string culture) => culture.StartsWith(currentLanguage));
		if (!string.IsNullOrEmpty(recommendedCulture))
		{
			return recommendedCulture;
		}
		return "en-US";
	}

	public static bool TryGetUIValue(string key, out string value)
	{
		if (key == null)
		{
			value = null;
			return false;
		}
		if (data.ui.TryGetValue(key, out value))
		{
			return true;
		}
		return false;
	}

	public static string GetUIValue(string key)
	{
		if (key == null)
		{
			return "ui.!null";
		}
		if (data.ui.TryGetValue(key, out var value))
		{
			return value;
		}
		return "ui.!" + key;
	}

	public static string GetSkillKey(Type skillType)
	{
		return GetSkillKey(skillType.Name);
	}

	public static string GetSkillKey(string skillType)
	{
		return skillType.Substring(3);
	}

	public static string GetSkillName(SkillTrigger skill, int configIndex)
	{
		return GetSkillName(GetSkillKey(skill.GetType()), configIndex);
	}

	public static string GetSkillName(string key, int configIndex)
	{
		if (!data.skills.TryGetValue(key, out var val))
		{
			return $"skills.!{key}[{configIndex}].name";
		}
		if (configIndex >= val.configs.Count)
		{
			return $"skills.{key}[!{configIndex}].name";
		}
		return val.configs[configIndex].name;
	}

	public static string GetSkillShortDesc(string key, int configIndex)
	{
		if (!data.skills.TryGetValue(key, out var val))
		{
			return $"skills.!{key}[{configIndex}].short";
		}
		if (configIndex >= val.configs.Count)
		{
			return $"skills.{key}[!{configIndex}].short";
		}
		return val.configs[configIndex].shortDescription;
	}

	public static List<LocaleNode> GetSkillDescription(string key, int configIndex)
	{
		if (!data.skills.TryGetValue(key, out var val))
		{
			return DebugNodeList($"skills.!{key}[{configIndex}].desc");
		}
		if (configIndex >= val.configs.Count)
		{
			return DebugNodeList($"skills.{key}[!{configIndex}].desc");
		}
		return val.configs[configIndex].description;
	}

	public static string GetSkillMemory(string key)
	{
		if (!data.skills.TryGetValue(key, out var val))
		{
			return "skills.!" + key + ".memory";
		}
		return val.memory;
	}

	public static string GetSkillNameKey(SkillTrigger skill, int configIndex)
	{
		string name = skill.GetType().Name;
		if (name.StartsWith("St_", StringComparison.OrdinalIgnoreCase))
		{
			return string.Format("Skill.{0}.{1}.name", name.Substring("St_".Length), configIndex);
		}
		return name;
	}

	public static string GetSkillDescriptionKey(SkillTrigger skill, int configIndex)
	{
		string name = skill.GetType().Name;
		if (name.StartsWith("St_", StringComparison.OrdinalIgnoreCase))
		{
			return string.Format("Skill.{0}.{1}.desc", name.Substring("St_".Length), configIndex);
		}
		return name;
	}

	public static string GetSkillLevelKey(int level)
	{
		return $"UI.SkillLevel{level}";
	}

	public static string GetGemKey(Type gemType)
	{
		return GetGemKey(gemType.Name);
	}

	public static string GetGemKey(string gemType)
	{
		return gemType.Substring(4);
	}

	public static string GetGemName(Gem gem)
	{
		return GetGemName(GetGemKey(gem.GetType().Name));
	}

	public static string GetGemName(string key)
	{
		if (!data.gems.TryGetValue(key, out var val))
		{
			return "gems.!" + key + ".name";
		}
		return val.name;
	}

	public static string GetGemShortDescription(string key)
	{
		if (!data.gems.TryGetValue(key, out var val))
		{
			return "gems.!" + key + ".short";
		}
		return val.shortDescription;
	}

	public static string GetGemTemplate(string key)
	{
		if (!data.gems.TryGetValue(key, out var val))
		{
			return "gems.!" + key + ".template {0}";
		}
		return val.template;
	}

	public static IReadOnlyList<LocaleNode> GetGemDescription(string key)
	{
		if (!data.gems.TryGetValue(key, out var val))
		{
			return DebugNodeList("gems.!" + key + ".desc");
		}
		return val.description;
	}

	public static string GetGemMemory(string key)
	{
		if (!data.gems.TryGetValue(key, out var val))
		{
			return "gems.!" + key + ".memory";
		}
		return val.memory;
	}

	public static string GetGemNameKey(Gem gem)
	{
		string name = gem.GetType().Name;
		if (name.StartsWith("Gem_", StringComparison.OrdinalIgnoreCase))
		{
			return "Gem_." + name.Substring("Gem_".Length) + ".name";
		}
		return name;
	}

	public static string GetGemDescriptionKey(Gem gem)
	{
		string name = gem.GetType().Name;
		if (name.StartsWith("Gem_", StringComparison.OrdinalIgnoreCase))
		{
			return "Gem_." + name.Substring("Gem_".Length) + ".desc";
		}
		return name;
	}

	public static string GetArtifactKey(Type artifactType)
	{
		return GetArtifactKey(artifactType.Name);
	}

	public static string GetArtifactKey(string artifactType)
	{
		return artifactType.Substring(9);
	}

	public static string GetAchievementName(string key)
	{
		if (!data.achievements.TryGetValue(key, out var val))
		{
			return "achievements.!" + key + ".name";
		}
		return val.name;
	}

	public static string GetAchievementDescription(string key)
	{
		if (!data.achievements.TryGetValue(key, out var val))
		{
			return "achievements.!" + key + ".desc";
		}
		return ProcessGenericBacktickedString(val.description, Dew.achievementsByName[key]);
	}

	public static string GetStarName(Type type)
	{
		return GetStarName(type.Name);
	}

	public static string GetStarName(string key)
	{
		if (!data.stars.TryGetValue(key, out var val))
		{
			return "stars.!" + key + ".name";
		}
		return val.name;
	}

	public static string GetStarLore(string key)
	{
		if (!data.stars.TryGetValue(key, out var val))
		{
			return "stars.!" + key + ".lore";
		}
		return val.lore;
	}

	public static List<LocaleNode> GetStarDescription(Type type)
	{
		return GetStarDescription(type.Name);
	}

	public static List<LocaleNode> GetStarDescription(string key)
	{
		if (!data.stars.TryGetValue(key, out var val))
		{
			return DebugNodeList("stars.!" + key + ".desc");
		}
		return val.description;
	}

	public static string GetCurseName(string key)
	{
		if (!data.curses.TryGetValue(key, out var val))
		{
			return "curses.!" + key + ".name";
		}
		return val.name;
	}

	public static List<LocaleNode> GetCurseDescription(string key)
	{
		if (!data.curses.TryGetValue(key, out var val))
		{
			return DebugNodeList("curses.!" + key + ".desc");
		}
		return val.description;
	}

	public static List<LocaleNode> GetCurseShortDescription(string key)
	{
		if (!data.curses.TryGetValue(key, out var val))
		{
			return DebugNodeList("curses.!" + key + ".short");
		}
		return val.shortDesc;
	}

	public static string GetCurseKey(string name)
	{
		return name.Substring(9);
	}

	public static ConversationData GetConversationData(string key)
	{
		if (key == null)
		{
			return null;
		}
		if (data.conversations.TryGetValue(key, out var d))
		{
			return d;
		}
		return null;
	}

	public static string GetArtifactName(string key)
	{
		if (key == null)
		{
			return "artifacts.!null.name";
		}
		if (!data.artifacts.TryGetValue(key, out var value))
		{
			return "artifacts.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetArtifactStory(string key)
	{
		if (key == null)
		{
			return "artifacts.!null.story";
		}
		if (!data.artifacts.TryGetValue(key, out var value))
		{
			return "artifacts.!" + key + ".story";
		}
		return value.story;
	}

	public static string[] GetArtifactShortStory(string key)
	{
		if (key == null)
		{
			return new string[1] { "artifacts.!null.shortstory" };
		}
		if (!data.artifacts.TryGetValue(key, out var value))
		{
			return new string[1] { "artifacts.!" + key + ".shortstory" };
		}
		return value.shortStory;
	}

	public static string GetTreasureKey(string treasureType)
	{
		return treasureType.Substring(9);
	}

	public static string GetTreasureName(string key)
	{
		if (key == null)
		{
			return "treasures.!null.name";
		}
		if (!data.treasures.TryGetValue(key, out var value))
		{
			return "treasures.!" + key + ".name";
		}
		return value.name;
	}

	public static string GetTreasureLore(string key)
	{
		if (key == null)
		{
			return "treasures.!null.name";
		}
		if (!data.treasures.TryGetValue(key, out var value))
		{
			return "treasures.!" + key + ".lore";
		}
		return value.lore;
	}

	public static IReadOnlyList<LocaleNode> GetTreasureDescription(string key)
	{
		if (!data.treasures.TryGetValue(key, out var val))
		{
			return DebugNodeList("treasures.!" + key + ".desc");
		}
		return val.description;
	}
}
