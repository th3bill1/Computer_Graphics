using Microsoft.Win32;
using System;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Computer_Graphics
{
    public partial class MainWindow : Window
    {
        private BitmapImage originalImage;
        private WriteableBitmap displayedImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
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

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder;
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        encoder = new PngBitmapEncoder();
                        break;
                    case 2:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case 3:
                        encoder = new BmpBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(displayedImage));
                using (var fileStream = System.IO.File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(fileStream);
                }
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
        private void ApplyInversion_Click(object sender, RoutedEventArgs e)
        {
            if (displayedImage != null)
            {
                displayedImage = FunctionFilters.Inversion(displayedImage);
                ImageDisplay.Source = displayedImage;
            }
        }
    }
}
