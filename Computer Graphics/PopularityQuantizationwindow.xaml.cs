using System.Windows;

namespace Computer_Graphics
{
    public partial class PopularityQuantizationWindow : Window
    {
        public int NumColors { get; private set; } = 16;

        public PopularityQuantizationWindow()
        {
            InitializeComponent();
            NumColorsBox.Text = "16";
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumColorsBox.Text, out int colors) && colors >= 2 && colors <= 256)
            {
                NumColors = colors;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Invalid input. Enter a number between 2 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
