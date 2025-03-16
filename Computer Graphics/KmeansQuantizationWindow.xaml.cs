using System.Windows;

namespace Computer_Graphics
{
    public partial class KMeansQuantizationWindow : Window
    {
        public int NumClusters { get; private set; } = 8;

        public KMeansQuantizationWindow()
        {
            InitializeComponent();
            NumClustersBox.Text = "8";
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumClustersBox.Text, out int clusters) && clusters >= 2 && clusters <= 256)
            {
                NumClusters = clusters;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Invalid input. Enter a number between 2 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
