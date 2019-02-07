using HotKeyFrame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace OverParse
{
    public partial class MainWindow : Window
    {
        private HotKey hotkey1, hotkey2, hotkey3, hotkey4, hotkey5, hotkey6;
#if DEBUG
        private HotKey hotkey7;
#endif

        private async Task SkillsLoad()
        {
            //skills.csv
            string[] skills = new string[0];
            try
            {
                if (Properties.Settings.Default.Language == "ja-JP") { skills = File.ReadAllLines(@"prop/skills_ja.csv"); }
                if (Properties.Settings.Default.Language == "en-US") { skills = File.ReadAllLines(@"prop/skills_en.csv"); }
                //if (Properties.Settings.Default.Language == "zh-TW") { skills = File.ReadAllLines(@"prop/skills_tw.csv"); }
            }
            catch
            {
                MessageBox.Show("skills.csvが存在しません。\n全ての最大ダメージはUnknownとなります。", "OverParse Setup", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            try
            {
                jaignoreskill = File.ReadAllLines(@"prop/jaignoreskills.csv");
            } catch (Exception error)
            {
                MessageBox.Show(error.ToString());
                jaignoreskill = new string[] { "12345678900" }; //nullだとエラーが出るので適当な値
            }

            try
            {
                critignoreskill = File.ReadAllLines(@"prop/critignoreskills.csv");
            } catch (Exception error)
            {
                MessageBox.Show(error.ToString());
                critignoreskill = new string[] { "12345678900" }; //nullだとエラーが出るので適当な値
            }

            #region XmlLoad
            try
            {
                Sepid.DBAtkID = Sepid.LswAtkID = Sepid.PwpAtkID = Sepid.AISAtkID = Sepid.RideAtkID = Sepid.HTFAtkID = Sepid.IgnoreAtkID = new uint[0];

                XmlDocument doc = new XmlDocument();
                doc.Load(@"prop/separate.xml");
                foreach (XmlElement el in doc.DocumentElement)
                {
                    switch (el.Name)
                    {
                        case "DB":
                            foreach (XmlNode val in el.ChildNodes)
                            {
                                if (val.NodeType != XmlNodeType.Comment)
                                {
                                    Array.Resize(ref Sepid.DBAtkID, Sepid.DBAtkID.Length + 1);
                                    Sepid.DBAtkID[Sepid.DBAtkID.Length - 1] = uint.Parse(val.InnerText);
                                }
                            }
                            break;
                        case "Lsw":
                            foreach (XmlElement val in el.ChildNodes)
                            {
                                Array.Resize(ref Sepid.LswAtkID, Sepid.LswAtkID.Length + 1);
                                Sepid.LswAtkID[Sepid.LswAtkID.Length - 1] = uint.Parse(val.InnerText);
                            }
                            break;
                        case "Pwp":
                            foreach (XmlElement val in el.ChildNodes)
                            {
                                Array.Resize(ref Sepid.PwpAtkID, Sepid.PwpAtkID.Length + 1);
                                Sepid.PwpAtkID[Sepid.PwpAtkID.Length - 1] = uint.Parse(val.InnerText);
                            }
                            break;
                        case "AIS":
                            foreach (XmlElement val in el.ChildNodes)
                            {
                                Array.Resize(ref Sepid.AISAtkID, Sepid.AISAtkID.Length + 1);
                                Sepid.AISAtkID[Sepid.AISAtkID.Length - 1] = uint.Parse(val.InnerText);
                            }
                            break;
                        case "Ride":
                            foreach (XmlElement val in el.ChildNodes)
                            {
                                Array.Resize(ref Sepid.RideAtkID, Sepid.RideAtkID.Length + 1);
                                Sepid.RideAtkID[Sepid.RideAtkID.Length - 1] = uint.Parse(val.InnerText);
                            }
                            break;
                        case "HTF":
                            foreach (XmlElement val in el.ChildNodes)
                            {
                                Array.Resize(ref Sepid.HTFAtkID, Sepid.HTFAtkID.Length + 1);
                                Sepid.HTFAtkID[Sepid.HTFAtkID.Length - 1] = uint.Parse(val.InnerText);
                            }
                            break;
                        case "Ignore":
                            foreach (XmlElement val in el.ChildNodes)
                            {
                                Array.Resize(ref Sepid.IgnoreAtkID, Sepid.IgnoreAtkID.Length + 1);
                                Sepid.IgnoreAtkID[Sepid.IgnoreAtkID.Length - 1] = uint.Parse(val.InnerText);
                            }
                            break;
                    }
                }
            } catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
            #endregion

            await Task.Run(() =>
            {
                foreach (string s in skills)
                {
                    string[] split = s.Split(',');
                    if (split.Length > 1)
                    {
                        skillDict.Add(uint.Parse(split[1]), split[0]);
                    }
                }
            });

        }

        private async Task Version_Check()
        {
            //new_version_check
            try
            {
                string content;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla / 5.0 OverParse / 4.0.0");
                    client.DefaultRequestHeaders.Add("Connection", "close");
                    client.Timeout = TimeSpan.FromSeconds(20.0);
                    content = await client.GetStringAsync("https://api.github.com/repos/remon-7l/overparse/releases/latest");
                }
                var m = Regex.Match(content, @"tag_name.........");
                var v = Regex.Match(m.Value, @"\d.\d.\d");
                var newVersion = Version.Parse(v.ToString());
                var nowVersion = Version.Parse("4.0.0");
                if (newVersion <= nowVersion) { updatemsg = ""; }
                if (nowVersion < newVersion)
                {
                    updatemsg = " - New version available(" + v.ToString() + ")";
                }
            } catch (HttpRequestException) { updatemsg = "Update check Error(server error)"; } catch (TaskCanceledException) { updatemsg = "Update check Error(Time-out)"; } catch (Exception) { updatemsg = "Update check Error"; }
        }

        private void HotKeyLoad()
        {
            try
            {
                hotkey1 = hotkey2 = hotkey3 = hotkey4 = hotkey5 = hotkey6 = new HotKey(this);

                hotkey1.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.E, new EventHandler(EndEncounter_Key), 0x0071);
                hotkey2.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.R, new EventHandler(EndEncounterNoLog_Key), 0x0072);
                hotkey3.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.D, new EventHandler(DefaultWindowSize_Key), 0x0073);
                hotkey4.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.A, new EventHandler(AlwaysOnTop_Key), 0x0074);
                hotkey5.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.F, new EventHandler(Capture_Key), 0x0075);
                hotkey6.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.M, new EventHandler(Bouyomi_Key), 0x0076);
