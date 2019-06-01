using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionManipulator
    {
        private readonly IMissionsSource _missionsSource;
        public const string FinalDirectory = "_FINAL";
        public const string LiveDirectory = "SRV1";
        public const string BrokenDirectory = "BROKEN";

        public MissionManipulator(IMissionsSource missionsSource)
        {
            _missionsSource = missionsSource;
        }

        public Task<IEnumerable<Mission>> GetPendingMissionsAsync() => _missionsSource.GetMissionsFromDirectoryAsync(FinalDirectory);

        public Task<IEnumerable<Mission>> GetLiveMissionsAsync() => _missionsSource.GetMissionsFromDirectoryAsync(LiveDirectory);

        public Task<IEnumerable<Mission>> GetBrokenMissionsAsync() => _missionsSource.GetMissionsFromDirectoryAsync(BrokenDirectory);

        public async Task<IEnumerable<MissionUpdate>> GetUpdatedMissionsAsync()
        {
            var missions = await GetLiveMissionsAsync().ConfigureAwait(false);
            var pending = await GetPendingMissionsAsync().ConfigureAwait(false);
            return missions.Join(
                pending,
                m => m.Name,
                m => m.Name,
                (m1, m2) => new MissionUpdate(m1, m2));
        }

        public Task<IEnumerable<string>> GetMissionsWithIncorrectNamesInLiveAsync() =>
            _missionsSource.GetFaultyFilesAsync(LiveDirectory);

        public async Task<IEnumerable<Mission>> GetDuplicateMissionsFromLiveAsync() =>
            (await GetLiveMissionsAsync().ConfigureAwait(false))
                .GroupBy(m => (m.Name, m.Type))
                .Where(g => g.Count() > 1)
                .SelectMany(g => g);
    }
}