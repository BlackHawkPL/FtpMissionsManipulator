using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class CachedFtpConnection : IFtpConnection
    {
        private readonly ITimeProvider _timeProvider;
        private readonly int _invalidateAfter;
        private readonly IFtpConnection _connection;
        private readonly Dictionary<string, string> _cache;
        private DateTime _lastAccessTime;
        private readonly object _cacheUpdateLock;

        public CachedFtpConnection(IFtpConnection inner, ITimeProvider timeProvider, int invalidateAfter = 5)
        {
            _timeProvider = timeProvider;
            _cacheUpdateLock = new object();
            _invalidateAfter = invalidateAfter;
            _connection = inner;
            _cache = new Dictionary<string, string>();
            _lastAccessTime = _timeProvider.GetCurrentTime();
        }

        public string GetStringResponse(string directory)
        {
            if (!_cache.ContainsKey(directory))
                _cache.Add(directory, _connection.GetStringResponse(directory));

            if (_lastAccessTime.AddSeconds(_invalidateAfter) >= _timeProvider.GetCurrentTime())
                return _cache[directory];

            _cache[directory] = _connection.GetStringResponse(directory);
            _lastAccessTime = _timeProvider.GetCurrentTime();

            return _cache[directory];
        }

        public async Task<string> GetStringResponseAsync(string directory)
        {
            var responseTask = _connection.GetStringResponseAsync(directory).ConfigureAwait(false);
            if (!_cache.ContainsKey(directory))
            {
                var response = await responseTask; //todo handle multiple tasks requesting one resource (request it once and serve multiple)
                lock (_cacheUpdateLock)
                {
                    if (!_cache.ContainsKey(directory))
                        _cache.Add(directory, response);
                }
            }

            if (_lastAccessTime.AddSeconds(_invalidateAfter) >= _timeProvider.GetCurrentTime())
                return _cache[directory];

            _cache[directory] = await responseTask;
            _lastAccessTime = _timeProvider.GetCurrentTime();

            return _cache[directory];
        }
    }
}