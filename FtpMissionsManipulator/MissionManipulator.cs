using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionManipulator
    {
        private readonly IMissionsSource _missionsSource;

        public MissionManipulator(IMissionsSource missionsSource)
        {
            _missionsSource = missionsSource;
        }

        private void LoadPendingMissions()
        {
            _missionsSource.GetMissionsFromDirectory("FINAL");
        }
    }
}