using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator.MissionSource
{
    public interface IMissionsSource
    {
        Task<IEnumerable<Mission>> GetMissionsFromDirectoryAsync(string directory);
        Task<IEnumerable<string>> GetFaultyFilesAsync(string directory);
        Task<bool> MoveMissionToFolderAsync(Mission mission, string source, string destination);
    }
}