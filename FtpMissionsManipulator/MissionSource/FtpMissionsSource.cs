using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IFtpConnection
    {
        string GetStringResponse(string directory);
    }

    public class FtpConnection : IFtpConnection
    {
        private readonly string _address;
        private readonly string _password;
        private readonly string _userName;

        public FtpConnection(string address, string userName, string password)
        {
            _address = address;
            _userName = userName;
            _password = password;
        }

        private string GetDirectoryAtAddress(string directory)
        {
            return _address + (!_address.EndsWith('/') ? "/" : "") + directory;
        }

        public string GetStringResponse(string directory)
        {
            var request = WebRequest.Create(GetDirectoryAtAddress(directory)) as FtpWebRequest;
            if (request == null)
                throw new Exception("webrequest was null");
        
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(_userName, _password);

            var response = request.GetResponse() as FtpWebResponse;

            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream);
            var result = reader.ReadToEnd();

            response.Close();
            reader.Close();
            return result;
        
        }
    }

    public class FtpMissionsSource : IMissionsSource
    {
        private readonly IMissionFactory _missionFactory;
        private readonly IFtpConnection _ftpConnection;

        public FtpMissionsSource(IMissionFactory missionFactory, IFtpConnection ftpConnection)
        {
            _ftpConnection = ftpConnection;
            _missionFactory = missionFactory;
        }

        public IEnumerable<Mission> GetMissionsFromDirectory(string directory)
        {
            var result = _ftpConnection.GetStringResponse(directory);

            return result
                .Split('\n')
                .Select(missionName => _missionFactory.GetMission(missionName))
                .ToList();
        }
    }
}