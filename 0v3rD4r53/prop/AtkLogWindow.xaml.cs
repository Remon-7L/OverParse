using System;
using System.Windows;
using System.Windows.Input;

namespace Ov3rD4r53
{
    /// <summary>
    /// AtkLogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AtkLogWindow : YKToolkit.Controls.Window
    {
        public AtkLogWindow() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AtkLogList.ItemsSource = MainWindow.userattacks;
            Update(sender, e);
            MainWindow.userattacks.CollectionChanged += Update;
        }

        private void Update(object sender, EventArgs e) => AtkCount.Content = MainWindow.userattacks.Count;

        private void Reset_Click(object sender, RoutedEventArgs e) => MainWindow.userattacks.Clear();

        private void Minimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.userattacks.CollectionChanged -= Update;
            SystemCommands.CloseWindow(this);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

    }
}
