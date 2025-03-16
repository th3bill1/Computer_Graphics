using System.Windows;
using System.Windows.Controls;

namespace Computer_Graphics
{
    public class QuantizationSettingsWindow : Window
    {
        private readonly TextBox NumColorsBox;
        public int NumColors { get; private set; } = 16;

        public QuantizationSettingsWindow(string title, int defaultColors = 16)
        {
            Title = title;
            Height = 150;
            Width = 250;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Grid grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock label = new TextBlock
            {
                Text = "Number of Colors:",
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            NumColorsBox = new TextBox
            {
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = defaultColors.ToString()
            };
            Grid.SetRow(NumColorsBox, 0);
            grid.Children.Add(NumColorsBox);

            Button applyButton = new Button
            {
                Content = "Apply",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };
            applyButton.Click += ApplyButton_Click;
            Grid.SetRow(applyButton, 1);
            grid.Children.Add(applyButton);

            Content = grid;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumColorsBox.Text, out int colors) && colors >= 2 && colors <= 256)
            {
                NumColors = colors;
                DialogResult = true; 
            }
            else
            {
                MessageBox.Show("Invalid input. Enter a number between 2 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
