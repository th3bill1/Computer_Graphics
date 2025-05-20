using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Computer_Graphics.Task1and2;
internal class GreyscaleConversion
{
    public static WriteableBitmap ConvertToGrayscale(WriteableBitmap source, double rWeight, double gWeight, double bWeight)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var b = pixelData[i];
            var g = pixelData[i + 1];
            var r = pixelData[i + 2];

            var gray = (byte)(rWeight * r + gWeight * g + bWeight * b);

            pixelData[i] = gray;
            pixelData[i + 1] = gray;
            pixelData[i + 2] = gray;
        }

        var grayscaleBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        grayscaleBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return grayscaleBitmap;
    }
}
