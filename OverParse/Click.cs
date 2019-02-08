using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace OverParse
{
    public partial class MainWindow : Window
    {

        private void EndEncounter_Click(object sender, RoutedEventArgs e)
        {
            //Ending encounter
            bool temp = Properties.Settings.Default.AutoEndEncounters;
            Properties.Settings.Default.AutoEndEncounters = false;
            //UpdateForm(null, null); // I'M FUCKING STUPID
            Properties.Settings.Default.AutoEndEncounters = temp;
            backup = current;
            backup.players = new List<Player>(current.players);

            /*List<Combatant> workingListCopy = new List<Combatant>();
            foreach (Combatant c in workingList)
            {
                Combatant temp2 = new Combatant(c.ID, c.Name, c.isTemporary);
                foreach (Attack a in c.Attacks) { temp2.Attacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                foreach (Attack a in c.AllyAttacks) { temp2.AllyAttacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                foreach (Attack a in c.DBAttacks) { temp2.DBAttacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                foreach (Attack a in c.LswAttacks) { temp2.LswAttacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                foreach (Attack a in c.PwpAttacks) { temp2.PwpAttacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                foreach (Attack a in c.AisAttacks) { temp2.AisAttacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                foreach (Attack a in c.RideAttacks) { temp2.RideAttacks.Add(new Attack(a.ID, a.Damage, a.JA, a.Cri)); }
                temp2.Damaged = c.Damaged;
                temp2.AllyDamage = c.AllyDamage;
                temp2.DBDamage = c.DBDamage;
                temp2.LswDamage = c.LswDamage;
                temp2.PwpDamage = c.PwpDamage;
                temp2.AisDamage = c.AisDamage;
                temp2.RideDamage = c.RideDamage;
                temp2.PercentReadDPS = c.PercentReadDPS;
                temp2.TScore = c.TScore;
                workingListCopy.Add(temp2);
            }*/
            //Saving last combatant list"

            //ExportWriteLog();
            string filename = WriteLog();
            if (filename != null)
            {
                if ((SessionLogs.Items[0] as MenuItem).Name == "SessionLogPlaceholder") { SessionLogs.Items.Clear(); }
                int items = SessionLogs.Items.Count;
                string prettyName = filename.Split('/').LastOrDefault();
                sessionLogFilenames.Add(filename);
                var menuItem = new MenuItem() { Name = "SessionLog_" + items.ToString(), Header = prettyName };
                menuItem.Click += OpenRecentLog_Click;
                SessionLogs.Items.Add(menuItem);
            }
            // (Properties.Settings.Default.LogToClipboard) { WriteClipboard(); }
            IsRunning = false;
            UpdateForm(this, null);
            speechcount = 1;
            GC.Collect();
        }

        public void EndEncounter_Key(object sender, EventArgs e) => EndEncounter_Click(null, null);

        private void EndEncounterNoLog_Click(object sender, RoutedEventArgs e)
        {
            /*
            current.ActiveTime = backup.ActiveTime; //一つ前のDPS計算用
            bool temp = Properties.Settings.Default.AutoEndEncounters;
            Properties.Settings.Default.AutoEndEncounters = false;
            UpdateForm(null, null);
            Properties.Settings.Default.AutoEndEncounters = temp;
            //Reinitializing log
            encounterlog = new Log(Properties.Settings.Default.Path);
            Log.startTimestamp = Log.nowTimestamp = Log.diffTime = 0;
            totalDamage = totalAllyDamage = totalDBDamage = totalLswDamage = totalPwpDamage = totalAisDamage = totalRideDamage = totalZanverse = totalFinish = 0;
            totalSD = 0;
            UpdateForm(null, null);
            */
            current = backup;
            current.players = new List<Player>(backup.players);
            IsRunning = false;
            UpdateForm(this, null);
            speechcount = 1;
            GC.Collect();
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
            if (Int32.TryParse(input.ResultText, out int x))
            {
                if (0 < x) { Properties.Settings.Default.EncounterTimeout = x; } else { MessageBox.Show("Error"); }
            } else
            {
                if (input.ResultText.Length > 0) { MessageBox.Show("Couldn't parse your input. Enter only a number."); }
            }
            AlwaysOnTop.IsChecked = Properties.Settings.Default.AlwaysOnTop;
        }

        //private void LogToClipboard_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.LogToClipboard = LogToClipboard.IsChecked;

        private void IsWriteTS_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.IsWriteTS = IsWriteTS.IsChecked;

        private void OpenLogsFolder_Click(object sender, RoutedEventArgs e) => Process.Start(Directory.GetCurrentDirectory() + "\\Logs");

        private void SeparateZanverse_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateZanverse = SeparateZanverse.IsChecked;
            UpdateForm(null, null);
        }

        private void SeparateFinish_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SeparateFinish = SeparateFinish.IsChecked;
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
                HideAIS.IsChecked = false;
                HideDB.IsChecked = false;
                HideRide.IsChecked = false;
                HidePwp.IsChecked = false;
            }
            UpdateForm(null, null);
        }

        private void HideAIS_Click(object sender, RoutedEventArgs e) { if (HideAIS.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HideDB_Click(object sender, RoutedEventArgs e) { if (HideDB.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HideRide_Click(object sender, RoutedEventArgs e) { if (HideRide.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HidePwp_Click(object sender, RoutedEventArgs e) { if (HidePwp.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void HideLsw_Click(object sender, RoutedEventArgs e) { if (HideLsw.IsChecked) { HidePlayers.IsChecked = false; } UpdateForm(null, null); }
        private void Onlyme_Click(object sender, RoutedEventArgs e) { Properties.Settings.Default.Onlyme = Onlyme.IsChecked; UpdateForm(null, null); }
        private void Nodecimal_Click(object sender, RoutedEventArgs e) { Properties.Settings.Default.Nodecimal = Nodecimal.IsChecked; UpdateForm(null, null); }

        private void QuestTime_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.QuestTime = QuestTime.IsChecked;
            if (Properties.Settings.Default.QuestTime)
            {
                current.ActiveTime = current.diffTime;
            } else
            {
                current.ActiveTime = current.newTimestamp - current.startTimestamp;
            }
        }

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
            } else
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
            IntPtr wasActive = WindowsServices.GetForegroundWindow();

            // hack for activating overparse window
            WindowState = WindowState.Minimized;
            Show();
            WindowState = WindowState.Normal;

            Topmost = AlwaysOnTop.IsChecked;
            AlwaysOnTop_Click(null, null);
            WindowsServices.SetForegroundWindow(wasActive);
        }

        private void AutoHideWindow_Click(object sender, RoutedEventArgs e)
        {
            if (AutoHideWindow.IsChecked && Properties.Settings.Default.AutoHideWindowWarning)
            {
                MessageBox.Show("これにより、PSO2またはOverParseがフォアグラウンドにない時は、OverParseのウィンドゥが非表示になります。\nウィンドゥを表示するには、Alt+TabでOverParseにするか、タスクバーのアイコンをクリックします。", "OverParse Setup", MessageBoxButton.OK, MessageBoxImage.Information);
                Properties.Settings.Default.AutoHideWindowWarning = false;
            }
            Properties.Settings.Default.AutoHideWindow = AutoHideWindow.IsChecked;
        }

        private void ClickthroughToggle(object sender, RoutedEventArgs e) => Properties.Settings.Default.ClickthroughEnabled = ClickthroughMode.IsChecked;

        private void OpenInstall_Click(object sender, RoutedEventArgs e)
        {
            Launcher launcher = new Launcher() { Owner = this }; launcher.ShowDialog();
        }

        private async void Updateskills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla / 5.0 OverParse / 4.0.0");
                    client.DefaultRequestHeaders.Add("Connection", "close");
                    client.Timeout = TimeSpan.FromSeconds(20.0);
                    var content = await client.GetAsync("https://remon-7l.github.io/ovp/skills_ja.csv");
                    using (var fileStream = File.Create(@"./prop/skills_ja.csv"))
                    {
                        using (var httpStream = await content.Content.ReadAsStreamAsync())
                        {
                            httpStream.CopyTo(fileStream);
                            await fileStream.FlushAsync();
                        }
                    }

                    var content = await client.GetAsync("https://wilsonltl.github.io/img/skill_eng.csv");
                    using (var fileStream = File.Create(@"./prop/skills_en.csv"))
                    {
                        using (var httpStream = await content.Content.ReadAsStreamAsync())
                        {
                            httpStream.CopyTo(fileStream);
                            await fileStream.FlushAsync();
                        }
                    }

                    var content = await client.GetAsync("https://wilsonltl.github.io/img/skill_tw_hk.csv");
                    using (var fileStream = File.Create(@"./prop/skills_tw.csv"))
                    {
                    using (var httpStream = await content.Content.ReadAsStreamAsync())
                    {
                    httpStream.CopyTo(fileStream);
                    await fileStream.FlushAsync();
                     }
                    }

                    // Hi remon-7l, are you interesting to make other language selection? I can provide other other language skills.csv including english and chinese.
                    // These data is working by simply replace the skills_ja.csv, so it will be quite easy to make a new language too.
                    // Link of chinese vision: https://wilsonltl.github.io/img/skill_tw_hk.csv
		                // Link of English vision: https://wilsonltl.github.io/img/skill_eng.csv
		                // English vision translation source: Arks-Visiphone
		                // Chinese vision translation source: gamer.com
                    // Cheers:P - WilsonLTL

                }
            } catch
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

        private void Capture(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmap;

            System.Windows.Point point = CombatantData.PointToScreen(new System.Windows.Point(0.0d, 0.0d));
            Rect target = new Rect(point.X + 2.0, point.Y + 1.0, CombatantData.ActualWidth - 3.0, CombatantData.ActualHeight + 17.0);
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
