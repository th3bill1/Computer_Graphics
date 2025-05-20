using System.Windows;

namespace Computer_Graphics;

internal static class Selector
{
    public static SelectedControl? CreateControlForShape(
        Shape shape,
        Point clickPos,
        ref Point previousMousePos,
        Polygon? polygonToClip,
        out Polygon? updatedPolygonToClip
    )
    {
        updatedPolygonToClip = polygonToClip;

        return shape switch
        {
            Line line => GetLineControl(line, clickPos),
            Circle circle => GetCircleControl(circle, clickPos),
            Polygon polygon => GetPolygonControl(polygon, clickPos, ref previousMousePos),
            Rectangle rectangle => GetRectangleControl(rectangle, clickPos, ref previousMousePos, polygonToClip, out updatedPolygonToClip),
            _ => null
        };
    }

    private static SelectedControl? GetLineControl(Line line, Point clickPos)
    {
        if (Helpers.IsNear(line.Start, clickPos))
            return new LineEndpointControl(line, true);
        if (Helpers.IsNear(line.End, clickPos))
            return new LineEndpointControl(line, false);
        return null;
    }

    private static SelectedControl? GetCircleControl(Circle circle, Point clickPos)
    {
        if (Helpers.IsNear(circle.Center, clickPos))
            return new CircleCenterControl(circle);
        if (Math.Abs(Helpers.PythDistance(circle.Center, clickPos) - circle.Radius) < 5)
            return new CircleRadiusControl(circle);
        return null;
    }

    private static SelectedControl? GetPolygonControl(Polygon polygon, Point clickPos, ref Point previousMousePos)
    {
        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            var a = polygon.Vertices[i];
            var b = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
            if (new Line { Start = a, End = b }.IsClose(clickPos, 5))
            {
                previousMousePos = clickPos;
                return new PolygonEdgeControl(polygon, i);
            }
        }

        for (int i = 0; i < polygon.Vertices.Count; i++)
        {
            if (Helpers.IsNear(polygon.Vertices[i], clickPos))
                return new PolygonVertexControl(polygon, i);
        }

        return null;
    }

    private static SelectedControl? GetRectangleControl(Rectangle r, Point clickPos, ref Point previous, Polygon? clip, out Polygon? updated)
    {
        updated = clip;

        if (clip != null)
        {
            clip.ClippingRectangle = r;
            updated = null;
            return null;
        }

        Point[] corners =
        {
            r.TopLeft,
            r.BottomRight,
            new Point(r.TopLeft.X, r.BottomRight.Y),
            new Point(r.BottomRight.X, r.TopLeft.Y)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            if (Helpers.IsNear(corners[i], clickPos))
                return new RectangleVertexControl(r, i);
        }

        Point[] edgeStarts = { corners[0], corners[1], corners[2], corners[3] };
        Point[] edgeEnds = { corners[3], corners[2], corners[1], corners[0] };

        for (int i = 0; i < 4; i++)
        {
            if (new Line { Start = edgeStarts[i], End = edgeEnds[i] }.IsClose(clickPos, 5))
            {
                previous = clickPos;
                return new RectangleEdgeControl(r, i);
            }
        }

        Point center = new(
            (r.TopLeft.X + r.BottomRight.X) / 2,
            (r.TopLeft.Y + r.BottomRight.Y) / 2);

        if (Helpers.IsNear(center, clickPos))
        {
            previous = clickPos;
            return new RectangleCenterControl(r);
        }

        return null;
    }
}
