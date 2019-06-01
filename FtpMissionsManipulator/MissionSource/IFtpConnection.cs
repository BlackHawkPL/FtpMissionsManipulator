using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IFtpConnection
    {
        string GetStringResponse(string directory);
        Task<string> GetStringResponseAsync(string directory);
    }
}