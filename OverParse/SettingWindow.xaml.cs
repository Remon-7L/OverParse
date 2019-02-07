using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OverParse
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        public bool IsSetting = false;
        MainWindow m = (MainWindow)Application.Current.MainWindow;

        public SettingWindow()
        {
            InitializeComponent();
            IsSetting = true;
            Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            FontList.DataContext = Fonts.SystemFontFamilies.ToList();

            IP.Text = Properties.Settings.Default.BouyomiIP;
            FontSizeBox.Content = Properties.Settings.Default.FontSize.ToString("N1");

            // - - - -

            if (Properties.Settings.Default.Language == "ja-JP") { JA.IsChecked = true; } else if (Properties.Settings.Default.Language == "en-US") { EN.IsChecked = true; }
            // else if (Properties.Settings.Default.Language == "zh-TW") { TW.IsChecked = true; }
            // - - - -

            if (Properties.Settings.Default.BackContent == "Color") { RadioColor.IsChecked = true; } else if (Properties.Settings.Default.BackContent == "Image") { RadioImage.IsChecked = true; }
            BackColorInput.Content = Properties.Settings.Default.BackColor;
            BackPreview.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BackColor));
            TextColorBox.Content = Properties.Settings.Default.FontColor;
            TextColorBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.FontColor));

            ForegroundUIColor.Content = Properties.Settings.Default.Foreground;
            ForegroundUIColor.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.Foreground));
            PathLabel.Content = "Path : " + Properties.Settings.Default.ImagePath;
            if (File.Exists(Properties.Settings.Default.ImagePath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(Properties.Settings.Default.ImagePath));
                    PreviewImage.Source = bitmap;
                } catch { }
            } else { PathLabel.Content = "Image directory path Error"; }


            //UpdateInv Slider Loading
            if (Properties.Settings.Default.Updateinv < 1000)
            {
                ChangeInv.Value = Properties.Settings.Default.Updateinv / 50;
            } else if (1000 <= Properties.Settings.Default.Updateinv)
            {
                ChangeInv.Value = (Properties.Settings.Default.Updateinv - 1000) / 500 + 11;
            }
            ChangeInvResult.Content = Properties.Settings.Default.Updateinv + "ms";

        }

        #region FontTab
        private void FontList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontList.Items.Count < 1) { return; }
            Properties.Settings.Default.Font = FontList.SelectedItem.ToString();
            m.CombatantData.FontFamily = (FontFamily)FontList.SelectedItem;
        }

        private void FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSetting)
            {
                Properties.Settings.Default.FontSize = Math.Round(FontSizeSlider.Value, 1);
                m.CombatantData.FontSize = Properties.Settings.Default.FontSize;
                FontSizeBox.Content = Properties.Settings.Default.FontSize;
            }
        }

        private void JA_Checked(object sender, RoutedEventArgs e) => Properties.Settings.Default.Language = "ja-JP";
        private void EN_Checked(object sender, RoutedEventArgs e) => Properties.Settings.Default.Language = "en-US";
        #endregion

        #region BackgroundTab
        private void RadioColor_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackContent = "Color";
            m.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BackColor))
            {
                Opacity = Properties.Settings.Default.ListOpacity
            };
        }

        private void BackColor_Click(object sender, RoutedEventArgs e)
        {
            SelectColor color = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BackColor)) { Owner = this };
            color.ShowDialog();
            if (color.DialogResult == true)
            {
                BackColorInput.Content = color.ResultColor.ToString();
                BackPreview.Fill = new SolidColorBrush(color.ResultColor);
                Properties.Settings.Default.BackColor = color.ResultColor.ToString();
                if (Properties.Settings.Default.BackContent == "Color") { m.Background = new SolidColorBrush(color.ResultColor); }
            }
        }

        private void RadioImage_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackContent = "Image";
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(Properties.Settings.Default.ImagePath));
                ImageBrush brush = new ImageBrush
                {
                    ImageSource = bitmap,
                    Stretch = Stretch.UniformToFill
                };
                m.Background = brush;
                m.Background.Opacity = Properties.Settings.Default.ListOpacity;
            } catch { m.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }
        }

        private void ImageSelect_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image(*.png,*.jpg,*.jpeg,*.gif,*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp"
            };
            if (dialog.ShowDialog() != true) { return; }
            Properties.Settings.Default.ImagePath = dialog.FileName;
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(Properties.Settings.Default.ImagePath));
                ImageBrush brush = new ImageBrush
                {
                    ImageSource = bitmap,
                    Stretch = Stretch.UniformToFill
                };
                m.Background = brush;
                PreviewImage.Source = bitmap;
                PathLabel.Content = "Path : " + Properties.Settings.Default.ImagePath;
            } catch { m.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }
        }

        private void WindowOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSetting)
            {
                m.TheWindow.Opacity = WindowOpacity.Value;
                Properties.Settings.Default.WindowOpacity = WindowOpacity.Value;
            }
        }

        private void BackOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSetting)
            {
                m.Background.Opacity = BackOpacity.Value;
                Properties.Settings.Default.ListOpacity = BackOpacity.Value;
            }
        }

        private void Graph_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.ShowDamageGraph = Graph.IsChecked;
        private void Highlight_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.HighlightYourDamage = Highlight.IsChecked;
        #endregion

        #region ColorTab
        private void UIColor_Click(object sender, RoutedEventArgs e)
        {
            SelectColor uicolor = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.Foreground)) { Owner = this };
            uicolor.ShowDialog();
            if (uicolor.DialogResult == true)
            {
                ForegroundUIColor.Content = uicolor.ResultColor.ToString();
                ForegroundUIColor.Foreground = new SolidColorBrush(uicolor.ResultColor);
                Properties.Settings.Default.Foreground = uicolor.ResultColor.ToString();
                m.BindingGroup.UpdateSources();
            }
        }

        private void ListColor_Click(object sender, RoutedEventArgs e)
        {
            SelectColor listcolor = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.FontColor)) { Owner = this };
            listcolor.ShowDialog();
            if (listcolor.DialogResult == true)
            {
                TextColorBox.Content = listcolor.ResultColor.ToString();
                TextColorBox.Foreground = new SolidColorBrush(listcolor.ResultColor);
                m.CombatantData.Foreground = new SolidColorBrush(listcolor.ResultColor);
                Properties.Settings.Default.FontColor = listcolor.ResultColor.ToString();
            }
        }
        #endregion

        #region ColumnTab
        private void DamageSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DamageSI = DamageSI.IsChecked;
            if (Properties.Settings.Default.ListDmg)
            {
                if (Properties.Settings.Default.DamageSI) { m.CDmgHC.Width = new GridLength(47); } else { m.CDmgHC.Width = new GridLength(78); }
            } else { m.CombatantView.Columns.Remove(m.DamageColumn); m.CDmgHC.Width = new GridLength(0); }
        }

        private void DamagedSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DamagedSI = DamagedSI.IsChecked;
            if (Properties.Settings.Default.ListDmgd)
            {
                if (Properties.Settings.Default.DamagedSI) { m.DmgDHC.Width = new GridLength(47); } else { m.DmgDHC.Width = new GridLength(78); }
            } else { m.CombatantView.Columns.Remove(m.DamagedColumn); m.CDmgDHC.Width = new GridLength(0); }
        }

        private void DPSSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DPSSI = DPSSI.IsChecked;
            if (Properties.Settings.Default.ListDPS)
            {
                if (Properties.Settings.Default.DPSSI) { m.DPSHC.Width = new GridLength(47); } else { m.DPSHC.Width = new GridLength(56); }
            } else { m.CombatantView.Columns.Remove(m.DPSColumn); m.DPSHC.Width = new GridLength(0); }
        }

        private void MaxSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MaxSI = MaxSI.IsChecked;
            if (Properties.Settings.Default.ListHit)
            {
                if (Properties.Settings.Default.MaxSI) { m.CDmgHC.Width = new GridLength(47); } else { m.CDmgHC.Width = new GridLength(62); }
            } else { m.CombatantView.Columns.Remove(m.HColumn); m.CMdmgHC.Width = new GridLength(0); }
        }
        #endregion

        #region TTSTab
        private void Bouyomi_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.Bouyomi = Bouyomi.IsChecked;
        private void BouyomiFormat_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.BouyomiFormat = BouyomiFormat.IsChecked;

        private void IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsSetting)
            {
                if (System.Net.IPAddress.TryParse(IP.Text, out System.Net.IPAddress addr))
                {
                    IPResult.Content = "OK"; IPResult.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    Properties.Settings.Default.BouyomiIP = addr.ToString();
                    ResultURI.Content = addr.ToString() + " : " + Properties.Settings.Default.BouyomiPort;
                } else
                {
                    IPResult.Content = "NG"; IPResult.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }
            }
        }
        #endregion

        #region SystemTab
        private void ChangeInv_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSetting)
            {
                if ((int)ChangeInv.Value < 11)
                {
                    Properties.Settings.Default.Updateinv = (int)ChangeInv.Value * 50;
                } else if (11 <= (int)ChangeInv.Value)
                {
                    Properties.Settings.Default.Updateinv = 1000 + ((int)ChangeInv.Value - 11) * 500;
                }
                m.damageTimer.Interval = new TimeSpan(0, 0, 0, 0, Properties.Settings.Default.Updateinv);
                ChangeInvResult.Content = Properties.Settings.Default.Updateinv + "ms";
            }
        }

        private void LowResources_Click(object sender, RoutedEventArgs e)
        {
            Process thisProcess = Process.GetCurrentProcess();
            Properties.Settings.Default.LowResources = LowResources.IsChecked;
            if (Properties.Settings.Default.LowResources)
            {
                thisProcess.PriorityClass = ProcessPriorityClass.Idle;
            } else
            {
                thisProcess.PriorityClass = ProcessPriorityClass.Normal;
            }
        }

        private void CPUdraw_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CPUdraw = CPUdraw.IsChecked;
            if (Properties.Settings.Default.CPUdraw)
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            } else
            {
                RenderOptions.ProcessRenderMode = RenderMode.Default;
            }
        }

        private void Clock_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Clock = Clock.IsChecked;
            if (Properties.Settings.Default.Clock) { m.Datetime.Visibility = Visibility.Visible; } else { m.Datetime.Visibility = Visibility.Collapsed; }
        }

        private void OpenAppData(object sender, RoutedEventArgs e) => Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"/OverParse/");

        private void LowResources_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
            Description.Text = "CPUの基本割り込み処理優先度を下げ、他のプロセスへ処理リソースを譲渡します。\n処理が追いついている(アイドル時間が存在している)場合は影響ありません\nOverParseの処理が止まる場合があります。";

        private void CPUdraw_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
            Description.Text = "強制的にソフトウェアレンダリングへ切り替えます。\n画面出力がdGPUの場合は画面合成時にCPU=>GPUへの転送が発生する為逆効果になります";

        private void Clock_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
            Description.Text = "デバッグ用\nUpdateForm();が発生したタイミングで現在のPC時刻を表示します";

        private void AppData_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
            Description.Text = "設定をリセットする場合はOverParseを終了させてからuser.configを削除して下さい\nIf need reset OverParse, App close and delete user.config";
        #endregion

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            if (FontList.SelectedItem != null) { Properties.Settings.Default.Font = FontList.SelectedItem.ToString(); }

            //Column Settings
            GridLength temp = new GridLength(0);
            m.CombatantView.Columns.Clear();
            if (PlayerName.IsChecked) { m.CombatantView.Columns.Add(m.NameColumn); m.CNameHC.Width = new GridLength(1, GridUnitType.Star); } else { m.CNameHC.Width = temp; }
            if (Variable.IsChecked)
            {
                if (Percent.IsChecked) { m.CombatantView.Columns.Add(m.PercentColumn); m.CPercentHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CPercentHC.Width = temp; }
                if (TScore.IsChecked) { m.CombatantView.Columns.Add(m.TScoreColumn); m.CTScoreHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CPercentHC.Width = temp; }
                if (Damage.IsChecked) { m.CombatantView.Columns.Add(m.DamageColumn); m.CDmgHC.Width = new GridLength(0.8, GridUnitType.Star); } else { m.CDmgHC.Width = temp; }
                if (Damaged.IsChecked) { m.CombatantView.Columns.Add(m.DamagedColumn); m.CDmgDHC.Width = new GridLength(0.6, GridUnitType.Star); } else { m.CDmgDHC.Width = temp; }
                if (PlayerDPS.IsChecked) { m.CombatantView.Columns.Add(m.DPSColumn); m.CDPSHC.Width = new GridLength(0.6, GridUnitType.Star); } else { m.CDPSHC.Width = temp; }
                if (PlayerJA.IsChecked) { m.CombatantView.Columns.Add(m.JAColumn); m.CJAHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CJAHC.Width = temp; }
                if (Critical.IsChecked) { m.CombatantView.Columns.Add(m.CriColumn); m.CCriHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CCriHC.Width = temp; }
                if (MaxHit.IsChecked) { m.CombatantView.Columns.Add(m.HColumn); m.CMdmgHC.Width = new GridLength(0.6, GridUnitType.Star); } else { m.CMdmgHC.Width = temp; }
            } else
            {
                if (Percent.IsChecked) { m.CombatantView.Columns.Add(m.PercentColumn); m.CPercentHC.Width = new GridLength(39.0); } else { m.CPercentHC.Width = temp; }
                if (TScore.IsChecked) { m.CombatantView.Columns.Add(m.TScoreColumn); m.CTScoreHC.Width = new GridLength(39.0); } else { m.CTScoreHC.Width = temp; }
                if (Damage.IsChecked) { m.CombatantView.Columns.Add(m.DamageColumn); m.CDmgHC.Width = new GridLength(78.0); } else { m.CDmgHC.Width = temp; }
                if (Damaged.IsChecked) { m.CombatantView.Columns.Add(m.DamagedColumn); m.CDmgDHC.Width = new GridLength(56.0); } else { m.CDmgDHC.Width = temp; }
                if (PlayerDPS.IsChecked) { m.CombatantView.Columns.Add(m.DPSColumn); m.CDPSHC.Width = new GridLength(56.0); } else { m.CDPSHC.Width = temp; }
                if (PlayerJA.IsChecked) { m.CombatantView.Columns.Add(m.JAColumn); m.CJAHC.Width = new GridLength(39.0); } else { m.CJAHC.Width = temp; }
                if (Critical.IsChecked) { m.CombatantView.Columns.Add(m.CriColumn); m.CCriHC.Width = new GridLength(39.0); } else { m.CCriHC.Width = temp; }
                if (MaxHit.IsChecked) { m.CombatantView.Columns.Add(m.HColumn); m.CMdmgHC.Width = new GridLength(62.0); } else { m.CMdmgHC.Width = temp; }
            }
            if (AtkName.IsChecked) { m.CombatantView.Columns.Add(m.MaxHitColumn); m.CAtkHC.Width = new GridLength(1.7, GridUnitType.Star); } else { m.CAtkHC.Width = temp; }
            if (Tabchk.IsChecked) { m.TabHC.Width = new GridLength(30.0); m.CTabHC.Width = new GridLength(30.0); } else { m.TabHC.Width = temp; m.CTabHC.Width = temp; }
            Properties.Settings.Default.ListName = PlayerName.IsChecked;
            Properties.Settings.Default.ListPct = Percent.IsChecked;
            Properties.Settings.Default.ListTS = TScore.IsChecked;
            Properties.Settings.Default.ListDmg = Damage.IsChecked;
            Properties.Settings.Default.ListDmgd = Damaged.IsChecked;
            Properties.Settings.Default.ListDPS = PlayerDPS.IsChecked;
            Properties.Settings.Default.ListJA = PlayerJA.IsChecked;
            Properties.Settings.Default.ListCri = Critical.IsChecked;
            Properties.Settings.Default.ListHit = MaxHit.IsChecked;
            Properties.Settings.Default.ListAtk = AtkName.IsChecked;
            Properties.Settings.Default.ListTab = Tabchk.IsChecked;
            Properties.Settings.Default.Variable = Variable.IsChecked;
            // - - - -

            Properties.Settings.Default.Save();
            DialogResult = true;
        }

        private void GitHub_Click(object sender, RoutedEventArgs e) => Process.Start("https://github.com/Remon-7L");
        private void Twitter_Click(object sender, RoutedEventArgs e) => Process.Start("https://twitter.com/Remon_7L");

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) { DragMove(); }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);
    }

    public class FontNameConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = value as FontFamily;
            var currentLang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            return v.FamilyNames.FirstOrDefault(o => o.Key == currentLang).Value ?? v.Source;
        }

        public object ConvertBack(object value, Type type, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }

}
