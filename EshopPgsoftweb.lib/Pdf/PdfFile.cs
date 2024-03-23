using iTextSharp.text;
using iTextSharp.text.pdf;

namespace eshoppgsoftweb.lib.Pdf
{
    public class PdfFile
    {
        Document doc;
        PdfWriter writer;
        PdfFonts fonts;

        public PdfFonts Fonts
        {
            get
            {
                return this.fonts;
            }
        }
        public float PageWidth
        {
            get
            {
                return this.doc.PageSize.Width;
            }
        }
        public float PageWidthCenter
        {
            get
            {
                return this.doc.PageSize.Width / 2;
            }
        }
        public float PageHeight
        {
            get
            {
                return this.doc.PageSize.Height;
            }
        }
        public float PageHeightCenter
        {
            get
            {
                return this.doc.PageSize.Height / 2;
            }
        }

        public PdfFile(Document aDoc, PdfWriter aWriter, PdfFonts aFonts)
        {
            this.doc = aDoc;
            this.writer = aWriter;
            this.fonts = aFonts;
        }

        public void NewPage()
        {
            this.doc.NewPage();
        }

        public void EmptyLine(string fontId = null)
        {
            WriteParagraph(new PdfTextItem(" ", fontId));
        }

        public void WriteParagraph(params PdfTextItem[] list)
        {
            this.doc.Add(CreateParagraph(list));
        }
        /// <summary>
        /// Returns <b>false</b> if not all text was rendered
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool WriteParagraphAtRectangle(float x, float y, float width, float height, params PdfTextItem[] list)
        {
            ColumnText ct = new ColumnText(this.writer.DirectContent);
            ct.SetSimpleColumn(CreateRectangle(x, y, width, height));

            Paragraph par = CreateParagraph(list);
            par.SetLeading(0, 1.2f);
            ct.AddElement(par);
            return ct.Go() != 2; // Return value can be COLUMN ENDED == 1 or TEXT ENDED == 2
        }
        public bool CenterParagraphAtRectangle(float x, float y, float width, float height, params PdfTextItem[] list)
        {
            ColumnText ct = new ColumnText(this.writer.DirectContent);
            ct.SetSimpleColumn(CreateRectangle(x, y, width, height));

            Paragraph par = CreateParagraph(list);
            par.Alignment = Element.ALIGN_CENTER;
            par.SetLeading(0, 1.2f);
            ct.AddElement(par);
            return ct.Go() != 2; // Return value can be COLUMN ENDED == 1 or TEXT ENDED == 2
        }
        public Rectangle CreateRectangle(float x, float y, float width, float height)
        {
            float llx = x;
            float urx = x + width;
            float ury = GetPdfVerticalPosition(y);
            float lly = ury - height;

            return new Rectangle(llx, lly, urx, ury);
        }
        public Paragraph CreateParagraph(params PdfTextItem[] list)
        {
            Paragraph par = new Paragraph();
            for (int i = 0; i < list.Length; i++)
            {
                par.Add(new Chunk(list[i].Text, this.fonts.GetFont(list[i].FontId)));
            }

            return par;
        }

        public void ChangeDrawColor(BaseColor color)
        {
            PdfContentByte cb = this.writer.DirectContent;
            cb.SetColorFill(color);
            cb.SetColorStroke(color);
        }

        public void WriteTextAtPosition(float x, float y, PdfTextItem text)
        {
            Font font = this.fonts.GetFont(text.FontId);
            PdfContentByte cb = this.writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(font.BaseFont, font.Size);
            cb.SetTextMatrix(x, GetPdfVerticalPosition(y));  //(xPos, yPos)
            cb.ShowText(text.Text);
            cb.EndText();
        }

        public void CenterTextAtPosition(float y, PdfTextItem text)
        {
            Font font = this.fonts.GetFont(text.FontId);

            float textWidth = this.fonts.GetTextWidth(text.Text, font);
            float x = this.PageWidthCenter - (textWidth / 2);

            PdfContentByte cb = this.writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(font.BaseFont, font.Size);
            cb.SetTextMatrix(x, GetPdfVerticalPosition(y));  //(xPos, yPos)
            cb.ShowText(text.Text);
            cb.EndText();
        }

        public void RightTextAtPosition(float x, float y, PdfTextItem text)
        {
            Font font = this.fonts.GetFont(text.FontId);

            float textWidth = this.fonts.GetTextWidth(text.Text, font);
            x = x - textWidth;

            PdfContentByte cb = this.writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(font.BaseFont, font.Size);
            cb.SetTextMatrix(x, GetPdfVerticalPosition(y));  //(xPos, yPos)
            cb.ShowText(text.Text);
            cb.EndText();
        }

        public void DrawRectangle(float x, float y, float width, float height)
        {
            PdfContentByte cb = this.writer.DirectContent;
            float y1 = GetPdfVerticalPosition(y);
            float y2 = GetPdfVerticalPosition(y + height);
            cb.MoveTo(x, y1);
            cb.LineTo(x + width, y1);
            cb.LineTo(x + width, y2);
            cb.LineTo(x, y2);
            cb.ClosePath();
            cb.Stroke();
        }

        public void DrawLine(float x1, float y1, float x2, float y2, float lineWidth = 1f)
        {
            PdfContentByte cb = this.writer.DirectContent;
            cb.SetLineWidth(lineWidth);
            cb.MoveTo(x1, GetPdfVerticalPosition(y1));
            cb.LineTo(x2, GetPdfVerticalPosition(y2));
            cb.ClosePath();
            cb.Stroke();
        }
        public void DrawVerticalLine(float x, float y, float height, float lineWidth = 1f)
        {
            DrawLine(x, y, x, y + height, lineWidth);
        }
        public void DrawHorizontalLine(float x, float y, float width, float lineWidth = 1f)
        {
            DrawLine(x, y, x + width, y, lineWidth);
        }

        public void AddImgAtPosition(string name, float width, float height, float padding, float x, float y)
        {
            Image img = Image.GetInstance(name);
            //Resize image depend upon your need
            img.ScaleToFit(width, height);
            //Give space before image
            img.SpacingBefore = padding;
            //Give some space after the image
            img.SpacingAfter = padding;
            img.Alignment = Element.ALIGN_CENTER;
            img.SetAbsolutePosition(x, GetPdfVerticalPosition(y));

            this.doc.Add(img);
        }

        public void AddImgAtPosition(System.Drawing.Image image, float width, float height, float padding, float x, float y)
        {
            Image img = Image.GetInstance(image, BaseColor.WHITE);
            //Resize image depend upon your need
            img.ScaleToFit(width, height);
            //Give space before image
            img.SpacingBefore = padding;
            //Give some space after the image
            img.SpacingAfter = padding;
            img.Alignment = Element.ALIGN_CENTER;
            img.SetAbsolutePosition(x, GetPdfVerticalPosition(y));

            this.doc.Add(img);
        }

        private float GetPdfVerticalPosition(float y)
        {
            return this.PageHeight - y;
        }
    }

    public class PdfTextItem
    {
        public string Text { get; private set; }
        public string FontId { get; private set; }

        public PdfTextItem(string aText, string aFontId = null)
        {
            this.Text = aText;
            this.FontId = aFontId;
        }
    }
}
