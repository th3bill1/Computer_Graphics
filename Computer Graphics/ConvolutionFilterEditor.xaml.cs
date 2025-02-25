using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace Computer_Graphics
{
    public partial class ConvolutionFilterEditorWindow : Window
    {
        private const string CONV_FOLDER = "Resources/LUTs/";
        private int rows = 3, cols = 3;
        private double[,] kernel;
        private int anchorRow, anchorCol;

        public ConvolutionFilterEditorWindow()
        {
            InitializeComponent();
            PopulateKernelSizeSelectors();
            InitializeKernel();
            LoadFilters();
        }

        private void PopulateKernelSizeSelectors()
        {
            List<int> sizes = new() { 1, 3, 5, 7, 9 };
            RowsComboBox.ItemsSource = sizes;
            ColumnsComboBox.ItemsSource = sizes;
            RowsComboBox.SelectedItem = 3;
            ColumnsComboBox.SelectedItem = 3;
        }

        private void InitializeKernel()
        {
            kernel = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    kernel[i, j] = 0;

            anchorRow = rows / 2;
            anchorCol = cols / 2;

            UpdateKernelGrid();
        }

        private void UpdateKernelGrid()
        {
            KernelGrid.Columns.Clear();
            KernelGrid.Items.Clear();

            for (int j = 0; j < cols; j++)
                KernelGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = j.ToString(),
                    Binding = new System.Windows.Data.Binding($"[{j}]")
                });

            for (int i = 0; i < rows; i++)
            {
                double[] rowValues = new double[cols];
                for (int j = 0; j < cols; j++)
                    rowValues[j] = kernel[i, j];

                KernelGrid.Items.Add(rowValues);
            }
        }

        private void KernelSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RowsComboBox.SelectedItem is int newRows && ColumnsComboBox.SelectedItem is int newCols)
            {
                rows = newRows;
                cols = newCols;
                InitializeKernel();
            }
        }

        private void DivisorChanged(object sender, TextChangedEventArgs e)
        {
            if (AutoComputeDivisorCheckBox.IsChecked == true)
                AutoComputeDivisor();
        }

        private void AutoComputeDivisorChecked(object sender, RoutedEventArgs e)
        {
            AutoComputeDivisor();
        }

        private void AutoComputeDivisor()
        {
            double sum = kernel.Cast<double>().Sum();
            DivisorTextBox.Text = (sum == 0) ? "1" : sum.ToString(CultureInfo.InvariantCulture);
        }

        private void LoadFilters()
        {
            string fullConvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONV_FOLDER);

            if (!Directory.Exists(fullConvPath))
                Directory.CreateDirectory(fullConvPath);

            FilterSelectionComboBox.Items.Clear();
            FilterSelectionComboBox.Items.Add("New Filter");

            string[] convFiles = Directory.GetFiles(fullConvPath, "*.conv");
            foreach (string filePath in convFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath).Replace("_", " ");
                FilterSelectionComboBox.Items.Add(fileName);
            }

            FilterSelectionComboBox.SelectedIndex = 0;
        }

        private void FilterSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterSelectionComboBox.SelectedItem == null) return;

            string selectedFilter = FilterSelectionComboBox.SelectedItem.ToString();
            if (selectedFilter == "New Filter")
                InitializeKernel();
            else
                LoadFilter(selectedFilter.Replace(" ", "_") + ".conv");
        }

        private void LoadFilter(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONV_FOLDER, fileName);
            if (!File.Exists(filePath)) return;

            string[] lines = File.ReadAllLines(filePath);
            string[] sizeParts = lines[0].Split(',');
            rows = int.Parse(sizeParts[0]);
            cols = int.Parse(sizeParts[1]);

            kernel = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                double[] rowValues = lines[i + 1].Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
                for (int j = 0; j < cols; j++)
                    kernel[i, j] = rowValues[j];
            }

            UpdateKernelGrid();
        }

        private void SaveFilter_Click(object sender, RoutedEventArgs e)
        {
            string inputName = Microsoft.VisualBasic.Interaction.InputBox("Enter filter name:", "Save Filter", "My Filter");
            if (string.IsNullOrWhiteSpace(inputName)) return;

            string fileName = inputName.Replace(" ", "_") + ".conv";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONV_FOLDER, fileName);

            List<string> lines = new() { $"{rows},{cols}" };
            for (int i = 0; i < rows; i++)
                lines.Add(string.Join(",", Enumerable.Range(0, cols).Select(j => kernel[i, j].ToString(CultureInfo.InvariantCulture))));

            File.WriteAllLines(filePath, lines);
            LoadFilters();
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.ApplyConvolutionFilter(kernel, rows, cols);
        }
    }
}
