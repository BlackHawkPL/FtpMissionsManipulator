using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class ConcurrentFtpConnection : IFtpConnection
    {
        private readonly IFtpConnection _inner;
        private readonly Dictionary<string, Task<IEnumerable<string>>> _pending;

        public ConcurrentFtpConnection(IFtpConnection inner)
        {
            _pending = new Dictionary<string, Task<IEnumerable<string>>>();
            _inner = inner;
        }

        public Task<IEnumerable<string>> GetDirectoryListingAsync(string directory)
        {
            if (!_pending.ContainsKey(directory))
            {
                var task = _inner.GetDirectoryListingAsync(directory);
                task.ContinueWith(_ => _pending.Remove(directory));
                _pending.Add(directory, task);
            }

            return _pending[directory];
        }

        public Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            return _inner.MoveFileAsync(fileName, sourceDir, targetDir);
        }

        public Task DeleteFileAsync(string fileName, string directory)
        {
            return _inner.DeleteFileAsync(fileName, directory);
        }

        public Task<bool> TryConnectAsync(string host, int port, string user, string pass)
        {
            return _inner.TryConnectAsync(host, port, user, pass);
        }

        public Task ConnectAsync(string host, int port, string user, string pass)
        {
            return _inner.ConnectAsync(host, port, user, pass);
        }
    }
}