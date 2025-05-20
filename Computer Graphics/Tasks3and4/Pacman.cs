using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal class Pacman : Shape
{
    public Point Center;
    public Point Mouth1;
    public Point Mouth2;

    public override void Draw(WriteableBitmap bitmap, bool useAA)
    {
        if (useAA)
            DrawWithAA(bitmap);
        else
            DrawWithoutAA(bitmap);
    }

    private void DrawWithAA(WriteableBitmap bitmap)
    {
        int xc = (int)Center.X;
        int yc = (int)Center.Y;

        double radius = Helpers.PythDistance(Mouth1, Center);
        int rCeil = (int)Math.Ceiling(radius + Thickness);

        double angleStart = NormalizeAngleRad(Math.Atan2(Mouth1.Y - Center.Y, Mouth1.X - Center.X));
        double angleEnd = NormalizeAngleRad(Math.Atan2(Mouth2.Y - Center.Y, Mouth2.X - Center.X));

        if (angleEnd <= angleStart)
            angleEnd += 2 * Math.PI;

        bitmap.Lock();

        for (int y = -rCeil; y <= rCeil; y++)
        {
            for (int x = -rCeil; x <= rCeil; x++)
            {
                double px = xc + x;
                double py = yc + y;

                double distToCenter = Math.Sqrt(x * x + y * y);
                double diff = Math.Abs(distToCenter - radius);
                float coverage = Helpers.Coverage(Thickness, (float)diff, 0.5f);

                if (coverage > 0)
                {
                    double angle = NormalizeAngleRad(Math.Atan2(y, x));
                    if (IsAngleBetween(angle, angleStart, angleEnd))
                    {
                        int ix = (int)px;
                        int iy = (int)py;
                        if (ix >= 0 && ix < bitmap.PixelWidth && iy >= 0 && iy < bitmap.PixelHeight)
                        {
                            Color blended = Helpers.Lerp(Colors.White, Color, coverage);
                            unsafe
                            {
                                byte* pPixel = (byte*)bitmap.BackBuffer + iy * bitmap.BackBufferStride + ix * 4;
                                pPixel[0] = blended.B;
                                pPixel[1] = blended.G;
                                pPixel[2] = blended.R;
                                pPixel[3] = 255;
                            }
                        }
                    }
                }
            }
        }

        Helpers.SafeAddDirtyRect(bitmap, new Int32Rect(xc - rCeil, yc - rCeil, 2 * rCeil + 1, 2 * rCeil + 1));
        bitmap.Unlock();

        // Draw mouth lines with AA
        Point arcStart = new(
            Center.X + radius * Math.Cos(angleStart),
            Center.Y + radius * Math.Sin(angleStart));

        Point arcEnd = new(
            Center.X + radius * Math.Cos(angleEnd),
            Center.Y + radius * Math.Sin(angleEnd));

        new Line
        {
            Start = Center,
            End = arcStart,
            Color = Color,
            Thickness = Thickness
        }.Draw(bitmap, true);

        new Line
        {
            Start = Center,
            End = arcEnd,
            Color = Color,
            Thickness = Thickness
        }.Draw(bitmap, true);
    }
    private void DrawWithoutAA(WriteableBitmap bitmap)
    {
        int xc = (int)Center.X;
        int yc = (int)Center.Y;

        double radius = Helpers.PythDistance(Mouth1, Center);
        int r = (int)Math.Round(radius);

        double angleStart = NormalizeAngleRad(Math.Atan2(Mouth1.Y - Center.Y, Mouth1.X - Center.X));
        double angleEnd = NormalizeAngleRad(Math.Atan2(Mouth2.Y - Center.Y, Mouth2.X - Center.X));

        if (angleEnd <= angleStart)
            angleEnd += 2 * Math.PI;

        int d = 1 - r;
        int x = 0;
        int y = r;

        bitmap.Lock();
        PlotCirclePointsArc(bitmap, xc, yc, x, y, angleStart, angleEnd);
        while (x < y)
        {
            x++;
            if (d < 0)
            {
                d += 2 * x + 1;
            }
            else
            {
                y--;
                d += 2 * (x - y) + 1;
            }
            PlotCirclePointsArc(bitmap, xc, yc, x, y, angleStart, angleEnd);
        }
        Helpers.SafeAddDirtyRect(bitmap, new Int32Rect(xc - r, yc - r, 2 * r + 1, 2 * r + 1));
        bitmap.Unlock();

        Point arcStart = new(
            Center.X + radius * Math.Cos(angleStart),
            Center.Y + radius * Math.Sin(angleStart));

        Point arcEnd = new(
            Center.X + radius * Math.Cos(angleEnd),
            Center.Y + radius * Math.Sin(angleEnd));

        new Line
        {
            Start = Center,
            End = arcStart,
            Color = Color,
            Thickness = Thickness
        }.Draw(bitmap, false);

        new Line
        {
            Start = Center,
            End = arcEnd,
            Color = Color,
            Thickness = Thickness
        }.Draw(bitmap, false);
    }
    private void PlotCirclePointsArc(WriteableBitmap bmp, int xc, int yc, int x, int y, double angleStart, double angleEnd)
    {
        PlotIfInArc(xc + x, yc + y);
        PlotIfInArc(xc - x, yc + y);
        PlotIfInArc(xc + x, yc - y);
        PlotIfInArc(xc - x, yc - y);
        PlotIfInArc(xc + y, yc + x);
        PlotIfInArc(xc - y, yc + x);
        PlotIfInArc(xc + y, yc - x);
        PlotIfInArc(xc - y, yc - x);

        void PlotIfInArc(int px, int py)
        {
            Vector v = new(px - Center.X, py - Center.Y);
            double angle = NormalizeAngleRad(Math.Atan2(v.Y, v.X));

            if (IsAngleBetween(angle, angleStart, angleEnd))
            {
                if (px >= 0 && px < bmp.PixelWidth && py >= 0 && py < bmp.PixelHeight)
                {
                    unsafe
                    {
                        byte* pPixel = (byte*)bmp.BackBuffer + py * bmp.BackBufferStride + px * 4;
                        pPixel[0] = Color.B;
                        pPixel[1] = Color.G;
                        pPixel[2] = Color.R;
                        pPixel[3] = 255;
                    }
                }
            }
        }
    }
    private static double NormalizeAngleRad(double angle)
    {
        while (angle < 0) angle += 2 * Math.PI;
        while (angle >= 2 * Math.PI) angle -= 2 * Math.PI;
        return angle;
    }
    private static bool IsAngleBetween(double angle, double start, double end)
    {
        if (end < start) end += 2 * Math.PI;
        if (angle < start) angle += 2 * Math.PI;
        return angle >= start && angle <= end;
    }
    public override bool IsClose(Point p, double threshold = 5)
    {
        double distToCenter = Helpers.PythDistance(Center, new Point(p.X, p.Y));
        double angle = Math.Atan2(p.Y - Center.Y, p.X - Center.X);
        double radius = Helpers.PythDistance(Mouth1, Center);

        double angleStart = NormalizeAngleRad(Math.Atan2(Mouth1.Y - Center.Y, Mouth1.X - Center.X));
        double angleEnd = NormalizeAngleRad(Math.Atan2(Mouth2.Y - Center.Y, Mouth2.X - Center.X));
        angle = NormalizeAngleRad(angle);

        if (angleEnd <= angleStart)
            angleEnd += 2 * Math.PI;
        if (angle < angleStart)
            angle += 2 * Math.PI;

        bool onArc = angle >= angleStart && angle <= angleEnd &&
                     Math.Abs(distToCenter - radius) <= threshold;

        var line1 = new Line { Start = Center, End = new Point(Center.X + radius * Math.Cos(angleStart), Center.Y + radius * Math.Sin(angleStart)) };
        var line2 = new Line { Start = Center, End = new Point(Center.X + radius * Math.Cos(angleEnd), Center.Y + radius * Math.Sin(angleEnd)) };

        return onArc ||
               line1.IsClose(p, threshold) ||
               line2.IsClose(p, threshold);
    }

}