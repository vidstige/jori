using System.Xml;

namespace WpfEngine.Assets.Tileld
{
    public class Layer
    {

    }

    public class TileSet
    {

    }

    public class TileMap
    {
        private List<Layer> _layers;
        private List<TileSet> _tileSets;

        public TileMap(List<Layer> layers, List<TileSet> tilesets)
        {
            _layers = layers;
            _tileSets = tilesets;
        }

        private static TileSet ReadTileSet(XmlElement node)
        {
            return new TileSet();
        }
        private static Layer ReadLayer(XmlElement node)
        {
            return new Layer();
        }
        internal static TileMap Load(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            var layers = new List<Layer>();
            var tilesets = new List<TileSet>();
            foreach (XmlElement node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "tileset")
                {
                    tilesets.Add(ReadTileSet(node));
                }
                if (node.Name == "layer")
                {
                    layers.Add(ReadLayer(node));
                }
            }
            return new TileMap(layers, tilesets);
        }
    }
}
