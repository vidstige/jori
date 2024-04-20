using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace WpfEngine.Assets.Tileld
{
    public struct Size
    {
        public int Width;
        public int Height;
    }

    public class Chunk
    {
        private readonly int _x;
        private readonly int _y;
        private readonly Size _size;
        private readonly uint[] _data;
        
        public Size Size { get { return _size; } }
        public uint[] Data { get { return _data; } }

        public int X { get { return _x; } }
        public int Y { get { return _y; } }

        public Chunk(int x, int y, int width, int height, uint[] data)
        {
            _x = x;
            _y = y;
            _size = new Size() { Width = width, Height = height };
            _data = data;
        }
    }

    public class Layer
    {
        private readonly string _name;
        private readonly List<Chunk> _chunks;

        public Layer(string name, List<Chunk> chunks)
        {
            _name = name;
            _chunks = chunks;
        }

        public IEnumerable<Chunk> Chunks { get { return _chunks; } }
    }

    public class TileSet
    {
        private readonly string _name;
        private readonly int _firstgid;
        private readonly Size _tileSize;
        private readonly uint _count;
        private readonly int _columns;
        private readonly BitmapSource _image;

        public TileSet(string name, int firstgid, Size tileSize, uint count, int columns, BitmapSource image)
        {
            _name = name;
            _firstgid = firstgid;
            _tileSize = tileSize;
            _count = count;
            _columns = columns;
            _image = image;
        }
        public Size TileSize { get { return _tileSize; } }
        public BitmapSource Image { get { return _image; } }

        public bool Contains(uint gid)
        {
            return gid >= _firstgid && gid < _firstgid + _count;
        }

        internal Int32Rect GetSourceRect(uint gid)
        {
            var index = (int)(gid - _firstgid);
            var x = index % _columns;
            var y = index / _columns;
            return new Int32Rect(x * _tileSize.Width, y * _tileSize.Height, _tileSize.Width, _tileSize.Height);
        }
    }

    static class XmlElementExtensions
    {
        public static int GetInt32Attribute(this XmlElement node, string name)
        {
            var attribute = node.GetAttributeNode(name);
            if (attribute == null)
            {
                return 0;
            }
            return Convert.ToInt32(attribute.Value);
        }
        public static uint GetUInt32Attribute(this XmlElement node, string name)
        {
            var attribute = node.GetAttributeNode(name);
            if (attribute == null)
            {
                return 0;
            }
            return Convert.ToUInt32(attribute.Value);
        }
    }

    public class TileMap
    {
        private readonly Size _tileSize;
        private readonly List<Layer> _layers;
        private readonly List<TileSet> _tileSets;

        public IEnumerable<Layer> Layers { get { return _layers;  } }
        public IEnumerable<TileSet> TileSets { get { return _tileSets; } }

        public Size TileSize { get { return _tileSize; } }

        public TileMap(Size tileSize, List<Layer> layers, List<TileSet> tilesets)
        {
            _tileSize = tileSize;
            _layers = layers;
            _tileSets = tilesets;
        }

        public void BlitTo(WriteableBitmap buffer, Int32Rect rect, uint gid)
        {
            bool hflip = (gid & (1 << 31)) != 0;
            bool vflip = (gid & (1 << 30)) != 0;
            gid &= ~((1 << 31) + (1 << 30) + (1 << 29)); // clear top three bits
            foreach (var tileSet in _tileSets)
            {
                if (tileSet.Contains(gid))
                {
                    // create pixel array for tile
                    var format = PixelFormats.Bgra32;
                    var stride = tileSet.TileSize.Width * format.BitsPerPixel / 8;
                    var pixels = new byte[stride * tileSet.TileSize.Height];
                    // compute source rect
                    var sourceRect = tileSet.GetSourceRect(gid);
                    tileSet.Image.CopyPixels(sourceRect, pixels, stride, 0);
                    // blit pixels to buffer
                    if (rect.X >= 0 && rect.Y >= 0 && rect.X + rect.Width <= buffer.PixelWidth && rect.Y + rect.Height <= buffer.PixelHeight)
                    {
                        buffer.WritePixels(rect, pixels, stride, 0);
                    }
                    return;
                }
            }
        }
        
        private static TileSet ReadTileSetContent(int firstgid, XmlElement node, Uri uri)
        {
            var images = new List<BitmapSource>();
            foreach (XmlElement child in node.ChildNodes)
            {
                if (child.Name == "image")
                {
                    var source = child.GetAttribute("source");
                    var imageUri = new Uri(uri, source);
                    // load image
                    var bitmapImage = new BitmapImage(imageUri);
                    // convert it into bgra32
                    var converted = new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgra32, null, 0);
                    images.Add(converted);
                }
            }
            // TODO combine images if multiple?
            return new TileSet(
                node.GetAttribute("name"),
                firstgid,
                new Size() { Width = node.GetInt32Attribute("tilewidth"), Height = node.GetInt32Attribute("tileheight") },
                node.GetUInt32Attribute("tilecount"),
                node.GetInt32Attribute("columns"),
                images[0]
            );
        }

        private static TileSet ReadTileSet(XmlElement node, Uri uri)
        {
            // firstgid is always in the tileset element
            var firstgid = node.GetInt32Attribute("firstgid");

            var source = node.GetAttribute("source");
            if (!string.IsNullOrEmpty(source))
            {
                // read from file
                var doc = new XmlDocument();
                var sourceUri = new Uri(uri, source);
                doc.Load(UriOpener.Open(sourceUri));
                return ReadTileSetContent(firstgid, doc.DocumentElement, uri);
            } else {
                // read inline
                return ReadTileSetContent(firstgid, node, uri);
            }
        }

        private static Chunk ReadLayerChunk(XmlElement node)
        {
            var width = node.GetInt32Attribute("width");
            var height = node.GetInt32Attribute("height");

            var csv = node.InnerText;

            // parse csv
            uint[] data = new uint[width * height];
            string[] lines = csv.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int y = 0; y < lines.Length; y++)
            {
                var columns = lines[y].Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int x = 0; x < columns.Length; x++)
                {
                    data[width * y + x] = Convert.ToUInt32(columns[x]);
                }
            }

            var chunk = new Chunk(
                node.GetInt32Attribute("x"),
                node.GetInt32Attribute("y"),
                width,
                height,
                data
            );
            return chunk;
        }

        private static Layer ReadLayer(XmlElement node)
        {
            var name = node.GetAttribute("name");
            var width = node.GetInt32Attribute("width");
            var height = node.GetInt32Attribute("height");
            var chunks = new List<Chunk>();
            foreach (XmlElement data in node.ChildNodes)
            {
                var encoding = data.GetAttribute("encoding");
                if (encoding != "csv") {
                    throw new Exception($"Invalid encoding: {encoding}");
                }
                foreach (XmlElement chunk in data.ChildNodes)
                {
                    chunks.Add(ReadLayerChunk(chunk));
                }
            }
            return new Layer(name, chunks);
        }

        internal static TileMap Load(Uri uri)
        {
            var doc = new XmlDocument();
            doc.Load(UriOpener.Open(uri));
            var layers = new List<Layer>();
            var tilesets = new List<TileSet>();
            var map = doc.DocumentElement;
            var tileSize = new Size() { Width = map.GetInt32Attribute("tilewidth"), Height = map.GetInt32Attribute("tilewidth") };
            foreach (XmlElement node in map.ChildNodes)
            {
                if (node.Name == "tileset")
                {
                    tilesets.Add(ReadTileSet(node, uri));
                }
                if (node.Name == "layer")
                {
                    layers.Add(ReadLayer(node));
                }
            }
            return new TileMap(tileSize, layers, tilesets);
        }
    }
}
