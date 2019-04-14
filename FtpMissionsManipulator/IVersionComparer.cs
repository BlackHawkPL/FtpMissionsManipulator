using System.Collections.Generic;

namespace FtpMissionsManipulator
{
    public interface IVersionComparer : IComparer<MissionVersion>
    {
        bool IsFormatCorrect(MissionVersion version);
    }
}