﻿using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    private BitmapImage originalImage;
    private WriteableBitmap displayedImage;
    public WriteableBitmap DisplayedImage
    {
        get => displayedImage;
        set => displayedImage = value;
    }

    private const string LUT_FOLDER = "Resources/LUTs/";
    private const string PLACEHOLDER_PATH = "pack://application:,,,/Resources/placeholder.png";

    public MainWindow()
    {
        InitializeComponent();
        originalImage = new(new Uri(PLACEHOLDER_PATH));
        displayedImage = new(originalImage);
        ImageDisplay.Source = displayedImage;
        ImageOriginal.Source = originalImage;
        Instance = this;

        LoadLUTFilters();
    }

    private void LoadImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            originalImage = new BitmapImage(new Uri(openFileDialog.FileName));
            displayedImage = new WriteableBitmap(originalImage);
            ImageDisplay.Source = displayedImage;
            ImageOriginal.Source = displayedImage;
        }
    }

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        if (displayedImage == null)
        {
            MessageBox.Show("No image to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SaveFileDialog saveFileDialog = new()
        {
            Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            BitmapEncoder encoder = saveFileDialog.FilterIndex switch
            {
                1 => new PngBitmapEncoder(),
                2 => new JpegBitmapEncoder(),
                3 => new BmpBitmapEncoder(),
                _ => new PngBitmapEncoder()
            };

            encoder.Frames.Add(BitmapFrame.Create(displayedImage));
            using var fileStream = File.Create(saveFileDialog.FileName);
            encoder.Save(fileStream);
        }
    }

    private void ResetImage_Click(object sender, RoutedEventArgs e)
    {
        if (originalImage != null)
        {
            displayedImage = new WriteableBitmap(originalImage);
            ImageDisplay.Source = displayedImage;
        }
    }

    private void ApplyLUTFilter(string filePath)
    {
        if (displayedImage != null)
        {
            displayedImage = FunctionFilters.ApplyFunctionFromFile(displayedImage, filePath);
            ImageDisplay.Source = displayedImage;
        }
    }

    private void LoadLUTFilters()
    {
        string fullLUTPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LUT_FOLDER);

        if (!Directory.Exists(fullLUTPath))
        {
            MessageBox.Show($"LUT folder not found: {fullLUTPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Menu mainMenu = (Menu)FindName("MainMenu");
        MenuItem functionFiltersMenu = mainMenu.Items
            .OfType<MenuItem>()
            .FirstOrDefault(item => item.Header.ToString() == "Function filters");
        MenuItem convolutionFiltersMenu = mainMenu.Items
            .OfType<MenuItem>()
            .FirstOrDefault(item => item.Header.ToString() == "Convolution filters");

        if (functionFiltersMenu == null) return;

        string[] lutFiles = Directory.GetFiles(fullLUTPath, "*.filter");
        string[] convFiles = Directory.GetFiles(fullLUTPath, "*.conv");

        foreach (string filePath in lutFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            MenuItem filterItem = new()
            {
                Header = fileName.Replace("_"," "),
                Tag = filePath
            };
            filterItem.Click += (sender, e) =>
            {
                ApplyLUTFilter(filePath);
            };

            functionFiltersMenu.Items.Add(filterItem);
        }

        if (convolutionFiltersMenu == null) return;

        foreach (string filePath in convFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            MenuItem filterItem = new()
            {
                Header = fileName.Replace("_", " "),
                Tag = filePath
            };
            filterItem.Click += (sender, e) =>
            {
                ApplyConvolutionFilter(filePath);
            };

            convolutionFiltersMenu.Items.Add(filterItem);
        }
    }

    private void OpenFunctionFilterEditor_Click(object sender, RoutedEventArgs e)
    {
        FunctionFilterEditorWindow editorWindow = new();
        editorWindow.Show();
    }
    private void OpenConvolutionFilterEditor_Click(object sender, RoutedEventArgs e)
    {
        ConvolutionFilterEditorWindow editorWindow = new();
        editorWindow.Show();
    }

    private void ApplyConvolutionFilter(string filePath)
    {
        if (displayedImage != null)
        {
            var (kernel, rows, cols) = ConvolutionFilters.LoadConvolutionKernel(filePath);
            displayedImage = ConvolutionFilters.ApplyConvolutionFilter(displayedImage, kernel, rows, cols);
            ImageDisplay.Source = displayedImage;
        }
    }

    public void ApplyConvolutionFilter(double[,] kernel, int rows, int cols)
    {
        if (displayedImage != null)
        {
            displayedImage = ConvolutionFilters.ApplyConvolutionFilter(displayedImage, kernel, rows, cols);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyMedianFilter_Click(object sender, EventArgs e)
    {
        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter an integer value for N:", "Enter N", "3");

        if (int.TryParse(input, out int n))
        {
            displayedImage = MedianFilter.ApplyMedianFilter(displayedImage, n);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ConvertToGreyscale_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            var (rWeight, gWeight, bWeight) = GreyscaleParameterWindow.LoadParametersFromFile();

            displayedImage = GreyscaleConversion.ConvertToGrayscale(displayedImage, rWeight, gWeight, bWeight);
            ImageDisplay.Source = displayedImage;
        }
    }

    private void OpenGreyscaleEditor_Click(object sender, EventArgs e)
    {
        GreyscaleParameterWindow parameterWindow = new GreyscaleParameterWindow();
        parameterWindow.ShowDialog();
    }
    private void ApplyRandomDithering_Click(object sender, EventArgs e) 
    {
        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter number of shades:", "Enter N", "3");

        if (int.TryParse(input, out int n) && n > 1)
        {
            displayedImage = Dithering.ApplyRandomDithering(displayedImage, n);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyAvarageDithering_Click(object sender, EventArgs e)
    {
        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter number of shades:", "Enter N", "3");

        if (int.TryParse(input, out int n) && n > 1)
        {
            displayedImage = Dithering.ApplyAverageDithering(displayedImage, n);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyOrderedDithering_Click(object sender, EventArgs e)
    {
        //change error handling
        if (displayedImage != null)
        {
            string shadesInput = Microsoft.VisualBasic.Interaction.InputBox("Enter number of shades of grey (2-256):", "Ordered Dithering", "4");
            if (!int.TryParse(shadesInput, out int numShades) || numShades < 2 || numShades > 256)
            {
                MessageBox.Show("Invalid input. Please enter a number between 2 and 256.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sizeInput = Microsoft.VisualBasic.Interaction.InputBox("Enter Bayer matrix size (2, 3, 4, or 6):", "Ordered Dithering", "4");
            if (!int.TryParse(sizeInput, out int matrixSize) || (matrixSize != 2 && matrixSize != 3 && matrixSize != 4 && matrixSize != 6))
            {
                MessageBox.Show("Invalid input. Please enter 2, 3, 4, or 6.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            displayedImage = Dithering.ApplyOrderedDithering(displayedImage, numShades, matrixSize);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyErrorDithering_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            SettingsWindow settingsWindow = new SettingsWindow("Error Dithering", ["Number of shades"] ,typeof(Dithering.FilterType));
            bool? result = settingsWindow.ShowDialog(); 

            if (result == true)
            {
                int numShades = settingsWindow.Values[0];
                Dithering.FilterType selectedFilter = (Dithering.FilterType)settingsWindow.SelectedEnumValue;

                displayedImage = Dithering.ApplyErrorDiffusionDithering(displayedImage, numShades, selectedFilter);
                ImageDisplay.Source = displayedImage;
            }
        }
    }
    private void ApplyUniformQuantization_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            SettingsWindow settingsWindow = 
                new SettingsWindow("Uniform Quantization", ["Red divisions", "Green dvisions", "BDivisions"]);
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                int rDivisions = settingsWindow.Values[0];
                int gDivisions = settingsWindow.Values[1];
                int bDivisions = settingsWindow.Values[2];

                displayedImage = ColorQuantization.ApplyUniformQuantization(displayedImage, rDivisions, gDivisions, bDivisions);
                ImageDisplay.Source = displayedImage;
            }
        }
    }
    private void ApplyPopularityQuantization_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            SettingsWindow settingsWindow = new SettingsWindow("Popularity Quantization");
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                int numColors = settingsWindow.NumColors;

                displayedImage = ColorQuantization.ApplyPopularityQuantization(displayedImage, numColors);
                ImageDisplay.Source = displayedImage;
            }
        }
    }

    private void ApplyKmeansQuantization_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            SettingsWindow settingsWindow = new SettingsWindow("K-Means Quantization");
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                int numClusters = settingsWindow.NumColors;

                displayedImage = ColorQuantization.ApplyKMeansQuantization(displayedImage, numClusters);
                ImageDisplay.Source = displayedImage;
            }
        }
    }
    private void ApplyMediancutmQuantization_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            SettingsWindow settingsWindow = new SettingsWindow("Median Cut Quantization");
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                int numColors = settingsWindow.NumColors;

                displayedImage = ColorQuantization.ApplyMedianCutQuantization(displayedImage, numColors);
                ImageDisplay.Source = displayedImage;
            }
        }
    }
    private void ApplyOctreeQuantization_Click(object sender, EventArgs e)
    {
        if (displayedImage != null)
        {
            SettingsWindow settingsWindow = new SettingsWindow("Octree Quantization");
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                int numColors = settingsWindow.NumColors;

                displayedImage = ColorQuantization.ApplyOctreeQuantization(displayedImage, numColors);
                ImageDisplay.Source = displayedImage;
            }
        }
    }

}