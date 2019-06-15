using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ov3rD4r53
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : YKToolkit.Controls.Window
    {
        public static bool IsRunning, IsConnect;
        public static bool SepHTF, SepZvs, SepDB, SepLsw, SepPwp, SepAIS, SepRide, IsOnlyme, IsQuestTime, IsShowGraph, IsHighlight; // Properties.Settings.Default... Read is high cost, My BAD IDEA is this;
        public static StreamReader logReader = null;  //Assert null
        public static string[] currentPlayerID = null;
        public static string currentPlayerName = null;
        public static string[] jaignoreskill, critignoreskill, playerid;
        public static Dictionary<uint, string> skillDict = new Dictionary<uint, string>();
        public static DirectoryInfo damagelogs;
        public static FileInfo damagelogcsv;
        public static List<Player> workingList = new List<Player>();
        public static Session current = new Session();
        public static Session backup = new Session();
        public static ObservableCollection<Hit> userattacks = new ObservableCollection<Hit>();
        public DispatcherTimer damageTimer, logCheckTimer, inactiveTimer;
        public static Color MyColor, OddLeft, OddRgt, EveLeft, EveRgt, Other;
        private string updatemsg = " - Update checking...";
        private List<string> LogFilenames = new List<string>();
        private IntPtr hwndcontainer;
        public static Overlay overlay;

        public MainWindow()
        {
            try { Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OverParse"); Directory.CreateDirectory("Logs"); Directory.CreateDirectory("Export"); }
            catch (Exception ex)
            {
                MessageBox.Show($"OverParseに必要なアクセス権限がありません！\n管理者としてOverParseを実行してみるか、システムのアクセス権を確認して下さい！\nOverParseを別のフォルダーに移動してみるのも良いかも知れません。\n\n{ex.ToString()}");
                Application.Current.Shutdown();
            }
            Properties.Resources.Culture = CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
            InitializeComponent();
            Dispatcher.UnhandledException += ErrorToLog;
            if (!Properties.Settings.Default.BanChecked)
            {
                Launcher launcher = new Launcher(); launcher.ShowDialog();
                if (launcher.DialogResult != true && Application.Current != null) { Application.Current.Shutdown(); return; }
            }

            //if (Properties.Settings.Default.BouyomiStartup) { Process.Start(Properties.Settings.Default.BouyomiPath); }

            AlwaysOnTop.IsChecked = Properties.Settings.Default.AlwaysOnTop;
            ConfigLoad();
            LoadListColumn();
        }

        private void ErrorToLog(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Directory.CreateDirectory("ErrorLogs");
                string datetime = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
                string filename = $"ErrorLogs/ErrorLogs - {datetime}.txt";
                File.WriteAllText(filename, e.Exception.ToString());
            }
            catch
            {
                MessageBox.Show("OverParseはDirectory<ErrorLogs>の作成に失敗しました。" + Environment.NewLine + "OverParse内のディレクトリにErrorLogを保存しました。");
                string datetime = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
                File.WriteAllText($"ErrorLogs - {datetime}.txt", e.Exception.ToString());
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            damagelogs = new DirectoryInfo(Properties.Settings.Default.Path + "\\damagelogs");
            if (Directory.Exists(Properties.Settings.Default.Path + "\\damagelogs") && damagelogs.GetFiles().Any())
            {
                damagelogcsv = damagelogs.GetFiles().Where(f => Regex.IsMatch(f.Name, @"\d+\.")).OrderByDescending(f => f.Name).FirstOrDefault();
                FileStream fileStream = File.Open(damagelogcsv.DirectoryName + "\\" + damagelogcsv.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fileStream.Seek(0, SeekOrigin.Begin);
                logReader = new StreamReader(fileStream);

                List<string> cid = new List<string>();
                while (!logReader.EndOfStream)
                {
                    string line = logReader.ReadLine();
                    if (line == "") { continue; }
                    string[] parts = line.Split(',');
                    if (parts[0] == "0" && parts[3] == "YOU") { cid.Add(parts[2]);  }
                }

                cid.Sort();
                currentPlayerID = cid.ToArray();
                fileStream.Seek(0, SeekOrigin.End);
                logReader = new StreamReader(fileStream);
            }

            SkillsLoad();
            Task VerCheck = Version_Check();
            HotKeyLoad();

            await Task.WhenAll(VerCheck);

            damageTimer = new DispatcherTimer();
            logCheckTimer = new DispatcherTimer();
            inactiveTimer = new DispatcherTimer();
            damageTimer.Tick += new EventHandler(UpdateForm);
            damageTimer.Interval = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.Updateinv);
            logCheckTimer.Tick += new EventHandler(CheckNewCsv);
            logCheckTimer.Interval = new TimeSpan(0, 0, 20);
            inactiveTimer.Tick += new EventHandler(HideIfInactive);
            inactiveTimer.Interval = new TimeSpan(0, 0, 1);
            damageTimer.Start();
            logCheckTimer.Start();
            inactiveTimer.Start();
        }

        private void HideIfInactive(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.AutoHideWindow) { return; }
            string title = NativeMethods.GetActiveWindowTitle();
            string[] relevant = { "OverParse", "Ov3rD4r53", "OverParse Setup", "OverParse Error", "Encounter Timeout", "Phantasy Star Online 2", "Settings", "AtkLog", "Detalis", "Color", "Ov3rD4r53 Install" };
            if (!relevant.Contains(title))
            {
                Opacity = 0;
            }
            else
            {
                TheWindow.Opacity = Properties.Settings.Default.WindowOpacity;
            }
        }

        private void CheckNewCsv(object sender, EventArgs e)
        {
            if (!damagelogs.Exists || !damagelogs.GetFiles().Any()) { return; }
            FileInfo curornewcsv = damagelogs.GetFiles().Where(f => Regex.IsMatch(f.Name, @"\d+\.")).OrderByDescending(f => f.Name).FirstOrDefault();
            if (damagelogcsv != null && curornewcsv.LastWriteTimeUtc <= damagelogcsv.LastWriteTimeUtc) { return; }
            damagelogcsv = curornewcsv;
            FileStream fileStream = File.Open(damagelogcsv.DirectoryName + "\\" + damagelogcsv.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.Begin);
            logReader = new StreamReader(fileStream);


            while (!logReader.EndOfStream)
            {
                string line = logReader.ReadLine();
                if (line == "") { continue; }
                string[] parts = line.Split(',');
                if (parts[0] == "0" && parts[3] == "YOU") { currentPlayerID.Append(parts[2]); }
            }

            fileStream.Seek(0, SeekOrigin.End);
            logReader = new StreamReader(fileStream);
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            hwndcontainer = hwnd;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TheWindow.Opacity = Properties.Settings.Default.WindowOpacity;
            Window window = (Window)sender;
            window.Topmost = AlwaysOnTop.IsChecked;

            if (Properties.Settings.Default.ClickthroughEnabled)
            {
                int extendedStyle = NativeMethods.GetWindowLong(hwndcontainer, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(hwndcontainer, NativeMethods.GWL_EXSTYLE, extendedStyle & ~NativeMethods.WS_EX_TRANSPARENT);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = AlwaysOnTop.IsChecked;
            if (Properties.Settings.Default.ClickthroughEnabled)
            {
                int extendedStyle = NativeMethods.GetWindowLong(hwndcontainer, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(hwndcontainer, NativeMethods.GWL_EXSTYLE, extendedStyle | NativeMethods.WS_EX_TRANSPARENT);
            }
        }

        private void Window_StateChanged(object sender, EventArgs e) { if (WindowState == WindowState.Maximized) { WindowState = WindowState.Normal; } }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Windowを移動可能にする
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (damageTimer != null) { damageTimer.Stop(); }
                DragMove();
            }
            if (e.LeftButton == MouseButtonState.Released && damageTimer != null) { damageTimer.Start(); }
        }

        private void ListViewItem_MouseRightClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem data = sender as ListViewItem;
            Player data2 = (Player)data.DataContext;
            Detalis f = new Detalis(data2) { Owner = this };
            f.Show();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Closing...
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Maximized = false;
            }

            if (IsRunning) { WriteLog(); }
            Properties.Settings.Default.Save();
        }

    }
}
