using Jori.Engine;
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
        private Engine.Engine _engine;

        public MainWindow()
        {
            // https://pixramen.itch.io/2d-action-platformer-sci-fi-vagabond?download
            // https://quintino-pixels.itch.io/wasteland-plataformer-tileset
            // https://theflavare.itch.io/mondstadt-theme-background-pixel-art ?
            // https://thorbjorn.itch.io/tiled?download
            InitializeComponent();
            _engine = new Engine.Engine(32 * 16, 32 * 8, Dispatcher);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameArea.Source = _engine.Buffer;

            // load assets
            var uri = new Uri("/Assets/vagabond-idle.aseprite", UriKind.Relative);
            var resource= Application.GetResourceStream(uri);
            AsepriteLoader.Load(new BinaryReader(resource.Stream));
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