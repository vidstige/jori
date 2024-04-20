using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfEngine.Assets.Tileld;

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
        private TileMap? _map = null;

        public Engine(int width, int height, Dispatcher dispatcher)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Interval = TimeSpan.FromMilliseconds(40); // 25 fps
            _timer.Tick += Tick;
            _buffer = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        }

        public void LoadMap(string path)
        {
            _map = TileMap.Load(new Uri(path));
        }

        private void RenderMap(TileMap map)
        {
            var rect = new Int32Rect(0, 0, map.TileSize.Width, map.TileSize.Height);
            foreach (var layer in map.Layers)
            {
                foreach (var chunk in layer.Chunks)
                {
                    for (var y = 0; y < chunk.Size.Height; y++)
                    {
                        for (var x = 0; x < chunk.Size.Width; x++)
                        {
                            var index = (y * chunk.Size.Width) + x; // Assuming the default render order is used which is from right to bottom
                            var gid = chunk.Data[index]; // The tileset tile index

                            // Gid 0 is used to tell there is no tile set
                            if (gid == 0)
                            {
                                continue;
                            }

                            // Render tile
                            rect.X = (chunk.X + x) * map.TileSize.Width;
                            rect.Y = (chunk.Y + y) * map.TileSize.Height;
                            map.BlitTo(_buffer, rect, gid);
                        }
                    }
                }
            }
        }

        private void Tick(object? sender, EventArgs e)
        {
            Render();
        }

        public WriteableBitmap Buffer { get { return _buffer; } }
        

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

            if (_map != null) RenderMap(_map);
        }

    }
}
