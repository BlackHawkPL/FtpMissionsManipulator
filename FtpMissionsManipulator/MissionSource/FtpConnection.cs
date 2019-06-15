using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;

namespace FtpMissionsManipulator.MissionSource
{
    public class FtpConnection : IFtpConnection
    {
        private FtpClient _Client;

        public FtpConnection()
        {
        }

        public async Task<IEnumerable<string>> GetDirectoryListingAsync(string directory)
        {
            var files = await _Client.GetListingAsync(directory);

            return files.Select(f => f.Name);
        }

        public Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFileAsync(string fileName, string directory)
        {
            throw new NotImplementedException();
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

            _Client = client;
            return true;
        }
    }
}