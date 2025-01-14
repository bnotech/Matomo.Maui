﻿using Newtonsoft.Json;

namespace Matomo.Maui.Services.Storage
{
	public class SimpleStorage : ISimpleStorage
	{
		private string _filename;
		private Dictionary<string, object> _data;

		public SimpleStorage()
		{
			_filename = "matomodata";
			_data = new Dictionary<string, object>();
			ReadFromDisk();
		}

		public bool HasKey(string key)
		{
			return _data.ContainsKey(key);
		}

		public string Get(string key)
		{
			return Get<string>(key);
		}

		public T Get<T>(string key)
		{
			if (!_data.Any())
				ReadFromDisk();

			return (T)_data[key];
		}

		public T Get<T>(string key, T defaultValue)
		{
			if (HasKey(key))
				return Get<T>(key);

			Put<T>(key, defaultValue);
			return defaultValue;
		}

		public void Put<T>(string key, T value)
		{
			if (HasKey(key))
				_data[key] = value;
			else
				_data.Add(key, value);
			WriteToDisk();
		}

		private void ReadFromDisk()
		{
			if (File.Exists($"{_filename}.json"))
			{
				using (TextReader file = File.OpenText($"{_filename}.json"))
				{
					var json = file.ReadToEnd();
					_data = (Dictionary<string, object>)JsonConvert.DeserializeObject(json);
				}
			}
		}

		private void WriteToDisk()
		{
            using (StreamWriter file = File.CreateText($"{_filename}.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _data);
            }
        }
	}
}

