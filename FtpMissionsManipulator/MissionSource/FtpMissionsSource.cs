using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class FtpMissionsSource : IMissionsSource
    {
        private readonly IFtpConnection _ftpConnection;
        private readonly IMissionFactory _missionFactory;

        public FtpMissionsSource(IMissionFactory missionFactory, IFtpConnection ftpConnection)
        {
            _ftpConnection = ftpConnection;
            _missionFactory = missionFactory;
        }

        public async Task<IEnumerable<Mission>> GetMissionsFromDirectoryAsync(string directory)
        {
            var response = await _ftpConnection
                .GetDirectoryListingAsync(directory)
                .ConfigureAwait(false);

            var missions = CreateMissions(response);

            return missions
                .Where(m => m != null)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetFaultyFilesAsync(string directory)
        {
            var response = await _ftpConnection
                .GetDirectoryListingAsync(directory)
                .ConfigureAwait(false);

            var result = new List<string>();
            foreach (var missionName in response)
            {
                try
                {
                    _missionFactory.GetMission(missionName);
                }
                catch (ArgumentException)
                {
                    result.Add(missionName);
                }
            }

            return result;
        }

        public Task<bool> MoveMissionToFolderAsync(Mission mission, string source, string destination)
        {
            return _ftpConnection.MoveFileAsync(mission.FullName, source, destination);
        }

        public Task DeleteMissionAsync(Mission mission, string directory)
        {
            return _ftpConnection.DeleteFileAsync(mission.FullName, directory);
        }

        public Task DeleteFileAsync(string fileName, string directory)
        {
            return _ftpConnection.DeleteFileAsync(fileName, directory);
        }

        private IEnumerable<Mission> CreateMissions(IEnumerable<string> missionNames)
        {
            foreach (var missionName in missionNames)
            {
                Mission mission = null;
                try
                {
                    mission = _missionFactory.GetMission(missionName);
                }
                catch (ArgumentException)
                {
                }

                yield return mission;
            }
        }
    }
}