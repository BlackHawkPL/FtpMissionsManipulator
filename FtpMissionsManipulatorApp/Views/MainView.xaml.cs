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
            DataContext = new MainViewModel(this, new ManipulatorFactory());

            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var password = (sender as PasswordBox)?.Password;

            ((MainViewModel) DataContext).Password = password;
        }
    }
}
