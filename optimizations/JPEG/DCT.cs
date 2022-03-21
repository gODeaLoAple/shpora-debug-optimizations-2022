using System;

namespace JPEG
{
    public class DCT
    {
        public const int Size = 8;
        private static readonly double Sqrt2Reversed = 1 / Math.Sqrt(2);

        public static readonly double[,] BasisFunction;

        static DCT()
        {
            BasisFunction = new double[Size, Size];
            for (var u = 0; u < Size; ++u)
            {
                for (var m = 0; m < Size; ++m)
                {
                    BasisFunction[u, m] = Math.Cos((2d * u + 1d) * m * Math.PI / (2 * Size));
                }
            }
        }
        
        public static void DCT2D(double[,] input, double[,] output, double[,] cacheG)
        {
            var height = input.GetLength(0);
            var width = input.GetLength(1);
            var beta = Beta(height, width);

            for (var v = 0; v < height; ++v)
            {
                for (var u = 0; u < width; ++u)
                {
                    var s = 0d;
                    
                    for (var y = 0; y < height; ++y)
                    {
                        for (var x = 0; x < width; ++x)
                        {
                            s += input[x, y] * cacheG[x, u] * cacheG[y, v];
                        }
                    }
                
                    output[u, v] = s * beta  * Alpha(u) * Alpha(v);
                }
            }
        }

        public static void IDCT2D(double[,] input, double[,] output, double[,] cacheG)
        {
            var width = input.GetLength(1);
            var height = input.GetLength(0);
            var beta = Beta(height, width);

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var s = 0d;
                    for (var v = 0; v < height; ++v)
                    {
                        for (var u = 0; u < width; ++u)
                        {
                            s += input[u, v] * cacheG[x, u] * cacheG[y, v] * DCT.Alpha(u) * DCT.Alpha(v);
                        }
                    }

                    output[x, y] = s * beta;
                }
            }
        }

        public static double Alpha(int u) => u == 0 ? Sqrt2Reversed : 1;

        private static double Beta(int height, int width)
        {
            return 1d / width + 1d / height;
        }
    }
}