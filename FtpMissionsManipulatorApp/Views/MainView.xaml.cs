using System.IO;
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
        public MainView()
        {
            DataContext = new MainViewModel(this, new MissionSourceFactory());
            PrepareTestDir();

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

            ((MainViewModel) DataContext).Password = password;
        }
    }
}
