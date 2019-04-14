namespace FtpMissionsManipulator
{
    public class Mission
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public MissionType Type { get; set; }
        public MissionVersion Version { get; set; }
    }
}