namespace FtpMissionsManipulator.MissionSource
{
    public interface IFtpConnection
    {
        string GetStringResponse(string directory);
    }
}