namespace CGlab5;

public class Mesh
{
    public List<Vertex> Vertices = [];
    public List<int> Indices = [];

    public void AddTriangle(Vertex a, Vertex b, Vertex c)
    {
        int baseIndex = Vertices.Count;
        Vertices.Add(a);
        Vertices.Add(b);
        Vertices.Add(c);
        Indices.Add(baseIndex);
        Indices.Add(baseIndex + 1);
        Indices.Add(baseIndex + 2);
    }
}
