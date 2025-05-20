using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace Computer_Graphics;

public partial class Tasks3and4Window : Window
{
    private WriteableBitmap _bitmap = Helpers.WhiteBitmap(_bitmapWidth, _bitmapHeight, _dpi);

    private const int _bitmapWidth = 600;
    private const int _bitmapHeight = 600;
    private const int _dpi = 96;
    private const int defaultThickness = 1;
    private Color defaultColor = Colors.Black;
    public Tasks3and4Window()
    {
        InitializeComponent();
        ImageControl.Source = _bitmap;
        _clickHandlers = new()
    {
        { DrawMode.Line, p => drawing.Line = StartOrFinishShape(drawing.Line, l => l.End = p, 
        () => new Line { Start = p, End = p, Color = defaultColor, Thickness = defaultThickness }) },
        { DrawMode.Circle, p => drawing.Circle = StartOrFinishShape(drawing.Circle, c => c.Radius = Helpers.PythDistance(p, c.Center),
        () => new Circle { Center = p, Radius = 0, Color = defaultColor, Thickness = defaultThickness }) },
        { DrawMode.Rectangle, p => drawing.Rectangle = StartOrFinishShape(drawing.Rectangle, r => r.BottomRight = p, 
        () => new Rectangle { TopLeft = p, BottomRight = p, Color = defaultColor, Thickness = defaultThickness }) },
        { DrawMode.Polygon, HandlePolygonClick },
        { DrawMode.Pacman, HandlePacmanClick },
        { DrawMode.Edit, HandleEditClick }
    };
    }
    private readonly Dictionary<DrawMode, Action<Point>> _clickHandlers;
    private enum DrawMode { None, Line, Circle, Polygon, Pacman, Edit, Rectangle }
    private List<Shape> shapes = [];
    private readonly List<Line> tempPolyLines = [];
    private bool UseAA => AntiAliasingCheckBox.IsChecked == true;

    private Shape? selectedShape = null;
    private struct ShapeState
    {
        public Line? Line;
        public Circle? Circle;
        public Polygon? Polygon;
        public Pacman? Pacman;
        public Rectangle? Rectangle;
    }

    private ShapeState drawing;
    private DrawMode currentMode = DrawMode.None;
    private bool isMoving = false;
    private SelectedControl? selectedControl = null;
    private Point previousMousePos = new(0, 0);
    private Polygon? polygonToClip = null;
    private void SetMode(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
            currentMode = Enum.Parse<DrawMode>(btn.Content.ToString() ?? "None");
    }
    public void ApplyAA_Click(object sender, RoutedEventArgs e) => RedrawAll();
    public void ApplyThickness_Click(object sender, RoutedEventArgs e)
    {
        if(selectedShape != null)selectedShape.Thickness = (int)ThicknessSlider.Value;
        RedrawAll();
    }

    private void ResetBitmap()
    {
        _bitmap = Helpers.WhiteBitmap(_bitmapWidth, _bitmapHeight, _dpi);
        ImageControl.Source = _bitmap;
    }
        
