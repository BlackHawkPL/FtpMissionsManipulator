using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Threading.Tasks;
using FluentFTP;

namespace FtpMissionsManipulator.MissionSource
{
    public class FtpConnection : IFtpConnection
    {
        public Action<string> LoggingAction { get; set; }
        private FtpClient _client;

        public FtpConnection(Action<string> loggingAction)
        {
            LoggingAction = loggingAction;
        }

        public async Task<IEnumerable<string>> GetDirectoryListingAsync(string directory)
        {
            try
            {
                var files = await _client.GetListingAsync(directory);

                return files.Select(f => f.Name);
            }
            catch (Exception e)
            {
                throw new FtpException(e.Message);
            }
        }

        public async Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            try
            {
                return await _client.MoveFileAsync(sourceDir + '/' + fileName, targetDir + '/' + fileName);
            }
            catch (Exception e)
            {
                throw new FtpException(e.Message);
            }
        }

        public async Task DeleteFileAsync(string fileName, string directory)
        {
            try
            {
                await _client.DeleteFileAsync(directory + '/' + fileName);
            }
            catch (Exception e)
            {
                throw new FtpException(e.Message);
            }
        }

        public async Task<bool> TryConnectAsync(string host, int port, string user, string pass)
        {
            try
            {
                await ConnectAsync(host, port, user, pass);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task ConnectAsync(string host, int port, string user, string pass)
        {
            var client = new FtpClient(host, port, user, pass)
            {
                ConnectTimeout = 5000,
                EncryptionMode = FtpEncryptionMode.Explicit,
                
            };
            client.ValidateCertificate += (control, args) =>
            {
                if ((args.PolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
                    args.Accept = true;
            };

            try
            {
                await client.ConnectAsync();
                _client = client;
                _client.OnLogEvent = (traceLevel, message) =>
                {
                    if (traceLevel == FtpTraceLevel.Warn)
                        LoggingAction?.Invoke(message);
                };
            }
            catch (Exception e)
            {
                throw new FtpException(e.Message);
            }
        }
    }
}