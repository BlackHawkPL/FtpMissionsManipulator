using System;
using System.IO;
using System.Net;
using System.Threading;
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

        public string GetStringResponse(string directory)
        {
            return GetStringResponseAsync(directory).Result;
        }

        public async Task<string> GetStringResponseAsync(string directory)
        {
            if (!(WebRequest.Create(GetDirectoryAtAddress(directory)) is FtpWebRequest request)) //todo handle errors
                throw new Exception("webrequest was null");

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(_userName, _password);

            if (!(request.GetResponse() is FtpWebResponse response))
                throw new Exception("response was null");

            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream ?? throw new InvalidOperationException());
            var result = await reader.ReadToEndAsync().ConfigureAwait(false);

            response.Close();
            reader.Close();
            return result;

        }
    }
}