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
