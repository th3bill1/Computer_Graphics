using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal class Line : Shape
{
    public Point Start;
    public Point End;
    public override void Draw(WriteableBitmap bitmap, bool useAA)
    {
        Start = new Point(
        Math.Clamp(Start.X, 0, bitmap.PixelWidth - 1),
        Math.Clamp(Start.Y, 0, bitmap.PixelHeight - 1));

        End = new Point(
            Math.Clamp(End.X, 0, bitmap.PixelWidth - 1),
            Math.Clamp(End.Y, 0, bitmap.PixelHeight - 1));
        if (useAA)
            DrawWithAA(bitmap);
        else
            DrawWithoutAA(bitmap);
    }
    private void DrawWithoutAA(WriteableBitmap bitmap)
    {
        int r = Thickness / 2;
        int diameter = 2 * r + 1;
        bool[,] brush = GenerateCircularBrush(r);

        int x0 = (int)Start.X;
        int y0 = (int)Start.Y;
        int x1 = (int)End.X;
        int y1 = (int)End.Y;

        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            ApplyBrush(bitmap, x0, y0, brush, Color);

            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
    private void DrawWithAA(WriteableBitmap bmp)
    {
        int x0 = (int)Start.X;
        int y0 = (int)Start.Y;
        int x1 = (int)End.X;
        int y1 = (int)End.Y;

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            (x0, y0) = (y0, x0);
            (x1, y1) = (y1, x1);
        }

        if (x0 > x1)
        {
            (x0, x1) = (x1, x0);
            (y0, y1) = (y1, y0);
        }

        int dx = x1 - x0;
        int dy = y1 - y0;
        int sy = dy >= 0 ? 1 : -1;
        dy = Math.Abs(dy);

        int dE = 2 * dy;
        int dNE = 2 * (dy - dx);
        int d = 2 * dy - dx;

        float invDenom = 1.0f / (2 * (float)Math.Sqrt(dx * dx + dy * dy));
        float two_dx_invDenom = 2 * dx * invDenom;

        int x = x0;
        int y = y0;

        bmp.Lock();

        PlotAA(bmp, x, y, Thickness, 0, steep);
        for (int i = 1; PlotAA(bmp, x, y + i * sy, Thickness, i * two_dx_invDenom, steep) > 0; ++i) ;
        for (int i = 1; PlotAA(bmp, x, y - i * sy, Thickness, i * two_dx_invDenom, steep) > 0; ++i) ;

        while (x < x1)
        {
            ++x;
            int two_v_dx;

            if (d < 0)
            {
                two_v_dx = d + dx;
                d += dE;
            }
            else
            {
                two_v_dx = d - dx;
                d += dNE;
                y += sy;
            }

            float dist = two_v_dx * invDenom;
            PlotAA(bmp, x, y, Thickness, dist, steep);

            for (int i = 1; PlotAA(bmp, x, y + i * sy, Thickness, i * two_dx_invDenom - dist, steep) > 0; ++i) ;
            for (int i = 1; PlotAA(bmp, x, y - i * sy, Thickness, i * two_dx_invDenom + dist, steep) > 0; ++i) ;
        }

        bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
    }

    private float PlotAA(WriteableBitmap bmp, int x, int y, float thickness, float distance, bool steep)
    {
        int px = steep ? y : x;
        int py = steep ? x : y;
        return IntensifyPixel(bmp, px, py, thickness, distance);
    }
    private float IntensifyPixel(WriteableBitmap bmp, int x, int y, float thickness, float distance)
    {
        if (x < 0 || x >= bmp.PixelWidth || y < 0 || y >= bmp.PixelHeight)
            return 0;

        float r = 0.5f;
        float coverage = Helpers.Coverage(thickness, distance, r);
        if (coverage <= 0)
            return 0;

        Color blended = Helpers.Lerp(Colors.White, Color, coverage);
        unsafe
        {
            byte* pPixel = (byte*)bmp.BackBuffer + y * bmp.BackBufferStride + x * 4;
            pPixel[0] = blended.B;
            pPixel[1] = blended.G;
            pPixel[2] = blended.R;
            pPixel[3] = 255;
        }
        return coverage;
    }
    private void ApplyBrush(WriteableBitmap bitmap, int centerX, int centerY, bool[,] brush, Color color)
    {
        int r = brush.GetLength(0) / 2;

        bitmap.Lock();
        unsafe
        {
            IntPtr pBackBuffer = bitmap.BackBuffer;
            int stride = bitmap.BackBufferStride;

            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    if (!brush[y + r, x + r]) continue;

                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < bitmap.PixelWidth && py >= 0 && py < bitmap.PixelHeight)
                    {
                        byte* pPixel = (byte*)pBackBuffer + py * stride + px * 4;
                        pPixel[0] = color.B;
                        pPixel[1] = color.G;
                        pPixel[2] = color.R;
                        pPixel[3] = 255;
                    }
                }
            }
        }
        bitmap.AddDirtyRect(new Int32Rect(centerX - r, centerY - r, 2 * r + 1, 2 * r + 1));
        bitmap.Unlock();
    }
    private static bool[,] GenerateCircularBrush(int radius)
    {
        int size = 2 * radius + 1;
        bool[,] brush = new bool[size, size];
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                    brush[y + radius, x + radius] = true;
            }
        }
        return brush;
    }
    public override bool IsClose(Point p, double threshold = 5)
    {
        var ax = Start.X;
        var ay = Start.Y;
        var bx = End.X;
        var by = End.Y;
        var px = p.X;
        var py = p.Y;

        var dx = bx - ax;
        var dy = by - ay;
        var lengthSq = dx * dx + dy * dy;

        if (lengthSq == 0)
            return Helpers.PythDistance(new Point(ax, ay), new Point(px, py)) <= threshold;

        var t = ((px - ax) * dx + (py - ay) * dy) / lengthSq;
        t = Math.Max(0, Math.Min(1, t));

        var projX = ax + t * dx;
        var projY = ay + t * dy;

        return Helpers.PythDistance(new Point(projX, projY), new Point(px, py)) <= threshold;
    }

}
