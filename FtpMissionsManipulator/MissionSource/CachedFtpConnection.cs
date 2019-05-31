using System;
using System.Collections.Generic;

namespace FtpMissionsManipulator.MissionSource
{
    public class CachedFtpConnection : IFtpConnection
    {
        private readonly ITimeProvider _timeProvider;
        private readonly int _invalidateAfter;
        private readonly IFtpConnection _connection;
        private readonly Dictionary<string, string> _cache;
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
            if (!_cache.ContainsKey(directory))
                _cache.Add(directory, _connection.GetStringResponse(directory));

            if (_lastAccessTime.AddSeconds(_invalidateAfter) >= _timeProvider.GetCurrentTime())
                return _cache[directory];

            _cache[directory] = _connection.GetStringResponse(directory);
            _lastAccessTime = _timeProvider.GetCurrentTime();

            return _cache[directory];
        }
    }
}