using Computer_Graphics;
using System.Windows;

namespace Computer_Graphics;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Tasks1And2_Click(object sender, RoutedEventArgs e)
    {
        var window = new Tasks1and2Window();
        window.Show();
    }

    private void Button_Task3_Click(object sender, RoutedEventArgs e)
    {
        var window = new Task3Window();
        window.Show();
    }
}
