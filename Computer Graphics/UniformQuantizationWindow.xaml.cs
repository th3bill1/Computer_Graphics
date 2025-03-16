using System.Windows;

namespace Computer_Graphics
{
    public partial class UniformQuantizationWindow : Window
    {
        public int RDivisions { get; private set; } = 3;
        public int GDivisions { get; private set; } = 3;
        public int BDivisions { get; private set; } = 3;

        public UniformQuantizationWindow()
        {
            InitializeComponent();
            RedDivBox.Text = "3";
            GreenDivBox.Text = "3";
            BlueDivBox.Text = "3";
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RedDivBox.Text, out int r) && r >= 1 && r <= 256 &&
                int.TryParse(GreenDivBox.Text, out int g) && g >= 1 && g <= 256 &&
                int.TryParse(BlueDivBox.Text, out int b) && b >= 1 && b <= 256)
            {
                RDivisions = r;
                GDivisions = g;
                BDivisions = b;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Invalid input. Enter numbers between 1 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
