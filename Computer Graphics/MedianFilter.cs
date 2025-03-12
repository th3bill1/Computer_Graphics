using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal class MedianFilter
{
    public static WriteableBitmap ApplyMedianFilter(WriteableBitmap bitmap, int size)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if (size < 1 || size % 2 == 0) throw new ArgumentException("Wrong size", nameof(size));

        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;

        WriteableBitmap outputBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);

        byte[] pixelData = new byte[height * stride];
        byte[] outputData = new byte[height * stride];

        bitmap.CopyPixels(pixelData, stride, 0);

        int radius = size / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * stride) + (x * 4);

                byte[] redValues = new byte[size * size];
                byte[] greenValues = new byte[size * size];
                byte[] blueValues = new byte[size * size];

                int count = 0;

                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int nx = Math.Clamp(x + kx, 0, width - 1);
                        int ny = Math.Clamp(y + ky, 0, height - 1);
                        int nIndex = (ny * stride) + (nx * 4);

                        blueValues[count] = pixelData[nIndex];
                        greenValues[count] = pixelData[nIndex + 1];
                        redValues[count] = pixelData[nIndex + 2];
                        count++;
                    }
                }
                Array.Sort(blueValues);
                Array.Sort(greenValues);
                Array.Sort(redValues);

                int medianIndex = count / 2;
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