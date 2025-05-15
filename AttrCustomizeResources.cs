using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AttrCustomizeResources
{
	private static AttrCustomizeConfig _config;

	public static AttrCustomizeConfig Config
	{
		get
		{
			if (_config == null)
			{
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AttrCustomizeConfig.json");
				if (!File.Exists(path))
				{
					string contents = JsonConvert.SerializeObject(AttrCustomizeConfig.DefaultConfig, Formatting.Indented);
					File.WriteAllText(path, contents);
					_config = AttrCustomizeConfig.DefaultConfig;
					return _config;
				}
				string sourceJson = File.ReadAllText(path);
				string targetJson = JsonConvert.SerializeObject(AttrCustomizeConfig.DefaultConfig, Formatting.Indented);
				targetJson = MergeJson(sourceJson, targetJson);
				_config = JsonConvert.DeserializeObject<AttrCustomizeConfig>(targetJson) ?? AttrCustomizeConfig.DefaultConfig;
				string contents2 = JsonConvert.SerializeObject(_config, Formatting.Indented);
				File.WriteAllText(path, contents2);
			}
			return _config;
		}
	}

	public static string MergeJson(string sourceJson, string targetJson)
	{
		JObject content = JObject.Parse(sourceJson);
		JObject jObject = JObject.Parse(targetJson);
		jObject.Merge(content, new JsonMergeSettings
		{
			MergeArrayHandling = MergeArrayHandling.Replace
		});
		return jObject.ToString();
	}
}
