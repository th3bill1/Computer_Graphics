using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;

internal class Polygon : Shape
{
    public List<Point> Vertices = [];

    public Color? FillColor { get; set; } = null;
    [JsonIgnore]
    public WriteableBitmap? FillImage { get; set; } = null;
    [JsonIgnore]
    public Rectangle? ClippingRectangle { get; set; } = null;
    public string? FillImagePath
    {
        get
        {
            if (FillImage == null)
                return null;
            BitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(FillImage));
            using var fileStream = File.Create($"C:\\Users\\Dell\\Downloads\\FillImage-{Guid.NewGuid()}.png");
            encoder.Save(fileStream);
            return fileStream.Name;
        }
        set
        {
            if (!string.IsNullOrEmpty(value) && File.Exists(value))
                FillImage = new WriteableBitmap(new BitmapImage(new Uri(value)));
            else
                FillImage = null;
        }
    }


    public override void Draw(WriteableBitmap bitmap, bool useAA)
    {
        if (useAA)
            DrawWithAA(bitmap);
        else
            DrawWithoutAA(bitmap);
        DrawClippedFragments(bitmap, Colors.Red, useAA);

        if (FillColor != null || FillImage != null)
            Fill(bitmap);
    }

    private void DrawWithoutAA(WriteableBitmap bitmap)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            Point start = Vertices[i];
            Point end = Vertices[(i + 1) % Vertices.Count];
            new Line { Start = start, End = end, Color = Color, Thickness = Thickness }.Draw(bitmap, false);
        }
    }

    private void DrawWithAA(WriteableBitmap bitmap)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            Point start = Vertices[i];
            Point end = Vertices[(i + 1) % Vertices.Count];
            new Line { Start = start, End = end, Color = Color, Thickness = Thickness }.Draw(bitmap, true);
        }
    }

    public void Fill(WriteableBitmap bitmap)
    {
        if (Vertices.Count < 3) return;

        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;

        // Build Edge Table
        Dictionary<int, List<Edge>> edgeTable = new();
        for (int i = 0; i < Vertices.Count; i++)
        {
            Point a = Vertices[i];
            Point b = Vertices[(i + 1) % Vertices.Count];

            if (a.Y == b.Y) continue;

            Point upper = a.Y < b.Y ? a : b;
            Point lower = a.Y < b.Y ? b : a;

            int yMin = (int)Math.Ceiling(upper.Y);
            int yMax = (int)Math.Ceiling(lower.Y);
            double x = upper.X;
            double invSlope = (lower.X - upper.X) / (lower.Y - upper.Y);

            if (!edgeTable.ContainsKey(yMin))
                edgeTable[yMin] = new List<Edge>();

            edgeTable[yMin].Add(new Edge { YMax = yMax, X = x, InvSlope = invSlope });
        }

        int scanY = edgeTable.Keys.Min();
        List<Edge> activeEdges = new();

        bitmap.Lock();

        while (edgeTable.Count > 0 || activeEdges.Count > 0)
        {
            if (edgeTable.TryGetValue(scanY, out var newEdges))
            {
                activeEdges.AddRange(newEdges);
                edgeTable.Remove(scanY);
            }
            activeEdges.RemoveAll(e => e.YMax <= scanY);

            activeEdges.Sort((a, b) => a.X.CompareTo(b.X));

            for (int i = 0; i < activeEdges.Count - 1; i += 2)
            {
                int xStart = (int)Math.Round(activeEdges[i].X);
                int xEnd = (int)Math.Round(activeEdges[i + 1].X);

                for (int x = xStart; x < xEnd; x++)
                {
                    if (x < 0 || x >= width || scanY < 0 || scanY >= height)
                        continue;

                    Color fill = FillColor ?? Color;

                    if (FillImage != null)
                    {
                        int fx = x % FillImage.PixelWidth;
                        int fy = scanY % FillImage.PixelHeight;

                        unsafe
                        {
                            byte* src = (byte*)FillImage.BackBuffer + fy * FillImage.BackBufferStride + fx * 4;
                            fill = Color.FromRgb(src[2], src[1], src[0]);
                        }
                    }

                    unsafe
                    {
                        byte* pPixel = (byte*)bitmap.BackBuffer + scanY * bitmap.BackBufferStride + x * 4;
                        pPixel[0] = fill.B;
                        pPixel[1] = fill.G;
                        pPixel[2] = fill.R;
                        pPixel[3] = 255;
                    }
                }
            }

            foreach (var edge in activeEdges)
                edge.X += edge.InvSlope;

            scanY++;
        }

        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        bitmap.Unlock();
    }

    private class Edge
    {
        public int YMax;
        public double X;
        public double InvSlope;
    }

    public override bool IsClose(Point p, double threshold = 5)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            Point start = Vertices[i];
            Point end = Vertices[(i + 1) % Vertices.Count];
            var line = new Line { Start = start, End = end, Color = Color, Thickness = Thickness };
            if (line.IsClose(p, threshold))
                return true;
        }
        return false;
    }
    public void DrawClippedFragments(WriteableBitmap bitmap, Color highlightColor, bool useAA)
    {
        if (ClippingRectangle == null || Vertices.Count < 2)
            return;

        double xMin = Math.Min(ClippingRectangle.TopLeft.X, ClippingRectangle.BottomRight.X);
        double xMax = Math.Max(ClippingRectangle.TopLeft.X, ClippingRectangle.BottomRight.X);
        double yMin = Math.Min(ClippingRectangle.TopLeft.Y, ClippingRectangle.BottomRight.Y);
        double yMax = Math.Max(ClippingRectangle.TopLeft.Y, ClippingRectangle.BottomRight.Y);
        for (int i = 0; i < Vertices.Count; i++)
        {
            Point p0 = Vertices[i];
            Point p1 = Vertices[(i + 1) % Vertices.Count];
            DrawLiangBarsky(p0, p1, xMin, xMax, yMin, yMax, highlightColor, bitmap, useAA);
        }
    }
    private void DrawLiangBarsky(Point p1, Point p2, double xmin, double xmax, double ymin, double ymax, Color highlightColor, WriteableBitmap bitmap, bool useAA)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;

        List<double> denoms = [ -dx, dx, -dy, dy ];
        List<double> nums = [p1.X - xmin, xmax - p1.X, p1.Y - ymin, ymax - p1.Y];

        double tEnter = 0.0;
        double tExit = 1.0;

        for (int i = 0; i < 4; i++)
        {
            if (denoms[i] == 0)
            {
                if (nums[i] < 0)
                    return;
            }
            else
            {
                double t = nums[i] / denoms[i];
                if (denoms[i] < 0)
                {
                    if (t > tEnter) tEnter = t;
                }
                else
                {
                    if (t < tExit) tExit = t;
                }
            }
        }

        if (tEnter > tExit)
            return; 

        Point clippedStart = new(
            p1.X + tEnter * dx,
            p1.Y + tEnter * dy
        );

        Point clippedEnd = new(
            p1.X + tExit * dx,
            p1.Y + tExit * dy
        );

        var clippedLine = new Line
        {
            Start = clippedStart,
            End = clippedEnd,
            Color = highlightColor,
            Thickness = Thickness + 2
        };
        clippedLine.Draw(bitmap, useAA);
    }
}
