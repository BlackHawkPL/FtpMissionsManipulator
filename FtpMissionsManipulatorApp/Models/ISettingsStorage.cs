namespace FtpMissionsManipulatorApp
{
    public interface ISettingsStorage
    {
        void SetSetting(string key, string value);
        string GetSetting(string name);
        bool HasSetting(string name);
    }
}