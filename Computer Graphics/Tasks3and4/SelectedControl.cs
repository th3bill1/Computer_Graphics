using System.Windows;

namespace Computer_Graphics;

internal abstract class SelectedControl
{
    public abstract void Update(Point newPosition, ref Point previousPosition);
}
internal class LineEndpointControl(Line line, bool isStart) : SelectedControl
{
    public Line Line { get; } = line;
    public bool IsStart { get; } = isStart;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        if (IsStart)
            Line.Start = newPosition;
        else
            Line.End = newPosition;
    }
}
internal class CircleCenterControl(Circle circle) : SelectedControl
{
    public Circle Circle { get; } = circle;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        Circle.Center = newPosition;
    }
}
internal class CircleRadiusControl(Circle circle) : SelectedControl
{
    public Circle Circle { get; } = circle;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        Circle.Radius = Helpers.PythDistance(Circle.Center, newPosition);
    }
}
internal class PolygonVertexControl(Polygon polygon, int index) : SelectedControl
{
    public Polygon Polygon { get; } = polygon;
    public int VertexIndex { get; } = index;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        Polygon.Vertices[VertexIndex] = newPosition;
    }
}
internal class PolygonEdgeControl(Polygon polygon, int edgeIndex) : SelectedControl
{
    public Polygon Polygon { get; } = polygon;
    public int EdgeIndex { get; } = edgeIndex;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        Vector delta = newPosition - previousPosition;
        int i1 = EdgeIndex;
        int i2 = (EdgeIndex + 1) % Polygon.Vertices.Count;
        Polygon.Vertices[i1] += delta;
        Polygon.Vertices[i2] += delta;
    }
}
internal class RectangleVertexControl(Rectangle rectangle, int index) : SelectedControl
{
    public Rectangle Rectangle { get; } = rectangle;
    public int VertexIndex { get; } = index;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        switch (VertexIndex)
        {
            case 0:
                Rectangle.TopLeft = newPosition;
                break;
            case 1:
                Rectangle.BottomRight = newPosition;
                break;
            case 2:
                Rectangle.TopLeft = new Point(newPosition.X, Rectangle.TopLeft.Y);
                Rectangle.BottomRight = new Point(Rectangle.BottomRight.X, newPosition.Y);
                break;
            case 3:
                Rectangle.TopLeft = new Point(Rectangle.TopLeft.X, newPosition.Y);
                Rectangle.BottomRight = new Point(newPosition.X, Rectangle.BottomRight.Y);
                break;
        }
    }
}
internal class RectangleEdgeControl(Rectangle rectangle, int edgeIndex) : SelectedControl
{
    public Rectangle Rectangle { get; } = rectangle;
    public int EdgeIndex { get; } = edgeIndex;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        Vector delta = newPosition - previousPosition;

        switch (EdgeIndex)
        {
            case 0: // top
                Rectangle.TopLeft = new Point(Rectangle.TopLeft.X, Rectangle.TopLeft.Y + delta.Y);
                break;
            case 1: // right
                Rectangle.BottomRight = new Point(Rectangle.BottomRight.X + delta.X, Rectangle.BottomRight.Y);
                break;
            case 2: // bottom
                Rectangle.BottomRight = new Point(Rectangle.BottomRight.X, Rectangle.BottomRight.Y + delta.Y);
                break;
            case 3: // left
                Rectangle.TopLeft = new Point(Rectangle.TopLeft.X + delta.X, Rectangle.TopLeft.Y);
                break;
        }
    }
}
internal class RectangleCenterControl(Rectangle rectangle) : SelectedControl
{
    public Rectangle Rectangle { get; } = rectangle;

    public override void Update(Point newPosition, ref Point previousPosition)
    {
        Vector delta = newPosition - previousPosition;
        Rectangle.TopLeft += delta;
        Rectangle.BottomRight += delta;
    }
}
