using System;
using System.Collections.Generic;
using JPEG.Images;

namespace JPEG.Compression.DCT;

public class DctCompressor : BaseCompressor
{
    private readonly double[] _subMatrix;

    public DctCompressor(int[] quantizationMatrix) : base(quantizationMatrix)
    {
        _subMatrix = new double[SquaredSize];
    }

    protected override void Compress(PixelRgb[] pixelMap, double[] output, Func<PixelRgb, double> selector)
    {
        PutSubMatrix(pixelMap, _subMatrix, selector);
        DiscretCosineTransform.DCT2D(_subMatrix, output);
    }

    private static void PutSubMatrix(IReadOnlyList<PixelRgb> pixelMap, IList<double> subMatrix, Func<PixelRgb, double> selector)
    {
        for (var n = 0; n < SquaredSize; ++n)
        {
            subMatrix[n] = selector(pixelMap[n]);
        }
    }
}