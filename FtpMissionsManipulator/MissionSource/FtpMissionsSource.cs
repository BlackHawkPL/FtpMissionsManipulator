using System;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<Mission> GetMissionsFromDirectory(string directory)
        {
            var result = _ftpConnection.GetStringResponse(directory);

            return result
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(missionName =>
                {
                    missionName = missionName
                        .Trim()
                        .Split('/')
                        .Last();
                    try
                    {
                        var mission = _missionFactory.GetMission(missionName);
                        return mission;
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine($"{missionName}: {e.Message}");
                        return null;
                    }
                })
                .Where(m => m != null)
                .ToList();
        }
    }
}