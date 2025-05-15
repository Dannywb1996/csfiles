using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DewInternal;

[CreateAssetMenu(fileName = "New Dew Localization Build Data", menuName = "Dew Localization Build Data", order = 1)]
public class DewLocalizationBuildData : SerializedScriptableObject
{
	public Dictionary<string, PerLanguageLocalizationData> dataByLanguage;

	private Dictionary<string, DewFieldInfo> _fieldInfos;

	public IReadOnlyDictionary<string, DewFieldInfo> fieldInfos => _fieldInfos;

	public void InitForRuntime()
	{
		_fieldInfos = new Dictionary<string, DewFieldInfo>();
		foreach (KeyValuePair<string, PerLanguageLocalizationData> data in dataByLanguage)
		{
			foreach (KeyValuePair<string, SkillData> skill2 in data.Value.skills)
			{
				foreach (SkillConfigData config in skill2.Value.configs)
				{
					foreach (LocaleNode descNode in config.description)
					{
						if (descNode.type != LocaleNodeType.Expression)
						{
							continue;
						}
						foreach (ExpressionChildNode expData in descNode.expressionData.nodes)
						{
							if (expData.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(expData.value))
							{
								InitFieldData(expData);
							}
						}
					}
				}
			}
			foreach (KeyValuePair<string, GemData> gem2 in data.Value.gems)
			{
				foreach (LocaleNode descNode2 in gem2.Value.description)
				{
					if (descNode2.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode expData2 in descNode2.expressionData.nodes)
					{
						if (expData2.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(expData2.value))
						{
							InitFieldData(expData2);
						}
					}
				}
			}
			foreach (KeyValuePair<string, StarData> star2 in data.Value.stars)
			{
				foreach (LocaleNode descNode3 in star2.Value.description)
				{
					if (descNode3.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode expData3 in descNode3.expressionData.nodes)
					{
						if (expData3.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(expData3.value))
						{
							InitFieldData(expData3);
						}
					}
				}
			}
			foreach (KeyValuePair<string, CurseData> curse in data.Value.curses)
			{
				foreach (LocaleNode descNode4 in curse.Value.description)
				{
					if (descNode4.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode expData4 in descNode4.expressionData.nodes)
					{
						if (expData4.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(expData4.value))
						{
							InitFieldData(expData4);
						}
					}
				}
				foreach (LocaleNode descNode5 in curse.Value.shortDesc)
				{
					if (descNode5.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode expData5 in descNode5.expressionData.nodes)
					{
						if (expData5.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(expData5.value))
						{
							InitFieldData(expData5);
						}
					}
				}
			}
			foreach (KeyValuePair<string, TreasureData> treasure2 in data.Value.treasures)
			{
				foreach (LocaleNode descNode6 in treasure2.Value.description)
				{
					if (descNode6.type != LocaleNodeType.Expression)
					{
						continue;
					}
					foreach (ExpressionChildNode expData6 in descNode6.expressionData.nodes)
					{
						if (expData6.type == ExpressionChildNodeType.FieldName && !_fieldInfos.ContainsKey(expData6.value))
						{
							InitFieldData(expData6);
						}
					}
				}
			}
		}
		Debug.Log($"DewLocalization init for runtime: {fieldInfos.Count} Field Infos");
		void InitFieldData(ExpressionChildNode expressionChildNode)
		{
			string[] parentSplit = expressionChildNode.value.Split("::");
			if (Dew.TryGetTypeFromShortName(parentSplit[0], out var type))
			{
				FieldInfo field = type.GetField(parentSplit[1], BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					_fieldInfos[expressionChildNode.value] = new DewFieldInfo
					{
						requiresTarget = false,
						valueGetters = new Func<object, object>[1]
						{
							(object _) => field.GetValue(null)
						},
						isProperty = false
					};
					return;
				}
			}
			string[] fieldSplit = parentSplit[1].Split('.');
			_fieldInfos[expressionChildNode.value] = new DewFieldInfo
			{
				requiresTarget = true,
				valueGetters = new Func<object, object>[fieldSplit.Length],
				isProperty = false
			};
			Type currentFieldParentType = Dew.GetTypeFromShortName(parentSplit[0]);
			for (int i = 0; i < fieldSplit.Length; i++)
			{
				FieldInfo field2 = currentFieldParentType.GetField(fieldSplit[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field2 != null)
				{
					_fieldInfos[expressionChildNode.value].valueGetters[i] = delegate(object obj)
					{
						if (obj == null)
						{
							return "!nullobj";
						}
						try
						{
							return field2.GetValue(obj);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							return "!exception";
						}
					};
					currentFieldParentType = field2.FieldType;
				}
				else
				{
					PropertyInfo property = currentFieldParentType.GetProperty(fieldSplit[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(property != null))
					{
						throw new Exception("Cannot resolve field or property '" + fieldSplit[i] + "' in type '" + currentFieldParentType.Name + "' of expression '" + expressionChildNode.value + "'");
					}
					DewFieldInfo temp = _fieldInfos[expressionChildNode.value];
					temp.isProperty = true;
					_fieldInfos[expressionChildNode.value] = temp;
					_fieldInfos[expressionChildNode.value].valueGetters[i] = delegate(object obj)
					{
						if (obj == null)
						{
							return "!nullobj";
						}
						try
						{
							return property.GetValue(obj);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							return "!exception";
						}
					};
					currentFieldParentType = property.PropertyType;
				}
			}
		}
	}
}
