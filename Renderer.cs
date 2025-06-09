using System.Numerics;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CGlab5;

public class Renderer(WriteableBitmap bmp, Mesh mesh, Camera cam)
{
    private readonly WriteableBitmap bmp = bmp;
    private readonly  Mesh mesh = mesh;
    private readonly Camera cam = cam;
    private readonly float[,] zBuffer = new float[bmp.PixelHeight, bmp.PixelWidth];

    public void Render()
    {
        bmp.Lock();
        try
        {
            unsafe
            {
                IntPtr pBackBuffer = bmp.BackBuffer;
                int stride = bmp.BackBufferStride;

                for (int y = 0; y < bmp.PixelHeight; y++)
                {
                    for (int x = 0; x < bmp.PixelWidth; x++)
                    {
                        *((int*)(pBackBuffer + y * stride + x * 4)) = 0;
                        zBuffer[y, x] = float.PositiveInfinity;
                    }
                }

                Matrix4x4 model = Matrix4x4.Identity;
                Matrix4x4 view = cam.GetViewMatrix();
                Matrix4x4 proj = cam.GetProjectionMatrix();

                Matrix4x4 modelInverseTranspose;
                Matrix4x4.Invert(model, out modelInverseTranspose);
                modelInverseTranspose = Matrix4x4.Transpose(modelInverseTranspose);

                Matrix4x4 mvp = model * view * proj;

                foreach (var idx in mesh.Indices.Chunk(3))
                {
                    var v1 = mesh.Vertices[idx[0]];
                    var v2 = mesh.Vertices[idx[1]];
                    var v3 = mesh.Vertices[idx[2]];

                    Vector3 wp1 = Vector3.Transform(v1.Position, model);
                    Vector3 wp2 = Vector3.Transform(v2.Position, model);
                    Vector3 wp3 = Vector3.Transform(v3.Position, model);

                    Vector3 faceNormal = Vector3.Cross(wp2 - wp1, wp3 - wp1);
                    if (Vector3.Dot(faceNormal, cam.Position - wp1) < 0)
                        continue;

                    Vector4 clip1 = Vector4.Transform(new Vector4(wp1, 1), view * proj);
                    Vector4 clip2 = Vector4.Transform(new Vector4(wp2, 1), view * proj);
                    Vector4 clip3 = Vector4.Transform(new Vector4(wp3, 1), view * proj);

                    var n1 = Vector3.Normalize(Vector3.TransformNormal(v1.Normal, modelInverseTranspose));
                    var n2 = Vector3.Normalize(Vector3.TransformNormal(v2.Normal, modelInverseTranspose));
                    var n3 = Vector3.Normalize(Vector3.TransformNormal(v3.Normal, modelInverseTranspose));

                    RasterizeTriangle(clip1, clip2, clip3, n1, n2, n3, wp1, wp2, wp3, cam.Position, pBackBuffer, stride);
                }

                bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            }
        }
        finally
        {
            bmp.Unlock();
        }
    }

    private unsafe void RasterizeTriangle(
        Vector4 clip1, Vector4 clip2, Vector4 clip3,
        Vector3 n1, Vector3 n2, Vector3 n3,
        Vector3 wp1, Vector3 wp2, Vector3 wp3,
        Vector3 cameraPos,
        IntPtr pBackBuffer,
        int stride)
    {
        Vector3[] ndc = new Vector3[3];
        Vector4[] clips = new[] { clip1, clip2, clip3 };
        for (int i = 0; i < 3; i++)
        {
            if (Math.Abs(clips[i].W) < 1e-5f) return;
            clips[i] /= clips[i].W;
            ndc[i] = new Vector3(clips[i].X, clips[i].Y, clips[i].Z);
        }

        int w = bmp.PixelWidth, h = bmp.PixelHeight;
        Vector2[] screen = ndc.Select(p => new Vector2(
            (p.X + 1) * 0.5f * w,
            (1 - p.Y) * 0.5f * h)).ToArray();

        int minX = (int)MathF.Floor(screen.Min(v => v.X));
        int maxX = (int)MathF.Ceiling(screen.Max(v => v.X));
        int minY = (int)MathF.Floor(screen.Min(v => v.Y));
        int maxY = (int)MathF.Ceiling(screen.Max(v => v.Y));

        minX = Math.Clamp(minX, 0, w - 1);
        maxX = Math.Clamp(maxX, 0, w - 1);
        minY = Math.Clamp(minY, 0, h - 1);
        maxY = Math.Clamp(maxY, 0, h - 1);

        float EdgeFunction(Vector2 a, Vector2 b, Vector2 c) =>
            (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);

        float area = EdgeFunction(screen[0], screen[1], screen[2]);
        if (area == 0) return;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new(x + 0.5f, y + 0.5f);

                float w0 = EdgeFunction(screen[1], screen[2], p);
                float w1 = EdgeFunction(screen[2], screen[0], p);
                float w2 = EdgeFunction(screen[0], screen[1], p);

                if (w0 < 0 || w1 < 0 || w2 < 0) continue;

                w0 /= area;
                w1 /= area;
                w2 /= area;

                float z = w0 * ndc[0].Z + w1 * ndc[1].Z + w2 * ndc[2].Z;
                if (zBuffer[y, x] < z) continue;
                zBuffer[y, x] = z;

                float iw0 = 1f / clip1.W;
                float iw1 = 1f / clip2.W;
                float iw2 = 1f / clip3.W;
                float denom = w0 * iw0 + w1 * iw1 + w2 * iw2;
                w0 = (w0 * iw0) / denom;
                w1 = (w1 * iw1) / denom;
                w2 = (w2 * iw2) / denom;

                Vector3 worldP = w0 * wp1 + w1 * wp2 + w2 * wp3;
                Vector3 normal = Vector3.Normalize(w0 * n1 + w1 * n2 + w2 * n3);

                Vector3 lightPos = new(2, 2, -2); // Static light
                Vector3 L = Vector3.Normalize(lightPos - worldP);
                Vector3 V = Vector3.Normalize(cameraPos - worldP);
                Vector3 R = Vector3.Reflect(L, normal);

                Vector3 ka = new(0.1f, 0.1f, 0.1f);
                Vector3 kd = new(0.7f, 0.3f, 0.3f);
                Vector3 ks = new(0.6f, 0.6f, 0.6f);
                Vector3 Ia = new(1, 1, 1);
                Vector3 Il = new(1, 1, 1);
                float shininess = 32;

                Vector3 ambient = Ia * ka;
                float diff = MathF.Max(Vector3.Dot(normal, L), 0);
                float spec = 0;
                if (diff > 0)
                {
                    spec = MathF.Pow(MathF.Max(Vector3.Dot(R, V), 0), shininess);
                }
                Vector3 diffuse = Il * kd * diff;
                Vector3 specular = Il * ks * spec;

                Vector3 color = ambient + diffuse + specular;
                byte r = (byte)(MathF.Min(1, color.X) * 255);
                byte g = (byte)(MathF.Min(1, color.Y) * 255);
                byte b = (byte)(MathF.Min(1, color.Z) * 255);
                int finalColor = (r << 16) | (g << 8) | b;

                *((int*)(pBackBuffer + y * stride + x * 4)) = finalColor;
            }
        }
    }
}
