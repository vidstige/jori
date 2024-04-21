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
        private readonly Size _strides;

        public Size Size { get { return _size; } }
        public byte[] Pixels { get { return _pixels; } }

        // Note: strides are in units of 4-bytes rather than single bytes, even though pixels are "bytes"
        public Bitmap(Size size, byte[] pixels, Size strides)
        {
            _size = size;
            _pixels = pixels;
            _strides = strides;
        }

        public Bitmap(Size size, byte[] pixels): this(size, pixels, new Size(1, size.Width))
        {   
        }

        internal int Index(int bx, int by)
        {
            return bx * _strides.Width + by * _strides.Height;
        }
    }
}
