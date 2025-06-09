using System.Numerics;

namespace CGlab5;

public class Camera
{
    public float Distance = 3f;
    public float Yaw = 0.5f;
    public float Pitch = 0.2f;

    public Vector3 Position => new(
    Distance * MathF.Cos(Pitch) * MathF.Sin(Yaw),
    Distance * MathF.Sin(Pitch),
    Distance * MathF.Cos(Pitch) * MathF.Cos(Yaw));

    public Matrix4x4 GetViewMatrix()
    {
        System.Diagnostics.Debug.WriteLine($"Camera Position: {Position}");
        return Matrix4x4.CreateLookAt(Position, Vector3.Zero, Vector3.UnitY);
    }

    public Matrix4x4 GetProjectionMatrix(float aspect = 800f / 600f)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, aspect, 0.1f, 100f);
    }

    public void Rotate(double dx, double dy)
    {
        Yaw += (float)dx * 0.01f;
        Pitch = Math.Clamp(Pitch - (float)dy * 0.01f, -1.5f, 1.5f);
    }
}
