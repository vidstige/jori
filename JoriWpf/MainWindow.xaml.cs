using System.IO;
using System.Windows;
using System.Windows.Input;
using WpfEngine;
using WpfEngine.Assets.Aseprite;

namespace Jori
{
    class Player
    {
        public int X;
        public int Y;
    }

    public class Game
    {
    }

    public partial class MainWindow : Window
    {
        private Engine _engine;
        private Player _player = new Player() { X = 100, Y = 50 };
        public MainWindow()
        {
            // https://pixramen.itch.io/2d-action-platformer-sci-fi-vagabond?download
            // https://quintino-pixels.itch.io/wasteland-plataformer-tileset
            // https://theflavare.itch.io/mondstadt-theme-background-pixel-art ?
            // https://thorbjorn.itch.io/tiled?download
            InitializeComponent();
            _engine = new Engine(32 * 32, 32 * 21, Dispatcher);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GameArea.Source = _engine.Buffer;

            // load assets
            var uri = new Uri("/Assets/vagabond-idle.aseprite", UriKind.Relative);
            var resource = Application.GetResourceStream(uri);

            _engine.LoadMap("C:\\Users\\Samuel Carlsson\\source\\repos\\Jori\\Levels\\Mondstadt Tileset Platform - Basic\\basic.tmx");

            var idle = AsepriteLoader.Load(new BinaryReader(resource.Stream));
            _engine.Start();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    _player.X += 5;
                    break;
                case Key.Right:
                    _player.X -= 5;
                    break;
            }
        }
    }
}