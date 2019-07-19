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

namespace Ov3rD4r53
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : YKToolkit.Controls.Window
    {
        public bool IsSetting = false;
        MainWindow m = (MainWindow)Application.Current.MainWindow;

        public SettingWindow()
        {
            InitializeComponent();

            IsSetting = true;
            Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            FontList.DataContext = Fonts.SystemFontFamilies.ToList();
            FontList.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(x => x.Source == Properties.Settings.Default.Font);
            FontList.ScrollIntoView(Fonts.SystemFontFamilies.FirstOrDefault(x => x.Source == Properties.Settings.Default.Font));

            IsSetting = false;
            OverlayFont.DataContext = Fonts.SystemFontFamilies.ToList();
            OverlayFont.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(x => x.Source == Properties.Settings.Default.OverlayFont);
            IsSetting = true;

            IP.Text = Properties.Settings.Default.BouyomiIP;
            FontSizeBox.Content = Properties.Settings.Default.FontSize.ToString("N1");

            // - - - - LoadUI
            if (Properties.Settings.Default.Language == "ja-JP") { JA.IsChecked = true; }
            else if (Properties.Settings.Default.Language == "zh-TW") { TWHK.IsChecked = true; }
            else if (Properties.Settings.Default.Language == "en-US") { EN.IsChecked = true; }

            // - - - -

            if (Properties.Settings.Default.BackContent == "Color") { RadioColor.IsChecked = true; }
            else if (Properties.Settings.Default.BackContent == "Image") { RadioImage.IsChecked = true; }
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
                }
                catch { }
            }
            else { PathLabel.Content = "Image directory path Error"; }


            //UpdateInv Slider Loading
            if (Properties.Settings.Default.Updateinv < 1000)
            {
                ChangeInv.Value = Properties.Settings.Default.Updateinv / 50;
            }
            else if (1000 <= Properties.Settings.Default.Updateinv)
            {
                ChangeInv.Value = (Properties.Settings.Default.Updateinv - 1000) / 500 + 11;
            }
            ChangeInvResult.Content = Properties.Settings.Default.Updateinv + "ms";

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingTab.SelectedIndex == 4)
            {
                OverlayFont.ScrollIntoView(Fonts.SystemFontFamilies.FirstOrDefault(x => x.Source == Properties.Settings.Default.OverlayFont));
            }
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
        private void TWHK_Checked(object sender, RoutedEventArgs e) => Properties.Settings.Default.Language = "zh-TW";
        #endregion

        #region BackgroundTab
        private void RadioColor_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BackContent = "Color";
            m.ContentBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.BackColor))
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
                if (Properties.Settings.Default.BackContent == "Color") { m.ContentBackground = new SolidColorBrush(color.ResultColor); }
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
                m.ContentBackground = brush;
                m.ContentBackground.Opacity = Properties.Settings.Default.ListOpacity;
            }
            catch { m.ContentBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }
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
                m.ContentBackground = brush;
                PreviewImage.Source = bitmap;
                PathLabel.Content = "Path : " + Properties.Settings.Default.ImagePath;
            }
            catch { m.ContentBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0A0A0A")); }
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
                m.ContentBackground.Opacity = BackOpacity.Value;
                Properties.Settings.Default.ListOpacity = BackOpacity.Value;
            }
        }
        #endregion

        #region ColorTab

        #region Brush
        /* 後でメソッドとかに置き換える */


        private void MyBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor mybrushcol = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.MyColorBrush)) { Owner = this };
            mybrushcol.ShowDialog();
            if (mybrushcol.DialogResult == true)
            {
                MyBrush.Content = mybrushcol.ResultColor.ToString();
                MyBrush.Foreground = new SolidColorBrush(mybrushcol.ResultColor);
                MainWindow.MyColor = mybrushcol.ResultColor;
                Properties.Settings.Default.MyColorBrush = mybrushcol.ResultColor.ToString();
            }
        }

        private void OddLeftBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor oddleft = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.OddLeftColor)) { Owner = this };
            oddleft.ShowDialog();
            if (oddleft.DialogResult == true)
            {
                OddLeftBrush.Content = oddleft.ResultColor.ToString();
                OddLeftBrush.Foreground = new SolidColorBrush(oddleft.ResultColor);
                MainWindow.OddLeft = oddleft.ResultColor;
                Properties.Settings.Default.OddLeftColor = oddleft.ResultColor.ToString();
            }
        }

        private void EveLeftBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor eveleft = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.EveLeftColor)) { Owner = this };
            eveleft.ShowDialog();
            if (eveleft.DialogResult == true)
            {
                EveLeftBrush.Content = eveleft.ResultColor.ToString();
                EveLeftBrush.Foreground = new SolidColorBrush(eveleft.ResultColor);
                MainWindow.EveLeft = eveleft.ResultColor;
                Properties.Settings.Default.EveLeftColor = eveleft.ResultColor.ToString();
            }
        }

        private void OddRgtBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor oddrgt = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.OddRgtColor)) { Owner = this };
            oddrgt.ShowDialog();
            if (oddrgt.DialogResult == true)
            {
                OddRgtBrush.Content = oddrgt.ResultColor.ToString();
                OddRgtBrush.Foreground = new SolidColorBrush(oddrgt.ResultColor);
                MainWindow.OddRgt = oddrgt.ResultColor;
                Properties.Settings.Default.OddRgtColor = oddrgt.ResultColor.ToString();
            }
        }

        private void EveRgtBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor evergt = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.EveRgtColor)) { Owner = this };
            evergt.ShowDialog();
            if (evergt.DialogResult == true)
            {
                EveRgtBrush.Content = evergt.ResultColor.ToString();
                EveRgtBrush.Foreground = new SolidColorBrush(evergt.ResultColor);
                MainWindow.EveRgt = evergt.ResultColor;
                Properties.Settings.Default.EveRgtColor = evergt.ResultColor.ToString();
            }
        }

        private void OtherBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor other = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.OtherBrush)) { Owner = this };
            other.ShowDialog();
            if (other.DialogResult == true)
            {
                OtherBrush.Content = other.ResultColor.ToString();
                OtherBrush.Foreground = new SolidColorBrush(other.ResultColor);
                MainWindow.Other = other.ResultColor;
                Properties.Settings.Default.OtherBrush = other.ResultColor.ToString();
            }
        }

        #endregion Brush

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

        private void IsGraph_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsShowGraph = (bool)IsGraphBtn.IsChecked;
            MainWindow.IsShowGraph = (bool)IsGraphBtn.IsChecked;
        }

        private void IsHighLight_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsGraphHighLight = (bool)IsHighLightBtn.IsChecked;
            MainWindow.IsHighlight = (bool)IsHighLightBtn.IsChecked;
        }


        #endregion ColorTab

        #region ColumnTab
        private void DamageSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DamageSI = (bool)DamageSI.IsChecked;
            if (Properties.Settings.Default.ListDmg)
            {
                if (Properties.Settings.Default.DamageSI) { m.CDmgHC.Width = new GridLength(47); } else { m.CDmgHC.Width = new GridLength(78); }
            }
            else { m.CombatantView.Columns.Remove(m.DamageColumn); m.CDmgHC.Width = new GridLength(0); }
        }

        private void DamagedSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DamagedSI = (bool)DamagedSI.IsChecked;
            if (Properties.Settings.Default.ListDmgd)
            {
                if (Properties.Settings.Default.DamagedSI) { m.DmgDHC.Width = new GridLength(47); } else { m.DmgDHC.Width = new GridLength(78); }
            }
            else { m.CombatantView.Columns.Remove(m.DamagedColumn); m.CDmgDHC.Width = new GridLength(0); }
        }

        private void DPSSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DPSSI = (bool)DPSSI.IsChecked;
            if (Properties.Settings.Default.ListDPS)
            {
                if (Properties.Settings.Default.DPSSI) { m.DPSHC.Width = new GridLength(47); } else { m.DPSHC.Width = new GridLength(56); }
            }
            else { m.CombatantView.Columns.Remove(m.DPSColumn); m.DPSHC.Width = new GridLength(0); }
        }

        private void MaxSI_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MaxSI = (bool)MaxSI.IsChecked;
            if (Properties.Settings.Default.ListHit)
            {
                if (Properties.Settings.Default.MaxSI) { m.CDmgHC.Width = new GridLength(47); } else { m.CDmgHC.Width = new GridLength(62); }
            }
            else { m.CombatantView.Columns.Remove(m.HColumn); m.CMdmgHC.Width = new GridLength(0); }
        }
        #endregion

        #region OverlayTab
        private void OverlayFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontList.Items.Count < 1) { return; }
            Properties.Settings.Default.OverlayFont = OverlayFont.SelectedItem.ToString();
            if (MainWindow.overlay != null && MainWindow.overlay.IsLoaded) { MainWindow.overlay.FontFamily = (FontFamily)FontList.SelectedItem; }
        }

        private void OverlayHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSetting)
            {
                double vle;
                vle = Math.Round(OverlayHeight.Value);
                if (MainWindow.overlay != null && MainWindow.overlay.IsLoaded) { MainWindow.overlay.Height = vle; }
                Properties.Settings.Default.OverlayHeight = vle;
            }
        }

        private void OverlayWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsSetting)
            {
                double vle;
                vle = Math.Round(OverlayWidth.Value);
                if (MainWindow.overlay != null && MainWindow.overlay.IsLoaded) { MainWindow.overlay.Width = vle; }
                Properties.Settings.Default.OverlayWidth = vle;
            }
        }

        #region Brush
        private void OverlayBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor brush = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.OverlayBrush)) { Owner = this };
            brush.ShowDialog();
            if (brush.DialogResult == true)
            {
                OverlayAllBrush.Content = brush.ResultColor.ToString();
                OverlayAllBrush.Foreground = new SolidColorBrush(brush.ResultColor);
                if (MainWindow.overlay != null && MainWindow.overlay.IsLoaded) { MainWindow.overlay.Foreground = new SolidColorBrush(brush.ResultColor); }
                Properties.Settings.Default.OverlayBrush = brush.ResultColor.ToString();
            }
        }

        private void OverlayWinBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor brush = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.WinDiffBrush)) { Owner = this };
            brush.ShowDialog();
            if (brush.DialogResult == true)
            {
                WinDiffBrush.Content = brush.ResultColor.ToString();
                WinDiffBrush.Foreground = new SolidColorBrush(brush.ResultColor);
                Overlay.WinBrush = brush.ResultColor;
                Properties.Settings.Default.WinDiffBrush = brush.ResultColor.ToString();
            }
        }

        private void OverlayLoseBrush_Click(object sender, RoutedEventArgs e)
        {
            SelectColor brush = new SelectColor((Color)ColorConverter.ConvertFromString(Properties.Settings.Default.LoseDiffBrush)) { Owner = this };
            brush.ShowDialog();
            if (brush.DialogResult == true)
            {
                LoseDiffBrush.Content = brush.ResultColor.ToString();
                LoseDiffBrush.Foreground = new SolidColorBrush(brush.ResultColor);
                Overlay.LoseBrush = brush.ResultColor;
                Properties.Settings.Default.LoseDiffBrush = brush.ResultColor.ToString();
            }
        }

        #endregion Brush

        #endregion OverlayTab

        #region TTSTab
        private void Bouyomi_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.Bouyomi = (bool)Bouyomi.IsChecked;
        private void BouyomiFormat_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.BouyomiFormat = (bool)BouyomiFormat.IsChecked;

        private void IP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsSetting)
            {
                if (System.Net.IPAddress.TryParse(IP.Text, out System.Net.IPAddress addr))
                {
                    IPResult.Content = "OK"; IPResult.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    Properties.Settings.Default.BouyomiIP = addr.ToString();
                    ResultURI.Content = addr.ToString() + " : " + Properties.Settings.Default.BouyomiPort;
                }
                else
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
                }
                else if (11 <= (int)ChangeInv.Value)
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
            Properties.Settings.Default.LowResources = (bool)LowResources.IsChecked;
            if (Properties.Settings.Default.LowResources)
            {
                thisProcess.PriorityClass = ProcessPriorityClass.Idle;
            }
            else
            {
                thisProcess.PriorityClass = ProcessPriorityClass.Normal;
            }
        }

        private void CPUdraw_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CPUdraw = (bool)CPUdraw.IsChecked;
            if (Properties.Settings.Default.CPUdraw)
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
            else
            {
                RenderOptions.ProcessRenderMode = RenderMode.Default;
            }
        }

        private void Clock_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Clock = (bool)Clock.IsChecked;
            m.Datetime.Visibility = Properties.Settings.Default.Clock ? Visibility.Visible : Visibility.Collapsed;
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Urlbox.Text == "Add Other...")
            {
                Urlbox.Text = "";
                Urlbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
        }
        #endregion

        private void OK_button_Click(object sender, RoutedEventArgs e)
        {
            if (FontList.SelectedItem != null) { Properties.Settings.Default.Font = FontList.SelectedItem.ToString(); }

            //Column Settings
            GridLength temp = new GridLength(0);
            m.CombatantView.Columns.Clear();
            if ((bool)PlayerName.IsChecked) { m.CombatantView.Columns.Add(m.NameColumn); m.CNameHC.Width = new GridLength(1, GridUnitType.Star); } else { m.CNameHC.Width = temp; }
            if ((bool)Variable.IsChecked)
            {
                if ((bool)Percent.IsChecked) { m.CombatantView.Columns.Add(m.PercentColumn); m.CPercentHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CPercentHC.Width = temp; }
                if ((bool)TScore.IsChecked) { m.CombatantView.Columns.Add(m.TScoreColumn); m.CTScoreHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CPercentHC.Width = temp; }
                if ((bool)Damage.IsChecked) { m.CombatantView.Columns.Add(m.DamageColumn); m.CDmgHC.Width = new GridLength(0.8, GridUnitType.Star); } else { m.CDmgHC.Width = temp; }
                if ((bool)Damaged.IsChecked) { m.CombatantView.Columns.Add(m.DamagedColumn); m.CDmgDHC.Width = new GridLength(0.6, GridUnitType.Star); } else { m.CDmgDHC.Width = temp; }
                if ((bool)PlayerDPS.IsChecked) { m.CombatantView.Columns.Add(m.DPSColumn); m.CDPSHC.Width = new GridLength(0.6, GridUnitType.Star); } else { m.CDPSHC.Width = temp; }
                if ((bool)PlayerJA.IsChecked) { m.CombatantView.Columns.Add(m.JAColumn); m.CJAHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CJAHC.Width = temp; }
                if ((bool)Critical.IsChecked) { m.CombatantView.Columns.Add(m.CriColumn); m.CCriHC.Width = new GridLength(0.4, GridUnitType.Star); } else { m.CCriHC.Width = temp; }
                if ((bool)MaxHit.IsChecked) { m.CombatantView.Columns.Add(m.HColumn); m.CMdmgHC.Width = new GridLength(0.6, GridUnitType.Star); } else { m.CMdmgHC.Width = temp; }
            }
            else
            {
                if ((bool)Percent.IsChecked) { m.CombatantView.Columns.Add(m.PercentColumn); m.CPercentHC.Width = new GridLength(39.0); } else { m.CPercentHC.Width = temp; }
                if ((bool)TScore.IsChecked) { m.CombatantView.Columns.Add(m.TScoreColumn); m.CTScoreHC.Width = new GridLength(39.0); } else { m.CTScoreHC.Width = temp; }
                if ((bool)Damage.IsChecked) { m.CombatantView.Columns.Add(m.DamageColumn); m.CDmgHC.Width = new GridLength(78.0); } else { m.CDmgHC.Width = temp; }
                if ((bool)Damaged.IsChecked) { m.CombatantView.Columns.Add(m.DamagedColumn); m.CDmgDHC.Width = new GridLength(56.0); } else { m.CDmgDHC.Width = temp; }
                if ((bool)PlayerDPS.IsChecked) { m.CombatantView.Columns.Add(m.DPSColumn); m.CDPSHC.Width = new GridLength(56.0); } else { m.CDPSHC.Width = temp; }
                if ((bool)PlayerJA.IsChecked) { m.CombatantView.Columns.Add(m.JAColumn); m.CJAHC.Width = new GridLength(39.0); } else { m.CJAHC.Width = temp; }
                if ((bool)Critical.IsChecked) { m.CombatantView.Columns.Add(m.CriColumn); m.CCriHC.Width = new GridLength(39.0); } else { m.CCriHC.Width = temp; }
                if ((bool)MaxHit.IsChecked) { m.CombatantView.Columns.Add(m.HColumn); m.CMdmgHC.Width = new GridLength(62.0); } else { m.CMdmgHC.Width = temp; }
            }
            if ((bool)AtkName.IsChecked) { m.CombatantView.Columns.Add(m.MaxHitColumn); m.CAtkHC.Width = new GridLength(1.7, GridUnitType.Star); } else { m.CAtkHC.Width = temp; }
            if ((bool)Tabchk.IsChecked) { m.TabHC.Width = new GridLength(30.0); m.CTabHC.Width = new GridLength(30.0); } else { m.TabHC.Width = temp; m.CTabHC.Width = temp; }
            Properties.Settings.Default.ListName = (bool)PlayerName.IsChecked;
            Properties.Settings.Default.ListPct = (bool)Percent.IsChecked;
            Properties.Settings.Default.ListTS = (bool)TScore.IsChecked;
            Properties.Settings.Default.ListDmg = (bool)Damage.IsChecked;
            Properties.Settings.Default.ListDmgd = (bool)Damaged.IsChecked;
            Properties.Settings.Default.ListDPS = (bool)PlayerDPS.IsChecked;
            Properties.Settings.Default.ListJA = (bool)PlayerJA.IsChecked;
            Properties.Settings.Default.ListCri = (bool)Critical.IsChecked;
            Properties.Settings.Default.ListHit = (bool)MaxHit.IsChecked;
            Properties.Settings.Default.ListAtk = (bool)AtkName.IsChecked;
            Properties.Settings.Default.ListTab = (bool)Tabchk.IsChecked;
            Properties.Settings.Default.Variable = (bool)Variable.IsChecked;
            // - - - -

            Properties.Settings.Default.Save();
            DialogResult = true;
        }

        private void GitHub_Click(object sender, RoutedEventArgs e) => Process.Start("https://github.com/TyroneSama/OverParse/network/members");

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
