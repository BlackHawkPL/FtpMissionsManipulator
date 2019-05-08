namespace FtpMissionsManipulator
{
    public interface IMissionVersionFactory
    {
        MissionVersion GetMissionVersion(string textRepresentation, IMissionVersionComparer missionVersionComparer);
    }
}