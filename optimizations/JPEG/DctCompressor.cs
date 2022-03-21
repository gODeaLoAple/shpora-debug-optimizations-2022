using System;
using System.Collections.Generic;
using JPEG.Images;

namespace JPEG;

public class DctCompressor
{
    private readonly int _quality;
    private readonly double[] _subMatrix;
    private readonly double[] _dctBuffer;
    private readonly byte[] _bytesBuffer;
    private readonly byte[] _zigZagBuffer;
    public PixelRgb[] PixelMap { get; }

    public DctCompressor(int quality)
    {
        const int squaredSize = DCT.SquareSize;
        _quality = quality;
        _subMatrix = new double[squaredSize];
        _dctBuffer = new double[squaredSize];
        _bytesBuffer =  new byte[squaredSize];
        _zigZagBuffer =  new byte[squaredSize];
        PixelMap = new PixelRgb[squaredSize];
        for (var i = 0; i < DCT.SquareSize; i++)
        {
            PixelMap[i] = new PixelRgb();
        }
    }



    public void Compress(Span<byte> memory, IEnumerable<Func<PixelRgb, double>> selectors)
    {
        var i = 0;
        foreach (var selector in selectors)
        {
            PutSubMatrix(selector);
            DCT.DCT2D(_subMatrix, _dctBuffer);
            PutQuantized(_dctBuffer, _bytesBuffer, _quality);
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
    
    public static void ZigZagScan(byte[] channelFreqs, byte[] output)
    {
        output[0]  = channelFreqs[0 * DCT.Size + 0];
        output[1]  = channelFreqs[0 * DCT.Size + 1];
        output[2]  = channelFreqs[1 * DCT.Size + 0];
        output[3]  = channelFreqs[2 * DCT.Size + 0];
        output[4]  = channelFreqs[1 * DCT.Size + 1];
        output[5]  = channelFreqs[0 * DCT.Size + 2];
        output[6]  = channelFreqs[0 * DCT.Size + 3];
        output[7]  = channelFreqs[1 * DCT.Size + 2];
        output[8]  = channelFreqs[2 * DCT.Size + 1];
        output[9]  = channelFreqs[3 * DCT.Size + 0];
        output[10] = channelFreqs[4 * DCT.Size + 0];
        output[11] = channelFreqs[3 * DCT.Size + 1];
        output[12] = channelFreqs[2 * DCT.Size + 2];
        output[13] = channelFreqs[1 * DCT.Size + 3];
        output[14] = channelFreqs[0 * DCT.Size + 4];
        output[15] = channelFreqs[0 * DCT.Size + 5];
        output[16] = channelFreqs[1 * DCT.Size + 4];
        output[17] = channelFreqs[2 * DCT.Size + 3];
        output[18] = channelFreqs[3 * DCT.Size + 2];
        output[19] = channelFreqs[4 * DCT.Size + 1];
        output[20] = channelFreqs[5 * DCT.Size + 0];
        output[21] = channelFreqs[6 * DCT.Size + 0];
        output[22] = channelFreqs[5 * DCT.Size + 1];
        output[23] = channelFreqs[4 * DCT.Size + 2];
        output[24] = channelFreqs[3 * DCT.Size + 3];
        output[25] = channelFreqs[2 * DCT.Size + 4];
        output[26] = channelFreqs[1 * DCT.Size + 5];
        output[27] = channelFreqs[0 * DCT.Size + 6];
        output[28] = channelFreqs[0 * DCT.Size + 7];
        output[29] = channelFreqs[1 * DCT.Size + 6];
        output[30] = channelFreqs[2 * DCT.Size + 5];
        output[31] = channelFreqs[3 * DCT.Size + 4];
        output[32] = channelFreqs[4 * DCT.Size + 3];
        output[33] = channelFreqs[5 * DCT.Size + 2];
        output[34] = channelFreqs[6 * DCT.Size + 1];
        output[35] = channelFreqs[7 * DCT.Size + 0];
        output[36] = channelFreqs[7 * DCT.Size + 1];
        output[37] = channelFreqs[6 * DCT.Size + 2];
        output[38] = channelFreqs[5 * DCT.Size + 3];
        output[39] = channelFreqs[4 * DCT.Size + 4];
        output[40] = channelFreqs[3 * DCT.Size + 5];
        output[41] = channelFreqs[2 * DCT.Size + 6];
        output[42] = channelFreqs[1 * DCT.Size + 7];
        output[43] = channelFreqs[2 * DCT.Size + 7];
        output[44] = channelFreqs[3 * DCT.Size + 6];
        output[45] = channelFreqs[4 * DCT.Size + 5];
        output[46] = channelFreqs[5 * DCT.Size + 4];
        output[47] = channelFreqs[6 * DCT.Size + 3];
        output[48] = channelFreqs[7 * DCT.Size + 2];
        output[49] = channelFreqs[7 * DCT.Size + 3];
        output[50] = channelFreqs[6 * DCT.Size + 4];
        output[51] = channelFreqs[5 * DCT.Size + 5];
        output[52] = channelFreqs[4 * DCT.Size + 6];
        output[53] = channelFreqs[3 * DCT.Size + 7];
        output[54] = channelFreqs[4 * DCT.Size + 7];
        output[55] = channelFreqs[5 * DCT.Size + 6];
        output[56] = channelFreqs[6 * DCT.Size + 5];
        output[57] = channelFreqs[7 * DCT.Size + 4];
        output[58] = channelFreqs[7 * DCT.Size + 5];
        output[59] = channelFreqs[6 * DCT.Size + 6];
        output[60] = channelFreqs[5 * DCT.Size + 7];
        output[61] = channelFreqs[6 * DCT.Size + 7];
        output[62] = channelFreqs[7 * DCT.Size + 6];
        output[63] = channelFreqs[7 * DCT.Size + 7];
    }

    private static void PutQuantized(double[] channelFreqs, byte[] output, int quality)
    {
        var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(quality);
        const int size = DCT.Size;
        for(var y = 0; y < size; y++)
        {
            for(var x = 0; x < size; x++)
            {
                output[y * size +  x] = (byte)(channelFreqs[y * size +  x] / quantizationMatrix[y, x]);
            }
        }
    }
}