using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NPoco;
using System;
using System.Collections.Generic;
using System.IO;

namespace eshoppgsoftweb.lib.Tasks.Ecommerce
{
    public class CategoryOfferToPdf
    {
        private static float widthMargin = 20;
        private static float widthPadding = 0;

        public DateTime PrintDateTime { get; private set; }

        private CategoryOfferModel DataModel;

        public CategoryOfferToPdf(Guid categoryKey, string imgPath)
        {
            this.PrintDateTime = DateTime.Now;
            this.DataModel = new CategoryOfferModel(categoryKey, imgPath);
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

                        //for (int i = 0; i < 10; i++)
                        {
                            foreach (ProductModel product in this.DataModel.Products.Items)
                            {
                                if (y + itemHeight > pageBottom)
                                {
                                    pdf.NewPage();
                                    y = PageHeader(pdf, ++pagenb);
                                }

                                OneIteData(pdf, product, y, ++cnt);
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
            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float y = 20;
            float x = left;

            float headImgRatio = 120f / 800f;
            float headImgWith = pdf.PageWidth;
            float headImgHeight = headImgRatio * headImgWith;
            float headImgRatio2 = 200f / 800f;
            float headImgHeight2 = headImgRatio2 * headImgWith;
            float topHeight = headImgHeight + headImgHeight2;

            if (pagenb > 1)
            {
                topHeight = 20;
                pdf.ChangeDrawColor(pdf.Fonts.SecondaryColor);
            }
            else
            {
                pdf.ChangeDrawColor(pdf.Fonts.PrimaryColor);

                pdf.AddImgAtPosition(string.Format("{0}/{1}", this.DataModel.ImgPath, "pgsoftweb-offer-header-print.png"), headImgWith, headImgHeight, 0, 0, headImgHeight);
                pdf.WriteTextAtPosition(150, 40, new PdfTextItem(this.DataModel.CategoryData.CategoryName.ToUpper(), PdfFonts.F_BOLD_24));
                pdf.WriteTextAtPosition(150, 74, new PdfTextItem("Kvalitné výrobky ", PdfFonts.F_BOLD_10));

                pdf.AddImgAtPosition(string.Format("{0}/{1}", this.DataModel.ImgPath, "pgsoftweb-offer-header2-print.png"), headImgWith, headImgHeight2, 0, 0, headImgHeight + headImgHeight2);

                pdf.ChangeDrawColor(pdf.Fonts.SecondaryColor);
                pdf.CenterTextAtPosition(headImgHeight + 20, new PdfTextItem("Objednávku si spravíte jednoducho telefonicky,", PdfFonts.F_NORMAL_11));
                pdf.CenterTextAtPosition(headImgHeight + 35, new PdfTextItem("ale aj cez SMS, email alebo online na eshope.", PdfFonts.F_NORMAL_11));
                pdf.CenterTextAtPosition(headImgHeight + 60, new PdfTextItem(string.Format("{0}      {1}      www.gloziksoft.sk", this.DataModel.Eshoppgsoftweb.Phone, this.DataModel.Eshoppgsoftweb.Email), PdfFonts.F_BOLD_11));

                pdf.CenterParagraphAtRectangle(20, headImgHeight + headImgHeight2 / 2f, pdf.PageWidth - 40, headImgHeight2 / 2f, new PdfTextItem(this.DataModel.HeadInfoMsg, PdfFonts.F_NORMAL_11));
            }

            y += topHeight;
            pdf.RightTextAtPosition(x + 30, y, new PdfTextItem("Kód", PdfFonts.F_BOLD_10));
            pdf.WriteTextAtPosition(x + 40, y, new PdfTextItem("Názov", PdfFonts.F_BOLD_10));

            x = right;
            pdf.RightTextAtPosition(x - 30, y, new PdfTextItem("Cena", PdfFonts.F_BOLD_10));
            pdf.RightTextAtPosition(x, y, new PdfTextItem("MJ", PdfFonts.F_BOLD_10));

            pdf.DrawHorizontalLine(left, y + 3, pdf.PageWidth - 2 * (widthMargin + widthPadding));

            headImgRatio = 50f / 800f;
            headImgWith = pdf.PageWidth;
            headImgHeight = headImgRatio * headImgWith;
            pdf.AddImgAtPosition(string.Format("{0}/{1}", this.DataModel.ImgPath, "gloziksoft-offer-footer-print.png"), headImgWith, headImgHeight, 0, 0, pdf.PageHeight);
            pdf.ChangeDrawColor(pdf.Fonts.PrimaryColor);
            pdf.CenterTextAtPosition(pdf.PageHeight - 15f, new PdfTextItem("... viac informácií na www.gloziksoft.sk", PdfFonts.F_BOLD_10));

            return y + 20;
        }

        private float OneIteData(PdfFile pdf, ProductModel product, float y, int cnt)
        {
            pdf.ChangeDrawColor(pdf.Fonts.DefaultColor);

            float left = widthMargin + widthPadding;
            float right = pdf.PageWidth - widthMargin - widthPadding;

            float x = left;

            pdf.RightTextAtPosition(x + 30, y, new PdfTextItem(product.ProductCode, PdfFonts.F_NORMAL_10));
            pdf.WriteTextAtPosition(x + 40, y, new PdfTextItem(product.ProductName, PdfFonts.F_NORMAL_10));

            x = right;
            pdf.RightTextAtPosition(x - 30, y, new PdfTextItem(PriceUtil.NumberToTwoDecString(product.GetCurrentPrice_WithVat()), PdfFonts.F_NORMAL_10));
            pdf.RightTextAtPosition(x, y, new PdfTextItem(product.UnitTypeName, PdfFonts.F_NORMAL_10));

            pdf.ChangeDrawColor(pdf.Fonts.PrimaryColor);
            pdf.DrawHorizontalLine(left, y + 3, pdf.PageWidth - 2 * (widthMargin + widthPadding), 0.2f);

            return y;
        }

    }

    public class CategoryOfferModel
    {
        public string ImgPath { get; private set; }
        public SysConstModel Eshoppgsoftweb { get; private set; }
        public CategoryModel CategoryData { get; private set; }
        public ProductPagingListModel Products { get; private set; }

        public string HeadInfoMsg
        {
            get
            {
                return GetHeadInfoMsg();
            }
        }

        public CategoryOfferModel(Guid pkCaregory, string imgPath)
        {
            this.ImgPath = imgPath;
            this.Eshoppgsoftweb = SysConstModel.CreateCopyFrom(new SysConstRepository().Get());
            this.CategoryData = CategoryModel.CreateCopyFrom(new EshoppgsoftwebCategoryRepository().Get(pkCaregory));


            EshoppgsoftwebProductFilter filter = new EshoppgsoftwebProductFilter();
            filter.OnlyIsVisible = true;
            filter.ProductCategoryKeyList = new List<string>();
            filter.ProductCategoryKeyList.Add(pkCaregory.ToString());

            Page<EshoppgsoftwebProduct> productsPage = new EshoppgsoftwebProductRepository().GetPage(
                        page: 1,
                        itemsPerPage: _PagingModel.AllItemsPerPage,
                        filter: filter);

            this.Products = ProductPagingListModel.CreateCopyFrom(
                productsPage,
                new ProductModelDropDowns(),
                loadPrices: true);
            this.Products.Items.Sort(new ProductModelRecomendationComparer(filter));
        }

        private string GetHeadInfoMsg()
        {
            return this.CategoryData.CategoryOfferText;
        }
    }
}
