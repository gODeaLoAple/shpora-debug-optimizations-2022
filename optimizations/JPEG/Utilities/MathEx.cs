
using System;

namespace JPEG.Utilities
{
    public struct MatrixIndexHelper
    {
        private readonly int _width;
        private readonly int _height;

        public MatrixIndexHelper(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public int GetIndex(int x, int y)
        {
            return x + y * _width;
        }

        public (int, int) FromIndex(int n) => (n % _width, n / _width);
    }
}