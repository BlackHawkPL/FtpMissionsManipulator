using System;

namespace FtpMissionsManipulator
{
    public class MissionUpdate : IEquatable<MissionUpdate>
    {
        public MissionUpdate(Mission oldMission, Mission newMission)
        {
            OldMission = oldMission;
            NewMission = newMission;
        }

        public Mission OldMission { get; }
        public Mission NewMission { get; }

        public bool Equals(MissionUpdate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(OldMission, other.OldMission) && Equals(NewMission, other.NewMission);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MissionUpdate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((OldMission != null ? OldMission.GetHashCode() : 0) * 397) ^
                       (NewMission != null ? NewMission.GetHashCode() : 0);
            }
        }
    }
}