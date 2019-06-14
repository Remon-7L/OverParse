using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Ov3rD4r53
{
    public partial class MainWindow : YKToolkit.Controls.Window
    {
        private void EndEncounter_Click(object sender, RoutedEventArgs e)
        {
            bool temp = Properties.Settings.Default.AutoEndEncounters;
            Properties.Settings.Default.AutoEndEncounters = false;
            //UpdateForm(null, null); // I'M FUCKING STUPID
            Properties.Settings.Default.AutoEndEncounters = temp;
            backup = current;
            backup.players = new List<Player>(current.players);

            string filename = WriteLog();
            if (filename != null)
            {
                if ((SessionLogs.Items[0] as MenuItem).Name == "SessionLogPlaceholder") { SessionLogs.Items.Clear(); }
                int items = SessionLogs.Items.Count;
                string prettyName = filename.Split('/').LastOrDefault();
                LogFilenames.Add(filename);
                var menuItem = new MenuItem() { Name = "SessionLog_" + items.ToString(), Header = prettyName };
                menuItem.Click += OpenRecentLog_Click;
                SessionLogs.Items.Add(menuItem);
            }
            // if(Properties.Settings.Default.LogToClipboard) { WriteClipboard(); }
            IsRunning = false;
            UpdateForm(this, null);
            speechcount = 1;
        }

        public void EndEncounter_Key(object sender, EventArgs e) => EndEncounter_Click(null, null);

        private void EndEncounterNoLog_Click(object sender, RoutedEventArgs e)
        {
            current = backup;
            current.players = new List<Player>(backup.players);
            IsRunning = false;
            UpdateForm(this, null);
            speechcount = 1;
        }

        private void EndEncounterNoLog_Key(object sender, EventArgs e) => EndEncounterNoLog_Click(sender, null);

        private void AutoEndEncounters_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoEndEncounters = AutoEndEncounters.IsChecked;
            SetEncounterTimeout.IsEnabled = AutoEndEncounters.IsChecked;
        }

        private void SetEncounterTimeout_Click(object sender, RoutedEventArgs e)
        {
            AlwaysOnTop.IsChecked = false;
            Inputbox input = new Inputbox("Encounter Timeout", "何秒経過すればエンカウントを終了させますか？", Properties.Settings.Default.EncounterTimeout.ToString()) { Owner = this };
            input.ShowDialog();
            if (int.TryParse(input.ResultText, out int x))
            {
                if (0 < x) { Properties.Settings.Default.EncounterTimeout = x; } else { MessageBox.Show("Error"); }
            }
            else
            {
                if (input.ResultText.Length > 0) { MessageBox.Show("Couldn't parse your input. Enter only a number."); }
            }
            AlwaysOnTop.IsChecked = Properties.Settings.Default.AlwaysOnTop;
        }

        //private void LogToClipboard_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.LogToClipboard = LogToClipboard.IsChecked;

        private void IsWriteTS_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.IsWriteTS = IsWriteTS.IsChecked;

        private void OpenLogsFolder_Click(object sender, RoutedEventArgs e) => Process.Start(Directory.GetCurrentDirectory() + "\\Logs");

        private void OpenRecentLog_Click(object sender, RoutedEventArgs e) => Process.Start(Directory.GetCurrentDirectory() + "\\" + LogFilenames[SessionLogs.Items.IndexOf(e.OriginalSource as MenuItem)]);

        private void SeparateZanverse_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateZanverse = SeparateZanverse.IsChecked;
            SepZvs = SeparateZanverse.IsChecked;
            UpdateForm(null, null);
        }

        private void SeparateFinish_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateFinish = SeparateFinish.IsChecked;
            SepHTF = SeparateFinish.IsChecked;
            UpdateForm(null, null);
        }

        private void SeparateAIS_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateAIS = SeparateAIS.IsChecked;
            HideAIS.IsEnabled = SeparateAIS.IsChecked;
            HidePlayers.IsEnabled = (SeparateAIS.IsChecked || SeparateDB.IsChecked || SeparateRide.IsChecked || SeparatePwp.IsChecked || SeparateLsw.IsChecked);
            UpdateForm(null, null);
        }

        private void SeparateDB_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateDB = SeparateDB.IsChecked;
            HideDB.IsEnabled = SeparateDB.IsChecked;
            HidePlayers.IsEnabled = (SeparateAIS.IsChecked || SeparateDB.IsChecked || SeparateRide.IsChecked || SeparatePwp.IsChecked || SeparateLsw.IsChecked);
            SepDB = SeparateDB.IsChecked;
            UpdateForm(null, null);
        }

        private void SeparateRide_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateRide = SeparateRide.IsChecked;
            HideRide.IsEnabled = SeparateRide.IsChecked;
            HidePlayers.IsEnabled = (SeparateAIS.IsChecked || SeparateDB.IsChecked || SeparateRide.IsChecked || SeparatePwp.IsChecked || SeparateLsw.IsChecked);
            UpdateForm(null, null);
        }

        private void SeparatePwp_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparatePwp = SeparatePwp.IsChecked;
            HidePwp.IsEnabled = SeparatePwp.IsChecked;
            HidePlayers.IsEnabled = (SeparateAIS.IsChecked || SeparateDB.IsChecked || SeparateRide.IsChecked || SeparatePwp.IsChecked || SeparateLsw.IsChecked);
            UpdateForm(null, null);
        }

        private void SeparateLsw_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateLsw = SeparateLsw.IsChecked;
            HideLsw.IsEnabled = SeparateLsw.IsChecked;
            HidePlayers.IsEnabled = (SeparateAIS.IsChecked || SeparateDB.IsChecked || SeparateRide.IsChecked || SeparatePwp.IsChecked || SeparateLsw.IsChecked);
            UpdateForm(null, null);
        }

        private void HidePlayers_Click(object sender, RoutedEventArgs e)
        {
            if (HidePlayers.IsChecked)
            {
                HideAIS.IsChecked = false; HideDB.IsChecked = false;
                HideRide.IsChecked = false; HidePwp.IsChecked = false;
            }
            UpdateForm(null, null);
        }

        private void HideAIS_Click(object sender, RoutedEventArgs e) { if (HideAIS.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HideDB_Click(object sender, RoutedEventArgs e) { if (HideDB.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HideRide_Click(object sender, RoutedEventArgs e) { if (HideRide.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HidePwp_Click(object sender, RoutedEventArgs e) { if (HidePwp.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HideLsw_Click(object sender, RoutedEventArgs e) { if (HideLsw.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void Onlyme_Click(object sender, RoutedEventArgs e) { Properties.Settings.Default.Onlyme = Onlyme.IsChecked; IsOnlyme = Onlyme.IsChecked; UpdateForm(null, null); }
        private void Nodecimal_Click(object sender, RoutedEventArgs e) { Properties.Settings.Default.Nodecimal = Nodecimal.IsChecked; UpdateForm(null, null); }

        private void QuestTime_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.QuestTime = QuestTime.IsChecked;

        private void DefaultWindowSize_Click(object sender, RoutedEventArgs e) { Height = 275; Width = 670; }
        private void DefaultWindowSize_Key(object sender, EventArgs e) { Height = 275; Width = 670; }

        private void SettingWindow_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow dialog = new SettingWindow() { Owner = this };
            dialog.ShowDialog();
        }

        private void Bouyomi_Click(object sender, RoutedEventArgs e)
        {
            if (0 < Process.GetProcessesByName("BouyomiChan").Length)
            {
                IsConnect = true;
                BouyomiEnable.IsChecked = true;
            }
            else
            {
                MessageBox.Show(this, "BouyomiChan.exeの起動を検出できませんでした。");
                IsConnect = false;
                BouyomiEnable.IsChecked = false;
            }
        }

        private void Bouyomi_Key(object sender, EventArgs e) => Bouyomi_Click(sender, null);

        private void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AlwaysOnTop = AlwaysOnTop.IsChecked;
            OnActivated(e);
        }

        public void AlwaysOnTop_Key(object sender, EventArgs e)
        {
            AlwaysOnTop.IsChecked = !AlwaysOnTop.IsChecked;
            IntPtr wasActive = NativeMethods.GetForegroundWindow();

            // hack for activating overparse window
            WindowState = WindowState.Minimized;
            Show();
            WindowState = WindowState.Normal;

            Topmost = AlwaysOnTop.IsChecked;
            AlwaysOnTop_Click(null, null);
            NativeMethods.SetForegroundWindow(wasActive);
        }

        private void AutoHideWindow_Click(object sender, RoutedEventArgs e)
        {
            if (AutoHideWindow.IsChecked && Properties.Settings.Default.AutoHideWindowWarning)
            {
                MessageBox.Show("これにより、ゲームまたはOvPがフォアグラウンドにない時は、OvPのウィンドウが非表示になります。\nウィンドウを表示するには、Alt+Tabで切り替えるか、タスクバーのアイコンをクリックします。", "Ov3rD4r53", MessageBoxButton.OK, MessageBoxImage.Information);
                Properties.Settings.Default.AutoHideWindowWarning = false;
            }
            Properties.Settings.Default.AutoHideWindow = AutoHideWindow.IsChecked;
        }

        private void ClickthroughToggle(object sender, RoutedEventArgs e) => Properties.Settings.Default.ClickthroughEnabled = ClickthroughMode.IsChecked;

        private void OpenInstall_Click(object sender, RoutedEventArgs e)
        {
            Launcher launcher = new Launcher() { Owner = this }; launcher.ShowDialog();
        }

        private void Updateskills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    Stream stream = client.OpenRead("https://remon-7l.github.io/skills_ja.csv");
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        string content = streamReader.ReadToEnd();
                        File.WriteAllText("skills_ja.csv", content);
                    }
                }
            }
            catch
            {
                MessageBox.Show("skills.csvの取得に失敗しました。");
            }
        }

#if DEBUG
        private void DebugWindow_Key(object sender, EventArgs e)
        {
            DebugWindow debugWindow = new DebugWindow();
            debugWindow.Show();
        }
#endif

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            AtkLogWindow window = new AtkLogWindow() { Owner = this };
            window.Show();
        }

        private void Overlay_Click(object sender, RoutedEventArgs e)
        {
            if (overlay == null || !overlay.IsLoaded) { overlay = new Overlay(); }
            overlay.Show();
        }

        private void Capture(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmap;

            System.Windows.Point point = CombatantData.PointToScreen(new System.Windows.Point(0.0d, 0.0d));
            Rect target = new Rect(point.X + 3.0, point.Y + 2.0, CombatantData.ActualWidth - 3.0, CombatantData.ActualHeight + 17.0);
            using (Bitmap screen = new Bitmap((int)target.Width, (int)target.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics bmp = Graphics.FromImage(screen))
                {
                    bmp.CopyFromScreen((int)target.X, (int)target.Y, 0, 0, screen.Size);
                    bitmap = Imaging.CreateBitmapSourceFromHBitmap(screen.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            Clipboard.SetImage(bitmap);
        }

        private void Capture_Key(object sender, EventArgs e) => Capture(sender, null);

    }
}
