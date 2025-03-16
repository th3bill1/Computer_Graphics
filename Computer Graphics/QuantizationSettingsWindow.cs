using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Computer_Graphics
{
    public class QuantizationSettingsWindow : Window
    {
        private readonly TextBox? NumColorsBox;
        private readonly List<TextBox> ValueBoxes = [];
        public int NumColors { get; private set; } = 16;
        public List<int> Values { get; private set; } = [];

        public QuantizationSettingsWindow(string title, List<string>? altFields = null)
        {
            Title = title;
            Width = 300;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Grid grid = new Grid
            {
                Margin = new Thickness(10)            };

            if (altFields != null && altFields.Count > 0)
            {
                for (int i = 0; i < altFields.Count; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Border rowBorder = new Border
                    {
                        BorderThickness = new Thickness(0, 0, 0, 5),
                        Padding = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Child = CreateRow(altFields[i], out TextBox valueBox)
                    };

                    ValueBoxes.Add(valueBox);
                    Grid.SetRow(rowBorder, i);
                    grid.Children.Add(rowBorder);
                }
            }
            else
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Border rowBorder = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 5),
                    Padding = new Thickness(5),
                    Child = CreateRow("Number of Colors:", out NumColorsBox)
                };

                Grid.SetRow(rowBorder, 0);
                grid.Children.Add(rowBorder);
            }

            Button applyButton = new Button
            {
                Content = "Apply",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };
            applyButton.Click += ApplyButton_Click;

            int buttonRow = (altFields != null) ? altFields.Count : 1;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(applyButton, buttonRow);
            grid.Children.Add(applyButton);

            Content = grid;
        }

        private static StackPanel CreateRow(string labelText, out TextBox textBox)
        {
            StackPanel row = new StackPanel { Orientation = Orientation.Horizontal };

            TextBlock label = new TextBlock
            {
                Text = labelText,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            textBox = new TextBox
            {
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = "16"
            };

            row.Children.Add(label);
            row.Children.Add(textBox);
            return row;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Values.Clear();

            if (ValueBoxes.Count > 0)
            {
                foreach (var box in ValueBoxes)
                {
                    if (int.TryParse(box.Text, out int value) && value >= 2 && value <= 256)
                    {
                        Values.Add(value);
                    }
                    else
                    {
                        MessageBox.Show("Invalid input. Enter a number between 2 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                DialogResult = true;
            }
            else if (NumColorsBox != null)
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
}
