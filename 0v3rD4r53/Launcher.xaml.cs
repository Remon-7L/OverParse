using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Ov3rD4r53
{
    /// <summary>
    /// Launcher.xaml の相互作用ロジック
    /// </summary>
    public partial class Launcher : Window
    {
        /// <summary>
        /// Plugin Install Not support now  I'm sry   Remon_7L
        /// </summary>
        public Launcher()
        {
            InitializeComponent();
            BanCheck.IsChecked = Properties.Settings.Default.BanChecked;
            SetBin.IsEnabled = Properties.Settings.Default.BanChecked;
            BinPath.Content = "pso2_bin : " + Properties.Settings.Default.Path;
            PathCheck();
        }

        private void BanCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BanChecked = (bool)BanCheck.IsChecked;
            SetBin.IsEnabled = (bool)BanCheck.IsChecked;
            if (BanCheck.IsChecked == false) { Continue_Button.IsEnabled = false; }
            PathCheck();
        }

        private void PathCheck()
        {
            if (File.Exists(Properties.Settings.Default.Path + "\\pso2.exe"))
            {
                PathResult.Content = "OK"; PathResult.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                Status.Content = "Status : ";
                if (Properties.Settings.Default.BanChecked)
                {
                    Continue_Button.IsEnabled = true;
                }
                else
                {
                    Continue_Button.IsEnabled = false;
                }
            }
            else
            {
                PathResult.Content = "Error"; PathResult.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                Status.Content = "Status : pso2_bin check Failed";
            }
        }

        private void SetBin_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
            {
                Properties.Settings.Default.Path = dialog.SelectedPath;
                BinPath.Content = "pso2_bin : " + Properties.Settings.Default.Path;
                PathCheck();
            }
            else
            {
                PathCheck();
            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }
        private void Continue_Click(object sender, RoutedEventArgs e) => DialogResult = true;
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.BanChecked || !File.Exists(Properties.Settings.Default.Path + "\\pso2.exe")) { Application.Current.Shutdown(); }
            SystemCommands.CloseWindow(this);
        }

    }
}