#if DEBUG
                hotkey7 = new HotKey(this);
                hotkey7.Regist(ModifierKeys.Control | ModifierKeys.Shift, Key.F11, new EventHandler(DebugWindow_Key), 0x0077);
#endif
            } catch
            {
                MessageBox.Show("OverParseはホットキーを初期化出来ませんでした。　多重起動していないか確認して下さい！\nプログラムは引き続き使用できますが、ホットキーは反応しません。", "OverParse Setup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ConfigLoad()
        {
            MainWindow m = (MainWindow)Application.Current.MainWindow;
            Process thisProcess = Process.GetCurrentProcess();
            Left = Properties.Settings.Default.Left;
            Top = Properties.Settings.Default.Top;

            AutoEndEncounters.IsChecked = Properties.Settings.Default.AutoEndEncounters;
            SetEncounterTimeout.IsEnabled = Properties.Settings.Default.AutoEndEncounters;
            //LogToClipboard.IsChecked = Properties.Settings.Default.LogToClipboard;
            IsWriteTS.IsChecked = Properties.Settings.Default.IsWriteTS;
            // - - - -
            SeparateZanverse.IsChecked = Properties.Settings.Default.SeparateZanverse;
            SeparateFinish.IsChecked = Properties.Settings.Default.SeparateFinish;
            SeparateDB.IsChecked = Properties.Settings.Default.SeparateDB;
            SeparateLsw.IsChecked = Properties.Settings.Default.SeparateLsw;
            SeparatePwp.IsChecked = Properties.Settings.Default.SeparatePwp;
            SeparateAIS.IsChecked = Properties.Settings.Default.SeparateAIS;
            SeparateRide.IsChecked = Properties.Settings.Default.SeparateRide;
            Onlyme.IsChecked = Properties.Settings.Default.Onlyme;
            Nodecimal.IsChecked = Properties.Settings.Default.Nodecimal;
            QuestTime.IsChecked = Properties.Settings.Default.QuestTime;
            // - - - -
            AlwaysOnTop.IsChecked = Properties.Settings.Default.AlwaysOnTop;
            AutoHideWindow.IsChecked = Properties.Settings.Default.AutoHideWindow;
            ClickthroughMode.IsChecked = Properties.Settings.Default.ClickthroughEnabled;

            TheWindow.Opacity = Properties.Settings.Default.WindowOpacity;

            if (Properties.Settings.Default.LowResources) { thisProcess.PriorityClass = ProcessPriorityClass.Idle; }
            if (Properties.Settings.Default.CPUdraw) { RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly; }

            if (Properties.Settings.Default.BackContent == "Color")
            {
                try { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BackColor)); } catch { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }
            } else if (Properties.Settings.Default.BackContent == "Image")
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(Properties.Settings.Default.ImagePath));
                    ImageBrush brush = new ImageBrush
                    {
                        ImageSource = bitmap,
                        Stretch = Stretch.UniformToFill
                    };
                    Background = brush;
                } catch { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }
            } else { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }

            //リスト前面カラー
            System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Properties.Settings.Default.FontColor);
            CombatantData.Foreground = new SolidColorBrush(color);

            if (0 < Process.GetProcessesByName("BouyomiChan").Length)
            {
                IsConnect = true;
                BouyomiEnable.IsChecked = true;
            } else
            {
                IsConnect = false;
                BouyomiEnable.IsChecked = false;
            }

            //色を適用した後、最後に設定
            m.Background.Opacity = Properties.Settings.Default.ListOpacity;
        }

        private void LoadListColumn()
        {
            GridLength temp = new GridLength(0);
            if (!Properties.Settings.Default.ListName) { CombatantView.Columns.Remove(NameColumn); CNameHC.Width = temp; }
            if (Properties.Settings.Default.Variable)
            {
                if (Properties.Settings.Default.ListPct) { CPercentHC.Width = new GridLength(0.4, GridUnitType.Star); } else { CombatantView.Columns.Remove(PercentColumn); CPercentHC.Width = temp; }
                if (Properties.Settings.Default.ListTS) { CTScoreHC.Width = new GridLength(0.4, GridUnitType.Star); } else { CombatantView.Columns.Remove(TScoreColumn); CTScoreHC.Width = temp; }
                if (Properties.Settings.Default.ListDmg) { CDmgHC.Width = new GridLength(0.8, GridUnitType.Star); } else { CombatantView.Columns.Remove(DamageColumn); CDmgHC.Width = temp; }
                if (Properties.Settings.Default.ListDmgd) { CDmgDHC.Width = new GridLength(0.6, GridUnitType.Star); } else { CombatantView.Columns.Remove(DamagedColumn); CDmgDHC.Width = temp; }
                if (Properties.Settings.Default.ListDPS) { CDPSHC.Width = new GridLength(0.6, GridUnitType.Star); } else { CombatantView.Columns.Remove(DPSColumn); CDPSHC.Width = temp; }
                if (Properties.Settings.Default.ListJA) { CJAHC.Width = new GridLength(0.4, GridUnitType.Star); } else { CombatantView.Columns.Remove(JAColumn); CJAHC.Width = temp; }
                if (Properties.Settings.Default.ListCri) { CCriHC.Width = new GridLength(0.4, GridUnitType.Star); } else { CombatantView.Columns.Remove(CriColumn); CCriHC.Width = temp; }
                if (Properties.Settings.Default.ListHit) { CMdmgHC.Width = new GridLength(0.6, GridUnitType.Star); } else { CombatantView.Columns.Remove(HColumn); CMdmgHC.Width = temp; }
            } else
            {
                if (!Properties.Settings.Default.ListPct) { CombatantView.Columns.Remove(PercentColumn); CPercentHC.Width = temp; }
                if (!Properties.Settings.Default.ListTS) { CombatantView.Columns.Remove(TScoreColumn); CTScoreHC.Width = temp; }
                if (Properties.Settings.Default.ListDmg)
                {
                    if (Properties.Settings.Default.DamageSI) { CDmgHC.Width = new GridLength(50); }
                } else { CombatantView.Columns.Remove(DamageColumn); CDmgHC.Width = temp; }
                if (Properties.Settings.Default.ListDmgd)
                {
                    if (Properties.Settings.Default.DamagedSI) { CDmgDHC.Width = new GridLength(50); }
                } else { CombatantView.Columns.Remove(DamagedColumn); CDmgDHC.Width = temp; }
                if (Properties.Settings.Default.ListDPS)
                {
                    if (Properties.Settings.Default.DPSSI) { CDPSHC.Width = new GridLength(50); }
                } else { CombatantView.Columns.Remove(DPSColumn); CDPSHC.Width = temp; }
                if (!Properties.Settings.Default.ListJA) { CombatantView.Columns.Remove(JAColumn); CJAHC.Width = temp; }
                if (!Properties.Settings.Default.ListCri) { CombatantView.Columns.Remove(CriColumn); CCriHC.Width = temp; }
                if (Properties.Settings.Default.ListHit)
                {
                    if (Properties.Settings.Default.MaxSI) { CMdmgHC.Width = new GridLength(50); }
                } else { CombatantView.Columns.Remove(HColumn); CMdmgHC.Width = temp; }
            }
            if (!Properties.Settings.Default.ListAtk) { CombatantView.Columns.Remove(MaxHitColumn); CAtkHC.Width = temp; }
            if (!Properties.Settings.Default.ListTab) { TabHC.Width = temp; CTabHC.Width = temp; }
        }

    }
}
