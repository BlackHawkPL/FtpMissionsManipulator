using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FtpMissionsManipulator;
using FtpMissionsManipulatorApp.Annotations;
using MessageBox = System.Windows.Forms.MessageBox;

namespace FtpMissionsManipulatorApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Window _window;
        private readonly IManipulatorFactory _manipulatorFactory;
        private IMissionsManipulator _manipulator;

        public MainViewModel(Window w, IManipulatorFactory factory)
        {
            _window = w;
            _manipulatorFactory = factory;
            ConnectCommand = new RoutedCommand();
            ListLiveCommand = new RoutedCommand();
            Missions = new ObservableCollection<Mission>();

            SetupCommand(ConnectCommand, Connect, true);
            SetupCommand(ListLiveCommand, ListLive, () => _manipulator != null);
        }

        private void SetupCommand(ICommand command, Action action, Func<bool> canExecute)
        {
            _window.CommandBindings.Add(new CommandBinding(command,
                (sender, args) =>
                {
                    action();
                },
                (sender, args) => args.CanExecute = canExecute()));
        }

        private void SetupCommand(ICommand command, Action action, bool canExecute)
        {
            SetupCommand(command, action, () => canExecute);
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
        public RoutedCommand ListLiveCommand { get; set; }

        private async void Connect()
        {
            _manipulator = await _manipulatorFactory.SetupAsync(Address, Port, Username, Password)
                .ConfigureAwait(false);

            if (_manipulator == null)
                MessageBox.Show("Could not connect");
        }

        private async void ListLive()
        {
            var missions = await _manipulator.GetLiveMissionsAsync()
                .ConfigureAwait(true);
            foreach (var mission in missions)
            {
                Missions.Add(mission);
            }
        }

        public ObservableCollection<Mission> Missions { get; set; }

        public string Password { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

    }
}