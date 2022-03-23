using System.Collections.Generic;
using JPEG.Compression.DCT;

namespace JPEG.Compression.FFT;

public class DctDecompressor : BaseDecompressor
{

    private readonly double[] _dequantizedBuffer;

    public DctDecompressor(int[] quantizationMatrix) : base(quantizationMatrix)
    {
        _dequantizedBuffer = new double[SquaredSize];
    }
     
    public override void Decompress(byte[] zigZagBuffer, double[] channelBuffer, int[] quantizationMatrix)
    {
        DeQuantize(zigZagBuffer, _dequantizedBuffer, quantizationMatrix);
        DiscretCosineTransform.IDCT2D(_dequantizedBuffer, channelBuffer);
    }
     
    private static void DeQuantize(IReadOnlyList<byte> quantizedBytes, IList<double> output, IReadOnlyList<int> quantizationMatrix)
    {
        for (var i = 0; i < SquaredSize; i++)
        {
            output[i] = ((sbyte)quantizedBytes[i]) * quantizationMatrix[i];//NOTE cast to sbyte not to loose negative numbers
        }
    }
}