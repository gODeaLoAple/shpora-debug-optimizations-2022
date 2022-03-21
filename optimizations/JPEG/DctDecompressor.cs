using System;
using JPEG.Images;

namespace JPEG;

 public class DctDecompressor
 {
     private readonly byte[,] _zigZagBuffer;
     private readonly double[] _dequantizedBuffer;
     private readonly PixelYCbCr[,] _pixelMap;
     private readonly double[] _channelBuffer;

     public DctDecompressor()
     {
         _zigZagBuffer = new byte[DCT.Size, DCT.Size];
         _dequantizedBuffer = new double[DCT.Size * DCT.Size];
         _channelBuffer = new double[DCT.Size * DCT.Size];
         _pixelMap = new PixelYCbCr[DCT.Size, DCT.Size];

         for (var y = 0; y < DCT.Size; y++)
         {
             for (var x = 0; x < DCT.Size; x++)
             {
                 _pixelMap[y, x] = new PixelYCbCr();
             }
         }
     }
     
     public PixelYCbCr[,] Decompress(Span<byte> decoded, int quality, Action<PixelYCbCr, double>[] transforms)
     {
         for (var i = 3 - 1; i >= 0; i--)
         {
            var transform = transforms[i];
            var start = i * DCT.Size * DCT.Size;
            var end = start + DCT.Size * DCT.Size;
            var slice = decoded[start..end];
            ZigZagUnScan(slice, _zigZagBuffer);
            DeQuantize(_zigZagBuffer, _dequantizedBuffer, quality);
            DCT.IDCT2D(_dequantizedBuffer, _channelBuffer);
            ShiftMatrixValues(_channelBuffer, _pixelMap, transform);
         }


         return _pixelMap;
     }
     
     private static void ZigZagUnScan(Span<byte> quantizedBytes, byte[,] output)
     {
         output[0, 0] = quantizedBytes[0];
         output[0, 1] = quantizedBytes[1];
         output[0, 2] = quantizedBytes[5];
         output[0, 3] = quantizedBytes[6];
         output[0, 4] = quantizedBytes[14];
         output[0, 5] = quantizedBytes[15];
         output[0, 6] = quantizedBytes[27];
         output[0, 7] = quantizedBytes[28];
         output[1,0] = quantizedBytes[2];
         output[1,1] = quantizedBytes[4];
         output[1,2] = quantizedBytes[7];
         output[1,3] = quantizedBytes[13];
         output[1,4] = quantizedBytes[16];
         output[1,5] = quantizedBytes[26];
         output[1,6] = quantizedBytes[29];
         output[1,7] = quantizedBytes[42];
         output[2,0] = quantizedBytes[3];
         output[2,1] = quantizedBytes[8];
         output[2,2] = quantizedBytes[12];
         output[2,3] = quantizedBytes[17];
         output[2,4] = quantizedBytes[25];
         output[2,5] = quantizedBytes[30];
         output[2,6] = quantizedBytes[41];
         output[2,7] = quantizedBytes[43];
         output[3,0] = quantizedBytes[9];
         output[3,1] = quantizedBytes[11];
         output[3,2] = quantizedBytes[18];
         output[3,3] = quantizedBytes[24];
         output[3,4] = quantizedBytes[31];
         output[3,5] = quantizedBytes[40];
         output[3,6] = quantizedBytes[44];
         output[3,7] = quantizedBytes[53];
         output[4,0] = quantizedBytes[10];
         output[4,1] = quantizedBytes[19];
         output[4,2] = quantizedBytes[23];
         output[4,3] = quantizedBytes[32];
         output[4,4] = quantizedBytes[39];
         output[4,5] = quantizedBytes[45];
         output[4,6] = quantizedBytes[52];
         output[4,7] = quantizedBytes[54];
         output[5,0] = quantizedBytes[20];
         output[5,1] = quantizedBytes[22];
         output[5,2] = quantizedBytes[33];
         output[5,3] = quantizedBytes[38];
         output[5,4] = quantizedBytes[46];
         output[5,5] = quantizedBytes[51];
         output[5,6] = quantizedBytes[55];
         output[5,7] = quantizedBytes[60];
         output[6,0] = quantizedBytes[21];
         output[6,1] = quantizedBytes[34];
         output[6,2] = quantizedBytes[37];
         output[6,3] = quantizedBytes[47];
         output[6,4] = quantizedBytes[50];
         output[6,5] = quantizedBytes[56];
         output[6,6] = quantizedBytes[59];
         output[6,7] = quantizedBytes[61];
         output[7,0] = quantizedBytes[35];
         output[7,1] = quantizedBytes[36];
         output[7,2] = quantizedBytes[48];
         output[7,3] = quantizedBytes[49];
         output[7,4] = quantizedBytes[57];
         output[7,5] = quantizedBytes[58];
         output[7,6] = quantizedBytes[62];
         output[7,7] = quantizedBytes[63];
     }
     
     private static void ShiftMatrixValues(double[] subMatrix, PixelYCbCr[,] pixelMap, Action<PixelYCbCr, double> action)
     {
         for(var y = 0; y < DCT.Size; y++)
         for (var x = 0; x < DCT.Size; x++)
         {
             action(pixelMap[y, x], subMatrix[y * DCT.Size + x]);
         }
     }

     private static void DeQuantize(byte[,] quantizedBytes, double[] output, int quality)
     {
         var height = quantizedBytes.GetLength(0);
         var width = quantizedBytes.GetLength(1);
         var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(quality);

         for(var y = 0; y < height; y++)
         {
             for(var x = 0; x < width; x++)
             {
                 output[y * width + x] = ((sbyte)quantizedBytes[y, x]) * quantizationMatrix[y, x];//NOTE cast to sbyte not to loose negative numbers
             }
         }
     }
}