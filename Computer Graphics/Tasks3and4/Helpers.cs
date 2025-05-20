using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal static class Helpers
{
    public static WriteableBitmap WhiteBitmap(int width, int height, int dpi)
    {
        var bitmap = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Bgra32, null);

        int stride = width * 4;
        byte[] whitePixels = new byte[stride * height];

        for (int i = 0; i < whitePixels.Length; i += 4)
        {
            whitePixels[i + 0] = 255;
            whitePixels[i + 1] = 255;
            whitePixels[i + 2] = 255;
            whitePixels[i + 3] = 255;
        }

        bitmap.WritePixels(
            new Int32Rect(0, 0, width, height),
            whitePixels, stride, 0);
        return bitmap;
    }
    public static Color Lerp(Color a, Color b, float t)
    {
        byte r = (byte)(a.R + (b.R - a.R) * t);
        byte g = (byte)(a.G + (b.G - a.G) * t);
        byte bVal = (byte)(a.B + (b.B - a.B) * t);
        return Color.FromArgb(255, r, g, bVal);
    }
    public static float Coverage(float thickness, float distance, float r)
    {
        float half = thickness / 2.0f;
        float d = Math.Abs(distance);
        if (d >= half + r) return 0;
        if (d <= half - r) return 1;
        return 0.5f + 0.5f * (float)Math.Cos(Math.PI * (d - (half - r)) / (2 * r));
    }
    public static double PythDistance(Point x, Point y)
    {
        double dx = x.X - y.X;
        double dy = x.Y - y.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    public static bool IsNear(Point a, Point b, double threshold = 5)
    {
        return PythDistance(a, b) < threshold;
    }
    public static void SafeAddDirtyRect(WriteableBitmap bitmap, Int32Rect rect)
    {
        int x = Math.Max(0, rect.X);
        int y = Math.Max(0, rect.Y);
        int width = Math.Min(rect.Width, bitmap.PixelWidth - x);
        int height = Math.Min(rect.Height, bitmap.PixelHeight - y);

        if (width > 0 && height > 0)
        {
            bitmap.AddDirtyRect(new Int32Rect(x, y, width, height));
        }
    }
}
