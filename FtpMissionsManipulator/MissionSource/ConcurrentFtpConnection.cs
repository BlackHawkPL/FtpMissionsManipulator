using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class ConcurrentFtpConnection : IFtpConnection
    {
        private readonly Dictionary<string, Task<string>> _pending;
        private readonly IFtpConnection _inner;

        public ConcurrentFtpConnection(IFtpConnection inner)
        {
            _pending = new Dictionary<string, Task<string>>();
            _inner = inner;
        }

        public string GetStringResponse(string directory)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetStringResponseAsync(string directory)
        {
            if (!_pending.ContainsKey(directory))
                _pending.Add(directory, _inner.GetStringResponseAsync(directory));

            return _pending[directory];
        }
    }
}