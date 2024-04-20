using System.IO;
using System.Text.Encodings.Web;
using System.Windows;

namespace WpfEngine.Assets
{
    internal class UriOpener
    {
        public static Stream Open(Uri uri)
        {
            if (uri.Scheme == "file") {
                return File.OpenRead(Uri.UnescapeDataString(uri.AbsolutePath));
            }
            if (uri.Scheme == "pack") {
                return Application.GetResourceStream(uri).Stream;
            }
            throw new ArgumentException($"Can't handle scheme {uri.Scheme}");
        }
    }
}
