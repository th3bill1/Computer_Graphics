using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Computer_Graphics;

public partial class Task3Window : Window
{
    private enum DrawMode { None, Line, Circle, Polygon }

    private class LineShape
    {
        public Point Start;
        public Point End;
        public int Thickness;
        public bool AA;
        public Color Color;
    }

    private class CircleShape
    {
        public Point Center;
        public double Radius;
        public int Thickness;
        public Color Color;
    }

    private class PolygonShape
    {
        public List<Point> Vertices = new();
        public int Thickness;
        public Color Color;
    }

    private readonly List<LineShape> lines = new();
    private readonly List<CircleShape> circles = new();
    private readonly List<PolygonShape> polygons = new();

    private LineShape? selectedLine = null;
    private CircleShape? selectedCircle = null;
    private PolygonShape? currentPolygon = null;

    private Point? movingPoint = null;
    private enum CircleEditMode { None, MoveCenter, Resize }
    private CircleEditMode circleEditMode = CircleEditMode.None;

    private bool isDrawing = false;
    private bool isDrawingCircle = false;
    private bool isDrawingPolygon = false;

    private Point startPoint;
    private Point circleStartPoint;

    private DrawMode currentMode = DrawMode.None;

    public Task3Window() => InitializeComponent();
    private void SetDrawLineMode(object sender, RoutedEventArgs e) => currentMode = DrawMode.Line;

    private void SetDrawCircleMode(object sender, RoutedEventArgs e) => currentMode = DrawMode.Circle;

    private void SetDrawPolygonMode(object sender, RoutedEventArgs e) => currentMode = DrawMode.Polygon;

