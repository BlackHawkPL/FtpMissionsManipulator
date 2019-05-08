using System.Collections.Generic;

namespace FtpMissionsManipulator
{
    public interface IMissionVersionComparer : IComparer<MissionVersion>
    {
        bool IsFormatCorrect(MissionVersion version);
    }
}