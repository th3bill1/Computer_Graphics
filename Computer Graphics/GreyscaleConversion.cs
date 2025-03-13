using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Computer_Graphics;
internal class GreyscaleConversion
{
    public static WriteableBitmap ConvertToGrayscale(WriteableBitmap source, double rWeight, double gWeight, double bWeight)
    {
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte b = pixelData[i];
            byte g = pixelData[i + 1];
            byte r = pixelData[i + 2];

            byte gray = (byte)(rWeight * r + gWeight * g + bWeight * b);

            pixelData[i] = gray;
            pixelData[i + 1] = gray;
            pixelData[i + 2] = gray;
        }

        WriteableBitmap grayscaleBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        grayscaleBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return grayscaleBitmap;
    }
}
