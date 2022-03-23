using System;
using System.Collections.Generic;
using System.Numerics;

namespace JPEG.Compression.FFT;

public static class FourierTransform
{
    private const int MaxBits = 14;
    private static readonly int[][] ReversedBits = new int[MaxBits][];
    private static readonly Complex[,][] ComplexRotation = new Complex[MaxBits, 2][];

    public static void FFT(Complex[] data, DirectionFft directionFft)
    {
        var n = data.Length;
        var m = Utils.Log2(n);

        ReorderData(data);

        int tn = 1, tm;

        for (var k = 1; k <= m; k++)
        {
            var rotation = GetComplexRotation(k, directionFft);

            tm = tn;
            tn <<= 1;

            for (var i = 0; i < tm; i++)
            {
                var t = rotation[i];

                for (var even = i; even < n; even += tn)
                {
                    var odd = even + tm;
                    var ce = data[even];
                    var co = data[odd];

                    var tr = co.Real * t.Real - co.Imaginary * t.Imaginary;
                    var ti = co.Real * t.Imaginary + co.Imaginary * t.Real;

                    data[even] += new Complex(tr, ti);
                    data[odd] = new Complex(ce.Real - tr, ce.Imaginary - ti);
                }
            }
        }

        if (directionFft == DirectionFft.Forward)
        {
            for (var i = 0; i < n; i++)
            {
                data[i] /= n;
            }
        }
    }

    public static void FFT2(Complex[] data, double[] output, Complex[] buffer, DirectionFft directionFft)
    {
        if (directionFft == DirectionFft.Forward)
        {
            ForwardFft(data, output, buffer);
        }
        else
        {
            InverseFft(data, output, buffer);
        }
    }

    public static void ForwardFft(Complex[] data, double[] output, Complex[] buffer)
    {
        var size = buffer.Length;
        var halfSize = size / 2;
        for (var j = 0; j < size; j++)
        {
            for (var i = 0; i < halfSize; i++)
            {
                buffer[i] = data[i * 2 + j * size];
                buffer[size - 1 - i] = data[(i * 2 + 1) + j * size];
            }
            FFT(buffer, DirectionFft.Forward);
            for (var i = 0; i < size; i++)
            {
                data[i  + j * size] = W(i, 2 * size) * buffer[i];
            }
        }
        
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < halfSize; j++)
            {
                buffer[j] = data[i + (j * 2) * size];
                buffer[size - 1 - j] = data[i + (j * 2 + 1) * size];
            }
            FFT(buffer, DirectionFft.Forward);
            for (var j = 0; j < size; j++)
            {
                data[i + j * size] =  W(j, 2 * size) *  buffer[j];
            }
        }
        
        for (var j = 0; j < size; j++)
        {
            for (var i = 0; i < size; i++)
            {
                output[i + j * size] = 8 * data[i + j * size].Real;
            }
        }
    }

    public static void InverseFft(Complex[] data, double[] output, Complex[] buffer)
    {
        var size = buffer.Length;
        var halfSize = size / 2;
        
        for (var j = 0; j < size; j++)
        {
            for (var i = 0; i < size; i++)
            {
                buffer[i] = W(i, 2 * size) * data[i + j * size];
            }
            FFT(buffer, DirectionFft.Backward);
            for (var i = 0; i < halfSize; i++)
            {
                data[i * 2 + j * size] = buffer[i];
                data[(i * 2 + 1) + j * size] =  buffer[size - 1 - i];
            }
        }
        
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                buffer[j] =W(j, 2 * size) * data[i + j * size];
            }
            FFT(buffer, DirectionFft.Backward);
            for (var j = 0; j < halfSize; j++)
            {
                data[i + (j * 2) * size] = buffer[j];
                data[i + (j * 2 + 1) * size] =  buffer[size - 1 - j];
            }
        }
        
        for (var j = 0; j < size; j++)
        {
            for (var i = 0; i < size; i++)
            {
                output[i + j * size] = data[i + j * size].Real / 8;
            }
        }
    }
    
    public static Complex W(double k, double m)
    {
        return Complex.Exp(new Complex(0, - k * Math.PI / m));
    }

    private static int[] GetReversedBits(int numberOfBits)
    {
        if (ReversedBits[numberOfBits - 1] == null)
        {
            var n = Utils.Pow2(numberOfBits);
            var rBits = new int[n];

            for (var i = 0; i < n; i++)
            {
                var oldBits = i;
                var newBits = 0;

                for (var j = 0; j < numberOfBits; j++)
                {
                    newBits = (newBits << 1) | (oldBits & 1);
                    oldBits >>= 1;
                }

                rBits[i] = newBits;
            }

            ReversedBits[numberOfBits - 1] = rBits;
        }

        return ReversedBits[numberOfBits - 1];
    }

    private static Complex[] GetComplexRotation(int numberOfBits, DirectionFft directionFft)
    {
        var directionIndex = directionFft == DirectionFft.Forward ? 0 : 1;

        if (ComplexRotation[numberOfBits - 1, directionIndex] == null)
        {
            var n = 1 << (numberOfBits - 1);
            var uR = 1.0;
            var uI = 0.0;
            var angle = Math.PI / n * (int)directionFft;
            var wR = Math.Cos(angle);
            var wI = Math.Sin(angle);
            double t;
            var rotation = new Complex[n];

            for (var i = 0; i < n; i++)
            {
                rotation[i] = new Complex(uR, uI);
                t = uR * wI + uI * wR;
                uR = uR * wR - uI * wI;
                uI = t;
            }

            ComplexRotation[numberOfBits - 1, directionIndex] = rotation;
        }

        return ComplexRotation[numberOfBits - 1, directionIndex];
    }

    private static void ReorderData(IList<Complex> data)
    {
        var len = data.Count;

        var rBits = GetReversedBits(Utils.Log2(len));

        for (var i = 0; i < len; i++)
        {
            var s = rBits[i];

            if (s > i)
            {
                (data[i], data[s]) = (data[s], data[i]);
            }
        }
    }
}