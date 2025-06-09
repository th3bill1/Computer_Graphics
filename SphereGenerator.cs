using System.Numerics;

namespace CGlab5;

public static class SphereGenerator
{
    public static Mesh CreateSphere(float radius, int meridians, int parallels)
    {
        var mesh = new Mesh();

        for (int i = 0; i <= parallels; i++)
        {
            float v = i / (float)parallels;
            float theta = v * MathF.PI;

            for (int j = 0; j <= meridians; j++)
            {
                float u = j / (float)meridians;
                float phi = u * 2 * MathF.PI;

                float x = radius * MathF.Sin(theta) * MathF.Cos(phi);
                float y = radius * MathF.Cos(theta);
                float z = radius * MathF.Sin(theta) * MathF.Sin(phi);

                Vector3 pos = new(x, y, z);
                mesh.Vertices.Add(new Vertex(pos, Vector3.Normalize(pos)));
            }
        }

        for (int i = 0; i < parallels; i++)
        {
            for (int j = 0; j < meridians; j++)
            {
                int a = i * (meridians + 1) + j;
                int b = a + meridians + 1;

                mesh.Indices.Add(a);
                mesh.Indices.Add(b);
                mesh.Indices.Add(a + 1);

                mesh.Indices.Add(a + 1);
                mesh.Indices.Add(b);
                mesh.Indices.Add(b + 1);
            }
        }

        return mesh;
    }
}
