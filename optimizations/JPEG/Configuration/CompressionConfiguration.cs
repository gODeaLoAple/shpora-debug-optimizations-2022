namespace JPEG.Configuration;

internal class CompressionConfiguration
{
    public CompressionConfiguration(string name, CompressorFactory compressorFactory, DecompressorFactory decompressorFactory)
    {
        Name = name;
        CompressorFactory = compressorFactory;
        DecompressorFactory = decompressorFactory;
    }

    public string Name { get; }
    public CompressorFactory CompressorFactory { get;  }
    public DecompressorFactory DecompressorFactory { get; }
}