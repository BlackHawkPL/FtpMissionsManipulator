using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class FtpConnection : IFtpConnection
    {
        private readonly string _address;
        private readonly string _password;
        private readonly string _userName;

        public FtpConnection(string address, string userName, string password)
        {
            _address = address; //todo handle incorrect credentials
            _userName = userName;
            _password = password;
        }

        private string GetDirectoryAtAddress(string directory)
        {
            return _address + (!_address.EndsWith('/') ? "/" : "") + directory;
        }

        public Task<string> GetDirectoryListingAsync(string directory)
        {
            return GetResponseAsync(directory, WebRequestMethods.Ftp.ListDirectory);
        }

        private async Task<string> GetResponseAsync(string directory, string requestMethod, string renameTo = null)
        {
            if (!(WebRequest.Create(GetDirectoryAtAddress(directory)) is FtpWebRequest request)) //todo handle errors
                throw new Exception("webrequest was null");

            request.Method = requestMethod;
            request.Credentials = new NetworkCredential(_userName, _password);
            if (renameTo != null)
                request.RenameTo = renameTo;

            if (!(request.GetResponse() is FtpWebResponse response))
                throw new Exception("response was null"); //todo handle 550 (denied)

            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream ?? throw new InvalidOperationException());
            var result = await reader.ReadToEndAsync().ConfigureAwait(false);

            response.Close();
            reader.Close();
            return result;
        }

        public Task<bool> MoveFileAsync(string fileName, string sourceDir, string targetDir)
        {
            var result = GetResponseAsync($"{sourceDir}/{fileName}", WebRequestMethods.Ftp.Rename,
                $"/{targetDir}/{fileName}");
            Console.WriteLine("M: " + fileName);
            return Task.FromResult(true);
        }

        public Task DeleteFileAsync(string fileName, string directory)
        {
            var result = GetResponseAsync($"{directory}/{fileName}", WebRequestMethods.Ftp.DeleteFile);
            Console.WriteLine("D: " + fileName);
            return result;
        }
    }
}