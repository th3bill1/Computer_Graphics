using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal class Circle : Shape
{
    public Point Center;
    public double Radius;

    public override void Draw(WriteableBitmap bitmap, bool useAA)
    {
        if (useAA)
            DrawWithAA(bitmap);
        else
            DrawWithoutAA(bitmap);
    }

    private void DrawWithoutAA(WriteableBitmap bitmap)
    {
        int xc = (int)Center.X;
        int yc = (int)Center.Y;
        int r = (int)Radius;

        int d = 1 - r;
        int x = 0;
        int y = r;

        bitmap.Lock();

        PlotCirclePoints(bitmap, xc, yc, x, y);
        while (x < y)
        {
            if (d < 0)
            {
                d += 2 * x + 3;
            }
            else
            {
                d += 2 * (x - y) + 5;
                y--;
            }
            x++;
            PlotCirclePoints(bitmap, xc, yc, x, y);
        }

        int R = Math.Max(1, (int)Math.Round(Radius));
        Helpers.SafeAddDirtyRect(bitmap, new Int32Rect(xc - R, yc - R, 2 * R + 1, 2 * R + 1));
        bitmap.Unlock();
    }

    private void DrawWithAA(WriteableBitmap bitmap)
    {
        int xc = (int)Center.X;
        int yc = (int)Center.Y;
        int r = (int)Radius;
        int rCeil = (int)Math.Ceiling((decimal)r + Thickness);

        bitmap.Lock();

        for (int y = -rCeil; y <= rCeil; y++)
        {
            for (int x = -rCeil; x <= rCeil; x++)
            {
                double dist = Math.Sqrt(x * x + y * y);
                double diff = Math.Abs(dist - r);
                float coverage = Helpers.Coverage(Thickness, (float)diff, 0.5f);
                if (coverage > 0)
                {
                    int px = xc + x;
                    int py = yc + y;
                    if (px >= 0 && px < bitmap.PixelWidth && py >= 0 && py < bitmap.PixelHeight)
                    {
                        Color blended = Helpers.Lerp(Colors.White, Color, coverage);
                        unsafe
                        {
                            byte* pPixel = (byte*)bitmap.BackBuffer + py * bitmap.BackBufferStride + px * 4;
                            pPixel[0] = blended.B;
                            pPixel[1] = blended.G;
                            pPixel[2] = blended.R;
                            pPixel[3] = 255;
                        }
                    }
                }
            }
        }

        int R = Math.Max(1, (int)Math.Round(Radius));
        Helpers.SafeAddDirtyRect(bitmap, new Int32Rect(xc - R, yc - R, 2 * R + 1, 2 * R + 1));
        bitmap.Unlock();
    }

    private void PlotCirclePoints(WriteableBitmap bmp, int xc, int yc, int x, int y)
    {
        Plot8Symmetric(bmp, xc, yc, x, y);
    }

    private void Plot8Symmetric(WriteableBitmap bmp, int xc, int yc, int x, int y)
    {
        PutPixel(bmp, xc + x, yc + y);
        PutPixel(bmp, xc - x, yc + y);
        PutPixel(bmp, xc + x, yc - y);
        PutPixel(bmp, xc - x, yc - y);
        PutPixel(bmp, xc + y, yc + x);
        PutPixel(bmp, xc - y, yc + x);
        PutPixel(bmp, xc + y, yc - x);
        PutPixel(bmp, xc - y, yc - x);
    }

    private unsafe void PutPixel(WriteableBitmap bmp, int x, int y)
    {
        if (x < 0 || x >= bmp.PixelWidth || y < 0 || y >= bmp.PixelHeight)
            return;

        byte* pPixel = (byte*)bmp.BackBuffer + y * bmp.BackBufferStride + x * 4;
        pPixel[0] = Color.B;
        pPixel[1] = Color.G;
        pPixel[2] = Color.R;
        pPixel[3] = 255;
    }
    public override bool IsClose(Point p, double threshold = 5)
    {
        if(Helpers.IsNear(Center, p, threshold)) return true;
        double distance = Helpers.PythDistance(Center, new Point(p.X, p.Y));
        return Math.Abs(distance - Radius) <= threshold;
    }
}
