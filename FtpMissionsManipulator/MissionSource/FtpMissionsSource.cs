using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public class FtpMissionsSource : IMissionsSource
    {
        private readonly IMissionFactory _missionFactory;
        private readonly IFtpConnection _ftpConnection;

        public FtpMissionsSource(IMissionFactory missionFactory, IFtpConnection ftpConnection)
        {
            _ftpConnection = ftpConnection;
            _missionFactory = missionFactory;
        }

        private IEnumerable<Mission> CreateMissions(IEnumerable<string> missionNames)
        {
            foreach (var missionName in missionNames)
            {
                var name = missionName.Trim()
                    .Split('/')
                    .Last();

                Mission mission = null;
                try
                {
                    mission = _missionFactory.GetMission(name);
                }
                catch (ArgumentException)
                {
                }

                yield return mission;
            }
        }

        public async Task<IEnumerable<Mission>> GetMissionsFromDirectoryAsync(string directory)
        {
            var response = await _ftpConnection
                .GetDirectoryListingAsync(directory)
                .ConfigureAwait(false);

            var missionStrings = response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var missions = CreateMissions(missionStrings);

            return missions
                .Where(m => m != null)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetFaultyFilesAsync(string directory)
        {
            var response = await _ftpConnection
                .GetDirectoryListingAsync(directory)
                .ConfigureAwait(false);

            var missionStrings = response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();
            foreach (var missionName in missionStrings)
            {
                var name = missionName.Trim()
                    .Split('/')
                    .Last();

                try
                {
                    _missionFactory.GetMission(name);
                }
                catch (ArgumentException)
                {
                    result.Add(name);
                }
            }

            return result;
        }

        public Task<bool> MoveMissionToFolderAsync(Mission mission, string source, string destination)
        {
            return _ftpConnection.MoveFileAsync(mission.FullName, source, destination);
        }
    }
}