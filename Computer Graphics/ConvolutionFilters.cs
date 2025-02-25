using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Globalization;

namespace Computer_Graphics;
internal class ConvolutionFilters
{
    public static WriteableBitmap ApplyConvolutionFilter(WriteableBitmap bitmap, double[,] kernel, int rows, int cols)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        byte[] resultData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        int rowOffset = rows / 2;
        int colOffset = cols / 2;

        for (int y = rowOffset; y < height - rowOffset; y++)
        {
            for (int x = colOffset; x < width - colOffset; x++)
            {
                double blue = 0, green = 0, red = 0;

                for (int ky = -rowOffset; ky <= rowOffset; ky++)
                {
                    for (int kx = -colOffset; kx <= colOffset; kx++)
                    {
                        int pixelX = x + kx;
                        int pixelY = y + ky;
                        int pixelIndex = (pixelY * stride) + (pixelX * 4);
                        double kernelValue = kernel[ky + rowOffset, kx + colOffset];

                        blue += pixelData[pixelIndex] * kernelValue;
                        green += pixelData[pixelIndex + 1] * kernelValue;
                        red += pixelData[pixelIndex + 2] * kernelValue;
                    }
                }

                int resultIndex = (y * stride) + (x * 4);
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

        string[] lines = File.ReadAllLines(filePath);
        string[] sizeParts = lines[0].Split(',');
        int rows = int.Parse(sizeParts[0]);
        int cols = int.Parse(sizeParts[1]);


        double[,] kernel = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            double[] rowValues = lines[i + 1].Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            for (int j = 0; j < cols; j++)
            {
                kernel[i, j] = rowValues[j];
            }


        }
        return (kernel, rows, cols);
    }

    private static byte Clamp(double value) => (byte)(value < 0 ? 0 : (value > 255 ? 255 : value));
}
