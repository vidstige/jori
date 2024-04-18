using System.Data.Common;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Jori
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private WriteableBitmap _bitmap;

        public MainWindow()
        {
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