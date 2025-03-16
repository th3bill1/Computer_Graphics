using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics
{
    internal class ColorQuantization
    {
        public static WriteableBitmap ApplyUniformQuantization(WriteableBitmap source, int rDivisions, int gDivisions, int bDivisions)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            int rStep = 256 / rDivisions;
            int gStep = 256 / gDivisions;
            int bStep = 256 / bDivisions;

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte b = pixelData[i];
                byte g = pixelData[i + 1];
                byte r = pixelData[i + 2];

                r = QuantizeColor(r, rStep);
                g = QuantizeColor(g, gStep);
                b = QuantizeColor(b, bStep);

                pixelData[i] = b;
                pixelData[i + 1] = g;
                pixelData[i + 2] = r;
            }

            WriteableBitmap quantizedBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            quantizedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return quantizedBitmap;
        }
        private static byte QuantizeColor(byte color, int step)
        {
            return (byte)(Math.Round((double)color / step) * step);
        }
    }
}
