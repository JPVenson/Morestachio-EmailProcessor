using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Morestachio.MailProcessor.Ui.Services.DataImport;

namespace Morestachio.MailProcessor.Ui.Services.Settings
{
	public class PersistantSettingsService : IRequireInit
	{
		public PersistantSettingsService()
		{
			Settings = new List<SettingsMetaEntry>();
			
			SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"Morestachio MailProcessor");
		}

		public string SettingsDirectory { get; set; }
		public IList<SettingsMetaEntry> Settings { get; set; }

		public SettingsEntry LoadedSettings { get; set; }

		public bool SaveSetting(SettingsEntry settingsToSave)
		{
			try
			{
				var settings = JsonSerializer.Serialize(settingsToSave);
				
				if (!Directory.Exists(SettingsDirectory))
				{
					Directory.CreateDirectory(SettingsDirectory);
				}

				File.WriteAllText(Path.Combine(SettingsDirectory, settingsToSave.Name + ".mailSettings"), settings);

				var hasSetting = Settings.FirstOrDefault(e => e.Name == settingsToSave.Name);
				if (hasSetting == null)
				{
					Settings.Add(new SettingsMetaEntry()
					{
						Name = settingsToSave.Name,
						Path = Path.Combine(SettingsDirectory, settingsToSave.Name + ".mailSettings"),
						ValidLoading = true
					});
				}

				LoadedSettings = settingsToSave;
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public bool LoadSettings(SettingsMetaEntry settings)
		{
			try
			{
				LoadedSettings = JsonSerializer.Deserialize<SettingsEntry>(File.ReadAllText(settings.Path));
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		public void Init()
		{
			if (!Directory.Exists(SettingsDirectory))
			{
				return;
			}
			
			foreach (var enumerateFile in Directory.EnumerateFiles(SettingsDirectory, "*.mailSettings"))
			{
				try
				{
					Settings.Add(new SettingsMetaEntry()
					{
						Name = Path.GetFileNameWithoutExtension(enumerateFile),
						Path = enumerateFile
					});
				}
				catch (Exception e)
				{
				}	
			}
		}
	}

	public static class DictionaryExtensions
	{
		public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}

			return default;
		}
	}

	public class SettingsMetaEntry
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public bool ValidLoading { get; set; }
	}

	public class SettingsEntry
	{
		public SettingsEntry()
		{
			Values = new Dictionary<string, string>();
		}

		public string Name { get; set; }
		public IDictionary<string, string> Values { get; set; }
	}

	public class SettingsEntryError : SettingsEntry
	{
		public string Error { get; set; }
	}
}
