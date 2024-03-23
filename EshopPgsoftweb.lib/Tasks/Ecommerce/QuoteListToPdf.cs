using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace eshoppgsoftweb.lib.Tasks
{
    public class QuoteListToPdf
    {
        private static float widthMargin = 30;
        private static float widthPadding = 10;

        public DateTime PrintDateTime { get; private set; }

        private QuoteListModel DataModel;

        public QuoteListToPdf(_BaseController ctrl)
        {
            this.PrintDateTime = DateTime.Now;

            QuoteListFilterModel filter = QuoteListFilterModel.CreateCopyFrom(new EshoppgsoftwebUserPropRepository().Get(ctrl.CurrentSessionId, ConfigurationUtil.PropId_QuoteListFilterModel));
            this.DataModel = QuoteListModel.CreateCopyFrom(null,
                new QuoteForListRepository().GetPage(1, _PagingModel.AllItemsPerPage,
                    filter: new QuoteForListFilter()
                    {
                        QuoteId = filter.QuoteId,
                        SearchText = filter.SearchText,
                        From = filter.GetDateTimeFrom(),
                        To = filter.GetDateTimeTo(),
                        QuoteStates = filter.QuoteStates,
                    }
                ));
        }

        public PdfFilePrintResult GetPdf()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Document doc = new Document(PageSize.A4))
                {
                    using (PdfWriter writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        PdfFile pdf = new PdfFile(doc, writer, new PdfFonts());

                        float pageBottom = pdf.PageHeight - 30;
                        float itemHeight = 14;

                        int cnt = 0;
                        int pagenb = 1;
                        float y = PageHeader(pdf, pagenb);

                        decimal totalPrice = 0M;
                        foreach (QuoteForList quote in this.DataModel.Items)
                        {
                            if (y + itemHeight > pageBottom)
                            {
                                pdf.NewPage();
                                y = PageHeader(pdf, ++pagenb);
                            }

                            OneIteData(pdf, quote, y, ++cnt);
                            totalPrice += quote.QuotePriceWithVat;
                            y += itemHeight;
                        }

                        float footerHeight = 30;
                        if (y + footerHeight > pageBottom)
                        {
                            pdf.NewPage();
                            y = PageHeader(pdf, ++pagenb);
                        }

                        Footer(pdf, y, cnt, totalPrice);


                        doc.Close();
                        writer.Close();
                    }
                }

                return new PdfFilePrintResult("Objednavky.pdf", ms.ToArray());
            }
        }

        private float PageHeader(PdfFile pdf, int pagenb)
        {
            float lineHeight = 14;
            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float y = 50;
            float x = left;

            pdf.WriteTextAtPosition(left, y, new PdfTextItem("ZOZNAM OBJEDNÁVOK", PdfFonts.F_NORMAL_11));
            pdf.RightTextAtPosition(right, y, new PdfTextItem(string.Format("Tlač dňa: {0}, Strana: {1}", DateTimeUtil.GetDisplayDateTime(this.PrintDateTime), pagenb), PdfFonts.F_NORMAL_11));

            y += lineHeight;
            pdf.WriteTextAtPosition(x + 50, y, new PdfTextItem("Číslo", PdfFonts.F_BOLD_10));
            pdf.WriteTextAtPosition(x + 130, y, new PdfTextItem("Vytvorené", PdfFonts.F_BOLD_10));
            pdf.WriteTextAtPosition(x + 230, y, new PdfTextItem("Obec", PdfFonts.F_BOLD_10));

            x = right;
            pdf.RightTextAtPosition(x, y, new PdfTextItem("Cena", PdfFonts.F_BOLD_10));

            pdf.DrawHorizontalLine(left, y + 3, pdf.PageWidth - 2 * (widthMargin + widthPadding));

            pdf.CenterTextAtPosition(820, new PdfTextItem("gloziksoft.sk - tvorba eshopov - www.gloziksoft.sk", PdfFonts.F_NORMAL_6));

            return y + lineHeight;
        }

        private float OneIteData(PdfFile pdf, QuoteForList quote, float y, int cnt)
        {
            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float x = left;

            pdf.RightTextAtPosition(x + 30, y, new PdfTextItem(cnt.ToString(), PdfFonts.F_NORMAL_10));
            pdf.WriteTextAtPosition(x + 50, y, new PdfTextItem(quote.QuoteId, PdfFonts.F_NORMAL_10));
            pdf.WriteTextAtPosition(x + 130, y, new PdfTextItem(quote.DateFinishedView, PdfFonts.F_NORMAL_10));
            pdf.WriteTextAtPosition(x + 230, y, new PdfTextItem(quote.InvCity, PdfFonts.F_NORMAL_10));

            x = right;
            pdf.RightTextAtPosition(x, y, new PdfTextItem(PriceUtil.NumberToTwoDecString(quote.QuotePriceWithVat), PdfFonts.F_NORMAL_10));

            return y;
        }

        private void Footer(PdfFile pdf, float y, int cnt, decimal totalPrice)
        {
            float lineHeight = 14;
            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float x = left;

            pdf.DrawHorizontalLine(left, y, pdf.PageWidth - 2 * (widthMargin + widthPadding));

            y += lineHeight;

            pdf.RightTextAtPosition(x + 30, y, new PdfTextItem(cnt.ToString(), PdfFonts.F_BOLD_10));

            x = right;
            pdf.RightTextAtPosition(x, y, new PdfTextItem(PriceUtil.NumberToTwoDecString(totalPrice), PdfFonts.F_BOLD_10));

        }

    }
}
