using Jori.Engine.Assets;
using System.Data.Common;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

namespace Jori
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private WriteableBitmap _bitmap;

        public MainWindow()
        {
            // https://pixramen.itch.io/2d-action-platformer-sci-fi-vagabond?download
            // https://quintino-pixels.itch.io/wasteland-plataformer-tileset
            // https://theflavare.itch.io/mondstadt-theme-background-pixel-art ?
            // https://thorbjorn.itch.io/tiled?download
            InitializeComponent();
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(40), DispatcherPriority.Normal, Tick, Dispatcher);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _bitmap = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgr32, null);
            GameArea.Source = _bitmap;

            byte[] tile = new byte[32 * 32 * 4]; // B G R
            for (int i = 0; i < tile.Length; i++) { tile[i] = 128; }
            _bitmap.WritePixels(new Int32Rect(0, 0, 32, 32), tile, 32 * 4, 0);

            // load assets
            var uri = new Uri("/Assets/vagabond-idle.aseprite", UriKind.Relative);
            var resource= Application.GetResourceStream(uri);
            AsepriteLoader.Load(new BinaryReader(resource.Stream));
            //using var file = File.OpenRead("Assets\vagabond\assets\aseprite-files\vagabond-idle.aseprite");
            //var idle = AsepriteLoader.Load(new BinaryReader(file));
            //_timer.Start();
        }

        private void Tick(object? sender, EventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}