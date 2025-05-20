using System.Windows;
using System.Windows.Controls;

namespace Computer_Graphics.Task1and2;

public class SettingsWindow : Window
{
    private readonly TextBox? NumColorsBox;
    private readonly List<TextBox> ValueBoxes = [];
    private readonly ComboBox? EnumComboBox;

    public int NumColors { get; private set; } = 16;
    public List<int> Values { get; private set; } = [];
    public object? SelectedEnumValue { get; private set; }

    public SettingsWindow(string title, List<string>? altFields = null, Type? enumType = null)
    {
        Title = title;
        Width = 300;
        SizeToContent = SizeToContent.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var grid = new Grid { Margin = new Thickness(10) };

        var rowIndex = 0;

        if (altFields != null && altFields.Count > 0)
        {
            foreach (var field in altFields)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var rowBorder = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 5),
                    Padding = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Child = CreateRow(field, out var valueBox)
                };

                ValueBoxes.Add(valueBox);
                Grid.SetRow(rowBorder, rowIndex++);
                grid.Children.Add(rowBorder);
            }
        }
        else
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var rowBorder = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 5),
                Padding = new Thickness(5),
                Child = CreateRow("Number of Colors:", out NumColorsBox)
            };

            Grid.SetRow(rowBorder, rowIndex++);
            grid.Children.Add(rowBorder);
        }

        if (enumType != null && enumType.IsEnum)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var enumRowBorder = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 5),
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = CreateEnumRow(enumType, out EnumComboBox)
            };

            Grid.SetRow(enumRowBorder, rowIndex++);
            grid.Children.Add(enumRowBorder);
        }

        var applyButton = new Button
        {
            Content = "Apply",
            Width = 80,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10)
        };
        applyButton.Click += ApplyButton_Click;

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRow(applyButton, rowIndex);
        grid.Children.Add(applyButton);

        Content = grid;
    }

    private static StackPanel CreateRow(string labelText, out TextBox textBox)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        var label = new TextBlock
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

    private static StackPanel CreateEnumRow(Type enumType, out ComboBox comboBox)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        var label = new TextBlock
        {
            Text = "Select Value:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        comboBox = new ComboBox
        {
            Width = 150,
            ItemsSource = Enum.GetValues(enumType),
            SelectedIndex = 0
        };

        row.Children.Add(label);
        row.Children.Add(comboBox);
        return row;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        Values.Clear();

        if (EnumComboBox != null)
            SelectedEnumValue = EnumComboBox.SelectedItem;

        if (ValueBoxes.Count > 0)
        {
            foreach (var box in ValueBoxes)
            {
                if (int.TryParse(box.Text, out var value) && value >= 2 && value <= 256)
                    Values.Add(value);
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
            if (int.TryParse(NumColorsBox.Text, out var colors) && colors >= 2 && colors <= 256)
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
