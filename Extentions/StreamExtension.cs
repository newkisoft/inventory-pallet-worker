using System.IO;

namespace newki_inventory_pallet.Extentions
{
     public static class StreamExtension
    {
        public static byte[] ToBytes(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }      
    }
}