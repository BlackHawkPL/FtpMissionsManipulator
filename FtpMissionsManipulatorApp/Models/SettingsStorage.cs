using System.Configuration;
using System.Linq;

namespace FtpMissionsManipulatorApp
{
    class SettingsStorage : ISettingsStorage
    {
        private readonly KeyValueConfigurationCollection _settings;
        private readonly Configuration _config;

        public SettingsStorage()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _settings = _config.AppSettings.Settings;
        }

        private void Save()
        {
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(_config.AppSettings.SectionInformation.Name);
        }

        public void SetSetting(string key, string value)
        {
            _settings.Remove(key);
            _settings.Add(key, value);
            Save();
        }

        public string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }

        public bool HasSetting(string name)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(name);
        }
    }
}
