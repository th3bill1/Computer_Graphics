namespace Computer_Graphics;

internal class OctreeColorQuantizer
{
    private class OctreeNode
    {
        public int PixelCount { get; private set; } = 0;
        public int RedSum { get; private set; } = 0;
        public int GreenSum { get; private set; } = 0;
        public int BlueSum { get; private set; } = 0;
        public OctreeNode[] Children = new OctreeNode[8];
        public bool IsLeaf => PixelCount > 0;

        public void AddColor((byte R, byte G, byte B) color, int level)
        {
            if (level == 8)
            {
                PixelCount++;
                RedSum += color.R;
                GreenSum += color.G;
                BlueSum += color.B;
                return;
            }

            int index = ((color.R >> (7 - level)) & 1) << 2 |
                        ((color.G >> (7 - level)) & 1) << 1 |
                        ((color.B >> (7 - level)) & 1);

            if (Children[index] == null)
                Children[index] = new OctreeNode();

            Children[index].AddColor(color, level + 1);
        }

        public (byte R, byte G, byte B) GetAverageColor()
        {
            if (PixelCount == 0)
                return (128, 128, 128);

            return ((byte)(RedSum / PixelCount),
                    (byte)(GreenSum / PixelCount),
                    (byte)(BlueSum / PixelCount));
        }
    }

    private readonly OctreeNode root = new();
    private readonly List<OctreeNode> leaves = new();

    public void AddColor((byte R, byte G, byte B) color)
    {
        root.AddColor(color, 0);
    }

    public List<(byte R, byte G, byte B)> GeneratePalette(int numColors)
    {
        leaves.Clear();
        CollectLeaves(root, leaves);

        var priorityQueue = new SortedList<int, List<OctreeNode>>();

        foreach (var leaf in leaves)
        {
            if (!priorityQueue.ContainsKey(leaf.PixelCount))
                priorityQueue[leaf.PixelCount] = [];

            priorityQueue[leaf.PixelCount].Add(leaf);
        }

        while (priorityQueue.Count > numColors)
        {
            var firstKey = priorityQueue.Keys.First();
            var smallestLeafList = priorityQueue[firstKey];

            var smallestLeaf = smallestLeafList[0];
            smallestLeafList.RemoveAt(0);

            if (smallestLeafList.Count == 0)
                priorityQueue.Remove(firstKey);

            leaves.Remove(smallestLeaf);
        }

        return leaves.Select(leaf => leaf.GetAverageColor()).ToList();
    }

    private void CollectLeaves(OctreeNode node, List<OctreeNode> leafList)
    {
        if (node.IsLeaf)
            leafList.Add(node);
        else
            foreach (var child in node.Children)
                if (child != null) CollectLeaves(child, leafList);
    }
}
