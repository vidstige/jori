using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfEngine.Bitmaps;

namespace WpfEngine
{
    public static class WriteableBitmapExtensions
    {
        // Blends 8-bit component
        public static uint BlendComponent8(uint target, uint source, uint alpha)
        {
            return ((alpha * source) / 0xff) + (((0xff - alpha) * target) / 0xff);
        }

        public static uint BlendBgra32(uint target, uint source) {
            uint target_blue = (target >> 0) & 0xff;
            uint target_green = (target >> 8) & 0xff;
            uint target_red = (target >> 16) & 0xff;
            uint target_alpha = (target >> 24) & 0xff;

            uint source_blue = (source >> 0) & 0xff;
            uint source_green = (source >> 8) & 0xff;
            uint source_red = (source >> 16) & 0xff;
            uint source_alpha = (source >> 24) & 0xff;

            var tmp =
                ((BlendComponent8(target_blue, source_blue, source_alpha) & 0xff) << 0) +
                ((BlendComponent8(target_green, source_green, source_alpha) & 0xff) << 8) +
                ((BlendComponent8(target_red, source_red, source_alpha) & 0xff) << 16) +
                (((uint)0xff << 24));
            return tmp;                
        }

        // Writes to the buffer, respecting alpha values in this bitmap (WriteableBitmap.WritePixels does not)
        public static void Blit(this WriteableBitmap buffer, int x, int y, Bitmap bitmap)
        {
            if (buffer.Format != PixelFormats.Bgra32) throw new ArgumentException("Can only blit to Bgra32");

            buffer.Lock();
            try
            {
                unsafe
                {
                    // fix pixels
                    fixed (byte* bitmapPointer = bitmap.Pixels)
                    {
                        uint* source = (uint*)bitmapPointer;
                        var backBuffer = (uint*)buffer.BackBuffer;
                        for (int by = 0; by < bitmap.Size.Height; by++)
                        {
                            for (int bx = 0; bx < bitmap.Size.Width; bx++)
                            {
                                // special hack as BackBufferStride is in bytes, but we use uint*
                                int targetIndex = (x + bx) + (y + by) * (buffer.BackBufferStride >> 2);
                                int sourceIndex = bitmap.Index(bx, by);
                                // blend colors
                                backBuffer[targetIndex] = BlendBgra32(backBuffer[targetIndex], source[sourceIndex]);
                            }
                        }
                    }
                }
                buffer.AddDirtyRect(new System.Windows.Int32Rect(x, y, bitmap.Size.Width, bitmap.Size.Height));
            }
            finally
            {
                buffer.Unlock();
            }
        }
    }
}
