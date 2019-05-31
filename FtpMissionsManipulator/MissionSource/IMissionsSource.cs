using System.Collections.Generic;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IMissionsSource
    {
        IEnumerable<Mission> GetMissionsFromDirectory(string directory);
        IEnumerable<string> GetFaultyFiles(string directory);
    }
}