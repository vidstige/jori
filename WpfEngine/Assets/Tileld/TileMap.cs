using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        private readonly int _tilecount;
        private readonly int _columns;
        private readonly List<BitmapImage> _images = new List<BitmapImage>();

        public TileSet(string name, int firstgid, Size tileSize, int tilecount, int columns)
        {
            _name = name;
            _firstgid = firstgid;
            _tileSize = tileSize;
            _tilecount = tilecount;
            _columns = columns;
        }

        public List<BitmapImage> Images { get { return _images; } }
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
        
        private static TileSet ReadTileSetContent(int firstgid, XmlElement node, Uri uri)
        {
            var tileSet = new TileSet(
                node.GetAttribute("name"),
                firstgid,
                new Size() { Width = node.GetInt32Attribute("tilewidth"), Height = node.GetInt32Attribute("tileheight") },
                node.GetInt32Attribute("tilecount"),
                node.GetInt32Attribute("columns")
            );
            foreach (XmlElement child in node.ChildNodes)
            {
                if (child.Name == "image")
                {
                    var source = child.GetAttribute("source");
                    var imageUri = new Uri(uri, source);
                    var bitmapImage = new BitmapImage(imageUri);
                    tileSet.Images.Add(bitmapImage);
                }
            }
            return tileSet;
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
