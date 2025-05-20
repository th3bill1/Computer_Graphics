using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics.Task1and2;
internal class Dithering
{
    public enum FilterType
    {
        FloydSteinberg,
        Burkes,
        Stucki,
        Sierra,
        Atkinson
    }
    private static readonly Random random = new();
    public static WriteableBitmap ApplyRandomDithering(WriteableBitmap source, int numShades)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);

        var colorStep = 255 / (numShades - 1);

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var b = pixelData[i];
            var g = pixelData[i + 1];
            var r = pixelData[i + 2];

            var noise = random.Next(-colorStep / 2, colorStep / 2);
            r = QuantizeColor(r + noise, colorStep);
            g = QuantizeColor(g + noise, colorStep);
            b = QuantizeColor(b + noise, colorStep);

            pixelData[i] = b;
            pixelData[i + 1] = g;
            pixelData[i + 2] = r;
        }

        var ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return ditheredBitmap;
    }
    public static WriteableBitmap ApplyAverageDithering(WriteableBitmap source, int numShades)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);

        var totalPixels = width * height;
        long totalBrightness = 0;

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var b = pixelData[i];
            var g = pixelData[i + 1];
            var r = pixelData[i + 2];

            totalBrightness += (r + g + b) / 3;
        }

        var averageBrightness = (int)(totalBrightness / totalPixels);
        var colorStep = 255 / (numShades - 1);

        for (var i = 0; i < pixelData.Length; i += 4)
        {
            var b = pixelData[i];
            var g = pixelData[i + 1];
            var r = pixelData[i + 2];

            var pixelBrightness = (r + g + b) / 3;

            var threshold = averageBrightness;
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

        var ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
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
        var thresholdMap = GetBayerMatrix(matrixSize);

        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);

        var colorStep = 255 / (numShades - 1);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * stride + x * 4;

                var b = pixelData[index];
                var g = pixelData[index + 1];
                var r = pixelData[index + 2];

                var threshold = thresholdMap[y % matrixSize, x % matrixSize];

                var thresholdValue = threshold * 255 / (matrixSize * matrixSize);

                if (b > thresholdValue)
                    b = QuantizeColor(b + colorStep / 2, colorStep);
                else
                {
                    b = QuantizeColor(b - colorStep / 2, colorStep);
                }
                if (g > thresholdValue)
                    g = QuantizeColor(g + colorStep / 2, colorStep);
                else
                {
                    g = QuantizeColor(g - colorStep / 2, colorStep);
                }
                if (r > thresholdValue)
                    r = QuantizeColor(r + colorStep / 2, colorStep);
                else
                {
                    r = QuantizeColor(r - colorStep / 2, colorStep);
                }

                pixelData[index] = b;
                pixelData[index + 1] = g;
                pixelData[index + 2] = r;
            }
        }

        var ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
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
    public static WriteableBitmap ApplyErrorDiffusionDithering(WriteableBitmap source, int numShades, FilterType filterType)
    {
        var filter = GetErrorDiffusionFilter(filterType);

        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);

        var colorStep = 255 / (numShades - 1);
        var errorBuffer = new double[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * stride + x * 4;

                var b = pixelData[index];
                var g = pixelData[index + 1];
                var r = pixelData[index + 2];

                var oldGray = 0.299 * r + 0.587 * g + 0.114 * b + errorBuffer[x, y];
                var newGray = QuantizeColor((int)oldGray, colorStep);
                var error = oldGray - newGray;

                pixelData[index] = newGray;
                pixelData[index + 1] = newGray;
                pixelData[index + 2] = newGray;

                foreach (var (dx, dy, weight) in filter)
                {
                    var nx = x + dx;
                    var ny = y + dy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        errorBuffer[nx, ny] += error * weight;
                }
            }
        }

        var ditheredBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        ditheredBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return ditheredBitmap;
    }
    private static (int dx, int dy, double weight)[] GetErrorDiffusionFilter(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.FloydSteinberg => [
                (1, 0, 7.0 / 16), ( -1, 1, 3.0 / 16), (0, 1, 5.0 / 16), (1, 1, 1.0 / 16)
            ],
            FilterType.Burkes => [
                (1, 0, 8.0 / 32), (2, 0, 4.0 / 32), (-2, 1, 2.0 / 32), (-1, 1, 4.0 / 32), (0, 1, 8.0 / 32), (1, 1, 4.0 / 32), (2, 1, 2.0 / 32)
            ],
            FilterType.Stucki => [
                (1, 0, 8.0 / 42), (2, 0, 4.0 / 42), (-2, 1, 2.0 / 42), (-1, 1, 4.0 / 42), (0, 1, 8.0 / 42), (1, 1, 4.0 / 42), (2, 1, 2.0 / 42),
                (-2, 2, 1.0 / 42), (-1, 2, 2.0 / 42), (0, 2, 4.0 / 42), (1, 2, 2.0 / 42), (2, 2, 1.0 / 42)
            ],
            FilterType.Sierra => [
                (1, 0, 5.0 / 32), (2, 0, 3.0 / 32), (-2, 1, 2.0 / 32), (-1, 1, 4.0 / 32), (0, 1, 5.0 / 32), (1, 1, 4.0 / 32), (2, 1, 2.0 / 32),
                (-1, 2, 2.0 / 32), (0, 2, 3.0 / 32), (1, 2, 2.0 / 32)
            ],
            FilterType.Atkinson => [
                (1, 0, 1.0 / 8), (2, 0, 1.0 / 8), (-1, 1, 1.0 / 8), (0, 1, 1.0 / 8), (1, 1, 1.0 / 8), (0, 2, 1.0 / 8)
            ],
            _ => throw new ArgumentException("Invalid filter type.")
        };
    }
}