namespace FtpMissionsManipulator
{
    public class Mission
    {
        public Mission()
        {

        }

        public Mission(string fullName, MissionType type, int size, string name, MissionVersion version, string terrain)
        {
            FullName = fullName;
            Type = type;
            Size = size;
            Name = name;
            Terrain = terrain;
            Version = version;
        }

        public string Name { get; }
        public string FullName { get; }
        public MissionType Type { get; }
        public int Size { get; }
        public MissionVersion Version { get; }
        public string Terrain { get; }
    }
}