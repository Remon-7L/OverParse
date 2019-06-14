using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Ov3rD4r53
{
    /// <summary>
    /// Overlay.xaml の相互作用ロジック
    /// </summary>
    public partial class Overlay : Window
    {
        public static Color WinBrush, LoseBrush;
        public Overlay() => InitializeComponent();

        private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

    }
}
