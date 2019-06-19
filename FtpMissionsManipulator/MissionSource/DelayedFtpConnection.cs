using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class DelayedFtpConnection : IFtpConnection
    {
        private readonly IFtpConnection _inner;
        private readonly int _delay;
        public DelayedFtpConnection(IFtpConnection inner)
        {
            _inner = inner;
            _delay = 2000;
        }

        public async Task<IEnumerable<string>> GetDirectoryListingAsync(string directory)
        {
            await Task.Delay(_delay);
            return await _inner.GetDirectoryListingAsync(directory);
        }

        public async Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            await Task.Delay(_delay);
            return await _inner.MoveFileAsync(fileName, sourceDir, targetDir);
        }

        public async Task DeleteFileAsync(string fileName, string directory)
        {
            await Task.Delay(_delay);
            await _inner.DeleteFileAsync(fileName, directory);
        }

        public async Task<bool> TryConnectAsync(string host, int port, string user, string pass)
        {
            await Task.Delay(_delay);
            return await _inner.TryConnectAsync(host, port, user, pass);
        }
    }
}