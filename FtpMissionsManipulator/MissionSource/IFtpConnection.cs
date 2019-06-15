using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IFtpConnection
    {
        Task<IEnumerable<string>> GetDirectoryListingAsync(string directory);
        Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir);
        Task DeleteFileAsync(string fileName, string directory);
        Task<bool> TryConnectAsync(string host, int port, string user, string pass);

    }
}