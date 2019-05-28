using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionManipulator
    {
        private readonly IMissionsSource _missionsSource;
        public const string FinalDirectory = "_FINAL";
        public const string LiveDirectory = "SRV1";

        private IEnumerable<Mission> _liveMissions;
        private IEnumerable<Mission> _pendingMissions;

        public MissionManipulator(IMissionsSource missionsSource)
        {
            _missionsSource = missionsSource;
        }

        public IEnumerable<Mission> PendingMissions =>
            _pendingMissions ?? (_pendingMissions = _missionsSource.GetMissionsFromDirectory(FinalDirectory));

        public IEnumerable<Mission> LiveMissions =>
            _liveMissions ?? (_liveMissions = _missionsSource.GetMissionsFromDirectory(LiveDirectory));

        public IEnumerable<MissionUpdate> GetUpdatedMissions() =>
            LiveMissions.Join(PendingMissions,
                m => m.Name,
                m => m.Name,
                (m1, m2) => new MissionUpdate(m1, m2));
    }
}