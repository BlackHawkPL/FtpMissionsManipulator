namespace FtpMissionsManipulator
{
    public interface IMissionFactory
    {
        Mission GetMission(string missionName);
    }
}