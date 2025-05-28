using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CGlab5;

using Matrix = Structs.Matrix;
using Vector = Structs.Vector;
public partial class MainWindow : Window
{
    private struct Vertex
    {
        public double X, Y, Z;
        public Vertex(double x, double y, double z) => (X, Y, Z) = (x, y, z);
    }
    private struct Pyramid(Vertex[] vertices, (int, int)[] edges)
    {
        public Vertex[] Vertices = vertices;
        public (int, int)[] Edges = edges;
    }
    private Pyramid pyramid = new()
    {
        Vertices =
        [
            new Vertex(-1, -1, -1),
            new Vertex( 1, -1, -1),
            new Vertex( 1, -1,  1),
            new Vertex(-1, -1,  1),
            new Vertex( 0,  1,  0),
        ],
        Edges =
        [
        (0, 1), (1, 2), (2, 3), (3, 0),
        (0, 4), (1, 4), (2, 4), (3, 4),
        ]
    };
    public MainWindow()
    {
        InitializeComponent();
        DispatcherTimer timer = new DispatcherTimer();

        double angleY = 0;
        timer.Interval = TimeSpan.FromMilliseconds(15);
        timer.Tick += (s, e) => {
            angleY += 0.69;
            RenderScene(angleY);
        };
        timer.Start();
    }
    private void DrawLine(Point p1, Point p2)
    {
        var line = new Line
        {
            X1 = p1.X,
            Y1 = p1.Y,
            X2 = p2.X,
            Y2 = p2.Y,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        DrawCanvas.Children.Add(line);
    }
    private void RenderScene(double angleY)
    {
        DrawCanvas.Children.Clear();
        double width = DrawCanvas.ActualWidth;
        double height = DrawCanvas.ActualHeight;

        var model = Helpers.RotationY(angleY);
        var view = Helpers.LookAt(new Vector(0, 0, -4), new Vector(0, 0, 0), new Vector(0, 2, 0));
        var projection = Helpers.Perspective(Math.PI / 3, width / height, 0.1, 100);

        var mvp = Matrix.Multiply(projection, Matrix.Multiply(view, model));

        var projected = pyramid.Vertices.Select(v =>
        {
            var vec = new Vector(v.X, v.Y, v.Z);
            vec = Matrix.Multiply(mvp, vec);

            if (vec.W != 0)
            {
                vec = new Vector(vec.X / vec.W, vec.Y / vec.W, vec.Z / vec.W);
            }

            double screenX = (vec.X + 1) * 0.5 * width;
            double screenY = (1 - vec.Y) * 0.5 * height;
            return new Point(screenX, screenY);
        }).ToArray();

        foreach (var (a, b) in pyramid.Edges)
            DrawLine(projected[a], projected[b]);
    }


}