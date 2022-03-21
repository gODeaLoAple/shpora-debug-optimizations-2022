﻿using System;
using System.Data;

namespace JPEG
{
    public class DCT
    {
        public const int Size = 8;
        public const int SquareSize = Size * Size;
        private static readonly double Sqrt2Reversed = 1 / Math.Sqrt(2);

        public static readonly double[,] F;

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

            F = f;

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
                    CacheDtc[m * SquareSize + n] = f[x, u] * f[y, v] * beta * Alpha(u) * Alpha(v);
                    CacheInverseDtc[m + n * SquareSize] = f[u, x] * f[v, y] * beta * Alpha(x) * Alpha(y);
                }
            }
        }
        
        public static void DCT2D(double[] input, double[,] output)
        {
            for (var n = 0; n < SquareSize; n++)
            {
                var s = 0d;
                var offset = n * SquareSize;

                for (var m = 0; m < SquareSize; m++)
                {
                    s += input[m] * CacheDtc[m + offset];
;               }

                output[n / Size, n % Size] = s;
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