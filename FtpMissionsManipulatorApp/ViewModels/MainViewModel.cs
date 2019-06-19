using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

            RetrieveMissionsCommand = new DelegateCommand(async () => await GetMissionsAsync(), () => IsConnected)
                .ObservesProperty(() => IsConnected);
            MovePendingToLiveCommand =
                new DelegateCommand(async () => await MovePendingToLiveAsync(), () => PendingMissionsLoaded)
                    .ObservesProperty(() => PendingMissionsLoaded);
            RemoveDuplicatesCommand = new DelegateCommand(async () => await RemoveDuplicatesAsync(), () => true);
            SaveSettingsCommand = new DelegateCommand(SaveSettings, () => AllFieldsFilled)
                .ObservesProperty(() => AllFieldsFilled);

            UpdateCommand = new DelegateCommand(async () => await UpdateMissionsAsync(), () => true);

            RemoveInvalidCommand = new DelegateCommand(async () => await RemoveInvalidAsync(), () => true);
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


        public ICommand ConnectCommand { get; set; }

        public ICommand RetrieveMissionsCommand { get; set; }

        public ICommand MovePendingToLiveCommand { get; set; }

        public ICommand RemoveDuplicatesCommand { get; set; }

        public ICommand SaveSettingsCommand { get; set; }

        public ICommand UpdateCommand { get; set; }

        public ICommand RemoveInvalidCommand { get; set; }

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

            if (_source == null)
                MessageBox.Show("Could not connect");
        }

        private async Task GetMissionsAsync()
        {
            IsLiveLoading = true;
            await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync(LiveDirectory), LiveMissions);
            OnPropertyChanged(() => Duplicates);
            OnPropertyChanged(() => LiveMissions);
            IsLiveLoading = false;
            IsPendingLoading = true;
            await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync(PendingDirectory), PendingMissions);
            IsPendingLoading = false;
            OnPropertyChanged(() => PendingMissions);

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
            return await Task.FromResult(LiveMissions.Join(
                PendingMissions,
                m => m.Name,
                m => m.Name,
                (m1, m2) => new MissionUpdate(m1, m2)));
        }

        private async Task RemoveDuplicatesAsync()
        {
            foreach (var duplicate in SelectedDuplicates)
            {
                await _source.DeleteMissionAsync(duplicate, LiveDirectory);
            }
        }

        private async Task RemoveInvalidAsync()
        {
            foreach (var invalid in SelectedInvalid)
            {
                await _source.DeleteFileAsync(invalid, LiveDirectory);
            }
        }

        private async Task UpdateMissionsAsync()
        {
            foreach (var update in SelectedUpdates)
            {
                //todo
            }
        }

        private void SaveSettings()
        {
            _settingsStorage.SetSetting("Username", Username);
            _settingsStorage.SetSetting("Address", Address);
            _settingsStorage.SetSetting("Port", Port.ToString());
        }

        public ObservableCollection<Mission> PendingMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> LiveMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> Duplicates { get; } = new ObservableCollection<Mission>();

        public IList<Mission> SelectedDuplicates
        {
            get => _selectedDuplicates;
            set
            {
                _selectedDuplicates = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> InvalidMissions { get; } = new ObservableCollection<string>();
        public IList<string> SelectedInvalid
        {
            get => _selectedInvalid;
            set
            {
                _selectedInvalid = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<MissionUpdate> Updates { get; } = new ObservableCollection<MissionUpdate>();
        public IList<MissionUpdate> SelectedUpdates
        {
            get => _selectedUpdates;
            set
            {
                _selectedUpdates = value;
                OnPropertyChanged();
            }
        }

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

    }
}