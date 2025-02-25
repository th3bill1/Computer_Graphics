using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Computer_Graphics;

public partial class FunctionFilterEditorWindow : Window
{
    private List<Point> functionPoints = new();
    private Polyline functionGraph = new();
    private const string LUT_FOLDER = "Resources/LUTs/";
    private string currentFilterName = "New_Filter";
    private bool isNewFilter = true;

    public FunctionFilterEditorWindow()
    {
        InitializeComponent();
        LoadFilters();
        SelectNewFilter();
    }

    private void LoadFilters()
    {
        string fullLUTPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER);

        if (!Directory.Exists(fullLUTPath))
        {
            Directory.CreateDirectory(fullLUTPath);
            return;
        }

        FilterSelectionComboBox.Items.Clear();
        FilterSelectionComboBox.Items.Add("New Filter");

        string[] lutFiles = Directory.GetFiles(fullLUTPath, "*.filter");
        foreach (string filePath in lutFiles)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath).Replace("_", " ");
            FilterSelectionComboBox.Items.Add(fileName);
        }

        FilterSelectionComboBox.SelectedIndex = 0;
    }

    private void FilterSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FilterSelectionComboBox.SelectedItem != null)
        {
            var selectedFilter = FilterSelectionComboBox.SelectedItem.ToString();

            if (selectedFilter == "New Filter")
            {
                SelectNewFilter();
            }
            else
            {
                LoadFilter(selectedFilter.Replace(" ", "_") + ".filter");
            }
        }
    }

    private void SelectNewFilter()
    {
        isNewFilter = true;
        currentFilterName = "New_Filter";
        InitializeFunctionGraph();
    }

    private void InitializeFunctionGraph()
    {
        functionGraph = FunctionGraph;
        functionPoints =
        [
            new Point(0, 255),
            new Point(255, 0)
        ];
        DrawFunction();
    }

    private void DrawFunction()
    {
        functionGraph.Points.Clear();
        foreach (var p in functionPoints.OrderBy(p => p.X))
        {
            functionGraph.Points.Add(new Point(p.X, 255 - p.Y));
        }
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickedPoint = e.GetPosition(GraphCanvas);
        clickedPoint.Y = 255 - clickedPoint.Y;

        if (clickedPoint.X > 0 && clickedPoint.X < 255)
        {
            functionPoints.Add(clickedPoint);
            functionPoints = functionPoints.OrderBy(p => p.X).ToList();
            DrawFunction();
        }
    }

    private void LoadFilter(string fileName)
    {
        string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER, fileName);
        if (!File.Exists(filePath)) return;

        functionPoints.Clear();
        using (StreamReader reader = new StreamReader(filePath))
        {
            var line = reader.ReadLine();
            byte[] lut = line.Split(',').Select(byte.Parse).ToArray();

            for (int x = 0; x < lut.Length; x++)
            {
                functionPoints.Add(new Point(x, 255 - lut[x]));
            }
        }

        currentFilterName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        isNewFilter = false;
        DrawFunction();
    }

    private void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {
        if (MainWindow.Instance?.DisplayedImage == null) return;

        byte[] lut = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            lut[i] = ApplyFunction((byte)i);
        }

        string tempLUTPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER, currentFilterName + ".filter");
        File.WriteAllText(tempLUTPath, string.Join(",", lut));

        MainWindow.Instance.DisplayedImage = FunctionFilters.ApplyFunctionFromFile(MainWindow.Instance.DisplayedImage, tempLUTPath);
        MainWindow.Instance.ImageDisplay.Source = MainWindow.Instance.DisplayedImage;
    }

    private byte ApplyFunction(byte input)
    {
        for (int i = 0; i < functionPoints.Count - 1; i++)
        {
            var p1 = functionPoints[i];
            var p2 = functionPoints[i + 1];

            if (input >= p1.X && input <= p2.X)
            {
                double ratio = (input - p1.X) / (p2.X - p1.X);
                return (byte)(p1.Y + ratio * (p2.Y - p1.Y));
            }
        }
        return input;
    }

    private void SaveFilter_Click(object sender, RoutedEventArgs e)
    {
        if (isNewFilter)
        {
            string inputName = Microsoft.VisualBasic.Interaction.InputBox("Enter filter name:", "Save Filter", "My Filter");

            if (string.IsNullOrWhiteSpace(inputName))
            {
                MessageBox.Show("Filter name cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentFilterName = inputName.Replace(" ", "_");
            isNewFilter = false;
        }

        byte[] lut = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            lut[i] = ApplyFunction((byte)i);
        }

        string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER, currentFilterName + ".filter");
        File.WriteAllText(savePath, string.Join(",", lut));

        MessageBox.Show($"Filter saved as: {currentFilterName}.filter", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

        LoadFilters();
    }
}
