using System.Windows;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;
internal class Rectangle : Shape
{
    public Point TopLeft;
    public Point BottomRight;

    public override void Draw(WriteableBitmap bitmap, bool useAA)
    {
        if (useAA)
            DrawWithAA(bitmap);
        else
            DrawWithoutAA(bitmap);
    }
    private void DrawWithoutAA(WriteableBitmap bitmap)
    {
        Line line1 = new Line { Start = TopLeft, End = new Point(BottomRight.X, TopLeft.Y), Color = Color, Thickness = Thickness };
        Line line2 = new Line { Start = new Point(BottomRight.X, TopLeft.Y), End = BottomRight, Color = Color, Thickness = Thickness };
        Line line3 = new Line { Start = BottomRight, End = new Point(TopLeft.X, BottomRight.Y), Color = Color, Thickness = Thickness };
        Line line4 = new Line { Start = new Point(TopLeft.X, BottomRight.Y), End = TopLeft, Color = Color, Thickness = Thickness };
        line1.Draw(bitmap, false);
        line2.Draw(bitmap, false);
        line3.Draw(bitmap, false);
        line4.Draw(bitmap, false);
    }
    private void DrawWithAA(WriteableBitmap bitmap)
    {
        Line line1 = new Line { Start = TopLeft, End = new Point(BottomRight.X, TopLeft.Y), Color = Color, Thickness = Thickness };
        Line line2 = new Line { Start = new Point(BottomRight.X, TopLeft.Y), End = BottomRight, Color = Color, Thickness = Thickness };
        Line line3 = new Line { Start = BottomRight, End = new Point(TopLeft.X, BottomRight.Y), Color = Color, Thickness = Thickness };
        Line line4 = new Line { Start = new Point(TopLeft.X, BottomRight.Y), End = TopLeft, Color = Color, Thickness = Thickness };
        line1.Draw(bitmap, true);
        line2.Draw(bitmap, true);
        line3.Draw(bitmap, true);
        line4.Draw(bitmap, true);
    }
    public override bool IsClose(Point p, double threshold = 5)
    {
        Line line1 = new Line { Start = TopLeft, End = new Point(BottomRight.X, TopLeft.Y), Color = Color, Thickness = Thickness };
        Line line2 = new Line { Start = new Point(BottomRight.X, TopLeft.Y), End = BottomRight, Color = Color, Thickness = Thickness };
        Line line3 = new Line { Start = BottomRight, End = new Point(TopLeft.X, BottomRight.Y), Color = Color, Thickness = Thickness };
        Line line4 = new Line { Start = new Point(TopLeft.X, BottomRight.Y), End = TopLeft, Color = Color, Thickness = Thickness };
        if (line1.IsClose(p, threshold) || line2.IsClose(p, threshold) || line3.IsClose(p, threshold) || line4.IsClose(p, threshold))
            return true;
        Point center = new((TopLeft.X + BottomRight.X) / 2, (TopLeft.Y + BottomRight.Y) / 2);
        if(Helpers.IsNear(center, p, threshold))
            return true;
        return false;
    }
}