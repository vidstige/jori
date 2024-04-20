using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfEngine
{
    public class Engine
    {
        private readonly Dispatcher _dispatcher;
        private readonly WriteableBitmap _buffer;
        public Engine(int width, int height, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _buffer = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        }

        public WriteableBitmap Buffer { get { return _buffer; } }
    }
}
