using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Computer_Graphics
{
    public partial class FunctionFilterEditorWindow : Window
    {
        private List<Point> functionPoints = [];
        public ObservableCollection<EditablePoint> FunctionPointsView { get; set; } = [];

        private Polyline functionGraph = new();
        private const string LUT_FOLDER = "Resources/LUTs/";
        private string currentFilterName = "New_Filter";
        private bool isNewFilter = true;

        public FunctionFilterEditorWindow()
        {
            InitializeComponent();
            DataContext = this;

            FunctionPointsView.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (EditablePoint point in e.NewItems)
                        point.PropertyChanged += Point_PropertyChanged;
                }

                if (e.OldItems != null)
                {
                    foreach (EditablePoint point in e.OldItems)
                        point.PropertyChanged -= Point_PropertyChanged;
                }

                DrawFunction();
            };

            LoadFilters();
            SelectNewFilter();
        }
        private void Point_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is EditablePoint point && e.PropertyName == nameof(point.X))
            {
                if (!FunctionPointsView.Any(p => p.X == 0))
                {
                    point.X = 0;
                }
                if (!FunctionPointsView.Any(p => p.X == 255))
                {
                    point.X = 255;
                }
                var conflicts = FunctionPointsView.Where(p => p.X == point.X);
                foreach(EditablePoint conflict in conflicts)
                {
                    if (conflict != point) FunctionPointsView.Remove(conflict);
                }
            }

            DrawFunction();
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
            functionPoints.Clear();
            FunctionPointsView.Clear();

            if (!FunctionPointsView.Any(p => p.X == 0))
                FunctionPointsView.Add(new EditablePoint { X = 0, Y = 255 });

            if (!FunctionPointsView.Any(p => p.X == 255))
                FunctionPointsView.Add(new EditablePoint { X = 255, Y = 0 });

            DrawFunction();
        }
        private void DrawFunction()
        {
            functionPoints = [.. FunctionPointsView
                .Select(ep => new Point(ep.X*2, ep.Y*2))
                .OrderBy(p => p.X)];

            functionGraph.Points.Clear();
            foreach (var p in functionPoints)
            {
                functionGraph.Points.Add(p);
            }
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickedPoint = e.GetPosition(GraphCanvas);

            if (clickedPoint.X > 0 && clickedPoint.X < 520)
            {
                functionPoints.Add(clickedPoint);
                functionPoints = [.. functionPoints.OrderBy(p => p.X)];
                FunctionPointsView.Add(new EditablePoint { X = (byte)(clickedPoint.X/2), Y = (byte)(clickedPoint.Y/2) });
            }
        }

        private void LoadFilter(string fileName)
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER, fileName);
            if (!File.Exists(filePath)) return;

            functionPoints.Clear();
            FunctionPointsView.Clear();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0], out double x) &&
                        double.TryParse(parts[1], out double y))
                    {
                        functionPoints.Add(new Point(x, 255 - y));
                    }
                }
            }
            foreach (var p in functionPoints)
                FunctionPointsView.Add(new EditablePoint { X = (byte)p.X, Y = (byte)p.Y });

            currentFilterName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            isNewFilter = false;
            DrawFunction();
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance?.DisplayedImage == null) return;

            MainWindow.Instance.DisplayedImage = FunctionFilters.ApplyFunctionFilter(
                MainWindow.Instance.DisplayedImage,
                ApplyFunction
            );
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
            functionPoints = [.. FunctionPointsView
                .Select(ep => new Point(ep.X, ep.Y))
                .OrderBy(p => p.X)];

            string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER, currentFilterName + ".filter");
            using (StreamWriter writer = new StreamWriter(savePath))
            {
                foreach (var point in functionPoints)
                {
                    writer.WriteLine($"{(int)point.X},{(int)(255 - point.Y)}");
                }
            }

            MessageBox.Show($"Filter saved as: {currentFilterName}.filter", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadFilters();
        }

        private void DeletePoint_Click(object sender, RoutedEventArgs e)
        {
            if (PointsDataGrid.SelectedItem is EditablePoint selectedPoint)
            {
                if (selectedPoint.X == 0 || selectedPoint.X == 510)
                {
                    MessageBox.Show("Cannot delete border points (X=0 or X=255).", "Deletion Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                FunctionPointsView.Remove(selectedPoint);
                DrawFunction();
            }
            else
            {
                MessageBox.Show("Please select a point to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
