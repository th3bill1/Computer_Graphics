using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal class ColorQuantization
{
    private static readonly Random random = new();
    public static WriteableBitmap ApplyUniformQuantization(WriteableBitmap source, int rDivisions, int gDivisions, int bDivisions)
    {
        byte[] pixelData = GetPixelData(source, out int width, out int height, out int stride);

        int rStep = 256 / rDivisions, gStep = 256 / gDivisions, bStep = 256 / bDivisions;

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            pixelData[i + 2] = QuantizeColor(pixelData[i + 2], rStep);
            pixelData[i + 1] = QuantizeColor(pixelData[i + 1], gStep);
            pixelData[i] = QuantizeColor(pixelData[i], bStep);
        }

        return CreateBitmapFromPixelData(pixelData, width, height, stride, source);
    }
    public static WriteableBitmap ApplyPopularityQuantization(WriteableBitmap source, int numColors, int subdivisions = 4)
    {
        byte[] pixelData = GetPixelData(source, out int width, out int height, out int stride);
        var colorBins = GetColorBins(pixelData, subdivisions);

        var mostFrequentColors = GetTopColorsFromBins(colorBins, numColors);
        MapPixelsToNearestColor(pixelData, mostFrequentColors);

        return CreateBitmapFromPixelData(pixelData, width, height, stride, source);
    }
    public static WriteableBitmap ApplyKMeansQuantization(WriteableBitmap source, int numClusters)
    {
        byte[] pixelData = GetPixelData(source, out int width, out int height, out int stride);
        var pixels = ExtractPixels(pixelData);

        var clusters = RunKMeansClustering(pixels, numClusters);
        MapPixelsToNearestColor(pixelData, clusters);

        return CreateBitmapFromPixelData(pixelData, width, height, stride, source);
    }
    public static WriteableBitmap ApplyMedianCutQuantization(WriteableBitmap source, int numColors)
    {
        byte[] pixelData = GetPixelData(source, out int width, out int height, out int stride);
        var pixels = ExtractPixels(pixelData);

        var colorPalette = MedianCut(pixels, numColors);
        MapPixelsToNearestColor(pixelData, colorPalette);

        return CreateBitmapFromPixelData(pixelData, width, height, stride, source);
    }
    public static WriteableBitmap ApplyOctreeQuantization(WriteableBitmap source, int numColors)
    {
        byte[] pixelData = GetPixelData(source, out int width, out int height, out int stride);
        OctreeColorQuantizer octree = new();
        Dictionary<(byte, byte, byte), (byte, byte, byte)> colorMap = [];

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            var color = (pixelData[i + 2], pixelData[i + 1], pixelData[i]);
            octree.AddColor(color);
        }

        var palette = octree.GeneratePalette(numColors);
        MapPixelsToNearestColor(pixelData, palette);

        return CreateBitmapFromPixelData(pixelData, width, height, stride, source);
    }

    private static byte[] GetPixelData(WriteableBitmap source, out int width, out int height, out int stride)
    {
        width = source.PixelWidth;
        height = source.PixelHeight;
        stride = width * 4;
        byte[] pixelData = new byte[height * stride];

        source.CopyPixels(pixelData, stride, 0);
        return pixelData;
    }
    private static WriteableBitmap CreateBitmapFromPixelData(byte[] pixelData, int width, int height, int stride, WriteableBitmap source)
    {
        WriteableBitmap quantizedBitmap = new(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        quantizedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
        return quantizedBitmap;
    }
    private static byte QuantizeColor(byte color, int step)
    {
        return (byte)(Math.Round((double)color / step) * step);
    }
    private static Dictionary<(int, int, int), List<int>> GetColorBins(byte[] pixelData, int subdivisions)
    {
        int binSize = 256 / subdivisions;
        Dictionary<(int, int, int), List<int>> colorBins = [];

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            int r = pixelData[i + 2];
            int g = pixelData[i + 1];
            int b = pixelData[i];

            var binKey = (r / binSize, g / binSize, b / binSize);
            colorBins.TryAdd(binKey, []);
            colorBins[binKey].Add((r << 16) | (g << 8) | b);
        }

        return colorBins;
    }
    private static List<(byte, byte, byte)> GetTopColorsFromBins(Dictionary<(int, int, int), List<int>> bins, int numColors)
    {
        return bins.OrderByDescending(c => c.Value.Count)
                   .Take(numColors)
                   .Select(bin =>
                   {
                       var colors = bin.Value.GroupBy(c => c)
                                             .OrderByDescending(g => g.Count())
                                             .First().Key;
                       return (
                           (byte)(colors & 0xFF),
                           (byte)((colors >> 8) & 0xFF),
                           (byte)((colors >> 16) & 0xFF)
                       );
                   }).ToList();
    }

    private static List<(byte, byte, byte)> ExtractPixels(byte[] pixelData)
    {
        List<(byte, byte, byte)> pixels = [];
        for (int i = 0; i < pixelData.Length; i += 4)
            pixels.Add((pixelData[i + 2], pixelData[i + 1], pixelData[i]));

        return pixels;
    }
    private static void MapPixelsToNearestColor(byte[] pixelData, List<(byte, byte, byte)> palette)
    {
        for (int i = 0; i < pixelData.Length; i += 4)
        {
            var originalColor = (pixelData[i + 2], pixelData[i + 1], pixelData[i]);
            var closestColor = palette.OrderBy(c => ColorDistance(originalColor, c)).First();

            pixelData[i] = closestColor.Item3;
            pixelData[i + 1] = closestColor.Item2;
            pixelData[i + 2] = closestColor.Item1;
        }
    }
    private static List<(byte, byte, byte)> RunKMeansClustering(List<(byte, byte, byte)> pixels, int numClusters)
    {
        var clusters = pixels.OrderBy(_ => random.Next()).Take(numClusters).ToList();
        bool changed;
        Dictionary<(byte, byte, byte), (byte, byte, byte)> pixelClusterMapping = new();

        do
        {
            changed = false;
            Dictionary<(byte, byte, byte), List<(byte, byte, byte)>> clusterGroups = clusters.ToDictionary(c => c, _ => new List<(byte, byte, byte)>());

            foreach (var pixel in pixels)
            {
                var closestCluster = clusters.OrderBy(c => ColorDistance(pixel, c)).First();
                changed |= !pixelClusterMapping.TryGetValue(pixel, out var mappedCluster) || mappedCluster != closestCluster;
                pixelClusterMapping[pixel] = closestCluster;
                clusterGroups[closestCluster].Add(pixel);
            }

            clusters = clusterGroups.Where(c => c.Value.Count > 0)
                                    .Select(c => (
                                        (byte)c.Value.Average(p => p.Item1),
                                        (byte)c.Value.Average(p => p.Item2),
                                        (byte)c.Value.Average(p => p.Item3)
                                    ))
                                    .ToList();

        } while (changed);

        return clusters;
    }
    private static List<(byte, byte, byte)> MedianCut(List<(byte, byte, byte)> colors, int numColors)
    {
        Queue<List<(byte, byte, byte)>> colorGroups = new();
        colorGroups.Enqueue(colors);

        while (colorGroups.Count < numColors)
        {
            var largestGroup = colorGroups.OrderByDescending(g => g.Count).First();
            colorGroups = new Queue<List<(byte, byte, byte)>>(colorGroups.Where(g => g != largestGroup));

            if (largestGroup.Count < 2) break;

            int rangeR = largestGroup.Max(c => c.Item1) - largestGroup.Min(c => c.Item1);
            int rangeG = largestGroup.Max(c => c.Item2) - largestGroup.Min(c => c.Item2);
            int rangeB = largestGroup.Max(c => c.Item3) - largestGroup.Min(c => c.Item3);

            int splitChannel = (rangeR >= rangeG && rangeR >= rangeB) ? 0 : (rangeG >= rangeB) ? 1 : 2;

            largestGroup = largestGroup.OrderBy(c => splitChannel == 0 ? c.Item1 : splitChannel == 1 ? c.Item2 : c.Item3).ToList();
            int medianIndex = largestGroup.Count / 2;

            colorGroups.Enqueue(largestGroup.Take(medianIndex).ToList());
            colorGroups.Enqueue(largestGroup.Skip(medianIndex).ToList());
        }

        return colorGroups.Select(g => (
            (byte)g.Average(c => c.Item1),
            (byte)g.Average(c => c.Item2),
            (byte)g.Average(c => c.Item3)
        )).ToList();
    }
    private static int ColorDistance((byte, byte, byte) c1, (byte, byte, byte) c2)
    {
        return (c1.Item1 - c2.Item1) * (c1.Item1 - c2.Item1) +
               (c1.Item2 - c2.Item2) * (c1.Item2 - c2.Item2) +
               (c1.Item3 - c2.Item3) * (c1.Item3 - c2.Item3);
    }
}
