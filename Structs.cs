namespace CGlab5;
internal class Structs
{
    public struct Vector
    {
        public double X, Y, Z, W;
        public Vector(double x, double y, double z, double w = 1) => (X, Y, Z, W) = (x, y, z, w);
    }

    public struct Matrix(double[,] m)
    {
        public double[,] M = m;

        public static Vector Multiply(Matrix mat, Vector v)
        {
            double[] r = new double[4];
            for (int row = 0; row < 4; row++)
            {
                r[row] =
                    v.X * mat.M[row, 0] +
                    v.Y * mat.M[row, 1] +
                    v.Z * mat.M[row, 2] +
                    v.W * mat.M[row, 3];
            }
            return new Vector(r[0], r[1], r[2], r[3]);
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            var result = new double[4, 4];
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                    result[row, col] =
                        a.M[row, 0] * b.M[0, col] +
                        a.M[row, 1] * b.M[1, col] +
                        a.M[row, 2] * b.M[2, col] +
                        a.M[row, 3] * b.M[3, col];
            return new Matrix(result);
        }
    }
}