    private void ClearCanvas(object sender, RoutedEventArgs e)
    {
        lines.Clear();
        circles.Clear();
        polygons.Clear();
        DrawCanvas.Children.Clear();
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point click = e.GetPosition(DrawCanvas);

        if (currentMode == DrawMode.Line)
        {
            foreach (var line in lines)
            {
                if (IsNear(line.Start, click)) { selectedLine = line; movingPoint = line.Start; return; }
                if (IsNear(line.End, click)) { selectedLine = line; movingPoint = line.End; return; }
            }

            if (!isDrawing)
            {
                startPoint = click;
                isDrawing = true;
            }
            else
            {
                lines.Add(new LineShape
                {
                    Start = startPoint,
                    End = click,
                    Thickness = (int)ThicknessSlider.Value,
                    AA = AntiAliasingCheckBox.IsChecked == true,
                    Color = SelectedColor()
                });
                isDrawing = false;
                RedrawAll();
            }
        }
        else if (currentMode == DrawMode.Circle)
        {
            foreach (var circle in circles)
            {
                if (IsNear(circle.Center, click)) { selectedCircle = circle; circleEditMode = CircleEditMode.MoveCenter; return; }
                var edge = new Point(circle.Center.X + circle.Radius, circle.Center.Y);
                if (IsNear(edge, click)) { selectedCircle = circle; circleEditMode = CircleEditMode.Resize; return; }
            }

            if (!isDrawingCircle)
            {
                circleStartPoint = click;
                isDrawingCircle = true;
            }
            else
            {
                double radius = (click - circleStartPoint).Length;
                circles.Add(new CircleShape
                {
                    Center = circleStartPoint,
                    Radius = radius,
                    Thickness = (int)ThicknessSlider.Value,
                    Color = SelectedColor()
                });
                isDrawingCircle = false;
                RedrawAll();
            }
        }
        else if (currentMode == DrawMode.Polygon)
        {
            Point clickPoint = e.GetPosition(DrawCanvas);

            if (!isDrawingPolygon)
            {
                currentPolygon = new PolygonShape
                {
                    Thickness = (int)ThicknessSlider.Value,
                    Color = SelectedColor()
                };
                currentPolygon.Vertices.Add(clickPoint);
                isDrawingPolygon = true;
            }
            else if (currentPolygon != null && currentPolygon.Vertices.Count > 2 &&
                     IsNear(currentPolygon.Vertices[0], clickPoint, 10))
            {
                polygons.Add(currentPolygon);
                currentPolygon = null;
                isDrawingPolygon = false;
                RedrawAll();
            }
            else
            {
                currentPolygon?.Vertices.Add(clickPoint);
                RedrawAll();
            }
        }
    }
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && movingPoint != null && selectedLine != null)
        {
            Point newPos = e.GetPosition(DrawCanvas);
            if (movingPoint == selectedLine.Start)
                selectedLine.Start = newPos;
            else
                selectedLine.End = newPos;

            RedrawAll();
        }
        else if (selectedCircle != null)
        {
            Point current = e.GetPosition(DrawCanvas);
            switch (circleEditMode)
            {
                case CircleEditMode.MoveCenter:
                    selectedCircle.Center = current;
                    break;
                case CircleEditMode.Resize:
                    selectedCircle.Radius = (current - selectedCircle.Center).Length;
                    break;
            }
            RedrawAll();
        }
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        movingPoint = null;
        circleEditMode = CircleEditMode.None;
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point click = e.GetPosition(DrawCanvas);

        for (int i = 0; i < lines.Count; i++)
        {
            if (IsNearLine(lines[i], click))
            {
                lines.RemoveAt(i);
                RedrawAll();
                return;
            }
        }

        for (int i = 0; i < circles.Count; i++)
        {
            if ((click - circles[i].Center).Length <= circles[i].Radius + 5)
            {
                circles.RemoveAt(i);
                selectedCircle = null;
                RedrawAll();
                return;
            }
        }

        for (int i = 0; i < polygons.Count; i++)
        {
            var poly = polygons[i];
            if (poly.Vertices.Any(v => IsNear(v, click)))
            {
                polygons.RemoveAt(i);
                RedrawAll();
                return;
            }
        }
    }
    private void ApplyThickness_Click(object sender, RoutedEventArgs e)
    {
        if (selectedLine != null)
        {
            selectedLine.Thickness = (int)ThicknessSlider.Value;
            RedrawAll();
        }
    }

    private void RedrawAll()
    {
        DrawCanvas.Children.Clear();

        foreach (var line in lines)
        {
            DrawGuptaSproullLine(line.Start, line.End, line.AA, line.Thickness, line.Color);
        }

        foreach (var circle in circles)
        {
            DrawMidpointCircle(circle.Center, circle.Radius, circle.Thickness, circle.Color);
        }

        foreach (var polygon in polygons)
        {
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                Point a = polygon.Vertices[i];
                Point b = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                if (i < polygon.Vertices.Count - 1 || polygon.Vertices.Count > 2)
                    DrawGuptaSproullLine(a, b, AntiAliasingCheckBox.IsChecked == true, polygon.Thickness, polygon.Color);
            }
        }

        if (currentPolygon != null && currentPolygon.Vertices.Count > 1)
        {
            for (int i = 0; i < currentPolygon.Vertices.Count - 1; i++)
            {
                DrawGuptaSproullLine(
                    currentPolygon.Vertices[i],
                    currentPolygon.Vertices[i + 1],
                    AntiAliasingCheckBox.IsChecked == true,
                    currentPolygon.Thickness,
                    currentPolygon.Color);
            }
        }
    }
    private Color SelectedColor()
    {
        string? colorName = (ColorPicker.SelectedItem as ComboBoxItem)?.Content.ToString();
        return colorName switch
        {
            "Red" => Colors.Red,
            "Green" => Colors.Green,
            "Blue" => Colors.Blue,
            "Orange" => Colors.Orange,
            "Purple" => Colors.Purple,
            _ => Colors.Black,
        };
    }
    private void DrawGuptaSproullLine(Point start, Point end, bool useAA, int thickness, Color color)
    {
        int x0 = (int)start.X;
        int y0 = (int)start.Y;
        int x1 = (int)end.X;
        int y1 = (int)end.Y;

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep) (x0, y0, x1, y1) = (y0, x0, y1, x1);
        if (x0 > x1) (x0, x1, y0, y1) = (x1, x0, y1, y0);

        int dx = x1 - x0;
        int dy = y1 - y0;
        int absDy = Math.Abs(dy);
        int yStep = dy >= 0 ? 1 : -1;

        int d = 2 * absDy - dx;
        int dE = 2 * absDy;
        int dNE = 2 * (absDy - dx);

        int x = x0;
        int y = y0;

        float invDenom = 1f / (2f * (float)Math.Sqrt(dx * dx + dy * dy));
        float two_dx_invDenom = 2f * dx * invDenom;

        int two_v_dx;
        float baseIntensity;

        if (useAA)
        {
            PlotAntialiasedPixel(x, y, thickness, 0, steep, color);
            for (int i = 1; PlotAntialiasedPixel(x, y + i, thickness, i * two_dx_invDenom, steep, color); i++) ;
            for (int i = 1; PlotAntialiasedPixel(x, y - i, thickness, i * two_dx_invDenom, steep, color); i++) ;
        }
        else
        {
            var brush = GetCircularBrush(thickness);
            if (steep)
                PlotBrushPixel(y, x, color, brush);
            else
                PlotBrushPixel(x, y, color, brush);
        }

        while (x < x1)
        {
            x++;
            if (d < 0)
            {
                two_v_dx = d + dx;
                d += dE;
            }
            else
            {
                two_v_dx = d - dx;
                d += dNE;
                y += yStep;
            }

            if (useAA)
            {
                baseIntensity = two_v_dx * invDenom;
                PlotAntialiasedPixel(x, y, thickness, baseIntensity, steep, color);
                for (int i = 1; PlotAntialiasedPixel(x, y + i * yStep, thickness, i * two_dx_invDenom - baseIntensity, steep, color); i++) ;
                for (int i = 1; PlotAntialiasedPixel(x, y - i * yStep, thickness, i * two_dx_invDenom + baseIntensity, steep, color); i++) ;
            }
            else
            {
                var brush = GetCircularBrush(thickness);
                if (steep)
                    PlotBrushPixel(y, x, color, brush);
                else
                    PlotBrushPixel(x, y, color, brush);
            }
        }
    }
    private bool PlotAntialiasedPixel(int x, int y, float thickness, float distance, bool steep, Color baseColor)
    {
        float r = 0.5f;
        float cov = Coverage(thickness, distance, r);
        if (cov <= 0) return false;

        Color color = Color.FromArgb(
            (byte)(cov * 255),
            baseColor.R,
            baseColor.G,
            baseColor.B);

        Rectangle pixel = new Rectangle
        {
            Width = 1,
            Height = 1,
            Fill = new SolidColorBrush(color)
        };

        if (steep)
        {
            Canvas.SetLeft(pixel, y);
            Canvas.SetTop(pixel, x);
        }
        else
        {
            Canvas.SetLeft(pixel, x);
            Canvas.SetTop(pixel, y);
        }

        DrawCanvas.Children.Add(pixel);
        return true;
    }
    private void DrawMidpointCircle(Point center, double radius, int thickness, Color color)
    {
        int xc = (int)center.X;
        int yc = (int)center.Y;
        int r = (int)Math.Round(radius);

        int x = 0;
        int y = r;
        int d = 1 - r;

        var brush = GetCircularBrush(thickness);
        PlotCirclePoints(xc, yc, x, y, brush, color);

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

            PlotCirclePoints(xc, yc, x, y, brush, color);
        }
    }

    private void PlotCirclePoints(int xc, int yc, int x, int y, List<(int, int)> brush, Color color)
    {
        PlotBrushPixel(xc + x, yc + y, color, brush);
        PlotBrushPixel(xc - x, yc + y, color, brush);
        PlotBrushPixel(xc + x, yc - y, color, brush);
        PlotBrushPixel(xc - x, yc - y, color, brush);
        PlotBrushPixel(xc + y, yc + x, color, brush);
        PlotBrushPixel(xc - y, yc + x, color, brush);
        PlotBrushPixel(xc + y, yc - x, color, brush);
        PlotBrushPixel(xc - y, yc - x, color, brush);
    }
    private bool IsNear(Point a, Point b, double range = 10)
    {
        return (a - b).Length <= range;
    }
    private void PlotBrushPixel(int x, int y, Color color, List<(int, int)> brush)
    {
        foreach (var (dx, dy) in brush)
        {
            int px = x + dx;
            int py = y + dy;

            Rectangle pixel = new Rectangle
            {
                Width = 1,
                Height = 1,
                Fill = new SolidColorBrush(color)
            };

            Canvas.SetLeft(pixel, px);
            Canvas.SetTop(pixel, py);
            DrawCanvas.Children.Add(pixel);
        }
    }
    private List<(int, int)> GetCircularBrush(int thickness)
    {
        List<(int, int)> brush = new();
        int radius = thickness / 2;

        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    brush.Add((dx, dy));
                }
            }
        }

        return brush;
    }
    private float Coverage(float thickness, float distance, float r)
    {
        float w = thickness / 2f;
        if (distance >= w + r) return 0f;
        if (distance <= w - r) return 1f;

        float a = w - distance;
        return (float)((a + r) * (a + r) / (4 * r * w));
    }
    private bool IsNearLine(LineShape line, Point p)
    {
        double distance = DistanceFromPointToLine(p, line.Start, line.End);
        return distance <= line.Thickness + 5;
    }
    private double DistanceFromPointToLine(Point p, Point a, Point b)
    {
        double A = p.X - a.X;
        double B = p.Y - a.Y;
        double C = b.X - a.X;
        double D = b.Y - a.Y;

        double dot = A * C + B * D;
        double lenSq = C * C + D * D;
        double param = lenSq != 0 ? dot / lenSq : -1;

        double xx, yy;
        if (param < 0)
        {
            xx = a.X;
            yy = a.Y;
        }
        else if (param > 1)
        {
            xx = b.X;
            yy = b.Y;
        }
        else
        {
            xx = a.X + param * C;
            yy = a.Y + param * D;
        }

        double dx = p.X - xx;
        double dy = p.Y - yy;
        return Math.Sqrt(dx * dx + dy * dy);
    }

}