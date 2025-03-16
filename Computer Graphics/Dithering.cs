using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics
{
    internal class Dithering
    {
        private static readonly Random random = new();
        public static WriteableBitmap ApplyRandomDithering(WriteableBitmap source, int numShades)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            int colorStep = 255 / (numShades - 1);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte b = pixelData[i];
                byte g = pixelData[i + 1];
                byte r = pixelData[i + 2];

                int noise = random.Next(-colorStep / 2, colorStep / 2);
                r = QuantizeColor(r + noise, colorStep);
                g = QuantizeColor(g + noise, colorStep);
                b = QuantizeColor(b + noise, colorStep);

                pixelData[i] = b;
                pixelData[i + 1] = g;
                pixelData[i + 2] = r;
            }

            WriteableBitmap ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return ditheredBitmap;
        }
        public static WriteableBitmap ApplyAverageDithering(WriteableBitmap source, int numShades)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            int totalPixels = width * height;
            long totalBrightness = 0;

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte b = pixelData[i];
                byte g = pixelData[i + 1];
                byte r = pixelData[i + 2];

                totalBrightness += (r + g + b) / 3;
            }

            int averageBrightness = (int)(totalBrightness / totalPixels);
            int colorStep = 255 / (numShades - 1);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte b = pixelData[i];
                byte g = pixelData[i + 1];
                byte r = pixelData[i + 2];

                int pixelBrightness = (r + g + b) / 3;

                int threshold = averageBrightness;
                if (pixelBrightness > threshold)
                {
                    r = QuantizeColor(r + colorStep / 2, colorStep);
                    g = QuantizeColor(g + colorStep / 2, colorStep);
                    b = QuantizeColor(b + colorStep / 2, colorStep);
                }
                else
                {
                    r = QuantizeColor(r - colorStep / 2, colorStep);
                    g = QuantizeColor(g - colorStep / 2, colorStep);
                    b = QuantizeColor(b - colorStep / 2, colorStep);
                }

                pixelData[i] = b;
                pixelData[i + 1] = g;
                pixelData[i + 2] = r;
            }

            WriteableBitmap ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return ditheredBitmap;
        }
        private static byte QuantizeColor(int color, int step)
        {
            color = Math.Max(0, Math.Min(255, color));
            return (byte)(Math.Round((double)color / step) * step);
        }
        public static WriteableBitmap ApplyOrderedDithering(WriteableBitmap source, int numShades, int matrixSize)
        {
            int[,] thresholdMap = GetBayerMatrix(matrixSize);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            int colorStep = 255 / (numShades - 1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * stride) + (x * 4);

                    byte b = pixelData[index];
                    byte g = pixelData[index + 1];
                    byte r = pixelData[index + 2];

                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                    int threshold = thresholdMap[y % matrixSize, x % matrixSize];

                    int thresholdValue = (threshold * 255) / (matrixSize * matrixSize);

                    if (gray > thresholdValue)
                    {
                        gray = QuantizeColor(gray + colorStep / 2, colorStep);
                    }
                    else
                    {
                        gray = QuantizeColor(gray - colorStep / 2, colorStep);
                    }

                    pixelData[index] = gray;
                    pixelData[index + 1] = gray;
                    pixelData[index + 2] = gray;
                }
            }

            WriteableBitmap ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return ditheredBitmap;
        }
        private static int[,] GetBayerMatrix(int size)
        {
            return size switch
            {
                2 => new int[,] { { 0, 2 }, { 3, 1 } },
                3 => new int[,] { { 0, 7, 3 }, { 6, 5, 2 }, { 4, 1, 8 } },
                4 => new int[,] { { 0, 8, 2, 10 }, { 12, 4, 14, 6 }, { 3, 11, 1, 9 }, { 15, 7, 13, 5 } },
                6 => new int[,] {
                    { 0, 32, 8, 40, 2, 34 },
                    { 48, 16, 56, 24, 50, 18 },
                    { 12, 44, 4, 36, 14, 46 },
                    { 60, 28, 52, 20, 62, 30 },
                    { 3, 35, 11, 43, 1, 33 },
                    { 51, 19, 59, 27, 49, 17 }
                },
                _ => throw new ArgumentException("Invalid Bayer matrix size. Choose 2, 3, 4, or 6."),
            };
        }
        public static WriteableBitmap ApplyErrorDiffusionDithering(WriteableBitmap source, int numShades, string filterType)
        {
            (int dx, int dy, double weight)[] filter = GetErrorDiffusionFilter(filterType);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            int colorStep = 255 / (numShades - 1);
            double[,] errorBuffer = new double[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * stride) + (x * 4);

                    byte b = pixelData[index];
                    byte g = pixelData[index + 1];
                    byte r = pixelData[index + 2];

                    double oldGray = 0.299 * r + 0.587 * g + 0.114 * b + errorBuffer[x, y];
                    byte newGray = QuantizeColor((int)oldGray, colorStep);
                    double error = oldGray - newGray;

                    pixelData[index] = newGray;
                    pixelData[index + 1] = newGray;
                    pixelData[index + 2] = newGray;

                    foreach (var (dx, dy, weight) in filter)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            errorBuffer[nx, ny] += error * weight;
                        }
                    }
                }
            }

            WriteableBitmap ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return ditheredBitmap;
        }
        private static (int dx, int dy, double weight)[] GetErrorDiffusionFilter(string filterType)
        {
            return filterType switch
            {
                "Floyd-Steinberg" => [
                    (1, 0, 7.0 / 16), ( -1, 1, 3.0 / 16), (0, 1, 5.0 / 16), (1, 1, 1.0 / 16)
                ],
                "Burkes" => [
                    (1, 0, 8.0 / 32), (2, 0, 4.0 / 32), (-2, 1, 2.0 / 32), (-1, 1, 4.0 / 32), (0, 1, 8.0 / 32), (1, 1, 4.0 / 32), (2, 1, 2.0 / 32)
                ],
                "Stucki" => [
                    (1, 0, 8.0 / 42), (2, 0, 4.0 / 42), (-2, 1, 2.0 / 42), (-1, 1, 4.0 / 42), (0, 1, 8.0 / 42), (1, 1, 4.0 / 42), (2, 1, 2.0 / 42),
                    (-2, 2, 1.0 / 42), (-1, 2, 2.0 / 42), (0, 2, 4.0 / 42), (1, 2, 2.0 / 42), (2, 2, 1.0 / 42)
                ],
                "Sierra" => [
                    (1, 0, 5.0 / 32), (2, 0, 3.0 / 32), (-2, 1, 2.0 / 32), (-1, 1, 4.0 / 32), (0, 1, 5.0 / 32), (1, 1, 4.0 / 32), (2, 1, 2.0 / 32),
                    (-1, 2, 2.0 / 32), (0, 2, 3.0 / 32), (1, 2, 2.0 / 32)
                ],
                "Atkinson" => [
                    (1, 0, 1.0 / 8), (2, 0, 1.0 / 8), (-1, 1, 1.0 / 8), (0, 1, 1.0 / 8), (1, 1, 1.0 / 8), (0, 2, 1.0 / 8)
                ],
                _ => throw new ArgumentException("Invalid filter type.")
            };
        }
    }
}


// Check if the functions should first convert to gray or assume its gray already