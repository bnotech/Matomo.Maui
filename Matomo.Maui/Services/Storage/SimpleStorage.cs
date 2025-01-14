using Newtonsoft.Json;

namespace Matomo.Maui.Services.Storage
{
	public class SimpleStorage : ISimpleStorage
	{
		private const string GroupName = "matomodata";

		public bool HasKey(string key)
		{
			return Preferences.Default.ContainsKey(GetKey(key));
		}

		public string Get(string key)
		{
			return Preferences.Get(GetKey(key), string.Empty);
		}

		public T Get<T>(string key)
		{
			var json = Preferences.Get(GetKey(key), string.Empty);
			return JsonConvert.DeserializeObject<T>(json);
		}

		public T Get<T>(string key, T defaultValue)
		{
			if (!HasKey(key))
			{
				return defaultValue;
			}

			return Get<T>(key);
		}

		public void Put<T>(string key, T value)
		{
			var json = JsonConvert.SerializeObject(value);
			Preferences.Default.Set(GetKey(key), json);
		}

		private string GetKey(string nonGroupedKey)
		{
			return GroupName + nonGroupedKey;
		}
	}
}

