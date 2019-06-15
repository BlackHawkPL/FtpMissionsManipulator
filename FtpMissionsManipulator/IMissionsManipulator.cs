using System.Collections.Generic;
using System.Threading.Tasks;

namespace FtpMissionsManipulator
{
    public interface IMissionsManipulator
    {
        Task<IEnumerable<Mission>> GetPendingMissionsAsync();
        Task<IEnumerable<Mission>> GetLiveMissionsAsync();
        Task<IEnumerable<Mission>> GetBrokenMissionsAsync();
        Task<IEnumerable<MissionUpdate>> GetUpdatedMissionsAsync();
        Task<IEnumerable<string>> GetMissionsWithIncorrectNamesInLiveAsync();
        Task<IEnumerable<Mission>> GetDuplicateMissionsFromLiveAsync();
        Task MovePendingToLiveAsync();
    }
}