﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class ConcurrentFtpConnection : IFtpConnection
    {
        private readonly IFtpConnection _inner;
        private readonly Dictionary<string, Task<string>> _pending;

        public ConcurrentFtpConnection(IFtpConnection inner)
        {
            _pending = new Dictionary<string, Task<string>>();
            _inner = inner;
        }

        public Task<string> GetDirectoryListingAsync(string directory)
        {
            if (!_pending.ContainsKey(directory))
                _pending.Add(directory, _inner.GetDirectoryListingAsync(directory));

            return _pending[directory];
        }

        public Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            return _inner.MoveFileAsync(fileName, sourceDir, targetDir);
        }
    }
}