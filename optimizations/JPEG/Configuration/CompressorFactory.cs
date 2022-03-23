using JPEG.Compression;

namespace JPEG.Configuration;

public delegate BaseCompressor CompressorFactory(int[] quantizationMatrix);