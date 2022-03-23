using System.Collections.Generic;
using System.Numerics;
using JPEG.Compression.FFT;

namespace JPEG.Compression.DCT;

public class DctFftDecompressor : BaseDecompressor
 {

     private readonly Complex[] _dequantizedBuffer;
     private readonly Complex[] _buffer;

     public DctFftDecompressor(int[] quantizationMatrix) : base(quantizationMatrix)
     {
         _buffer = new Complex[Size];
         _dequantizedBuffer = new Complex[SquaredSize];
     }
     
     public override void Decompress(byte[] zigZagBuffer, double[] channelBuffer, int[] quantizationMatrix)
     {
         DeQuantize(zigZagBuffer, _dequantizedBuffer, quantizationMatrix);
         FourierTransform.FFT2(_dequantizedBuffer, channelBuffer, _buffer, DirectionFft.Backward);
     }
     
     private static void DeQuantize(IReadOnlyList<byte> quantizedBytes, IList<Complex> output, IReadOnlyList<int> quantizationMatrix)
     {
         for (var i = 0; i < SquaredSize; i++)
         {
             output[i] = ((sbyte)quantizedBytes[i]) * quantizationMatrix[i];//NOTE cast to sbyte not to loose negative numbers
         }
     }
}