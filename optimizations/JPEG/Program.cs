using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JPEG.Encoding;
using JPEG.Images;

namespace JPEG
{
	class Program
	{
		const int CompressionQuality = 70;

		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine(IntPtr.Size == 8 ? "64-bit version" : "32-bit version");
				var sw = Stopwatch.StartNew();
				//var fileName = @"earth.bmp";
				var fileName = @"sample.bmp";
				//var fileName = @"MARBLES.bmp";
				var compressedFileName = fileName + ".compressed." + CompressionQuality;
				var uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";
				
				using (var fileStream = File.OpenRead(fileName))
				using (var bmp = (Bitmap)Image.FromStream(fileStream, false, false))
                {
                    var sw2 = Stopwatch.StartNew();
                    var imageMatrix = new Matrix(bmp);
                    sw2.Stop();
                    Console.WriteLine(sw2.ElapsedMilliseconds);
    
					sw.Stop();
					Console.WriteLine($"{bmp.Width}x{bmp.Height} - {fileStream.Length / (1024.0 * 1024):F2} MB");
					sw.Start();
					var compressionResult = Compress(imageMatrix, CompressionQuality);
					compressionResult.Save(compressedFileName);
				}
    
				sw.Stop();
				Console.WriteLine("Compression: " + sw.Elapsed);
				sw.Restart();
				var compressedImage = CompressedImage.Load(compressedFileName);
                var resultBmp = Uncompress(compressedImage);
				resultBmp.Save(uncompressedFileName, ImageFormat.Bmp);
				Console.WriteLine("Decompression: " + sw.Elapsed);
				Console.WriteLine($"Peak commit size: {MemoryMeter.PeakPrivateBytes() / (1024.0*1024):F2} MB");
				Console.WriteLine($"Peak working set: {MemoryMeter.PeakWorkingSet() / (1024.0*1024):F2} MB");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static CompressedImage Compress(Matrix matrix, int quality = 50)
		{
            var compressors = new ConcurrentBag<DctCompressor>();
            var selectors = new Func<PixelRgb, double>[]
            {
                p => p.Y - 128,
                p => p.Cb - 128,
                p => p.Cr - 128
            };
            var allQuantizedBytesBuffer = new byte[3 * matrix.Width * matrix.Height];
            var pWidth = matrix.Width / DCT.Size;
            var pHeight = matrix.Height / DCT.Size;
            const int length = 3 * DCT.Size * DCT.Size;
            var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(quality);
            Parallel.For(0, pWidth * pHeight, n =>
            {
                var x = n % pWidth * DCT.Size;
                var y = n / pWidth * DCT.Size;
                
                if (!compressors.TryTake(out var compressor))
                {
                    compressor = new DctCompressor(quantizationMatrix);
                }

                var slice = allQuantizedBytesBuffer.AsSpan(n * length, length);
                lock (matrix)
                {
                    matrix.PutPixels(compressor.PixelMap, x, y, DCT.Size, DCT.Size);
                }
                compressor.Compress(slice, selectors);
                
                compressors.Add(compressor);
            });
            Console.WriteLine("Compressors count: " + compressors.Count);

            var compressedBytes = HuffmanCodec.Encode(allQuantizedBytesBuffer, out var decodeTable, out var bitsCount);

			return new CompressedImage
            {
                Quality = quality, 
                CompressedBytes = compressedBytes, 
                BitsCount = bitsCount,
                DecodeTable = decodeTable,
                Height = matrix.Height, 
                Width = matrix.Width
            };
		}
		
        private static Bitmap Uncompress(CompressedImage image)
        {
            var bitmap = new Bitmap(image.Width, image.Height);
            var matrix = new Matrix(bitmap);
            var decoded = HuffmanCodec.Decode(image.CompressedBytes, image.DecodeTable, image.BitsCount);
            
            var transforms = new Action<PixelYCbCr, double>[]
            {
                (p, v) => p.Y = v + 128,
                (p, v) => p.Cb = v + 128,
                (p, v) => p.Cr = v + 128,
            };

            var decompressors = new ConcurrentBag<DctDecompressor>();

            var pWidth = image.Width / DCT.Size;
            var pHeight = image.Height / DCT.Size;
            const int length = 3 * DCT.Size * DCT.Size;
            var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(image.Quality);
            Parallel.For(0, pWidth * pHeight, n =>
            {
                var x = n % pWidth * DCT.Size;
                var y = n / pWidth * DCT.Size;

                if (!decompressors.TryTake(out var decompressor))
                {
                    decompressor = new DctDecompressor(quantizationMatrix);
                }

                var part = decoded.AsSpan(n * length, length);
                var pixelMap = decompressor.Decompress(part, transforms);
                lock (matrix)
                {
                    matrix.SetPixels(pixelMap, x, y, DCT.Size, DCT.Size);
                }
                
                decompressors.Add(decompressor);
            });
            Console.WriteLine("Decompressors count: " + decompressors.Count);

            return bitmap;
        }
	}
}