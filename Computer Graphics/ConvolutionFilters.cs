using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Globalization;

namespace Computer_Graphics;
internal class ConvolutionFilters
{
    public static WriteableBitmap ApplyConvolutionFilter(WriteableBitmap bitmap, double[,] kernel, int kernelSize)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        byte[] resultData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        int kernelOffset = kernelSize / 2;

        for (int y = kernelOffset; y < height - kernelOffset; y++)
        {
            for (int x = kernelOffset; x < width - kernelOffset; x++)
            {
                double blue = 0, green = 0, red = 0;

                for (int ky = -kernelOffset; ky <= kernelOffset; ky++)
                {
                    for (int kx = -kernelOffset; kx <= kernelOffset; kx++)
                    {
                        int pixelIndex = ((y + ky) * stride) + ((x + kx) * 4);
                        double kernelValue = kernel[ky + kernelOffset, kx + kernelOffset];

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

    public static (double[,], int) LoadConvolutionKernel(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Convolution filter file not found!");

        string[] lines = File.ReadAllLines(filePath);
        int kernelSize = int.Parse(lines[0]);

        double[,] kernel = new double[kernelSize, kernelSize];
        for (int i = 0; i < kernelSize; i++)
        {
            double[] row = lines[i + 1]
            .Split(',')
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
            for (int j = 0; j < kernelSize; j++)
            {
                kernel[i, j] = row[j];
            }
        }
        return (kernel, kernelSize);
    }

    private static byte Clamp(double value) => (byte)(value < 0 ? 0 : (value > 255 ? 255 : value));
}
