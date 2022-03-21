using System;
using System.Collections.Generic;
using JPEG.Images;

namespace JPEG;

public class DctCompressor
{
    private readonly int _quality;
    private readonly double[] _subMatrix;
    private readonly double[,] _dctBuffer;
    private readonly byte[,] _bytesBuffer;
    private readonly byte[] _zigZagBuffer;
    public PixelRgb[] PixelMap { get; }

    public DctCompressor(int quality)
    {
        const int size = DCT.Size;
        _quality = quality;
        _subMatrix = new double[size * size];
        _dctBuffer = new double[size, size];
        _bytesBuffer =  new byte[size, size];
        _zigZagBuffer =  new byte[size * size];
        PixelMap = new PixelRgb[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                PixelMap[y * size + x] = new PixelRgb();
            }
        }
    }



    public void Compress(double[,] cacheG, Span<byte> memory, IEnumerable<Func<PixelRgb, double>> selectors)
    {
        var i = 0;
        foreach (var selector in selectors)
        {
            PutSubMatrix(selector);
            DCT.DCT2D(_subMatrix, _dctBuffer, cacheG);
            PutQuantized(_dctBuffer, _bytesBuffer, DCT.Size, _quality);
            ZigZagScan(_bytesBuffer, _zigZagBuffer);
            var start = (i * DCT.Size * DCT.Size);
            _zigZagBuffer.CopyTo(memory[start..]);
            ++i;
        }
    }

    private void PutSubMatrix(Func<PixelRgb, double> selector)
    {
        for (var n = 0; n < DCT.SquareSize; ++n)
        {
            _subMatrix[n] = selector(PixelMap[n]);
        }
    }
    
    public static void ZigZagScan(byte[,] channelFreqs, byte[] output)
    {
        output[0] = channelFreqs[0, 0];
        output[1] = channelFreqs[0, 1];
        output[2] = channelFreqs[1, 0];
        output[3] = channelFreqs[2, 0];
        output[4] = channelFreqs[1, 1];
        output[5] = channelFreqs[0, 2];
        output[6] = channelFreqs[0, 3];
        output[7] = channelFreqs[1, 2];
        output[8] = channelFreqs[2, 1];
        output[9] = channelFreqs[3, 0];
        output[10] = channelFreqs[4, 0];
        output[11] = channelFreqs[3, 1];
        output[12] = channelFreqs[2, 2];
        output[13] = channelFreqs[1, 3];
        output[14] = channelFreqs[0, 4];
        output[15] = channelFreqs[0, 5];
        output[16] = channelFreqs[1, 4];
        output[17] = channelFreqs[2, 3];
        output[18] = channelFreqs[3, 2];
        output[19] = channelFreqs[4, 1];
        output[20] = channelFreqs[5, 0];
        output[21] = channelFreqs[6, 0];
        output[22] = channelFreqs[5, 1];
        output[23] = channelFreqs[4, 2];
        output[24] = channelFreqs[3, 3];
        output[25] = channelFreqs[2, 4];
        output[26] = channelFreqs[1, 5];
        output[27] = channelFreqs[0, 6];
        output[28] = channelFreqs[0, 7];
        output[29] = channelFreqs[1, 6];
        output[30] = channelFreqs[2, 5];
        output[31] = channelFreqs[3, 4];
        output[32] = channelFreqs[4, 3];
        output[33] = channelFreqs[5, 2];
        output[34] = channelFreqs[6, 1];
        output[35] = channelFreqs[7, 0];
        output[36] = channelFreqs[7, 1];
        output[37] = channelFreqs[6, 2];
        output[38] = channelFreqs[5, 3];
        output[39] = channelFreqs[4, 4];
        output[40] = channelFreqs[3, 5];
        output[41] = channelFreqs[2, 6];
        output[42] = channelFreqs[1, 7];
        output[43] = channelFreqs[2, 7];
        output[44] = channelFreqs[3, 6];
        output[45] = channelFreqs[4, 5];
        output[46] = channelFreqs[5, 4];
        output[47] = channelFreqs[6, 3];
        output[48] = channelFreqs[7, 2];
        output[49] = channelFreqs[7, 3];
        output[50] = channelFreqs[6, 4];
        output[51] = channelFreqs[5, 5];
        output[52] = channelFreqs[4, 6];
        output[53] = channelFreqs[3, 7];
        output[54] = channelFreqs[4, 7];
        output[55] = channelFreqs[5, 6];
        output[56] = channelFreqs[6, 5];
        output[57] = channelFreqs[7, 4];
        output[58] = channelFreqs[7, 5];
        output[59] = channelFreqs[6, 6];
        output[60] = channelFreqs[5, 7];
        output[61] = channelFreqs[6, 7];
        output[62] = channelFreqs[7, 6];
        output[63] = channelFreqs[7, 7];
    }

    private static void PutQuantized(double[,] channelFreqs, byte[,] output, int size, int quality)
    {
        var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(quality);
        for(var y = 0; y < size; y++)
        {
            for(var x = 0; x < size; x++)
            {
                output[y, x] = (byte)(channelFreqs[y, x] / quantizationMatrix[y, x]);
            }
        }
    }
}