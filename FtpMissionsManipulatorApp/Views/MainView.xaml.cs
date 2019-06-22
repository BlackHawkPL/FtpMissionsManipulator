using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FtpMissionsManipulator;

namespace FtpMissionsManipulatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;

        public MainView()
        {
            _viewModel = new MainViewModel(new MissionSourceFactory(), new SettingsStorage());
            DataContext = _viewModel;
#if DEBUG
            PrepareTestDir();
#endif
            InitializeComponent();
        }

        private static void PrepareTestDir()
        {
            const string target = @"C:\ftp";
            const string source = @"C:\ftp_template";
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }

            foreach (var dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, target));

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, target), true);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var password = (sender as PasswordBox)?.Password;

            _viewModel.Password = password;
        }

        private static void UpdateCollection<T>(Action<IList<T>> listSetter, object list)
        {
            var listView = list as ListView;
            var result = listView.SelectedItems
                .Cast<T>()
                .ToList();
            listSetter(result);
        }

        private void DuplicatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCollection<Mission>(list => _viewModel.SelectedDuplicates = list, sender);
        }

        private void InvalidListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCollection<string>(list => _viewModel.SelectedInvalid = list, sender);
        }

        private void UpdatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCollection<MissionUpdate>(list => _viewModel.SelectedUpdates = list, sender);
        }

        private void PendingListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCollection<Mission>(list => _viewModel.SelectedPending = list, sender);
        }

        private void LiveListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCollection<Mission>(list => _viewModel.SelectedLive = list, sender);
        }
    }
}
