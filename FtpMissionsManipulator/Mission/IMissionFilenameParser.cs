namespace FtpMissionsManipulator
{
    public interface IMissionFilenameParser
    {
        bool IsMissionNameFormatValid(string mission);
        MissionVersion GetVersion(string mission);
        string GetName(string mission);
        MissionType GetMissionType(string mission);
        int GetSize(string mission);
        string GetTerrain(string mission);
    }
}