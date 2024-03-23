using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;

namespace eshoppgsoftweb.lib.Pdf
{
    public class PdfFonts
    {
        public const string F_BOLD_8 = "F_BOLD_8";
        public const string F_BOLD_10 = "F_BOLD_10";
        public const string F_BOLD_11 = "F_BOLD_11";
        public const string F_BOLD_14 = "F_BOLD_14";
        public const string F_BOLD_18 = "F_BOLD_18";
        public const string F_BOLD_24 = "F_BOLD_24";
        public const string F_NORMAL_5 = "F_NORMAL_5";
        public const string F_NORMAL_6 = "F_NORMAL_6";
        public const string F_NORMAL_8 = "F_NORMAL_8";
        public const string F_NORMAL_10 = "F_NORMAL_10";
        public const string F_NORMAL_11 = "F_NORMAL_11";

        public BaseColor DefaultColor;
        public BaseColor PrimaryColor;
        public BaseColor SecondaryColor;

        string currentFontId;
        Hashtable htFonts;

        public PdfFonts(string defaultFontFace = BaseFont.HELVETICA, string defaultEncoding = BaseFont.CP1250)
        {
            this.DefaultColor = new BaseColor(31, 26, 5);
            this.PrimaryColor = new BaseColor(8, 0, 102);
            this.SecondaryColor = new BaseColor(0, 0, 0);

            AddFont(F_BOLD_8, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 8, Font.BOLD));
            AddFont(F_BOLD_10, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 10, Font.BOLD));
            AddFont(F_BOLD_11, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 11, Font.BOLD));
            AddFont(F_BOLD_14, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 14, Font.BOLD));
            AddFont(F_BOLD_18, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 18, Font.BOLD));
            AddFont(F_BOLD_24, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 24, Font.BOLD));

            AddFont(F_NORMAL_5, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 5, Font.NORMAL));
            AddFont(F_NORMAL_6, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 6, Font.NORMAL));
            AddFont(F_NORMAL_8, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 8, Font.NORMAL));
            AddFont(F_NORMAL_10, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 10, Font.NORMAL));
            AddFont(F_NORMAL_11, FontFactory.GetFont(defaultFontFace, defaultEncoding, BaseFont.NOT_EMBEDDED, 11, Font.NORMAL));

            this.currentFontId = F_NORMAL_11;
        }

        private void AddFont(string key, Font font)
        {
            if (htFonts == null)
            {
                htFonts = new Hashtable();
            }
            if (!htFonts.ContainsKey(key))
            {
                htFonts.Add(key, font);
            }
        }

        public Font GetFont(string key)
        {
            if (htFonts == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(key))
            {
                key = this.currentFontId;
            }

            this.currentFontId = key;

            return htFonts.ContainsKey(this.currentFontId) ? (Font)htFonts[this.currentFontId] : null;
        }

        public float GetTextWidth(PdfTextItem text)
        {
            return GetTextWidth(text.Text, this.GetFont(text.FontId));
        }
        public float GetTextWidth(string text, Font font)
        {
            Chunk chunk = new Chunk(text, font);

            return chunk.GetWidthPoint();
        }
    }
}
