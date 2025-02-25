using Microsoft.Win32;
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

        Menu mainMenu = (Menu)this.FindName("MainMenu");
        MenuItem functionFiltersMenu = mainMenu.Items
            .OfType<MenuItem>()
            .FirstOrDefault(item => item.Header.ToString() == "Function filters");

        if (functionFiltersMenu == null) return;

        string[] lutFiles = Directory.GetFiles(fullLUTPath, "*.filter");

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
    }

    private void OpenFilterEditor_Click(object sender, RoutedEventArgs e)
    {
        FilterEditorWindow editorWindow = new();
        editorWindow.Show();
    }

    private void ApplyBlur_Click(object sender, RoutedEventArgs e)
    {
        if (displayedImage != null)
        {
            displayedImage = ConvolutionFilters.ApplyBlur(displayedImage);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyGaussianBlur_Click(object sender, RoutedEventArgs e)
    {
        if (displayedImage != null)
        {
            displayedImage = ConvolutionFilters.ApplyGaussianBlur(displayedImage);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplySharpen_Click(object sender, RoutedEventArgs e)
    {
        if (displayedImage != null)
        {
            displayedImage = ConvolutionFilters.ApplySharpen(displayedImage);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyEdgeDetection_Click(object sender, RoutedEventArgs e)
    {
        if (displayedImage != null)
        {
            displayedImage = ConvolutionFilters.ApplyEdgeDetection(displayedImage);
            ImageDisplay.Source = displayedImage;
        }
    }
    private void ApplyEmboss_Click(object sender, RoutedEventArgs e)
    {
        if (displayedImage != null)
        {
            displayedImage = ConvolutionFilters.ApplyEmboss(displayedImage);
            ImageDisplay.Source = displayedImage;
        }
    }
}
