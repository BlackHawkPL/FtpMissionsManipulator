namespace FtpMissionsManipulator
{
    public class MissionVersionFactory : IMissionVersionFactory
    {
        public MissionVersion GetMissionVersion(string textRepresentation,
            IMissionVersionComparer missionVersionComparer)
        {
            return new MissionVersion(textRepresentation, missionVersionComparer);
        }
    }
}