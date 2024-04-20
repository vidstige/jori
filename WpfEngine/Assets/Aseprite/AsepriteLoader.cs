using System.IO;
using System.IO.Compression;
using System.Text;

namespace WpfEngine.Assets.Aseprite
{
    internal static class BinaryReaderExtension
    {
        public static string ReadAseString(this BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    internal class Chunk
    {

    }

    internal class PaletteChunk : Chunk
    {
        internal const int Type = 0x2019;

        internal static PaletteChunk Read(BinaryReader reader)
        {
            var size = reader.ReadUInt32();
            var firstIndex = reader.ReadUInt32();
            var lastIndex = reader.ReadUInt32();
            reader.ReadBytes(8);  // reserved
            for (uint i = firstIndex; i <= lastIndex; i++)
            {
                var flags = reader.ReadUInt16();
                var color = reader.ReadUInt32();
                if ((flags & 1) == 1)
                {
                    var name = reader.ReadAseString();
                }

            }
            return new PaletteChunk();
        }
    }

    internal class LayerChunk : Chunk
    {
        internal const int Type = 0x2004;
        internal static LayerChunk Read(BinaryReader reader)
        {
            var flags = reader.ReadUInt16();
            var type = reader.ReadUInt16();
            var childLevel = reader.ReadUInt16();
            reader.ReadUInt16();  // ignore default layer width
            reader.ReadUInt16();  // ignore default layer height
            var blendMode = reader.ReadUInt16();
            var opacity = reader.ReadByte();
            reader.ReadBytes(3); // reserved
            var name = reader.ReadAseString();
            if (type == 0x0002)
            {
                var tilesetIndex = reader.ReadUInt32();

            }
            /*
           * WORD        Flags:
            1 = Visible
            2 = Editable
            4 = Lock movement
            8 = Background
            16 = Prefer linked cels
            32 = The layer group should be displayed collapsed
            64 = The layer is a reference layer
WORD        Layer type
            0 = Normal (image) layer
            1 = Group
            2 = Tilemap
WORD        Layer child level (see NOTE.1)
WORD        Default layer width in pixels (ignored)
WORD        Default layer height in pixels (ignored)
WORD        Blend mode (always 0 for layer set)
            Normal         = 0
            Multiply       = 1
            Screen         = 2
            Overlay        = 3
            Darken         = 4
            Lighten        = 5
            Color Dodge    = 6
            Color Burn     = 7
            Hard Light     = 8
            Soft Light     = 9
            Difference     = 10
            Exclusion      = 11
            Hue            = 12
            Saturation     = 13
            Color          = 14
            Luminosity     = 15
            Addition       = 16
            Subtract       = 17
            Divide         = 18
BYTE        Opacity
            Note: valid only if file header flags field has bit 1 set
BYTE[3]     For future (set to zero)
STRING      Layer name
+ If layer type = 2
DWORD     Tileset index
           * */
            return new LayerChunk();
        }
    }

    internal class ZLib
    {
        internal static byte[] Deflate(byte[] compressed)
        {
            using var target = new MemoryStream();
            using var source = new DeflateStream(new MemoryStream(compressed), CompressionLevel.Optimal);
            source.CopyTo(target);
            return target.ToArray();
        }
    }

    internal class CelChunk : Chunk
    {
        internal const int Type = 0x2005;
        internal static CelChunk Read(BinaryReader reader, ushort colorDepth)
        {
            var layerIndex = reader.ReadUInt16();
            var x = reader.ReadInt16();
            var y = reader.ReadInt16();
            var opacity = reader.ReadByte();
            var celType = reader.ReadUInt16();
            var zIndex = reader.ReadInt16();
            reader.ReadBytes(5); // reserved
            switch (celType)
            {
                case 0: // raw image
                    break;
                case 1: // Linked cell
                    break;
                case 2: // Compressed
                    var width = reader.ReadUInt16();
                    var height = reader.ReadUInt16();
                    var count = width * height * colorDepth / 8;
                    // skip zlib header
                    reader.ReadBytes(2);
                    var deflate = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                    var tmp = new byte[count];
                    deflate.Read(tmp, 0, count);
                    break;
                case 3: // Compressed Tilemap
                    break;
            }
            /*
WORD        Layer index (see NOTE.2)
SHORT       X position
SHORT       Y position
BYTE        Opacity level
WORD        Cel Type
            0 - Raw Image Data (unused, compressed image is preferred)
            1 - Linked Cel
            2 - Compressed Image
            3 - Compressed Tilemap
SHORT       Z-Index (see NOTE.5)
            0 = default layer ordering
            +N = show this cel N layers later
            -N = show this cel N layers back
BYTE[5]     For future (set to zero)
+ For cel type = 0 (Raw Image Data)
  WORD      Width in pixels
  WORD      Height in pixels
  PIXEL[]   Raw pixel data: row by row from top to bottom,
            for each scanline read pixels from left to right.
+ For cel type = 1 (Linked Cel)
  WORD      Frame position to link with
+ For cel type = 2 (Compressed Image)
  WORD      Width in pixels
  WORD      Height in pixels
  PIXEL[]   "Raw Cel" data compressed with ZLIB method (see NOTE.3)
+ For cel type = 3 (Compressed Tilemap)
  WORD      Width in number of tiles
  WORD      Height in number of tiles
  WORD      Bits per tile (at the moment it's always 32-bit per tile)
  DWORD     Bitmask for tile ID (e.g. 0x1fffffff for 32-bit tiles)
  DWORD     Bitmask for X flip
  DWORD     Bitmask for Y flip
  DWORD     Bitmask for diagonal flip (swap X/Y axis)
  BYTE[10]  Reserved
  TILE[]    Row by row, from top to bottom tile by tile
            compressed with ZLIB method (see NOTE.3)

             */
            return new CelChunk();

        }
    }

    public class AsepriteLoader
    {
        private static void ReadChunk(BinaryReader reader, ushort colorDepth)
        {
            var size = reader.ReadUInt32();
            var type = reader.ReadUInt16();
            var data = reader.ReadBytes((int)size - 6);
            var chunkStream = new MemoryStream(data);
            using var chunkReader = new BinaryReader(chunkStream);
            if (type == PaletteChunk.Type)
            {
                var paletteChunk = PaletteChunk.Read(chunkReader);
            }
            if (type == LayerChunk.Type)
            {
                var layerChunk = LayerChunk.Read(chunkReader);
            }
            if (type == CelChunk.Type)
            {
                var celChunk = CelChunk.Read(chunkReader, colorDepth);
            }
        }

        private static void ReadFrame(BinaryReader reader, ushort colorDepth)
        {
            var size = reader.ReadUInt32();
            var magic = reader.ReadUInt16(); // 0xF1Fa
            // Old field which specifies the number of "chunks" in this frame.If this value is 0xFFFF, we might have more chunks to read in this frame(so we have to use the new field)
            var oldChunks = reader.ReadUInt16();
            var duration = TimeSpan.FromMilliseconds(reader.ReadUInt16());
            reader.ReadUInt16(); // reserved
            // New field which specifies the number of "chunks" in this frame(if this is 0, use the old field)
            var chunks = reader.ReadUInt32();
            for (int i = 0; i < chunks; i++)
            {
                ReadChunk(reader, colorDepth);
            }
        }

        public static Sprite Load(BinaryReader reader)
        {
            var size = reader.ReadUInt32();
            var magic = reader.ReadUInt16();  // 0xA5E0
            var frames = reader.ReadUInt16();
            var width = reader.ReadUInt16();
            var height = reader.ReadUInt16();
            var colorDepth = reader.ReadUInt16();
            /*
              32 bpp = RGBA
              16 bpp = Grayscale
              8 bpp = Indexed*/
            var flags = reader.ReadUInt32();  // 1 = Layer opacity has valid value
            var speed = reader.ReadUInt16();  // milliseconds between frame DEPRECATED: You should use the frame duration field from each frame header
            reader.ReadUInt32(); // 0
            reader.ReadUInt32(); // 0
            var transparentIndex = reader.ReadByte();
            reader.ReadBytes(3); // ignore
            var numColors = reader.ReadUInt16();
            // if either is zero it means 1:1 aspect
            var pixelWidth = reader.ReadByte();
            var pixelHeight = reader.ReadByte();
            var positionX = reader.ReadInt16();
            var positionY = reader.ReadInt16();
            // may be zero to indicate no grid. Default is 16x16
            var gridWidth = reader.ReadUInt16();
            var gridHeight = reader.ReadUInt16();
            reader.ReadBytes(84); // ignore

            for (int i = 0; i < frames; i++)
            {
                ReadFrame(reader, colorDepth);
            }

            return new Sprite([]);
        }

    }
}
