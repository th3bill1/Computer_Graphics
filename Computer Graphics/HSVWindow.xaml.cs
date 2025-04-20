using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace Computer_Graphics;

partial class HSVWindow : Window
{
    private WriteableBitmap h;
    private WriteableBitmap s;
    private WriteableBitmap v;
    private WriteableBitmap rgb;
    public HSVWindow(WriteableBitmap displayedImage)
    {
        Convert(displayedImage);
        Title = "HSV Visualization";
        Width = 800;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Grid grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        Border hchannel = CreateImage(h, "H Channel");
        Grid.SetRow(hchannel, 0);
        Grid.SetColumn(hchannel, 0);
        grid.Children.Add(hchannel);
        Border schannel = CreateImage(s, "S Channel");
        Grid.SetRow(schannel, 0);
        Grid.SetColumn(schannel, 1);
        grid.Children.Add(schannel);
        Border vchannel = CreateImage(v, "V Channel");
        Grid.SetRow(vchannel, 1);
        Grid.SetColumn(vchannel, 0);
        grid.Children.Add(vchannel);
        Border rgbrestored = CreateImage(rgb, "Restored RGB");
        Grid.SetRow(rgbrestored, 1);
        Grid.SetColumn(rgbrestored, 1);
        grid.Children.Add(rgbrestored);

        Content = grid;
    }

    private static Border CreateImage(WriteableBitmap bitmap, string label)
    {
        StackPanel panel = new StackPanel();
        panel.Children.Add(new Image { Source = bitmap, Stretch = Stretch.Uniform });
        panel.Children.Add(new TextBlock
        {
            Text = label,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10)
        });

        return new Border
        {
            Child = panel,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Gray,
            Margin = new Thickness(3)
        };
    }

    private void Convert(WriteableBitmap displayedImage)
    {

        int width = displayedImage.PixelWidth;
        int height = displayedImage.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[height * stride];
        displayedImage.CopyPixels(pixelData, stride, 0);

        byte[] hData = new byte[height * stride];
        byte[] sData = new byte[height * stride];
        byte[] vData = new byte[height * stride];
        byte[] rgbRestored = new byte[height * stride];

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte b = pixelData[i];
            byte g = pixelData[i + 1];
            byte r = pixelData[i + 2];

            ColorToHSV(r, g, b, out double h, out double s, out double v);

            byte hByte = (byte)(h / 360 * 255);
            byte sByte = (byte)(s * 255);
            byte vByte = (byte)(v * 255);

            hData[i] = hData[i + 1] = hData[i + 2] = hByte;
            hData[i + 3] = 255;

            sData[i] = sData[i + 1] = sData[i + 2] = sByte;
            sData[i + 3] = 255;

            vData[i] = vData[i + 1] = vData[i + 2] = vByte;
            vData[i + 3] = 255;

            HSVToColor(h, s, v, out byte rr, out byte gg, out byte bb);
            rgbRestored[i] = bb;
            rgbRestored[i + 1] = gg;
            rgbRestored[i + 2] = rr;
            rgbRestored[i + 3] = 255;
        }

        h = new(width, height, 96, 96, PixelFormats.Bgra32, null);
        h.WritePixels(new Int32Rect(0, 0, width, height), hData, stride, 0);

        s = new(width, height, 96, 96, PixelFormats.Bgra32, null);
        s.WritePixels(new Int32Rect(0, 0, width, height), sData, stride, 0);

        v= new(width, height, 96, 96, PixelFormats.Bgra32, null);
        v.WritePixels(new Int32Rect(0, 0, width, height), vData, stride, 0);

        rgb = new(width, height, 96, 96, PixelFormats.Bgra32, null);
        rgb.WritePixels(new Int32Rect(0, 0, width, height), rgbRestored, stride, 0);
    }

    private static void ColorToHSV(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double rr = r / 255.0, gg = g / 255.0, bb = b / 255.0;
        double max = Math.Max(rr, Math.Max(gg, bb));
        double min = Math.Min(rr, Math.Min(gg, bb));
        v = max;

        double delta = max - min;
        s = (max == 0) ? 0 : delta / max;

        if (delta == 0)
        {
            h = 0;
        }
        else if (max == rr)
            h = 60 * (((gg - bb) / delta + 6) % 6);
        else if (max == gg)
            h = 60 * (((bb - rr) / delta) + 2);
        else
            h = 60 * (((rr - gg) / delta) + 4);
    }

    private static void HSVToColor(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double rr = 0, gg = 0, bb = 0;

        if (h < 60) { rr = c; gg = x; bb = 0; }
        else if (h < 120) { rr = x; gg = c; bb = 0; }
        else if (h < 180) { rr = 0; gg = c; bb = x; }
        else if (h < 240) { rr = 0; gg = x; bb = c; }
        else if (h < 300) { rr = x; gg = 0; bb = c; }
        else { rr = c; gg = 0; bb = x; }

        r = (byte)((rr + m) * 255);
        g = (byte)((gg + m) * 255);
        b = (byte)((bb + m) * 255);
    }
}
