using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IFtpConnection
    {
        Task<string> GetDirectoryListingAsync(string directory);
        Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir);
    }
}