namespace WpfEngine.Bitmaps
{
    public struct Size
    {
        public Size(int width, int height) { Width = width; Height = height; }
        public int Width;
        public int Height;
    }

    public class Bitmap
    {
        private readonly Size _size;
        private readonly byte[] _pixels;
        private readonly int[] _strides;

        public Size Size { get { return _size; } }
        public byte[] Pixels { get { return _pixels; } }

        // Note: strides are in units of 4-bytes rather than single bytes, even though pixels are "bytes"
        public Bitmap(Size size, byte[] pixels, int[] strides)
        {
            _size = size;
            _pixels = pixels;
            _strides = strides;
        }

        public Bitmap(Size size, byte[] pixels) : this(size, pixels, [0, 1, size.Width])
        {   
        }

        public void FlipHorizontally()
        {
            _strides[0] = _strides[2] - _strides[0] - _strides[1]; // start from the other side
            _strides[1] = -_strides[1];
        }

        public void FlipVertically()
        {
        }

        internal int Index(int bx, int by)
        {
            return _strides[0] + bx * _strides[1] + by * _strides[2];
        }
    }
}
