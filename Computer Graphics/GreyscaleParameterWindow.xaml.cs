using System.IO;
using System.Windows;

namespace Computer_Graphics
{
    public partial class GreyscaleParameterWindow : Window
    {
        private const string ParametersFile = "greyscale.parameters";
        public double R_Weight { get; private set; } = 0.299;
        public double G_Weight { get; private set; } = 0.587;
        public double B_Weight { get; private set; } = 0.114;

        public GreyscaleParameterWindow()
        {
            InitializeComponent();
            LoadParameters();
        }

        private void LoadParameters()
        {
            if (File.Exists(ParametersFile))
            {
                try
                {
                    string[] lines = File.ReadAllLines(ParametersFile);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && double.TryParse(parts[1], out double value))
                        {
                            if (parts[0] == "R_weight") R_Weight = value;
                            else if (parts[0] == "G_weight") G_Weight = value;
                            else if (parts[0] == "B_weight") B_Weight = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            RedWeightBox.Text = R_Weight.ToString();
            GreenWeightBox.Text = G_Weight.ToString();
            BlueWeightBox.Text = B_Weight.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(RedWeightBox.Text, out double r) &&
                double.TryParse(GreenWeightBox.Text, out double g) &&
                double.TryParse(BlueWeightBox.Text, out double b))
            {
                R_Weight = r;
                G_Weight = g;
                B_Weight = b;

                File.WriteAllLines(ParametersFile, new string[]
                {
                    $"R_weight {R_Weight}",
                    $"G_weight {G_Weight}",
                    $"B_weight {B_Weight}"
                });

                MessageBox.Show("Parameters saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter numeric values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(RedWeightBox.Text, out double r) &&
                double.TryParse(GreenWeightBox.Text, out double g) &&
                double.TryParse(BlueWeightBox.Text, out double b))
            {
                R_Weight = r;
                G_Weight = g;
                B_Weight = b;

                if (MainWindow.Instance != null && MainWindow.Instance.DisplayedImage != null)
                {
                    MainWindow.Instance.DisplayedImage = GreyscaleConversion.ConvertToGrayscale(
                        MainWindow.Instance.DisplayedImage, R_Weight, G_Weight, B_Weight);
                    MainWindow.Instance.ImageDisplay.Source = MainWindow.Instance.DisplayedImage;
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter numeric values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static (double, double, double) LoadParametersFromFile()
        {
            const string ParametersFile = "greyscale.parameters";
            double rWeight = 0.299, gWeight = 0.587, bWeight = 0.114;

            if (File.Exists(ParametersFile))
            {
                try
                {
                    string[] lines = File.ReadAllLines(ParametersFile);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && double.TryParse(parts[1], out double value))
                        {
                            if (parts[0] == "R_weight") rWeight = value;
                            else if (parts[0] == "G_weight") gWeight = value;
                            else if (parts[0] == "B_weight") bWeight = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading grayscale parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return (rWeight, gWeight, bWeight);
        }
    }
}
