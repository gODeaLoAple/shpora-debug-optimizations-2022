using System;
using System.Collections.Generic;
using JPEG.Images;

namespace JPEG;

public abstract class BaseCompressor
{
    public const int SquaredSize = DiscretCosineTransform.SquareSize;
    public const int Size = DiscretCosineTransform.Size;
    public readonly PixelRgb[] PixelMap;
    private readonly int[] _quantizationMatrix;
    private readonly byte[] _bytesBuffer;
    private readonly byte[] _zigZagBuffer;
    private readonly double[] _dctBuffer;

    public BaseCompressor(int[] quantizationMatrix)
    {
        const int squaredSize = Size * Size;
        _quantizationMatrix = quantizationMatrix;
        _bytesBuffer = new byte[squaredSize];
        _zigZagBuffer = new byte[squaredSize];
        _dctBuffer = new double[SquaredSize];

        PixelMap = new PixelRgb[squaredSize];
        for (var i = 0; i < squaredSize; i++)
        {
            PixelMap[i] = new PixelRgb();
        }
    }

    public void Compress(Span<byte> memory, IList<Func<PixelRgb, double>> selectors)
    {
        var count = selectors.Count;
        for (var i = 0; i < count; i++)
        {
            Compress(PixelMap, _dctBuffer, selectors[i]);
            PutQuantized(_dctBuffer, _bytesBuffer, _quantizationMatrix);
            ZigZagScan(_bytesBuffer, _zigZagBuffer);
            _zigZagBuffer.CopyTo(memory[(i * SquaredSize)..]);
        }
    }

    protected abstract void Compress(PixelRgb[] pixelMap, double[] output, Func<PixelRgb, double> selector);
    private static void ZigZagScan(byte[] channelFreqs, byte[] output)
    {
        output[0] = channelFreqs[0 *  Size + 0];
        output[1] = channelFreqs[0 *  Size + 1];
        output[2] = channelFreqs[1 *  Size + 0];
        output[3] = channelFreqs[2 *  Size + 0];
        output[4] = channelFreqs[1 *  Size + 1];
        output[5] = channelFreqs[0 *  Size + 2];
        output[6] = channelFreqs[0 *  Size + 3];
        output[7] = channelFreqs[1 *  Size + 2];
        output[8] = channelFreqs[2 *  Size + 1];
        output[9] = channelFreqs[3 *  Size + 0];
        output[10] = channelFreqs[4 * Size + 0];
        output[11] = channelFreqs[3 * Size + 1];
        output[12] = channelFreqs[2 * Size + 2];
        output[13] = channelFreqs[1 * Size + 3];
        output[14] = channelFreqs[0 * Size + 4];
        output[15] = channelFreqs[0 * Size + 5];
        output[16] = channelFreqs[1 * Size + 4];
        output[17] = channelFreqs[2 * Size + 3];
        output[18] = channelFreqs[3 * Size + 2];
        output[19] = channelFreqs[4 * Size + 1];
        output[20] = channelFreqs[5 * Size + 0];
        output[21] = channelFreqs[6 * Size + 0];
        output[22] = channelFreqs[5 * Size + 1];
        output[23] = channelFreqs[4 * Size + 2];
        output[24] = channelFreqs[3 * Size + 3];
        output[25] = channelFreqs[2 * Size + 4];
        output[26] = channelFreqs[1 * Size + 5];
        output[27] = channelFreqs[0 * Size + 6];
        output[28] = channelFreqs[0 * Size + 7];
        output[29] = channelFreqs[1 * Size + 6];
        output[30] = channelFreqs[2 * Size + 5];
        output[31] = channelFreqs[3 * Size + 4];
        output[32] = channelFreqs[4 * Size + 3];
        output[33] = channelFreqs[5 * Size + 2];
        output[34] = channelFreqs[6 * Size + 1];
        output[35] = channelFreqs[7 * Size + 0];
        output[36] = channelFreqs[7 * Size + 1];
        output[37] = channelFreqs[6 * Size + 2];
        output[38] = channelFreqs[5 * Size + 3];
        output[39] = channelFreqs[4 * Size + 4];
        output[40] = channelFreqs[3 * Size + 5];
        output[41] = channelFreqs[2 * Size + 6];
        output[42] = channelFreqs[1 * Size + 7];
        output[43] = channelFreqs[2 * Size + 7];
        output[44] = channelFreqs[3 * Size + 6];
        output[45] = channelFreqs[4 * Size + 5];
        output[46] = channelFreqs[5 * Size + 4];
        output[47] = channelFreqs[6 * Size + 3];
        output[48] = channelFreqs[7 * Size + 2];
        output[49] = channelFreqs[7 * Size + 3];
        output[50] = channelFreqs[6 * Size + 4];
        output[51] = channelFreqs[5 * Size + 5];
        output[52] = channelFreqs[4 * Size + 6];
        output[53] = channelFreqs[3 * Size + 7];
        output[54] = channelFreqs[4 * Size + 7];
        output[55] = channelFreqs[5 * Size + 6];
        output[56] = channelFreqs[6 * Size + 5];
        output[57] = channelFreqs[7 * Size + 4];
        output[58] = channelFreqs[7 * Size + 5];
        output[59] = channelFreqs[6 * Size + 6];
        output[60] = channelFreqs[5 * Size + 7];
        output[61] = channelFreqs[6 * Size + 7];
        output[62] = channelFreqs[7 * Size + 6];
        output[63] = channelFreqs[7 * Size + 7];
    }

    private static void PutQuantized(double[] channelFreqs, byte[] output, int[] quantizationMatrix)
    {
        for (var i = 0; i < SquaredSize; i++)
        {
            output[i] = (byte)(channelFreqs[i] / quantizationMatrix[i]);
        }
    }
}