    private void ClearBitmap(object sender, RoutedEventArgs e)
    {
        ResetBitmap();
        shapes.Clear();
    }
    private void RedrawAll()
    {
        ResetBitmap();
        foreach (var shape in shapes)
            shape.Draw(_bitmap, UseAA);
        foreach (var line in tempPolyLines)
            line.Draw(_bitmap, UseAA);
    }
    private void ApplyColor_Click(object sender, RoutedEventArgs e)
    {
        if (selectedShape != null)  selectedShape.Color = SelectedColor();
        RedrawAll();
    }
    private T? StartOrFinishShape<T>(T? current, Action<T> updateFinal, Func<T> createNew) where T : Shape
    {
        if (current == null)
            return createNew(); 

        updateFinal(current); 
        shapes.Add(current);
        RedrawAll();
        return null;
    }
    private void HandlePolygonClick(Point clickPos)
    {
        if (drawing.Polygon == null)
        {
            drawing.Polygon = new Polygon
            {
                Vertices = [clickPos],
                Color = defaultColor,
                Thickness = defaultThickness
            };
        }
        else if (Helpers.IsNear(clickPos, drawing.Polygon.Vertices[0]))
        {
            shapes.Add(drawing.Polygon);
            drawing.Polygon = null;
            tempPolyLines.Clear();
            RedrawAll();
        }
        else
        {
            drawing.Polygon.Vertices.Add(clickPos);
            UpdateTempPolygonEdge(clickPos);
        }
    }
    private void HandlePacmanClick(Point clickPos)
    {
        if (drawing.Pacman == null)
        {
            drawing.Pacman = new Pacman
            {
                Center = clickPos,
                Mouth1 = clickPos,
                Mouth2 = clickPos,
                Color = defaultColor,
                Thickness = defaultThickness
            };
        }
        else if (drawing.Pacman.Center == drawing.Pacman.Mouth1)
        {
            drawing.Pacman.Mouth1 = clickPos;

            new Line
            {
                Start = drawing.Pacman.Center,
                End = clickPos,
                Color = defaultColor,
                Thickness = defaultThickness
            }.Draw(_bitmap, UseAA);
        }
        else
        {
            drawing.Pacman.Mouth2 = clickPos;
            shapes.Add(drawing.Pacman);
            drawing.Pacman = null;
            RedrawAll();
        }
    }
    private void HandleEditClick(Point clickPos)
    {
        var closeShape = shapes.FirstOrDefault(selectedShape => selectedShape.IsClose(clickPos));
        if (closeShape != null)
        {
            selectedShape = closeShape;
            RedrawAll();
            isMoving = true;
            switch (closeShape)
            {
                case Line line:
                    if (Helpers.IsNear(line.Start, clickPos))
                    {
                        selectedControl = new LineEndpointControl(line, true);
                        break;
                    }
                    if (Helpers.IsNear(line.End, clickPos))
                    {
                        selectedControl = new LineEndpointControl(line, false);
                        break;
                    }
                    break;

                case Circle circle:
                    if (Helpers.IsNear(circle.Center, clickPos))
                    {
                        selectedControl = new CircleCenterControl(circle);
                        break;
                    }
                    if (Math.Abs(Helpers.PythDistance(circle.Center, clickPos) - circle.Radius) < 5)
                    {
                        selectedControl = new CircleRadiusControl(circle);
                        break;
                    }
                    break;

                case Polygon polygon:
                    for (int i = 0; i < polygon.Vertices.Count; i++)
                    {
                        var a = polygon.Vertices[i];
                        var b = polygon.Vertices[(i + 1) % polygon.Vertices.Count];
                        var edge = new Line { Start = a, End = b };
                        if (edge.IsClose(clickPos, 5))
                        {
                            selectedControl = new PolygonEdgeControl(polygon, i);
                            previousMousePos = clickPos;
                            break;
                        }
                    }
                    for (int i = 0; i < polygon.Vertices.Count; i++)
                    {
                        if (Helpers.IsNear(polygon.Vertices[i], clickPos))
                        {
                            selectedControl = new PolygonVertexControl(polygon, i);
                            break;
                        }
                    }
                    break;
                case Rectangle rectangle:
                    if (polygonToClip != null)
                    {
                        polygonToClip.ClippingRectangle = rectangle;
                        polygonToClip = null;
                        RedrawAll();
                        break;
                    }
                    if (Helpers.IsNear(rectangle.TopLeft, clickPos))
                    {
                        selectedControl = new RectangleVertexControl(rectangle, 0);
                        break;
                    }
                    else if (Helpers.IsNear(rectangle.BottomRight, clickPos))
                    {
                        selectedControl = new RectangleVertexControl(rectangle, 1);
                        break;
                    }
                    else if (Helpers.IsNear(new Point(rectangle.TopLeft.X, rectangle.BottomRight.Y), clickPos))
                    {
                        selectedControl = new RectangleVertexControl(rectangle, 2);
                        break;
                    }
                    else if (Helpers.IsNear(new Point(rectangle.BottomRight.X, rectangle.TopLeft.Y), clickPos))
                    {
                        selectedControl = new RectangleVertexControl(rectangle, 3);
                        break;
                    }
                    else
                    {
                        previousMousePos = clickPos;
                        Point p0 = rectangle.TopLeft;
                        Point p1 = new(rectangle.BottomRight.X, rectangle.TopLeft.Y);
                        Point p2 = rectangle.BottomRight;
                        Point p3 = new(rectangle.TopLeft.X, rectangle.BottomRight.Y);

                        if (new Line { Start = p0, End = p1 }.IsClose(clickPos, 5))
                            selectedControl = new RectangleEdgeControl(rectangle, 0);
                        else if (new Line { Start = p1, End = p2 }.IsClose(clickPos, 5))
                            selectedControl = new RectangleEdgeControl(rectangle, 1);
                        else if (new Line { Start = p2, End = p3 }.IsClose(clickPos, 5))
                            selectedControl = new RectangleEdgeControl(rectangle, 2);
                        else if (new Line { Start = p3, End = p0 }.IsClose(clickPos, 5))
                            selectedControl = new RectangleEdgeControl(rectangle, 3);
                    }
                    Point center = new(
                        (rectangle.TopLeft.X + rectangle.BottomRight.X) / 2,
                        (rectangle.TopLeft.Y + rectangle.BottomRight.Y) / 2);

                    if (Helpers.IsNear(center, clickPos))
                    {
                        selectedControl = new RectangleCenterControl(rectangle);
                        previousMousePos = clickPos;
                        break;
                    }

                    break;
                default:
                    break;
            }
        }
        else if (selectedShape != null)
        {
            selectedShape = null;
            RedrawAll();
        }
    }
    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickPos = e.GetPosition(ImageControl);
        if (_clickHandlers.TryGetValue(currentMode, out var handler))
            handler(clickPos);
    }
    private void Clip_Click(object sender, RoutedEventArgs e)
    {
        if(selectedShape is Polygon polygon)
            polygonToClip = polygon;
    }
    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        isMoving = false;
        if (selectedControl != null)
        {
            selectedControl = null;
            RedrawAll();
        }
    }
    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point clickPos = e.GetPosition(ImageControl);
        if (currentMode == DrawMode.Edit)
        {
            var closeShape = shapes.FirstOrDefault(selectedShape => selectedShape.IsClose(clickPos));
            if (closeShape != null)
            {
                shapes.Remove(closeShape);
                RedrawAll();
            }
        }
    }
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        Point currentPos = e.GetPosition(ImageControl);
        if (drawing.Line != null)
        {
            drawing.Line.End = currentPos;
            RedrawAll();
            drawing.Line.Draw(_bitmap, UseAA);
        }
        if (drawing.Circle != null)
        {
            drawing.Circle.Radius = Helpers.PythDistance(drawing.Circle.Center, currentPos);
            RedrawAll();
            drawing.Circle.Draw(_bitmap, UseAA);
        }
        if (drawing.Polygon != null) UpdateTempPolygonEdge(currentPos);
        if (drawing.Pacman != null)
        {
            if(drawing.Pacman.Center == drawing.Pacman.Mouth1)
            {
                RedrawAll();
                var tempLine = new Line
                {
                    Start = drawing.Pacman.Center,
                    End = currentPos,
                    Color = defaultColor,
                    Thickness = defaultThickness
                };
                tempLine.Draw(_bitmap, UseAA);
            }
            else
            {
                RedrawAll();
                drawing.Pacman.Mouth2 = currentPos;
                drawing.Pacman.Draw(_bitmap, UseAA);
            }
            
        }
        if (drawing.Rectangle != null)
        {
            drawing.Rectangle.BottomRight = currentPos;
            RedrawAll();
            drawing.Rectangle.Draw(_bitmap, UseAA);
        }
        if (isMoving && selectedControl != null)
        {
            selectedControl.Update(currentPos, ref previousMousePos);
            previousMousePos = currentPos;
            RedrawAll();
        }
    }
    private void UpdateTempPolygonEdge(Point pos)
    {
        if (drawing.Polygon == null) return;
        var tempLine = new Line
        {
            Start = drawing.Polygon.Vertices[^1],
            End = pos,
            Color = defaultColor,
            Thickness = defaultThickness
        };
        if (tempPolyLines.Count > drawing.Polygon.Vertices.Count - 1)
            tempPolyLines.RemoveAt(tempPolyLines.Count - 1);
        tempPolyLines.Add(tempLine);
        RedrawAll();
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
    private void SaveCanvasToFile(object sender, RoutedEventArgs e)
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            FileName = "canvas.json"
        };

        if (saveDialog.ShowDialog() == true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(shapes, options);
            File.WriteAllText(saveDialog.FileName, json);
        }
    }
    private void LoadCanvasFromFile(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json"
        };

        if (openDialog.ShowDialog() == true)
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            string json = File.ReadAllText(openDialog.FileName);
            var data = JsonSerializer.Deserialize<List<Shape>>(json, options);

            if (data != null)
            {
                shapes.Clear();
                shapes = data;
                RedrawAll();
            }
        }
    }
    private void FillRectangle_Click(object sender, RoutedEventArgs e)
    {
        if (selectedShape is Polygon p)
        {
            var sysColor = SelectedColor();
            p.FillColor = Color.FromRgb(sysColor.R, sysColor.G, sysColor.B);
            p.FillImage = null;
            RedrawAll();
        }
        else
        {
            MessageBox.Show("No polygon is currently being edited.");
            return;
        }
    }
    private void LoadFillingImage_Click(object sender, RoutedEventArgs e)
    {
        if (selectedShape is Polygon p)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var bitmapImage = new BitmapImage(new Uri(dialog.FileName));
                    var wb = new WriteableBitmap(bitmapImage);

                    p.FillImage = wb;
                    p.FillColor = null;
                    RedrawAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load image: {ex.Message}");
                }
            }
        }
        else MessageBox.Show("No polygon is currently being edtied.");
    }
}