using System;
using System.Numerics;
using JPEG.Images;

namespace JPEG;

public class DctFftDecompressor : BaseDecompressor
 {

     private readonly Complex[] _dequantizedBuffer;
     private readonly Complex[] _buffer;

     public DctFftDecompressor(int[] quantizationMatrix) : base(quantizationMatrix)
     {
         _buffer = new Complex[DiscretCosineTransform.Size];
         _dequantizedBuffer = new Complex[DiscretCosineTransform.Size * DiscretCosineTransform.Size];
     }
     
     public override void Decompress(byte[] zigZagBuffer, double[] channelBuffer, int[] quantizationMatrix)
     {
         DeQuantize(zigZagBuffer, _dequantizedBuffer, quantizationMatrix);
         FourierTransform.FFT2(_dequantizedBuffer, channelBuffer, _buffer, DirectionFft.Backward);
     }
     
     private static void DeQuantize(byte[] quantizedBytes, Complex[] output, int[] quantizationMatrix)
     {
         for (var i = 0; i < DiscretCosineTransform.SquareSize; i++)
         {
             output[i] = ((sbyte)quantizedBytes[i]) * quantizationMatrix[i];//NOTE cast to sbyte not to loose negative numbers
         }
     }
}