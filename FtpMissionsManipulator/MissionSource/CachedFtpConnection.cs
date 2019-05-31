using System;
using System.Collections.Generic;

namespace FtpMissionsManipulator.MissionSource
{
    class CachedFtpConnection : IFtpConnection
    {
        private readonly ITimeProvider _timeProvider;
        private readonly int _invalidateAfter;
        private readonly FtpConnection _connection;
        private readonly Dictionary<string, string> _cache;
        private DateTime _lastAccessTime;

        public CachedFtpConnection(string address, string userName, string password, ITimeProvider timeProvider, int invalidateAfter = 5)
        {
            _timeProvider = timeProvider;
            _invalidateAfter = invalidateAfter;
            _connection = new FtpConnection(address, userName, password);
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