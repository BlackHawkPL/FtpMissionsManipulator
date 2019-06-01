using System;

namespace FtpMissionsManipulator
{
    public class Mission : IEquatable<Mission>
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

        public bool Equals(Mission other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(other.FullName))
                return false;
            return string.Equals(FullName, other.FullName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Mission) obj);
        }

        public override int GetHashCode()
        {
            return FullName != null ? FullName.GetHashCode() : 0;
        }
    }
}