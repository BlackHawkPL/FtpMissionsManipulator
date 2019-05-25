using System.Collections.Generic;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionManipulator
    {
        private const string FinalDirectory = "_FINAL";
        private readonly IMissionsSource _missionsSource;
        private const string LiveDirectory = "SRV1";

        public MissionManipulator(IMissionsSource missionsSource)
        {
            _missionsSource = missionsSource;
        }

        public IEnumerable<Mission> GetPendingMissions()
        {
            return _missionsSource.GetMissionsFromDirectory(FinalDirectory);
        }

        public IEnumerable<Mission> GetLiveMissions()
        {
            return _missionsSource.GetMissionsFromDirectory(LiveDirectory);
        }
    }
}