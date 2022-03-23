using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using JPEG.Compression;
using JPEG.Compression.DCT;
using JPEG.Compression.FFT;
using JPEG.Configuration;
using JPEG.Encoding;
using JPEG.Images;
using JPEG.Utilities;

namespace JPEG
{
    class Program
	{
		const int CompressionQuality = 70;
        private const int Size = Chunk.Size;

        private static readonly CompressionConfiguration[] Configurations = {
            new(
                "DCT",
                matrix => new DctCompressor(matrix),
                matrix => new DctDecompressor(matrix)),
            new(
                "FFT",
                matrix => new DctFftCompressor(matrix),
                matrix => new DctFftDecompressor(matrix)),
        };
        
        static void Main(string[] args)
		{
			try
			{
				Console.WriteLine(IntPtr.Size == 8 ? "64-bit version" : "32-bit version");
				var sw = Stopwatch.StartNew();
                var configuration = Configurations[1];
                Console.WriteLine($"Algorithm: {configuration.Name}");
                var files = new[]
                {
                    @"earth.bmp",
                    @"sample.bmp",
                    @"MARBLES.bmp",
                };
				var fileName = files[1];
                Console.WriteLine($"Image: {fileName}");
				var compressedFileName = fileName + ".compressed." + CompressionQuality;
				var uncompressedFileName = fileName + ".uncompressed." + CompressionQuality + ".bmp";
				
				using (var fileStream = File.OpenRead(fileName))
				using (var bmp = (Bitmap)Image.FromStream(fileStream, false, false))
                {
                    var imageMatrix = new Matrix(bmp);
    
					sw.Stop();
					Console.WriteLine($"{bmp.Width}x{bmp.Height} - {fileStream.Length / (1024.0 * 1024):F2} MB");
					sw.Start();
					var compressionResult = Compress(imageMatrix, configuration.CompressorFactory, CompressionQuality);
					compressionResult.Save(compressedFileName);
				}
    
				sw.Stop();
				Console.WriteLine("Compression: " + sw.ElapsedMilliseconds);
				sw.Restart();
				var compressedImage = CompressedImage.Load(compressedFileName);
                var resultBmp = Uncompress(compressedImage, configuration.DecompressorFactory);
				resultBmp.Save(uncompressedFileName, ImageFormat.Bmp);
				Console.WriteLine("Decompression: " + sw.ElapsedMilliseconds);
				Console.WriteLine($"Peak commit size: {MemoryMeter.PeakPrivateBytes() / (1024.0*1024):F2} MB");
				Console.WriteLine($"Peak working set: {MemoryMeter.PeakWorkingSet() / (1024.0*1024):F2} MB");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
        }


        
		private static CompressedImage Compress(Matrix matrix, CompressorFactory factory, int quality = 50)
		{
            var compressors = new ConcurrentBag<BaseCompressor>();
            var selectors = new Func<PixelRgb, double>[]
            {
                p => p.Y - 128,
                p => p.Cb - 128,
                p => p.Cr - 128
            };
            var allQuantizedBytesBuffer = new byte[3 * matrix.Width * matrix.Height];
            var pWidth = matrix.Width / Size;
            var pHeight = matrix.Height / Size;
            const int length = 3 * Size * Size;
            var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(quality);
            Parallel.For(0, pWidth * pHeight, n =>
            {
                var x = n % pWidth * Size;
                var y = n / pWidth * Size;
                
                if (!compressors.TryTake(out var compressor))
                {
                    compressor = factory(quantizationMatrix);
                }

                var slice = allQuantizedBytesBuffer.AsSpan(n * length, length);
                lock (matrix)
                {
                    matrix.PutPixels(compressor.PixelMap, x, y, Size, Size);
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
		
        private static Bitmap Uncompress(CompressedImage image, DecompressorFactory factory)
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

            var decompressors = new ConcurrentBag<BaseDecompressor>();

            var pWidth = image.Width / Size;
            var pHeight = image.Height / Size;
            const int length = 3 * Size * Size;
            var quantizationMatrix = QuantizationMatrixHelper.GetQuantizationMatrix(image.Quality);
            Parallel.For(0, pWidth * pHeight, n =>
            {
                var x = n % pWidth * Size;
                var y = n / pWidth * Size;

                if (!decompressors.TryTake(out var decompressor))
                {
                    decompressor = factory(quantizationMatrix);
                }

                var part = decoded.AsSpan(n * length, length);
                var pixelMap = decompressor.Decompress(part, transforms);
                lock (matrix)
                {
                    matrix.SetPixels(pixelMap, x, y, Size, Size);
                }
                
                decompressors.Add(decompressor);
            });
            Console.WriteLine("Decompressors count: " + decompressors.Count);

            return bitmap;
        }
	}
}