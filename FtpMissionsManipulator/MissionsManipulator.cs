using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FtpMissionsManipulator.MissionSource;

namespace FtpMissionsManipulator
{
    public class MissionsManipulator : IMissionsManipulator
    {
        public const string FinalDirectory = "_FINAL";
        public const string LiveDirectory = "SRV1";
        public const string BrokenDirectory = "_BROKEN";
        private readonly IMissionsSource _missionsSource;

        public MissionsManipulator(IMissionsSource missionsSource)
        {
            _missionsSource = missionsSource;
        }

        public Task<IEnumerable<Mission>> GetPendingMissionsAsync()
        {
            return _missionsSource.GetMissionsFromDirectoryAsync(FinalDirectory);
        }

        public Task<IEnumerable<Mission>> GetLiveMissionsAsync()
        {
            return _missionsSource.GetMissionsFromDirectoryAsync(LiveDirectory);
        }

        public Task<IEnumerable<Mission>> GetBrokenMissionsAsync()
        {
            return _missionsSource.GetMissionsFromDirectoryAsync(BrokenDirectory);
        }

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

        public Task<IEnumerable<string>> GetMissionsWithIncorrectNamesInLiveAsync()
        {
            return _missionsSource.GetFaultyFilesAsync(LiveDirectory);
        }

        public async Task<IEnumerable<Mission>> GetDuplicateMissionsFromLiveAsync()
        {
            return (await GetLiveMissionsAsync().ConfigureAwait(false))
                .GroupBy(m => (m.Name, m.Type))
                .Where(g => g.Count() > 1)
                .SelectMany(g => g);
        }

        public async Task MovePendingToLiveAsync()
        {
            //todo mission can be moved in the process: first check if it's still there
            //todo tests for operations with faults
            var updates = await GetUpdatedMissionsAsync().ConfigureAwait(false);
            foreach (var update in updates)
            {
                //await _missionsSource.DeleteMissionAsync(update.OldMission, LiveDirectory).ConfigureAwait(false); //todo test manipulating on non existent files
                await _missionsSource.MoveMissionToFolderAsync(update.NewMission, FinalDirectory,
                    LiveDirectory).ConfigureAwait(false);
            }
        }
    }
}