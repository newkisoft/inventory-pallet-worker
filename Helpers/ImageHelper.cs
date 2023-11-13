using System.Drawing;
using System.IO;

namespace newki_inventory_pallet.Helpers
{
    public static class ImageHelper
    {

        public static Image ResizeImage(byte[] bytes, Size size)
        {
            using(var stream = new MemoryStream(bytes))
            using (var image = Image.FromStream(stream))
            {
                return (Image)(new Bitmap(image, size));
            }
        }
    }
}