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
        public const string BrokenDirectory = "BROKEN";

        private IEnumerable<Mission> _liveMissions;
        private IEnumerable<Mission> _pendingMissions;
        private IEnumerable<Mission> _brokenMissions;

        public MissionManipulator(IMissionsSource missionsSource)
        {
            _missionsSource = missionsSource;
        }

        public IEnumerable<Mission> PendingMissions =>
            _pendingMissions = _missionsSource.GetMissionsFromDirectory(FinalDirectory);

        public IEnumerable<Mission> LiveMissions =>
            _liveMissions = _missionsSource.GetMissionsFromDirectory(LiveDirectory);

        public IEnumerable<Mission> BrokenMissions =>
            _brokenMissions = _missionsSource.GetMissionsFromDirectory(BrokenDirectory);

        public IEnumerable<MissionUpdate> GetUpdatedMissions() =>
            LiveMissions.Join(PendingMissions,
                m => m.Name,
                m => m.Name,
                (m1, m2) => new MissionUpdate(m1, m2));

        public IEnumerable<string> GetMissionsWithIncorrectNamesInLive() =>
            _missionsSource.GetFaultyFiles(LiveDirectory);

        public IEnumerable<Mission> GetDuplicateMissionsFromLive() =>
            LiveMissions
                .GroupBy(m => (m.Name, m.Type))
                .Where(g => g.Count() > 1)
                .SelectMany(g => g);
    }
}