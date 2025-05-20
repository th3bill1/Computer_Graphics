using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Computer_Graphics.Task1and2;
internal class MedianFilter
{
    public static WriteableBitmap ApplyMedianFilter(WriteableBitmap bitmap, int size)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if (size < 1 || size % 2 == 0) throw new ArgumentException("Wrong size", nameof(size));

        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = width * 4;

        var outputBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);

        var pixelData = new byte[height * stride];
        var outputData = new byte[height * stride];

        bitmap.CopyPixels(pixelData, stride, 0);

        var radius = size / 2;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * stride + x * 4;

                var redValues = new byte[size * size];
                var greenValues = new byte[size * size];
                var blueValues = new byte[size * size];

                var count = 0;

                for (var ky = -radius; ky <= radius; ky++)
                {
                    for (var kx = -radius; kx <= radius; kx++)
                    {
                        var nx = Math.Clamp(x + kx, 0, width - 1);
                        var ny = Math.Clamp(y + ky, 0, height - 1);
                        var nIndex = ny * stride + nx * 4;

                        blueValues[count] = pixelData[nIndex];
                        greenValues[count] = pixelData[nIndex + 1];
                        redValues[count] = pixelData[nIndex + 2];
                        count++;
                    }
                }
                Array.Sort(blueValues);
                Array.Sort(greenValues);
                Array.Sort(redValues);

                var medianIndex = count / 2;
                outputData[index] = blueValues[medianIndex];
                outputData[index + 1] = greenValues[medianIndex];
                outputData[index + 2] = redValues[medianIndex];
                outputData[index + 3] = pixelData[index + 3];
            }
        }

        outputBitmap.WritePixels(new Int32Rect(0, 0, width, height), outputData, stride, 0);
        return outputBitmap;
    }
}