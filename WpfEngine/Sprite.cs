using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfEngine
{
    public class Frame
    {
        private int _width;
        private int _height;
        private byte[] _pixels; // bgr32
        private TimeSpan _duration;

        public Frame(int width, int height, byte[] pixels, TimeSpan timeSpan)
        {
            _width = width;
            _height = height;
            _pixels = pixels;
            _duration = timeSpan;
        }

        public void Blit(WriteableBitmap buffer, int x, int y)
        {
            var stride = _width * 4;
            buffer.WritePixels(new Int32Rect(x, y, _width, _height), _pixels, stride, 0);
        }
    }

    public class Sprite
    {
        private readonly Frame[] _frames;
        public Sprite(Frame[] frames)
        {
            _frames = frames;
        } 
    }

}
