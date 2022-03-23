using JPEG.Compression;

namespace JPEG.Configuration;

public delegate BaseDecompressor DecompressorFactory(int[] quantizationMatrix);