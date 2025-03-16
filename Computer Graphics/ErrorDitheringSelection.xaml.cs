using System;
using System.Windows;
using System.Windows.Controls;

namespace Computer_Graphics
{
    public partial class ErrorDitheringSelection : Window
    {
        public int NumShades { get; private set; } = 4;
        public string SelectedFilter { get; private set; } = "Floyd-Steinberg";

        public ErrorDitheringSelection()
        {
            InitializeComponent();
            ShadesBox.Text = "2";
            FilterDropdown.SelectedIndex = 0;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ShadesBox.Text, out int shades) && shades >= 2 && shades <= 256)
            {
                NumShades = shades;
                SelectedFilter = ((ComboBoxItem)FilterDropdown.SelectedItem).Content.ToString();
                this.DialogResult = true; // Close window and return success
            }
            else
            {
                MessageBox.Show("Invalid input. Enter a number between 2 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
