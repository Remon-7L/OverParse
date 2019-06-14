using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ov3rD4r53
{
    /// <summary>
    /// SelectColor.xaml の相互作用ロジック
    /// https://github.com/drogoganor/ColorPickerWPF
    /// </summary>
    public partial class SelectColor : Window // TODO: ARGB / 00xxxxxx
    {
        public Color ResultColor;
        public delegate void ColorPickerChangeHandler(Color color);
        public event ColorPickerChangeHandler OnPickColor;
        public bool IsSettingValues = true;

        public SelectColor(Color defcolor)
        {
            InitializeComponent();
            ResultColor = defcolor;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => SetColor(ResultColor);

        public void SetColor(Color color)
        {
            IsSettingValues = true;
            ResultColor = color;
            RSlider.Value = ResultColor.R;
            GSlider.Value = ResultColor.G;
            BSlider.Value = ResultColor.B;
            ASlider.Value = ResultColor.A;
            Alpha10.Text = ResultColor.A.ToString();
            Alpha16.Text = ResultColor.A.ToString("X");
            Red10.Text = ResultColor.R.ToString();
            Red16.Text = ResultColor.R.ToString("X");
            Green10.Text = ResultColor.G.ToString();
            Green16.Text = ResultColor.G.ToString("X");
            Blue10.Text = ResultColor.B.ToString();
            Blue16.Text = ResultColor.B.ToString("X");
            ColorDisplayBorder.Fill = new SolidColorBrush(ResultColor);
            ARGBLabel.Content = "ARGB(" + ResultColor.A + "," + ResultColor.R + "," + ResultColor.G + "," + ResultColor.B + ")";
            ColorCode.Content = ResultColor.ToString();
            IsSettingValues = false;
            OnPickColor?.Invoke(color);
        }

        protected void SampleImageClick(BitmapSource img, Point pos)
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/82a5731e-e201-4aaf-8d4b-062b138338fe/getting-pixel-information-from-a-bitmapimage?forum=wpf

            int stride = (int)img.Width * 4;
            int size = (int)img.Height * stride;
            byte[] pixels = new byte[size];

            img.CopyPixels(pixels, stride, 0);

            // Get pixel
            var x = (int)pos.X;
            var y = (int)pos.Y;

            int index = y * stride + 4 * x;

            byte red = pixels[index];
            byte green = pixels[index + 1];
            byte blue = pixels[index + 2];
            byte alpha = pixels[index + 3];

            var color = Color.FromArgb(alpha, blue, green, red);
            SetColor(color);
        }

        private void SampleImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            MouseMove += ColorPickerControl_MouseMove;
            MouseUp += ColorPickerControl_MouseUp;
        }

        private void ColorPickerControl_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(SampleImage);
            var img = SampleImage.Source as BitmapSource;

            if (pos.X > 0 && pos.Y > 0 && pos.X < img.PixelWidth && pos.Y < img.PixelHeight)
            {
                SampleImageClick(img, pos);
            }
        }

        private void ColorPickerControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            MouseMove -= ColorPickerControl_MouseMove;
            MouseUp -= ColorPickerControl_MouseUp;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource != SampleImage) { DragMove(); } }

        private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

        private void ASlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsSettingValues)
            {
                ResultColor.A = (byte)ASlider.Value;
                SetColor(ResultColor);
            }
        }

        private void RSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsSettingValues)
            {
                ResultColor.R = (byte)RSlider.Value;
                SetColor(ResultColor);
            }
        }

        private void GSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsSettingValues)
            {
                ResultColor.G = (byte)GSlider.Value;
                SetColor(ResultColor);
            }
        }

        private void BSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsSettingValues)
            {
                ResultColor.B = (byte)BSlider.Value;
                SetColor(ResultColor);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        private void Alpha10_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Alpha10.Text, out byte alpha))
            {
                ResultColor.A = alpha;
                SetColor(ResultColor);
            }
        }

        private void Alpha16_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Alpha16.Text,System.Globalization.NumberStyles.HexNumber, null, out byte alpha))
            {
                ResultColor.A = alpha;
                SetColor(ResultColor);
            }
        }

        private void Red10_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Red10.Text, out byte red))
            {
                ResultColor.R = red;
                SetColor(ResultColor);
            }
        }

        private void Red16_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Red16.Text, System.Globalization.NumberStyles.HexNumber, null, out byte red))
            {
                ResultColor.R = red;
                SetColor(ResultColor);
            }
        }

        private void Green10_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Green10.Text, out byte green))
            {
                ResultColor.G = green;
                SetColor(ResultColor);
            }
        }

        private void Green16_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Green16.Text, System.Globalization.NumberStyles.HexNumber, null, out byte green))
            {
                ResultColor.G = green;
                SetColor(ResultColor);
            }
        }

        private void Blue10_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Blue10.Text, out byte blue))
            {
                ResultColor.B = blue;
                SetColor(ResultColor);
            }
        }

        private void Blue16_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsSettingValues && byte.TryParse(Blue16.Text, System.Globalization.NumberStyles.HexNumber, null, out byte blue))
            {
                ResultColor.B = blue;
                SetColor(ResultColor);
            }
        }


    }
}
