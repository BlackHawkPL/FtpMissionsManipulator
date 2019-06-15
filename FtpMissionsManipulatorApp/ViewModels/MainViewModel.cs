using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FtpMissionsManipulator;
using FtpMissionsManipulatorApp.Annotations;
using Prism.Commands;
using MessageBox = System.Windows.Forms.MessageBox;

namespace FtpMissionsManipulatorApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Window _window;
        private readonly IMissionSourceFactory _missionSourceFactory;
        private IMissionsSource _source;
        private bool? _isShowingDuplicates = false;

        public MainViewModel(Window w, IMissionSourceFactory factory)
        {
            _window = w;
            _missionSourceFactory = factory;

            LiveMissions.CollectionChanged += (sender, e) => OnPropertyChanged(() => LiveMissionsLoaded);
            PendingMissions.CollectionChanged += (sender, e) => OnPropertyChanged(() => PendingMissionsLoaded);

            ConnectCommand = new DelegateCommand(async () => await ConnectAsync(), () => true);
            RetrieveMissionsCommand = new DelegateCommand(async () => await GetMissionsAsync(), () => IsConnected)
                .ObservesProperty(() => IsConnected);
            MovePendingToLiveCommand = new DelegateCommand(async () => await MovePendingToLiveAsync(), () => PendingMissionsLoaded)
                .ObservesCanExecute(_ => PendingMissionsLoaded);
            RemoveDuplicatesCommand = new DelegateCommand(async () => await RemoveDuplicatesAsync(), () => LiveMissionsLoaded)
                .ObservesCanExecute(_ => LiveMissionsLoaded);

            //SetupCommand(ConnectCommand, ConnectAsync, true);
            //SetupCommand(RetrieveMissionsCommand, GetMissionsAsync, () => _manipulator != null);
            //SetupCommand(MovePendingToLiveCommand, MovePendingToLiveAsync, () => PendingMissions.Any());
            //SetupCommand(RemoveDuplicatesCommand, RemoveDuplicatesAsync, () => LiveMissions.Any());
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


        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand RetrieveMissionsCommand { get; }
        public DelegateCommand MovePendingToLiveCommand { get; }
        public DelegateCommand RemoveDuplicatesCommand { get; }

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
            await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync("SRV1"), LiveMissions);
            await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync("_FINAL"), PendingMissions);
            //await ShowMissionsAsync(() => _source.GetMissionsFromDirectoryAsync(), DuplicateMissions);
        }

        private async Task ShowMissionsAsync(Func<Task<IEnumerable<Mission>>> source, ObservableCollection<Mission> destination)
        {
            var missions = source();

            destination.Clear();
            foreach (var mission in await missions.ConfigureAwait(true))
            {
                destination.Add(mission);
            }
        }

        private async Task MovePendingToLiveAsync()
        {
            foreach (var mission in PendingMissions) //todo mutex read and write
            {
                await _source.MoveMissionToFolderAsync(mission, "_FINAL", "SRV1");
            }
        }

        private async Task RemoveDuplicatesAsync()
        {
            
        }


        public ObservableCollection<Mission> PendingMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> LiveMissions { get; } = new ObservableCollection<Mission>();
        public ObservableCollection<Mission> DuplicateMissions { get; } = new ObservableCollection<Mission>();

        public bool IsConnected => _source != null;
        public bool LiveMissionsLoaded => LiveMissions.Any();
        public bool PendingMissionsLoaded => PendingMissions.Any();

        public bool? IsShowingLive
        {
            get => IsShowingDuplicates == false;
            set => IsShowingDuplicates = !value;
        }

        public bool? IsShowingDuplicates
        {
            get => _isShowingDuplicates;
            set
            {
                _isShowingDuplicates = value;
                OnPropertyChanged();
                OnPropertyChanged(() => IsShowingLive);
            }
        }

        public string Password { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

    }
}