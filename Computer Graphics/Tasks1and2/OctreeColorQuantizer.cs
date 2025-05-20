namespace Computer_Graphics.Task1and2;

internal class OctreeColorQuantizer
{
    private class OctreeNode
    {
        public int PixelCount { get; set; } = 0;
        public int RedSum { get;  set; } = 0;
        public int GreenSum { get;  set; } = 0;
        public int BlueSum { get; set; } = 0;
        public OctreeNode[] Children = new OctreeNode[8];
        public OctreeNode? Parent { get; private set; }
        public int Depth { get; private set; }
        public bool IsLeaf => PixelCount > 0;

        public OctreeNode(int depth, OctreeNode? parent = null)
        {
            Depth = depth;
            Parent = parent;
        }

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

            var index = (color.R >> 7 - level & 1) << 2 |
                        (color.G >> 7 - level & 1) << 1 |
                        color.B >> 7 - level & 1;

            if (Children[index] == null)
                Children[index] = new OctreeNode(level + 1, this);

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

    private readonly OctreeNode root = new(0);
    private readonly List<OctreeNode> leaves = new();
    private OctreeNode reducedRoot = new(0);

    public void AddColor((byte R, byte G, byte B) color)
    {
        root.AddColor(color, 0);
    }

    public List<(byte R, byte G, byte B)> GeneratePalette(int numColors)
    {
        leaves.Clear();
        CollectLeaves(root, leaves);

        reducedRoot = CloneTree(root);

        List<OctreeNode> nonLeafNodes = new();
        CollectNonLeafNodes(reducedRoot, nonLeafNodes);

        while (leaves.Count > numColors)
        {
            var deepestNode = nonLeafNodes.OrderByDescending(n => n.Depth)
                                          .ThenBy(n => n.PixelCount)
                                          .FirstOrDefault();

            if (deepestNode == null)
                break;

            CollapseNodeIntoLeaf(deepestNode);
            nonLeafNodes.Remove(deepestNode);
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

    private void CollectNonLeafNodes(OctreeNode node, List<OctreeNode> nonLeafList)
    {
        if (node == null) return;

        if (node.Children.Any(c => c != null))
        {
            nonLeafList.Add(node);
            foreach (var child in node.Children)
                if (child != null) CollectNonLeafNodes(child, nonLeafList);
        }
    }

    private void CollapseNodeIntoLeaf(OctreeNode node)
    {
        if (node == null) return;

        var totalPixels = 0;
        int redSum = 0, greenSum = 0, blueSum = 0;

        foreach (var child in node.Children)
        {
            if (child != null)
            {
                totalPixels += child.PixelCount;
                redSum += child.RedSum;
                greenSum += child.GreenSum;
                blueSum += child.BlueSum;
            }
        }

        node.PixelCount = totalPixels;
        node.RedSum = redSum;
        node.GreenSum = greenSum;
        node.BlueSum = blueSum;
        node.Children = new OctreeNode[8];

        leaves.Add(node);
    }

    private OctreeNode CloneTree(OctreeNode original)
    {
        if (original == null)
            return null;

        var clone = new OctreeNode(original.Depth);

        clone.PixelCount = original.PixelCount;
        clone.RedSum = original.RedSum;
        clone.GreenSum = original.GreenSum;
        clone.BlueSum = original.BlueSum;

        for (var i = 0; i < 8; i++)
        {
            if (original.Children[i] != null)
                clone.Children[i] = CloneTree(original.Children[i]);
        }

        return clone;
    }

    public (byte R, byte G, byte B) FindNearestColor((byte R, byte G, byte B) color)
    {
        return FindNearestColorInTree(reducedRoot, color);
    }

    private (byte R, byte G, byte B) FindNearestColorInTree(OctreeNode node, (byte R, byte G, byte B) color)
    {
        if (node.IsLeaf)
            return node.GetAverageColor();

        var index = (color.R >> 7 - node.Depth & 1) << 2 |
                    (color.G >> 7 - node.Depth & 1) << 1 |
                    color.B >> 7 - node.Depth & 1;

        if (node.Children[index] != null)
            return FindNearestColorInTree(node.Children[index], color);

        return node.GetAverageColor();
    }
}
