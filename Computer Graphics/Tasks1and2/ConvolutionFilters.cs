using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Globalization;

namespace Computer_Graphics.Task1and2;
internal class ConvolutionFilters
{
    public static WriteableBitmap ApplyConvolutionFilter(WriteableBitmap bitmap, double[,] kernel, int rows, int cols)
    {
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = width * 4;
        var pixelData = new byte[height * stride];
        var resultData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        var rowOffset = rows / 2;
        var colOffset = cols / 2;

        for (var y = rowOffset; y < height - rowOffset; y++)
        {
            for (var x = colOffset; x < width - colOffset; x++)
            {
                double blue = 0, green = 0, red = 0;

                for (var ky = -rowOffset; ky <= rowOffset; ky++)
                {
                    for (var kx = -colOffset; kx <= colOffset; kx++)
                    {
                        var pixelX = x + kx;
                        var pixelY = y + ky;
                        var pixelIndex = pixelY * stride + pixelX * 4;
                        var kernelValue = kernel[ky + rowOffset, kx + colOffset];

                        blue += pixelData[pixelIndex] * kernelValue;
                        green += pixelData[pixelIndex + 1] * kernelValue;
                        red += pixelData[pixelIndex + 2] * kernelValue;
                    }
                }

                var resultIndex = y * stride + x * 4;
                resultData[resultIndex] = Clamp(blue);
                resultData[resultIndex + 1] = Clamp(green);
                resultData[resultIndex + 2] = Clamp(red);
                resultData[resultIndex + 3] = pixelData[resultIndex + 3];
            }
        }

        WriteableBitmap filteredBitmap = new(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
        filteredBitmap.WritePixels(new Int32Rect(0, 0, width, height), resultData, stride, 0);

        return filteredBitmap;
    }


    public static (double[,], int, int) LoadConvolutionKernel(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Convolution filter file not found!");

        var lines = File.ReadAllLines(filePath);
        var sizeParts = lines[0].Split(',');
        var rows = int.Parse(sizeParts[0]);
        var cols = int.Parse(sizeParts[1]);


        var kernel = new double[rows, cols];
        for (var i = 0; i < rows; i++)
        {
            var rowValues = lines[i + 1].Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            for (var j = 0; j < cols; j++)
            {
                kernel[i, j] = rowValues[j];
            }


        }
        return (kernel, rows, cols);
    }

    private static byte Clamp(double value) => (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
}
