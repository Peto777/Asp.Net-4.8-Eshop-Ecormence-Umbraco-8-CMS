using dufeksoft.lib.Model;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class ProductPriceModel : _BaseModel
    {
        [Display(Name = "Produkt")]
        public Guid ProductKey { get; set; }

        [Required(ErrorMessage = "Dátum platí od musí byť zadaný")]
        [Date(ErrorMessage = "Zadali ste neplatný dátum Platí od")]
        [Display(Name = "Platí od")]
        public string ValidFrom { get; set; }

        [Date(ErrorMessage = "Zadali ste neplatný dátum Platí do")]
        [Display(Name = "Platí do")]
        public string ValidTo { get; set; }

        [Required(ErrorMessage = "DPH % musí byť zadané")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre DPH %")]
        [Display(Name = "DPH %")]
        public string VatRate { get; set; }
        [Required(ErrorMessage = "Cena bez DPH musí byť zadaná")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre cenu bez DPH")]
        [Display(Name = "Cena bez DPH")]
        public string Price_1_NoVat { get; set; }
        [Required(ErrorMessage = "Cena s DPH musí byť zadaná")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre cenu s DPH")]
        [Display(Name = "Cena s DPH")]
        public string Price_1_WithVat { get; set; }

        public ProductModel Product { get; set; }

        public ProductPriceModel()
        {
            this.Price_1_NoVat = "0";
            this.Price_1_WithVat = "0";
            this.VatRate = PriceUtil.NumberToEditorString(20M);
            this.ValidFrom = DateTimeUtil.GetDisplayDate(DateTime.Today.AddDays(1));
        }

        public void CopyDataFrom(EshoppgsoftwebProductPrice src)
        {
            if (src == null)
            {
                return;
            }
            this.pk = src.pk;
            this.ProductKey = src.ProductKey;
            this.ValidFrom = DateTimeUtil.GetDisplayDate(src.ValidFrom);
            this.ValidTo = DateTimeUtil.GetDisplayDate(src.ValidTo);
            this.VatRate = PriceUtil.NumberToEditorString(src.VatRate);
            this.Price_1_NoVat = PriceUtil.NumberToEditorString(src.Price_1_NoVat);
            this.Price_1_WithVat = PriceUtil.NumberToEditorString(src.Price_1_WithVat);
        }

        public void CopyDataTo(EshoppgsoftwebProductPrice trg)
        {
            trg.pk = this.pk;
            trg.ProductKey = this.ProductKey;
            trg.ValidFrom = DateTimeUtil.DisplayDateToDate(this.ValidFrom).Value;
            trg.ValidTo = DateTimeUtil.DisplayDateToDate(this.ValidTo);

            trg.VatRate = PriceUtil.NumberFromEditorString(this.VatRate);
            trg.Price_1_WithVat = PriceUtil.NumberFromEditorString(this.Price_1_WithVat);
            trg.Price_1_NoVat = VatUtil.CalculatePriceWithoutVat(trg.Price_1_WithVat, trg.VatRate);
        }

        public static ProductPriceModel CreateCopyFrom(EshoppgsoftwebProductPrice src)
        {
            ProductPriceModel trg = new ProductPriceModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static EshoppgsoftwebProductPrice CreateCopyFrom(ProductPriceModel src)
        {
            EshoppgsoftwebProductPrice trg = new EshoppgsoftwebProductPrice();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class ProductPriceListModel : List<ProductPriceModel>
    {
        public ProductModel Product { get; set; }

        public static ProductPriceListModel CreateCopyFrom(List<EshoppgsoftwebProductPrice> srcArray, bool standardPriceFirst = true)
        {
            ProductPriceListModel trgArray = new ProductPriceListModel();

            if (standardPriceFirst)
            {
                foreach (EshoppgsoftwebProductPrice src in srcArray)
                {
                    if (src.ValidTo == null)
                    {
                        trgArray.Add(ProductPriceModel.CreateCopyFrom(src));
                    }
                }
                foreach (EshoppgsoftwebProductPrice src in srcArray)
                {
                    if (src.ValidTo != null)
                    {
                        trgArray.Add(ProductPriceModel.CreateCopyFrom(src));
                    }
                }
            }
            else
            {
                foreach (EshoppgsoftwebProductPrice src in srcArray)
                {
                    trgArray.Add(ProductPriceModel.CreateCopyFrom(src));
                }
            }

            return trgArray;
        }
    }
}
