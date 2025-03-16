using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics
{
    internal class ColorQuantization
    {
        private static readonly Random random = new();
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

        public static WriteableBitmap ApplyPopularityQuantization(WriteableBitmap source, int numColors)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            Dictionary<int, int> colorFrequency = new();

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                int color = (pixelData[i + 2] << 16) | (pixelData[i + 1] << 8) | pixelData[i];
                if (colorFrequency.ContainsKey(color))
                    colorFrequency[color]++;
                else
                    colorFrequency[color] = 1;
            }

            var mostFrequentColors = colorFrequency.OrderByDescending(c => c.Value)
                                                   .Take(numColors)
                                                   .Select(c => (
                                                        (byte)(c.Key & 0xFF),
                                                        (byte)((c.Key >> 8) & 0xFF),
                                                        (byte)((c.Key >> 16) & 0xFF)
                                                    ))
                                                   .ToArray();

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte b = pixelData[i];
                byte g = pixelData[i + 1];
                byte r = pixelData[i + 2];

                var closestColor = mostFrequentColors.OrderBy(c => ColorDistance(r, g, b, c.Item3, c.Item2, c.Item1)).First();

                pixelData[i] = closestColor.Item1;
                pixelData[i + 1] = closestColor.Item2;
                pixelData[i + 2] = closestColor.Item3;
            }

            WriteableBitmap quantizedBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            quantizedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return quantizedBitmap;
        }
        public static WriteableBitmap ApplyKMeansQuantization(WriteableBitmap source, int numClusters)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            List<(byte R, byte G, byte B)> pixels = new();
            for (int i = 0; i < pixelData.Length; i += 4)
                pixels.Add((pixelData[i + 2], pixelData[i + 1], pixelData[i]));

            var clusters = pixels.OrderBy(_ => random.Next()).Take(numClusters).ToList();

            bool changed;
            Dictionary<(byte R, byte G, byte B), (byte R, byte G, byte B)> pixelClusterMapping = new();

            do
            {
                changed = false;
                Dictionary<(byte R, byte G, byte B), List<(byte R, byte G, byte B)>> newClusters = clusters.ToDictionary(c => c, _ => new List<(byte R, byte G, byte B)>());

                foreach (var pixel in pixels)
                {
                    var closestCluster = clusters.OrderBy(c => ColorDistance(pixel, c)).First();

                    if (!pixelClusterMapping.ContainsKey(pixel) || pixelClusterMapping[pixel] != closestCluster)
                    {
                        changed = true;
                        pixelClusterMapping[pixel] = closestCluster;
                    }

                    newClusters[closestCluster].Add(pixel);
                }

                clusters = newClusters.Where(c => c.Value.Count != 0)
                                      .Select(c => (
                                          (byte)(c.Value.Average(p => p.R)),
                                          (byte)(c.Value.Average(p => p.G)),
                                          (byte)(c.Value.Average(p => p.B))
                                      ))
                                      .ToList();

            } while (changed);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                var originalColor = (pixelData[i + 2], pixelData[i + 1], pixelData[i]);
                var mappedColor = pixelClusterMapping[originalColor];

                pixelData[i] = mappedColor.B;
                pixelData[i + 1] = mappedColor.G;
                pixelData[i + 2] = mappedColor.R;
            }

            WriteableBitmap quantizedBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            quantizedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return quantizedBitmap;
        }
        public static WriteableBitmap ApplyMedianCutQuantization(WriteableBitmap source, int numColors)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            List<(byte R, byte G, byte B)> pixels = new();
            for (int i = 0; i < pixelData.Length; i += 4)
                pixels.Add((pixelData[i + 2], pixelData[i + 1], pixelData[i]));

            var colorPalette = MedianCut(pixels, numColors);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                var originalColor = (pixelData[i + 2], pixelData[i + 1], pixelData[i]);
                var closestColor = colorPalette.OrderBy(c => ColorDistance(originalColor, c)).First();

                pixelData[i] = closestColor.B;
                pixelData[i + 1] = closestColor.G;
                pixelData[i + 2] = closestColor.R;
            }

            WriteableBitmap quantizedBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            quantizedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return quantizedBitmap;
        }
        private static List<(byte R, byte G, byte B)> MedianCut(List<(byte R, byte G, byte B)> colors, int numColors)
        {
            Queue<List<(byte R, byte G, byte B)>> colorGroups = new();
            colorGroups.Enqueue(colors);

            while (colorGroups.Count < numColors)
            {
                var largestGroup = colorGroups.OrderByDescending(g => g.Count).First();
                colorGroups = new Queue<List<(byte R, byte G, byte B)>>(colorGroups.Where(g => g != largestGroup));

                if (largestGroup.Count < 2) break;

                int rangeR = largestGroup.Max(c => c.R) - largestGroup.Min(c => c.R);
                int rangeG = largestGroup.Max(c => c.G) - largestGroup.Min(c => c.G);
                int rangeB = largestGroup.Max(c => c.B) - largestGroup.Min(c => c.B);

                var splitChannel = (rangeR >= rangeG && rangeR >= rangeB) ? 0 : (rangeG >= rangeB) ? 1 : 2;

                largestGroup = largestGroup.OrderBy(c => splitChannel == 0 ? c.R : splitChannel == 1 ? c.G : c.B).ToList();
                int medianIndex = largestGroup.Count / 2;

                colorGroups.Enqueue(largestGroup.Take(medianIndex).ToList());
                colorGroups.Enqueue(largestGroup.Skip(medianIndex).ToList());
            }
            return colorGroups.Select(g => (
                (byte)g.Average(c => c.R),
                (byte)g.Average(c => c.G),
                (byte)g.Average(c => c.B)
            )).ToList();
        }
        public static WriteableBitmap ApplyOctreeQuantization(WriteableBitmap source, int numColors)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            source.CopyPixels(pixelData, stride, 0);

            OctreeColorQuantizer octree = new();
            Dictionary<(byte, byte, byte), (byte, byte, byte)> colorMap = [];

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                var color = (pixelData[i + 2], pixelData[i + 1], pixelData[i]);
                octree.AddColor(color);
            }

            var palette = octree.GeneratePalette(numColors);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                var originalColor = (pixelData[i + 2], pixelData[i + 1], pixelData[i]);

                if (!colorMap.TryGetValue(originalColor, out var mappedColor))
                {
                    mappedColor = palette.OrderBy(c => ColorDistance(originalColor, c)).First();
                    colorMap[originalColor] = mappedColor;
                }

                pixelData[i] = mappedColor.Item3;
                pixelData[i + 1] = mappedColor.Item2;
                pixelData[i + 2] = mappedColor.Item1;
            }

            WriteableBitmap quantizedBitmap = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
            quantizedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

            return quantizedBitmap;
        }

        private static int ColorDistance(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            return (r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2);
        }
        private static int ColorDistance((byte R, byte G, byte B) c1, (byte R, byte G, byte B) c2)
        {
            return (c1.R - c2.R) * (c1.R - c2.R) +
                   (c1.G - c2.G) * (c1.G - c2.G) +
                   (c1.B - c2.B) * (c1.B - c2.B);
        }
    }
}
