using System.Numerics;

namespace CGlab5;
public struct Vertex(Vector3 pos, Vector3 norm)
{
    public Vector3 Position = pos;
    public Vector3 Normal = norm;
}
