namespace Computer_Graphics;
internal class Kernels
{
    public static readonly double[,] BlurKernel =
    {
        { 1.0 / 9, 1.0 / 9, 1.0 / 9 },
        { 1.0 / 9, 1.0 / 9, 1.0 / 9 },
        { 1.0 / 9, 1.0 / 9, 1.0 / 9 }
    };

    public static readonly double[,] GaussianBlurKernel =
    {
        { 1.0 / 16, 2.0 / 16, 1.0 / 16 },
        { 2.0 / 16, 4.0 / 16, 2.0 / 16 },
        { 1.0 / 16, 2.0 / 16, 1.0 / 16 }
    };

    public static readonly double[,] SharpenKernel =
    {
        {  0, -1,  0 },
        { -1,  5, -1 },
        {  0, -1,  0 }
    };

    public static readonly double[,] EdgeDetectionKernel =
    {
        { -1, -1, -1 },
        { -1,  8, -1 },
        { -1, -1, -1 }
    };

    public static readonly double[,] EmbossKernel =
    {
        { -2, -1,  0 },
        { -1,  1,  1 },
        {  0,  1,  2 }
    };
}
