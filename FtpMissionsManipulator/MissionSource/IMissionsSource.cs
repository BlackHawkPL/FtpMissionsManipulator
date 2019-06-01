using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IMissionsSource
    {
        IEnumerable<Mission> GetMissionsFromDirectory(string directory);
        IEnumerable<string> GetFaultyFiles(string directory);
        Task<IEnumerable<Mission>> GetMissionsFromDirectoryAsync(string directory);
        Task<IEnumerable<string>> GetFaultyFilesAsync(string directory);
    }
}