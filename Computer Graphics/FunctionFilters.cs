using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Computer_Graphics
{
    internal class FunctionFilters
    {
        public static Func<byte, byte> LoadFunctionFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Filter file not found!");

            byte[] lut = File.ReadAllText(filePath).Split(',').Select(byte.Parse).ToArray();
            return value => lut[value];
        }

        public static WriteableBitmap ApplyFunctionFromFile(WriteableBitmap bitmap, string filePath)
        {
            return ApplyFunctionFilter(bitmap, LoadFunctionFromFile(filePath));
        }

        public static WriteableBitmap ApplyFunctionFilter(WriteableBitmap bitmap, Func<byte, byte> filterFunction)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];
            bitmap.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = filterFunction(pixelData[i]);
                pixelData[i + 1] = filterFunction(pixelData[i + 1]);
                pixelData[i + 2] = filterFunction(pixelData[i + 2]);
            }

            WriteableBitmap filteredBitmap = new(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Bgra32, null);
            filteredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return filteredBitmap;
        }
    }
}
