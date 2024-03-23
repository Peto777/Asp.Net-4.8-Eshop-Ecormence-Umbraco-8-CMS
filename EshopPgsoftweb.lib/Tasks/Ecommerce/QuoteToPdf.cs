using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace eshoppgsoftweb.lib.Tasks.Ecommerce
{
    public class QuoteToPdf
    {
        public class QuoteData
        {
            public string ImgPath { get; private set; }

            public QuoteModel Quote { get; private set; }
            public SysConstModel Pgsoftweb { get; private set; }

            public QuoteData(Guid quoteKey, string imgPath) : this(QuoteModel.GetCompleteModel(quoteKey), imgPath)
            {
            }
            public QuoteData(QuoteModel quote, string imgPath)
            {
                this.ImgPath = imgPath;
                this.Quote = quote;
                this.Pgsoftweb = SysConstModel.CreateCopyFrom(new SysConstRepository().Get());
            }
        }

        private static float widthMargin = 30;
        private static float widthPadding = 10;

        public static PdfFilePrintResult GetQuotePdf(Guid quoteKey, string imgPath)
        {
            return CreateQuotePdf(new QuoteData(quoteKey, imgPath));
        }
        public static PdfFilePrintResult GetQuotePdf(QuoteModel quote, string imgPath)
        {
            return CreateQuotePdf(new QuoteData(quote, imgPath));
        }

        private static PdfFilePrintResult CreateQuotePdf(QuoteData quoteData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Document doc = new Document(PageSize.A4))
                {
                    using (PdfWriter writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        PdfFile pdf = new PdfFile(doc, writer, new PdfFonts());

                        for (int i = 0; i < 1; i++)
                        {
                            if (i > 0)
                            {
                                pdf.NewPage();
                            }

                            int formNb = i + 1;

                            // Quote header
                            float pageBottom = pdf.PageHeight - 30;
                            int pgNb = 1;
                            OneQuoteHeaderForm(pdf, quoteData, i + 1, pgNb);
                            OneQuoteHeaderData(pdf, quoteData, pgNb);

                            // Quote note
                            int noteHeight = OneQuoteHeaderNote(pdf, quoteData);

                            // Quote items
                            float y = 350 + noteHeight;
                            float headerHeight = 40;
                            float itemHeight = 40;

                            //if (quoteData.DoItems.Count > 0)
                            {
                                OneItemHeader(pdf, y);
                                y += headerHeight;

                                // test loop to generate more lines
                                //for (int j = 0; j < 20; j++)
                                {
                                    foreach (Product2QuoteModel item in quoteData.Quote.Items)
                                    {
                                        if (y + itemHeight > pageBottom)
                                        {
                                            pdf.NewPage();
                                            pgNb++;
                                            OneQuoteHeaderForm(pdf, quoteData, formNb, pgNb);
                                            OneQuoteHeaderData(pdf, quoteData, pgNb);
                                            y = 120;
                                            OneItemHeader(pdf, y);
                                            y += headerHeight;
                                        }

                                        OneItemData(pdf, item, y);
                                        y += itemHeight;

                                    }
                                }
                            }


                            // Quote footer
                            float footerHeight = 70;
                            if (y + footerHeight > pageBottom)
                            {
                                pdf.NewPage();
                                pgNb++;
                                OneQuoteHeaderForm(pdf, quoteData, formNb, pgNb);
                                OneQuoteHeaderData(pdf, quoteData, pgNb);
                                y = 150;
                            }

                            OneQuoteFooterForm(pdf, formNb, y);
                            OneQuoteFooterData(pdf, quoteData, y);
                        }

                        doc.Close();
                        writer.Close();
                    }
                }

                return new PdfFilePrintResult(string.Format("{0}.pdf", quoteData.Quote.QuoteId), ms.ToArray());
            }
        }
        private static void OneQuoteHeaderForm(PdfFile pdf, QuoteData quoteData, int nb, int pgNb)
        {
            float rectTop = 30;
            float rectHeight = 70;
            float rectWidth = pdf.PageWidth - 2 * widthMargin;

            // Box 1
            pdf.DrawRectangle(widthMargin, rectTop, rectWidth, rectHeight);
            pdf.WriteTextAtPosition(pdf.PageWidthCenter + 40, rectTop + 20, new PdfTextItem("Objednavka cislo:", PdfFonts.F_NORMAL_11));
            pdf.WriteTextAtPosition(pdf.PageWidthCenter + 40, rectTop + 35, new PdfTextItem("Zo dna:", PdfFonts.F_NORMAL_11));
            if (pgNb > 1)
            {
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + 40, rectTop + 60, new PdfTextItem("Strana", PdfFonts.F_BOLD_11));
                rectTop += rectHeight;
                rectHeight = 700;
                pdf.DrawRectangle(widthMargin, rectTop, rectWidth, rectHeight);
            }
            else
            {
                // Box 2
                rectTop += rectHeight;
                rectHeight = 220;
                pdf.DrawRectangle(widthMargin, rectTop, rectWidth, rectHeight);
                // Box 3
                rectTop += rectHeight;
                rectHeight = 480;
                pdf.DrawRectangle(widthMargin, rectTop, rectWidth, rectHeight);
            }
            pdf.CenterTextAtPosition(820, new PdfTextItem("gloziksoft.sk - tvorba eshopov - www.gloziksoft.sk", PdfFonts.F_NORMAL_6));
        }
        private static void OneQuoteHeaderData(PdfFile pdf, QuoteData quoteData, int pgNb)
        {
            float widthMargin = 30;
            float widthPadding = 10;
            float y = 30;

            // Box 1
            pdf.AddImgAtPosition(string.Format("{0}/{1}", quoteData.ImgPath, "gloziksoft-logo-print.png"), 90, 60, 0, widthMargin + 5, 95);
            pdf.WriteTextAtPosition(pdf.PageWidthCenter + 135, y + 20, new PdfTextItem(quoteData.Quote.QuoteId, PdfFonts.F_BOLD_11));
            pdf.WriteTextAtPosition(pdf.PageWidthCenter + 135, y + 35, new PdfTextItem(quoteData.Quote.DateFinishedView, PdfFonts.F_BOLD_11));
            if (pgNb > 1)
            {
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + 135, y + 60, new PdfTextItem(pgNb.ToString(), PdfFonts.F_BOLD_11));
            }
            else
            {
                // Box 2
                y = 115;
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y, new PdfTextItem("Dodavatel", PdfFonts.F_BOLD_8));
                float lineHeight = 13;
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Pgsoftweb.CompanyName), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Pgsoftweb.AddressStreet), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("{0} {1}", quoteData.Pgsoftweb.AddressZip, StringUtil.RemoveDiacritics(quoteData.Pgsoftweb.AddressCity)), PdfFonts.F_NORMAL_10));
                y += lineHeight; // empty line
                //pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(quoteData.LmData.AddressCountry, PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("ICO: {0}", quoteData.Pgsoftweb.CompanyIco), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("DIC: {0}", quoteData.Pgsoftweb.CompanyDic), PdfFonts.F_NORMAL_10));
                if (!string.IsNullOrEmpty(quoteData.Pgsoftweb.CompanyIcdph))
                {
                    pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("IC DPH: {0}", quoteData.Pgsoftweb.CompanyIcdph), PdfFonts.F_NORMAL_10));
                }
                y += lineHeight; // empty line
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("E-mail: {0}", quoteData.Pgsoftweb.Email), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("Telefon: {0}", quoteData.Pgsoftweb.Phone), PdfFonts.F_NORMAL_10));
                //pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("{0}", quoteData.LmData.CompanyRegister), PdfFonts.F_NORMAL_10));
                y += lineHeight; // empty line
                y += lineHeight; // empty line
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("Variabilny symbol: {0}", quoteData.Quote.QuoteId), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("IBAN: {0}", quoteData.Pgsoftweb.Iban), PdfFonts.F_NORMAL_10));
                //pdf.WriteTextAtPosition(x, y += lineHeight, new PdfTextItem(quoteData.Naplnspajzu.Swift, PdfFonts.F_NORMAL_11));
                pdf.WriteTextAtPosition(widthMargin + widthPadding, y += lineHeight, new PdfTextItem(string.Format("Banka: {0}", StringUtil.RemoveDiacritics(quoteData.Pgsoftweb.Bank)), PdfFonts.F_NORMAL_10));

                y = 115;
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y, new PdfTextItem("Odberatel:", PdfFonts.F_BOLD_8));
                if (quoteData.Quote.User.IsCompanyInvoice)
                {
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.CompanyName), PdfFonts.F_NORMAL_10));
                }
                else
                {
                    y += lineHeight; // empty line
                }
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.InvName), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.InvStreet), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("{0} {1}", quoteData.Quote.User.InvZip, StringUtil.RemoveDiacritics(quoteData.Quote.User.InvCity)), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(quoteData.Quote.User.InvCountry, PdfFonts.F_NORMAL_10));
                if (quoteData.Quote.User.IsCompanyInvoice)
                {
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("ICO: {0}", quoteData.Quote.User.CompanyIco), PdfFonts.F_NORMAL_10));
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("DIC: {0}", quoteData.Quote.User.CompanyDic), PdfFonts.F_NORMAL_10));
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("IC DPH: {0}", quoteData.Quote.User.CompanyIcdph), PdfFonts.F_NORMAL_10));
                }
                else
                {
                    y += lineHeight; // empty line
                    y += lineHeight; // empty line
                    y += lineHeight; // empty line
                }

                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("E-mail: {0}", quoteData.Quote.User.QuoteEmail), PdfFonts.F_NORMAL_10));
                pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("Telefon: {0}", quoteData.Quote.User.QuotePhone), PdfFonts.F_NORMAL_10));

                if (quoteData.Quote.User.IsDeliveryAddress)
                {
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem("Adresa dorucenia je ina ako fakturacna:", PdfFonts.F_BOLD_8));
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.DeliveryName), PdfFonts.F_NORMAL_10));
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.DeliveryStreet), PdfFonts.F_NORMAL_10));
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(string.Format("{0} {1}", quoteData.Quote.User.DeliveryZip, StringUtil.RemoveDiacritics(quoteData.Quote.User.DeliveryCity)), PdfFonts.F_NORMAL_10));
                    pdf.WriteTextAtPosition(pdf.PageWidthCenter + widthPadding, y += lineHeight, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.DeliveryCountry), PdfFonts.F_NORMAL_10));
                }
                else
                {
                    y += lineHeight; // empty line
                    y += lineHeight; // empty line
                    y += lineHeight; // empty line
                    y += lineHeight; // empty line
                    y += lineHeight; // empty line
                }
            }
        }
        private static int OneQuoteHeaderNote(PdfFile pdf, QuoteData quoteData)
        {
            if (string.IsNullOrEmpty(quoteData.Quote.User.Note))
            {
                return 0;
            }

            int y = 335;
            pdf.WriteTextAtPosition(widthMargin + widthPadding, y, new PdfTextItem("Poznamka", PdfFonts.F_BOLD_8));
            pdf.WriteParagraphAtRectangle(widthMargin + widthPadding, y + 5, pdf.PageWidth - 2 * (widthMargin + widthPadding), 100, new PdfTextItem(StringUtil.RemoveDiacritics(quoteData.Quote.User.Note), PdfFonts.F_NORMAL_8));
            pdf.DrawHorizontalLine(widthMargin, y + 110, pdf.PageWidth - 2 * widthMargin);

            return 115;
        }

        private static void OneQuoteFooterForm(PdfFile pdf, int nb, float y)
        {
            pdf.DrawHorizontalLine(widthMargin + widthPadding, y, pdf.PageWidth - 2 * (widthMargin + widthPadding));

            float x = widthMargin + widthPadding;
            y += 20;
            pdf.WriteTextAtPosition(x, y, new PdfTextItem("SPOLU:", PdfFonts.F_NORMAL_10));
        }
        private static void OneQuoteFooterData(PdfFile pdf, QuoteData quoteData, float y)
        {
            float x = pdf.PageWidth - widthMargin - widthPadding;
            y += 20;
            pdf.RightTextAtPosition(x, y, new PdfTextItem(quoteData.Quote.TotalPriceWithVatWithCurrency, PdfFonts.F_NORMAL_10));
        }

        private static void OneItemHeader(PdfFile pdf, float y)
        {
            float lineHeight = 14;
            float x = widthMargin + widthPadding;
            pdf.WriteTextAtPosition(x, y, new PdfTextItem("Polozka", PdfFonts.F_BOLD_10));

            y += lineHeight;
            pdf.WriteTextAtPosition(x, y, new PdfTextItem("Kod", PdfFonts.F_BOLD_8));

            float columnWidth = (pdf.PageWidth - 2 * widthMargin - 2 * widthPadding) / 4;

            x = pdf.PageWidth - widthMargin - widthPadding;
            pdf.RightTextAtPosition(x, y, new PdfTextItem("Celkom", PdfFonts.F_BOLD_8));
            x -= columnWidth;
            pdf.RightTextAtPosition(x, y, new PdfTextItem("Mnozstvo", PdfFonts.F_BOLD_8));
            x -= columnWidth;
            pdf.RightTextAtPosition(x, y, new PdfTextItem("Cena", PdfFonts.F_BOLD_8));

            pdf.DrawHorizontalLine(widthMargin + widthPadding, y + 3, pdf.PageWidth - 2 * (widthMargin + widthPadding));
        }
        private static void OneItemData(PdfFile pdf, Product2QuoteModel item, float y)
        {
            float lineHeight = 14;
            float x = widthMargin + widthPadding;
            if (item.IsProductItem)
            {
                pdf.WriteTextAtPosition(x, y, new PdfTextItem(StringUtil.RemoveDiacritics(item.ItemName), PdfFonts.F_NORMAL_10));
            }
            else
            {
                pdf.WriteTextAtPosition(x, y, new PdfTextItem(item.ItemCode, PdfFonts.F_NORMAL_10));
            }

            y += lineHeight;
            if (item.IsProductItem)
            {
                pdf.WriteTextAtPosition(x, y, new PdfTextItem(string.Format("{0}", item.ItemCode), PdfFonts.F_NORMAL_10));
            }
            else
            {
                pdf.WriteTextAtPosition(x, y, new PdfTextItem(StringUtil.RemoveDiacritics(item.ItemName), PdfFonts.F_NORMAL_10));
            }

            float columnWidth = (pdf.PageWidth - 2 * widthMargin - 2 * widthPadding) / 4;

            x = pdf.PageWidth - widthMargin - widthPadding;
            pdf.RightTextAtPosition(x, y, new PdfTextItem(item.TotalPriceWithCurrencyWithVat, PdfFonts.F_NORMAL_10));
            x -= columnWidth;
            if (item.IsProductItem)
            {
                pdf.RightTextAtPosition(x, y, new PdfTextItem(string.Format("{0} {1}", item.ItemPcs, StringUtil.RemoveDiacritics(item.UnitName)), PdfFonts.F_NORMAL_10));
            }
            x -= columnWidth;
            if (item.IsProductItem)
            {
                pdf.RightTextAtPosition(x, y, new PdfTextItem(item.BasePriceWithCurrencyWithVat, PdfFonts.F_NORMAL_10));
            }
        }
    }
}
