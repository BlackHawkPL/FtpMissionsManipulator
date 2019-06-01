using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class CachedFtpConnection : IFtpConnection
    {
        private readonly Dictionary<string, string> _cache;
        private readonly IFtpConnection _connection;
        private readonly int _invalidateAfter;
        private readonly ITimeProvider _timeProvider;
        private DateTime _lastAccessTime;

        public CachedFtpConnection(IFtpConnection inner, ITimeProvider timeProvider, int invalidateAfter = 5)
        {
            _timeProvider = timeProvider;
            _invalidateAfter = invalidateAfter;
            _connection = inner;
            _cache = new Dictionary<string, string>();
            _lastAccessTime = _timeProvider.GetCurrentTime();
        }

        public string GetStringResponse(string directory)
        {
            return GetStringResponseAsync(directory).Result;
        }

        public async Task<string> GetStringResponseAsync(string directory)
        {
            if (!_cache.ContainsKey(directory))
            {
                var response = await _connection.GetStringResponseAsync(directory).ConfigureAwait(false);
                if (!_cache.ContainsKey(directory))
                    _cache.Add(directory, response);
            }

            if (_lastAccessTime.AddSeconds(_invalidateAfter) >= _timeProvider.GetCurrentTime())
                return _cache[directory];

            _cache[directory] = await _connection.GetStringResponseAsync(directory).ConfigureAwait(false);
            _lastAccessTime = _timeProvider.GetCurrentTime();

            return _cache[directory];
        }
    }
}