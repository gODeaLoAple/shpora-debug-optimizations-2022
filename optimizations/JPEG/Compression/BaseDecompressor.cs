using System;
using System.Collections.Generic;
using JPEG.Images;
using JPEG.Utilities;

namespace JPEG.Compression;

public abstract class BaseDecompressor
{
    public const int SquaredSize = Chunk.SquaredSize;
    public const int Size = Chunk.Size;
    private readonly PixelYCbCr[] _pixelMap;
    private readonly int[] _quantizationMatrix;
    private readonly byte[] _zigZagBuffer;
    private readonly double[] _channelBuffer;
    public BaseDecompressor(int[] quantizationMatrix)
    {
        _quantizationMatrix = quantizationMatrix;
        _zigZagBuffer = new byte[SquaredSize];
        _channelBuffer = new double[SquaredSize];
        _pixelMap = new PixelYCbCr[SquaredSize];

        for (var i = 0; i < SquaredSize; i++)
        {
            _pixelMap[i] = new PixelYCbCr();
        }
    }
    
    public PixelYCbCr[] Decompress(Span<byte> decoded, Action<PixelYCbCr, double>[] transforms)
    {
        for (var i = 3 - 1; i >= 0; i--)
        {
            var transform = transforms[i];
            var start = i * SquaredSize;
            var end = start + SquaredSize;
            var slice = decoded[start..end];
            ZigZagUnScan(slice, _zigZagBuffer);
            Decompress(_zigZagBuffer, _channelBuffer, _quantizationMatrix);
            ShiftMatrixValues(_channelBuffer, _pixelMap, transform);
        }

        return _pixelMap;
    }

    public abstract void Decompress(byte[] zigZagBuffer, double[] channelBuffer, int[] quantizationMatrix);
    
    private static void ZigZagUnScan(Span<byte> quantizedBytes, IList<byte> output)
    {
        output[0 * Size +  0] = quantizedBytes[0];
        output[0 * Size +  1] = quantizedBytes[1];
        output[0 * Size +  2] = quantizedBytes[5];
        output[0 * Size +  3] = quantizedBytes[6];
        output[0 * Size +  4] = quantizedBytes[14];
        output[0 * Size +  5] = quantizedBytes[15];
        output[0 * Size +  6] = quantizedBytes[27];
        output[0 * Size +  7] = quantizedBytes[28];
        output[1 * Size + 0] = quantizedBytes[2];
        output[1 * Size + 1] = quantizedBytes[4];
        output[1 * Size + 2] = quantizedBytes[7];
        output[1 * Size + 3] = quantizedBytes[13];
        output[1 * Size + 4] = quantizedBytes[16];
        output[1 * Size + 5] = quantizedBytes[26];
        output[1 * Size + 6] = quantizedBytes[29];
        output[1 * Size + 7] = quantizedBytes[42];
        output[2 * Size + 0] = quantizedBytes[3];
        output[2 * Size + 1] = quantizedBytes[8];
        output[2 * Size + 2] = quantizedBytes[12];
        output[2 * Size + 3] = quantizedBytes[17];
        output[2 * Size + 4] = quantizedBytes[25];
        output[2 * Size + 5] = quantizedBytes[30];
        output[2 * Size + 6] = quantizedBytes[41];
        output[2 * Size + 7] = quantizedBytes[43];
        output[3 * Size + 0] = quantizedBytes[9];
        output[3 * Size + 1] = quantizedBytes[11];
        output[3 * Size + 2] = quantizedBytes[18];
        output[3 * Size + 3] = quantizedBytes[24];
        output[3 * Size + 4] = quantizedBytes[31];
        output[3 * Size + 5] = quantizedBytes[40];
        output[3 * Size + 6] = quantizedBytes[44];
        output[3 * Size + 7] = quantizedBytes[53];
        output[4 * Size + 0] = quantizedBytes[10];
        output[4 * Size + 1] = quantizedBytes[19];
        output[4 * Size + 2] = quantizedBytes[23];
        output[4 * Size + 3] = quantizedBytes[32];
        output[4 * Size + 4] = quantizedBytes[39];
        output[4 * Size + 5] = quantizedBytes[45];
        output[4 * Size + 6] = quantizedBytes[52];
        output[4 * Size + 7] = quantizedBytes[54];
        output[5 * Size + 0] = quantizedBytes[20];
        output[5 * Size + 1] = quantizedBytes[22];
        output[5 * Size + 2] = quantizedBytes[33];
        output[5 * Size + 3] = quantizedBytes[38];
        output[5 * Size + 4] = quantizedBytes[46];
        output[5 * Size + 5] = quantizedBytes[51];
        output[5 * Size + 6] = quantizedBytes[55];
        output[5 * Size + 7] = quantizedBytes[60];
        output[6 * Size + 0] = quantizedBytes[21];
        output[6 * Size + 1] = quantizedBytes[34];
        output[6 * Size + 2] = quantizedBytes[37];
        output[6 * Size + 3] = quantizedBytes[47];
        output[6 * Size + 4] = quantizedBytes[50];
        output[6 * Size + 5] = quantizedBytes[56];
        output[6 * Size + 6] = quantizedBytes[59];
        output[6 * Size + 7] = quantizedBytes[61];
        output[7 * Size + 0] = quantizedBytes[35];
        output[7 * Size + 1] = quantizedBytes[36];
        output[7 * Size + 2] = quantizedBytes[48];
        output[7 * Size + 3] = quantizedBytes[49];
        output[7 * Size + 4] = quantizedBytes[57];
        output[7 * Size + 5] = quantizedBytes[58];
        output[7 * Size + 6] = quantizedBytes[62];
        output[7 * Size + 7] = quantizedBytes[63];
    }

    private static void ShiftMatrixValues(IReadOnlyList<double> subMatrix, IReadOnlyList<PixelYCbCr> pixelMap, Action<PixelYCbCr, double> action)
    {
        for (var i = 0; i < SquaredSize; i++)
        {
            action(pixelMap[i], subMatrix[i]);
        }
    }
}