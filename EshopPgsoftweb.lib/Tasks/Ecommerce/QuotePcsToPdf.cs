using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace eshoppgsoftweb.lib.Tasks.Ecommerce
{
    public class QuotePcsListToPdf
    {
        private static float widthMargin = 30;
        private static float widthPadding = 10;

        public DateTime PrintDateTime { get; private set; }

        private QuotePcsListModel DataModel;

        public QuotePcsListToPdf(_BaseController ctrl)
        {
            this.PrintDateTime = DateTime.Now;

            QuoteListFilterModel filter = QuoteListFilterModel.CreateCopyFrom(new EshoppgsoftwebUserPropRepository().Get(ctrl.CurrentSessionId, ConfigurationUtil.PropId_QuoteListFilterModel));
            this.DataModel = new QuotePcsListModel(
                QuoteListModel.CreateCopyFrom(null,
                    new QuoteForListRepository().GetPage(1, _PagingModel.AllItemsPerPage,
                        filter: new QuoteForListFilter()
                        {
                            QuoteId = filter.QuoteId,
                            SearchText = filter.SearchText,
                            From = filter.GetDateTimeFrom(),
                            To = filter.GetDateTimeTo(),
                            QuoteStates = filter.QuoteStates,
                        }
                    )));
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

                        //for (int i = 0; i < 2; i++)
                        {
                            foreach (QuoteItemPcs itemPcs in this.DataModel.ItemList.Values)
                            {
                                if (y + itemHeight > pageBottom)
                                {
                                    pdf.NewPage();
                                    y = PageHeader(pdf, ++pagenb);
                                }

                                OneIteData(pdf, itemPcs, y, ++cnt);
                                y += itemHeight;
                            }
                        }

                        doc.Close();
                        writer.Close();
                    }
                }

                return new PdfFilePrintResult("Produkty.pdf", ms.ToArray());
            }
        }

        private float PageHeader(PdfFile pdf, int pagenb)
        {
            float lineHeight = 14;
            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float y = 50;
            float x = left;

            pdf.WriteTextAtPosition(left, y, new PdfTextItem("ZOZNAM OBJEDNANÝCH PRODUKTOV", PdfFonts.F_NORMAL_11));
            pdf.RightTextAtPosition(right, y, new PdfTextItem(string.Format("Tlač dňa: {0}, Strana: {1}", DateTimeUtil.GetDisplayDateTime(this.PrintDateTime), pagenb), PdfFonts.F_NORMAL_11));

            y += lineHeight;
            pdf.WriteTextAtPosition(x + 50, y, new PdfTextItem("Kód", PdfFonts.F_BOLD_10));
            pdf.WriteTextAtPosition(x + 150, y, new PdfTextItem("Názov", PdfFonts.F_BOLD_10));

            x = right;
            pdf.RightTextAtPosition(x - 30, y, new PdfTextItem("Množstvo", PdfFonts.F_BOLD_10));
            pdf.RightTextAtPosition(x, y, new PdfTextItem("MJ", PdfFonts.F_BOLD_10));

            pdf.DrawHorizontalLine(left, y + 3, pdf.PageWidth - 2 * (widthMargin + widthPadding));

            pdf.CenterTextAtPosition(820, new PdfTextItem("gloziksoft.sk - tvorba eshopov - www.gloziksoft.sk", PdfFonts.F_NORMAL_6));

            return y + lineHeight;
        }

        private float OneIteData(PdfFile pdf, QuoteItemPcs item, float y, int cnt)
        {
            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float x = left;

            pdf.RightTextAtPosition(x + 30, y, new PdfTextItem(cnt.ToString(), PdfFonts.F_NORMAL_10));
            pdf.WriteTextAtPosition(x + 50, y, new PdfTextItem(item.ItemCode, PdfFonts.F_NORMAL_10));
            pdf.WriteTextAtPosition(x + 150, y, new PdfTextItem(item.ItemName, PdfFonts.F_NORMAL_10));

            x = right;
            pdf.RightTextAtPosition(x - 30, y, new PdfTextItem(PriceUtil.NumberToTwoDecString(item.ItemPcs), PdfFonts.F_NORMAL_10));
            pdf.RightTextAtPosition(x, y, new PdfTextItem(item.ItemUnit, PdfFonts.F_NORMAL_10));

            return y;
        }

    }

    public class QuotePcsListModel
    {
        public QuoteListModel QuoteList { get; private set; }
        public SortedList<string, QuoteItemPcs> ItemList { get; private set; }

        public QuotePcsListModel(QuoteListModel quoteList)
        {
            this.QuoteList = quoteList;
            this.ItemList = new SortedList<string, QuoteItemPcs>();

            Hashtable htItems = new Hashtable();

            Product2QuoteRepository prod2QuoteRep = new Product2QuoteRepository();
            foreach (QuoteForList quote in this.QuoteList.Items)
            {
                foreach (Product2Quote item in prod2QuoteRep.GetQuoteProductItems(quote.pk))
                {
                    string key = item.ItemCode.PadLeft(6, ' ');
                    if (htItems.ContainsKey(key))
                    {
                        QuoteItemPcs itemPcs = (QuoteItemPcs)htItems[key];
                        itemPcs.ItemPcs += item.ItemPcs;
                    }
                    else
                    {
                        QuoteItemPcs itemPcs = new QuoteItemPcs();
                        itemPcs.ItemCode = item.ItemCode;
                        itemPcs.ItemName = item.ItemName;
                        itemPcs.ItemUnit = item.UnitName;
                        itemPcs.ItemPcs = item.ItemPcs;

                        htItems.Add(key, itemPcs);
                        this.ItemList.Add(key, itemPcs);
                    }
                }
            }
        }
    }

    public class QuoteItemPcs
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal ItemPcs { get; set; }
    }
}
