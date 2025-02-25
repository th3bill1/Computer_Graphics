using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;

public partial class FilterEditorWindow : Window
{
    private List<Point> functionPoints = [];
    private Polyline functionGraph = new();

    public FilterEditorWindow()
    {
        InitializeComponent();
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

    private void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {
        if (MainWindow.Instance?.DisplayedImage == null) return;

        WriteableBitmap bitmap = MainWindow.Instance.DisplayedImage;
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = ApplyFunction(pixelData[i]);
            pixelData[i + 1] = ApplyFunction(pixelData[i + 1]);
            pixelData[i + 2] = ApplyFunction(pixelData[i + 2]);
        }

        WriteableBitmap filteredBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
        filteredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
        MainWindow.Instance.DisplayedImage = filteredBitmap;
        MainWindow.Instance.ImageDisplay.Source = filteredBitmap;
    }

    private void SaveFilter_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Filter Files|*.filter" };
        if (saveDialog.ShowDialog() == true)
        {
            using StreamWriter writer = new StreamWriter(saveDialog.FileName);
            foreach (var p in functionPoints)
                writer.WriteLine($"{p.X} {p.Y}");
        }
    }
}
