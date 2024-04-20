using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfEngine
{
    public interface Blittable
    {
        void BlitTo(WriteableBitmap buffer);
    }

    public class Engine
    {
        private readonly DispatcherTimer _timer;
        private readonly WriteableBitmap _buffer;
        private readonly List<Blittable> _blittables = new List<Blittable>();

        public Engine(int width, int height, Dispatcher dispatcher)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Interval = TimeSpan.FromMilliseconds(40); // 25 fps
            _timer.Tick += Tick;
            _buffer = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        }

        private void Tick(object? sender, EventArgs e)
        {
            Render();
        }

        public WriteableBitmap Buffer { get { return _buffer; } }

        public void Add(Blittable blittable)
        {
            _blittables.Add(blittable);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Render()
        {
            // clear
            var pixels = new byte[_buffer.PixelWidth * _buffer.PixelHeight * 4];
            _buffer.WritePixels(new Int32Rect(0, 0, _buffer.PixelWidth, _buffer.PixelHeight), pixels, _buffer.PixelWidth * 4, 0);

            foreach (var blittable in _blittables)
            {
                blittable.BlitTo(_buffer);
            }
        }

    }
}
