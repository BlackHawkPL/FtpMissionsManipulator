using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using FtpMissionsManipulator;
using FtpMissionsManipulatorApp.Annotations;
using Prism.Commands;
using MessageBox = System.Windows.Forms.MessageBox;

namespace FtpMissionsManipulatorApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string LiveDirectory = "SRV1";
        private const string PendingDirectory = "_FINAL";
        private readonly IMissionSourceFactory _missionSourceFactory;
        private readonly ISettingsStorage _settingsStorage;
        private IMissionsSource _source;
        private string _username;
        private string _address;
        private int _port = 21;
        private bool _isLoading;
        private bool _isPendingLoading;
        private bool _isLiveLoading;
        private IList<Mission> _selectedDuplicates = new List<Mission>();
        private IList<string> _selectedInvalid = new List<string>();
        private IList<MissionUpdate> _selectedUpdates = new List<MissionUpdate>();
        private IList<Mission> _selectedPending = new List<Mission>();

        public MainViewModel(IMissionSourceFactory factory, ISettingsStorage settingsStorage)
        {
            _missionSourceFactory = factory;
            _settingsStorage = settingsStorage;

            SetupCommands();

            RetrieveSavedSettings();
        }

        private void SetupCommands()
        {
            ConnectCommand = SetupCommand(ConnectAsync, () => CanConnect, v => IsLoading = v);
            RetrieveMissionsCommand = SetupCommand(GetMissionsAsync, () => IsConnected, v => IsLoading = v);
            MovePendingToLiveCommand =
                SetupCommand(MovePendingToLiveAsync, () => PendingMissionsLoaded, v => IsLoading = v);
            RemoveDuplicatesCommand = SetupCommand(RemoveDuplicatesAsync, () => AnyDuplicatesSelected, v => IsLoading = v);
            SaveSettingsCommand = SetupCommand(async () => SaveSettings(), () => AllFieldsFilled, v => IsLoading = v);
            UpdateCommand = SetupCommand(UpdateMissionsAsync, () => AnyUpdatesSelected, v => IsLoading = v);
            RemoveInvalidCommand = SetupCommand(RemoveInvalidAsync, () => AnyInvalidSelected, v => IsLoading = v);
            UnselectAllCommand = SetupCommand(async () => UnselectAllAsync(), () => AnyPendingSelected, v => IsLoading = v);
        }

        private ICommand SetupCommand(Func<Task> action, Expression<Func<bool>> canExecute, Action<bool> progressChange)
        {
            IProgress<bool> progress = new Progress<bool>(progressChange);
            var command = new DelegateCommand(async () =>
                {
                    progress.Report(true);
                    await action();
                    progress.Report(false);
                }, canExecute.Compile())
                .ObservesProperty(canExecute);

            return command;
        }

        private void RetrieveSavedSettings()
        {
            Username = _settingsStorage.GetSetting("Username");
            Address = _settingsStorage.GetSetting("Address");
            int.TryParse(_settingsStorage.GetSetting("Port"), out var port);
            Port = port;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs((propertySelector.Body as MemberExpression)?.Member.Name));
        }

        public ICommand ConnectCommand { get; private set; }
        public ICommand RetrieveMissionsCommand { get; private set; }
        public ICommand MovePendingToLiveCommand { get; private set; }
        public ICommand RemoveDuplicatesCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand RemoveInvalidCommand { get; private set; }
        public ICommand UnselectAllCommand { get; private set; }

        public bool CanConnect { get; set; } = true;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        public bool IsLiveLoading
        {
            get => _isLiveLoading;
            set
            {
                _isLiveLoading = value;
                OnPropertyChanged();
            }
        }
        public bool IsPendingLoading
        {
            get => _isPendingLoading;
            set
            {
                _isPendingLoading = value;
                OnPropertyChanged();
            }
        }
        private async Task ConnectAsync()
        {
            _source = await _missionSourceFactory.SetupAsync(Address, Port, Username, Password)
                .ConfigureAwait(false);

            OnPropertyChanged(() => IsConnected);
            OnPropertyChanged(() => CurrentStatus);

            if (_source == null)
                MessageBox.Show("Could not connect");
        }

        private async Task GetMissionsAsync()
        {
            IsLiveLoading = true;
            await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync(LiveDirectory), LiveMissions);
            UpdateFilteredLive();
            IsLiveLoading = false;
            IsPendingLoading = true;
            await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync(PendingDirectory), PendingMissions);
            IsPendingLoading = false;

            await ShowMissionsAsync(GetDuplicatesAsync, Duplicates);
            await ShowMissionsAsync(GetUpdatesAsync, Updates);
            await ShowMissionsAsync(() => _source.GetFaultyFilesAsync(LiveDirectory), InvalidMissions);
        }

        private async Task ShowMissionsAsync<T>(Func<Task<IEnumerable<T>>> source, ICollection<T> destination)
        {
            var missions = source();

            destination.Clear();
            foreach (var mission in await missions.ConfigureAwait(true))
                destination.Add(mission);
        }

        private async Task MovePendingToLiveAsync()
        {
            foreach (var mission in PendingMissions) //todo mutex read and write
            {
                await _source.MoveMissionToFolderAsync(mission, PendingDirectory, LiveDirectory);
            }
        }

        private async Task<IEnumerable<Mission>> GetDuplicatesAsync()
        {
            return await Task.FromResult(LiveMissions
                .GroupBy(m => (m.Name, m.Type))
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)); //todo fromResult???
        }

        private async Task<IEnumerable<MissionUpdate>> GetUpdatesAsync()
        {
            var groupedPending = PendingMissions
                .GroupBy(m => (m.Name, m.Type));
            var groupedLive = LiveMissions
                .GroupBy(m => (m.Name, m.Type));

            var updates = groupedPending.Join(groupedLive,
                m => (m.First().Name, m.First().Type),
                m => (m.First().Name, m.First().Type),
                 (newMissions, oldMissions) => new MissionUpdate(oldMissions, newMissions));

            return await Task.FromResult(updates);
        }

        private async Task RemoveDuplicatesAsync()
        {
            foreach (var duplicate in SelectedDuplicates)
            {
                await _source.DeleteMissionAsync(duplicate, LiveDirectory);
            }

            await GetMissionsAsync();
        }

        private async Task RemoveInvalidAsync()
        {
            foreach (var invalid in SelectedInvalid)
            {
                await _source.DeleteFileAsync(invalid, LiveDirectory);
            }

            await GetMissionsAsync();
        }

        private async Task UpdateMissionsAsync()
        {
            foreach (var update in SelectedUpdates)
            {
                var latestPending = update.NewMissions.OrderByDescending(m => m.Version).First();
                var latestLive = update.OldMissions.OrderByDescending(m => m.Version).First();
                var isLatestInPending = latestPending.Version.CompareTo(latestLive.Version) > 0;
                var latest = isLatestInPending ? latestPending : latestLive;
                bool confirmed;

                if (update.OldMissions.Count() > 1 || update.NewMissions.Count() > 1)
                    confirmed = ShowPrompt(latestPending.FullName);
                else
                    confirmed = true;

                if (!confirmed)
                    return;

                var pendingMissionsToDelete =
                    update.NewMissions.Where(m => !m.Equals(latest));

                foreach (var mission in pendingMissionsToDelete)
                    await _source.DeleteMissionAsync(mission, PendingDirectory);

                var liveMissionsToDelete = update.OldMissions.Where(m => !m.Equals(latest));

                foreach (var mission in liveMissionsToDelete)
                    await _source.DeleteMissionAsync(mission, LiveDirectory);

                if (isLatestInPending)
                    await _source.MoveMissionToFolderAsync(latestPending, PendingDirectory, LiveDirectory);
            }

            await GetMissionsAsync();
        }

        private bool ShowPrompt(string missionName)
        {
            var result = MessageBox.Show($"{missionName} will be moved to live, other instances of this mission will be removed, do you want to continue?",
                "Confirmation",
                MessageBoxButtons.YesNo);
            return result == DialogResult.Yes;
        }

        private void SaveSettings()
        {
            _settingsStorage.SetSetting("Username", Username);
            _settingsStorage.SetSetting("Address", Address);
            _settingsStorage.SetSetting("Port", Port.ToString());
        }

        private void UnselectAllAsync()
        {
            SelectedPending = new List<Mission>();
        }

        public ObservableCollection<Mission> PendingMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> LiveMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> FilteredLiveMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> Duplicates { get; } = new ObservableCollection<Mission>();
        public IList<Mission> SelectedDuplicates
        {
            get => _selectedDuplicates;
            set
            {
                _selectedDuplicates = value;
                OnPropertyChanged();
                OnPropertyChanged(() => AnyDuplicatesSelected);
            }
        }

        public bool AnyDuplicatesSelected => SelectedDuplicates.Any();

        public ObservableCollection<string> InvalidMissions { get; } = new ObservableCollection<string>();
        public IList<string> SelectedInvalid
        {
            get => _selectedInvalid;
            set
            {
                _selectedInvalid = value;
                OnPropertyChanged();
                OnPropertyChanged(() => AnyInvalidSelected);
            }
        }
        public bool AnyInvalidSelected => SelectedInvalid.Any();

        public ObservableCollection<MissionUpdate> Updates { get; } = new ObservableCollection<MissionUpdate>();
        public IList<MissionUpdate> SelectedUpdates
        {
            get => _selectedUpdates;
            set
            {
                _selectedUpdates = value;
                OnPropertyChanged();
                OnPropertyChanged(() => AnyUpdatesSelected);
            }
        }

        public bool AnyUpdatesSelected => SelectedUpdates.Any();

        public bool AllFieldsFilled => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Username) && Port != 0;
        public bool IsConnected => _source != null;
        public bool LiveMissionsLoaded => LiveMissions.Any();
        public bool PendingMissionsLoaded => PendingMissions.Any();

        public string Password { get; set; }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(() => AllFieldsFilled);
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value; 
                OnPropertyChanged(() => AllFieldsFilled);
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged(() => AllFieldsFilled);
            }
        }

        public IList<Mission> SelectedPending
        {
            get => _selectedPending;
            set
            {
                _selectedPending = value;
                UpdateFilteredLive();
                OnPropertyChanged();
                OnPropertyChanged(() => AnyPendingSelected);
            }
        }

        public bool AnyPendingSelected => SelectedPending.Any();

        private void UpdateFilteredLive()
        {
            if (_selectedPending == null || _selectedPending.Count != 1)
            {
                FilteredLiveMissions.Clear();
                foreach (var mission in LiveMissions)
                    FilteredLiveMissions.Add(mission);
            }
            else
            {
                var selected = _selectedPending.First();

                FilteredLiveMissions.Clear();
                foreach (var mission in LiveMissions.Where(m => m.Name == selected.Name && m.Type == selected.Type))
                    FilteredLiveMissions.Add(mission);
            }
        }

        public string CurrentStatus => IsConnected ? "Connected" : "Disconnected";

        public ObservableCollection<string> LoggedOperations { get; } = new ObservableCollection<string>();

        public string CurrentOperation => "Idle";
    }
}