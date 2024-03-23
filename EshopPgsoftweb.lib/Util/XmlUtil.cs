using System.IO;
using System.Text;
using System.Xml;

namespace eshoppgsoftweb.lib.Util
{
    public class XmlUtil
    {
        public static byte[] ToWin1250(XmlDocument doc)
        {
            return ToEncoding(doc, Encoding.GetEncoding(1250));
        }

        public static byte[] ToEncoding(XmlDocument doc, Encoding enc)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Encoding = enc,
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };
                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    doc.Save(writer);
                }
                return ms.ToArray();
            }
        }
    }
}
