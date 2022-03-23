using System;
using System.Numerics;

namespace JPEG
{
    public class DCT
    {
        public const int Size = 8;
        public const int SquareSize = Size * Size;
        private static readonly double Sqrt2Reversed = 1 / Math.Sqrt(2);

        private static readonly double[] CacheDtc;
        private static readonly double[] CacheInverseDtc;

        static DCT()
        {
            var f = new double[Size, Size];
            for (var u = 0; u < Size; ++u)
            {
                for (var m = 0; m < Size; ++m)
                {
                    f[u, m] = Math.Cos((2d * u + 1d) * m * Math.PI / (2 * Size));
                }
            }

            var beta = Beta(Size, Size);
            CacheDtc = new double[SquareSize * SquareSize];
            CacheInverseDtc = new double[SquareSize * SquareSize];
            for (var n = 0; n < SquareSize; ++n)
            {
                for (var m = 0; m < SquareSize; ++m)
                {
                    var x = n % Size;
                    var y = n / Size;
                    var u = m % Size;
                    var v = m / Size;
                    var index = m  * SquareSize + n;
                    CacheDtc[index] = f[x, u] * f[y, v] * beta * Alpha(u) * Alpha(v);
                    CacheInverseDtc[index] = f[u, x] * f[v, y] * beta * Alpha(x) * Alpha(y);
                }
            }
        }
        
        public static void DCT2D(double[] input, double[] output)
        {
            for (var n = 0; n < SquareSize; n++)
            {
                var s = 0d;
                var offset = n * SquareSize;

                for (var m = 0; m < SquareSize; m++)
                {
                    s += input[m] * CacheDtc[m + offset];
;               }

                output[n] = s;
            }
        }
        
        private static void DTF1D(Span<Complex> input, Span<Complex> output, int count = SquareSize, int offset = 1)
        {
            if (count == 1)
            {
                output[0] = input[0];
            }
            else
            {
                DTF1D(input, output, count / 2, 2 * offset);
                DTF1D(input[offset..], output, count / 2, 2 * offset);
                for (var k = 0; k < count / 2 - 1; k++)
                {
                    var p = output[k];
                    var q = Complex.Exp(k * - 2 * Math.PI / count) * output[k + count / 2];
                    output[k] = p + q;
                    output[k + count / 2] = p - q;
                }
            }
        }
        
        public static void IDCT2D(double[] input, double[] output)
        {
            for (var n = 0; n < SquareSize; n++)
            {
                var offset = n * SquareSize;
                var s = 0d;

                for (var m = 0; m < SquareSize; m++)
                {
                    s += input[m] * CacheInverseDtc[m + offset];
                }
                
                output[n] = s;
            }
        }

        public static double Alpha(int u) => u == 0 ? Sqrt2Reversed : 1;

        private static double Beta(int height, int width)
        {
            return 1d / width + 1d / height;
        }
    }
}