namespace CGlab5;

using Matrix = Structs.Matrix;
using Vector = Structs.Vector;
internal class Helpers
{
    public static Matrix RotationY(double angle)
    {
        double rad = angle * Math.PI / 180;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        return new Matrix(new double[4, 4]
        {
        { cos,  0, sin, 0 },
        { 0,    1, 0,   0 },
        { -sin, 0, cos, 0 },
        { 0,    0, 0,   1 },
        });
    }

    public static Matrix LookAt(Vector eye, Vector target, Vector up)
    {
        Vector z = Normalize(Subtract(eye, target));
        Vector x = Normalize(Cross(up, z));
        Vector y = Cross(z, x);

        return new Matrix(new double[4, 4]
        {
        { x.X, x.Y, x.Z, -Dot(x, eye) },
        { y.X, y.Y, y.Z, -Dot(y, eye) },
        { z.X, z.Y, z.Z, -Dot(z, eye) },
        { 0,   0,   0,   1 }
        });
    }

    public static Matrix Perspective(double fov, double aspect, double near, double far)
    {
        double f = 1.0 / Math.Tan(fov / 2);
        return new Matrix(new double[4, 4]
        {
        { f / aspect, 0, 0,                              0 },
        { 0,          f, 0,                              0 },
        { 0,          0, (far + near) / (near - far),   (2 * far * near) / (near - far) },
        { 0,          0, -1,                             0 },
        });
    }
    public static Vector Subtract(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static double Dot(Vector a, Vector b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Vector Cross(Vector a, Vector b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X,
        0
    );
    public static Vector Normalize(Vector v)
    {
        double length = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return new Vector(v.X / length, v.Y / length, v.Z / length, 0);
    }

}
