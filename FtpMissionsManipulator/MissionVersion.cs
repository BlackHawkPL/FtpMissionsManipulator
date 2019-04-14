using System;

namespace FtpMissionsManipulator
{
    public class MissionVersion : IComparable<MissionVersion>, IEquatable<MissionVersion>
    {
        private readonly IVersionComparer _versionComparer;
        public string TextRepresentation { get; }

        public MissionVersion(string textRepresentation, IVersionComparer versionComparer)
        {
            TextRepresentation = textRepresentation;
            _versionComparer = versionComparer;
        }

        public int CompareTo(MissionVersion other)
        {
            return _versionComparer.Compare(this, other);
        }

        public bool Equals(MissionVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CompareTo(other) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((MissionVersion) obj);
        }

        public override int GetHashCode()
        {
            return TextRepresentation != null ? TextRepresentation.GetHashCode() : 0;
        }
    }
}