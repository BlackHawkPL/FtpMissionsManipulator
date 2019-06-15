using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator
{
    public interface IMissionsSource
    {
        Task<IEnumerable<Mission>> GetMissionsFromDirectoryAsync(string directory);
        Task<IEnumerable<string>> GetFaultyFilesAsync(string directory);
        Task<bool> MoveMissionToFolderAsync(Mission mission, string source, string destination);
        Task DeleteMissionAsync(Mission mission, string directory);
        Task DeleteFileAsync(string fileName, string directory);
    }
}