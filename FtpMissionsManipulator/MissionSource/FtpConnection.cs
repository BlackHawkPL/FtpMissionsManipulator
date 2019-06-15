﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentFTP;

namespace FtpMissionsManipulator.MissionSource
{
    public class FtpConnection : IFtpConnection
    {
        private FtpClient _client;

        public async Task<IEnumerable<string>> GetDirectoryListingAsync(string directory)
        {
            var files = await _client.GetListingAsync(directory);

            return files.Select(f => f.Name);
        }

        public async Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            return await _client.MoveFileAsync(sourceDir + '/' + fileName, targetDir + '/' + fileName);
        }

        public async Task DeleteFileAsync(string fileName, string directory)
        {
            await _client.DeleteFileAsync(directory + '/' + fileName);
        }

        public async Task<bool> TryConnectAsync(string host, int port, string user, string pass)
        {
            var client = new FtpClient(host, port, user, pass)
            {
                ConnectTimeout = 5000
            };

            try
            {
                await client.ConnectAsync().ConfigureAwait(false);
            }
            catch (FtpAuthenticationException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

            _client = client;
            return true;
        }
    }
}