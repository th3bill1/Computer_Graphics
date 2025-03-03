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

            List<(byte input, byte output)> keyPoints = [];

            foreach (string line in File.ReadAllLines(filePath))
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2 &&
                    byte.TryParse(parts[0], out byte input) &&
                    byte.TryParse(parts[1], out byte output))
                {
                    keyPoints.Add((input, output));
                }
            }

            if (!keyPoints.Any(p => p.input == 0)) keyPoints.Insert(0, (0, 0));
            if (!keyPoints.Any(p => p.input == 255)) keyPoints.Add((255, 255));

            keyPoints = keyPoints.OrderBy(p => p.input).ToList();

            return value => Interpolate(value, keyPoints);
        }

        private static byte Interpolate(byte value, List<(byte input, byte output)> keyPoints)
        {
            for (int i = 0; i < keyPoints.Count - 1; i++)
            {
                var (x1, y1) = keyPoints[i];
                var (x2, y2) = keyPoints[i + 1];

                if (value >= x1 && value <= x2)
                {
                    double ratio = (value - x1) / (double)(x2 - x1);
                    return (byte)(y1 + ratio * (y2 - y1));
                }
            }
            return value;
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
