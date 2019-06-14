using System.Windows;
using System.Windows.Input;

namespace Ov3rD4r53
{
    public partial class Inputbox : Window
    {
        public string ResultText = "";

        public Inputbox(string title = "", string text = "", string defalutvalue = "")
        {
            InitializeComponent();

            Title = title;
            Description.Content = text;
            InputBox.Text = defalutvalue;
            OnActivated(null);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ResultText = InputBox.Text;
            DialogResult = true;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResultText = InputBox.Text;
                DialogResult = true;
            }
            if (e.Key == Key.Escape) { SystemCommands.CloseWindow(this); }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }
    }
}
