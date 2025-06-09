using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CGlab5;

public partial class MainWindow : Window
{
    private WriteableBitmap bitmap;
    private const int WIDTH = 800;
    private const int HEIGHT = 600;

    private Camera camera;
    private Mesh sphereMesh;
    private Renderer renderer;

    private Point lastMousePos;

    public MainWindow()
    {
        InitializeComponent();

        bitmap = new WriteableBitmap(WIDTH, HEIGHT, 96, 96, PixelFormats.Bgr32, null);
        SceneImage.Source = bitmap;

        sphereMesh = SphereGenerator.CreateSphere(radius: 1f, meridians: 24, parallels: 16);
        camera = new Camera();
        renderer = new Renderer(bitmap, sphereMesh, camera);
        Loaded += (_, _) => renderer.Render();

        DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        timer.Tick += (s, e) => renderer.Render();
        timer.Start();
        MouseMove += OnMouseMove;
        MouseDown += (_, e) => lastMousePos = e.GetPosition(this);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Point pos = e.GetPosition(this);
            double dx = pos.X - lastMousePos.X;
            double dy = pos.Y - lastMousePos.Y;
            camera.Rotate(dx, dy);
            lastMousePos = pos;
        }
    }

    private void CameraSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (camera != null)
        {
            camera.Distance = (float)e.NewValue;
        }
    }
}
