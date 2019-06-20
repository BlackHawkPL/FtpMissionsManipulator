using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FtpMissionsManipulator
{
    public class MissionUpdate : IEquatable<MissionUpdate>
    {
        public MissionUpdate(IEnumerable<Mission> oldMissions, IEnumerable<Mission> newMissions)
        {
            OldMissions = oldMissions;
            NewMissions = newMissions;
        }

        public MissionUpdate(Mission oldMission, Mission newMission)
        {
            OldMissions = Enumerable.Empty<Mission>().Append(oldMission);
            NewMissions = Enumerable.Empty<Mission>().Append(newMission);
        }

        public IEnumerable<Mission> OldMissions { get; }
        public IEnumerable<Mission> NewMissions { get; }

        public bool Equals(MissionUpdate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return OldMissions.SequenceEqual(other.OldMissions) && NewMissions.SequenceEqual(other.NewMissions);
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
                return ((OldMissions != null ? OldMissions.GetHashCode() : 0) * 397) ^
                       (NewMissions != null ? NewMissions.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append(NewMissions.First().Name + " updated from ");

            if (!OldMissions.Any())
            {
                result.Append("NONE");
            }
            var isFirst = true;
            foreach (var version in OldMissions.Select(m => m.Version))
            {
                result.Append((isFirst ? "" : ", ") + version);
                isFirst = false;
            }

            result.Append(" to ");

            isFirst = true;
            foreach (var version in NewMissions.Select(m => m.Version))
            {
                result.Append((isFirst ? "" : ", ") + version);
                isFirst = false;
            }

            return result.ToString();
        }
    }
}