using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Computer_Graphics;
internal class FunctionFilters
{
    public static WriteableBitmap Inversion(WriteableBitmap bitmap)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = (byte)(255 - pixelData[i]);
            pixelData[i + 1] = (byte)(255 - pixelData[i + 1]);
            pixelData[i + 2] = (byte)(255 - pixelData[i + 2]);
        }

        WriteableBitmap invertedBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
        invertedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return invertedBitmap;
    }

    public static WriteableBitmap AdjustBrightness(WriteableBitmap bitmap, int brightness)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(pixelData[i] + brightness);
            pixelData[i + 1] = Clamp(pixelData[i + 1] + brightness);
            pixelData[i + 2] = Clamp(pixelData[i + 2] + brightness);
        }

        WriteableBitmap brightenedBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
        brightenedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return brightenedBitmap;
    }

    public static WriteableBitmap AdjustContrast(WriteableBitmap bitmap, double contrast)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        double factor = (259 * (contrast + 255)) / (255 * (259 - contrast));

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = Clamp(factor * (pixelData[i] - 128) + 128);
            pixelData[i + 1] = Clamp(factor * (pixelData[i + 1] - 128) + 128);
            pixelData[i + 2] = Clamp(factor * (pixelData[i + 2] - 128) + 128);
        }

        WriteableBitmap contrastBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
        contrastBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return contrastBitmap;
    }

    public static WriteableBitmap AdjustGamma(WriteableBitmap bitmap, double gamma)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        bitmap.CopyPixels(pixelData, stride, 0);

        byte[] gammaCorrection = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            gammaCorrection[i] = Clamp(Math.Pow(i / 255.0, gamma) * 255.0);
        }

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i] = gammaCorrection[pixelData[i]];   
            pixelData[i + 1] = gammaCorrection[pixelData[i + 1]];
            pixelData[i + 2] = gammaCorrection[pixelData[i + 2]];
        }

        WriteableBitmap gammaBitmap = new WriteableBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
        gammaBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return gammaBitmap;
    }

    private static byte Clamp(double value)
    {
        return (byte)(value < 0 ? 0 : (value > 255 ? 255 : value));
    }

}