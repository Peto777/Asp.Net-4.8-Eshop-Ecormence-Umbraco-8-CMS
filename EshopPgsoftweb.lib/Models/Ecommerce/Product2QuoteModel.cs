using dufeksoft.lib.Model;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class Product2QuoteModel : _BaseModel
    {
        public Guid PkQuote { get; set; }
        public Guid PkProduct { get; set; }
        public string NonProductId { get; set; }
        [Display(Name = "Produkt")]
        public string ProductCollectionKey { get; set; }
        public string ProductCollectionName
        {
            get
            {
                return GetProductCollectionName();
            }
        }
        [Display(Name = "Poradie")]
        public int ItemOrder { get; set; }
        [Display(Name = "Množstvo")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre množstvo")]
        [Required(ErrorMessage = "Množstvo musí byť zadané")]
        public string ItemPcs { get; set; }
        [Display(Name = "Kód")]
        [Required(ErrorMessage = "Kód musí byť zadaný")]
        public string ItemCode { get; set; }
        [Display(Name = "Názov")]
        [Required(ErrorMessage = "Názov musí byť zadaný")]
        public string ItemName { get; set; }
        [Display(Name = "MJ")]
        public string UnitName { get; set; }
        public int UnitTypeId { get; set; }
        [Required(ErrorMessage = "Hmotnosť jedného kusu musí byť zadaná")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre hmotnosť jedného kusu")]
        [Display(Name = "Hmotnosť jedného kusu")]
        public string UnitWeight { get; set; }
        [Required(ErrorMessage = "Cena bez DPH musí byť zadaná")]
        [Display(Name = "Cena bez DPH")]
        public string UnitPriceNoVat { get; set; }
        [Required(ErrorMessage = "Cena s DPH musí byť zadaná")]
        [Display(Name = "Cena s DPH")]
        public string UnitPriceWithVat { get; set; }
        [Required(ErrorMessage = "DPH % musí byť zadané")]
        [Display(Name = "DPH %")]
        public string UnitPriceVatPerc { get; set; }

        public ProductDropDown ProductCollection { get; set; }

        public ProductModel Product { get; set; }
        public string Url
        {
            get
            {
                return this.Product != null ? this.Product.Url : string.Empty;
            }
        }
        public string ProductCode
        {
            get
            {
                return this.Product != null ? this.Product.ProductCode : this.ItemCode;
            }
        }
        public string ProductName
        {
            get
            {
                return this.Product != null ? this.Product.ProductName : string.Format("{0}: {1}", this.ItemCode, this.ItemName);
            }
        }
        public string ProductDescription
        {
            get
            {
                return this.Product != null ? this.Product.ProductDescription : string.Empty;
            }
        }
        public string ProductImg
        {
            get
            {
                return this.Product != null ? this.Product.ProductImg : string.Empty;
            }
        }
        public string ProductPriceNoVat
        {
            get
            {
                return this.Product != null ? PriceUtil.GetPriceString(this.Product.GetCurrentPrice_NoVat()) : string.Empty;
            }
        }
        public string ProductPriceWithVat
        {
            get
            {
                return this.Product != null ? PriceUtil.GetPriceString(this.Product.GetCurrentPrice_WithVat()) : string.Empty;
            }
        }

        public decimal BasePriceNoVat
        {
            get
            {
                return PriceUtil.NumberFromEditorString(this.UnitPriceNoVat);
            }
        }
        public decimal BasePriceWithVat
        {
            get
            {
                return PriceUtil.NumberFromEditorString(this.UnitPriceWithVat);
            }
        }
        public decimal BasePriceVatPerc
        {
            get
            {
                return PriceUtil.NumberFromEditorString(this.UnitPriceVatPerc);
            }
        }
        public string BasePriceWithCurrencyNoVat
        {
            get
            {
                return PriceUtil.GetPriceString(this.BasePriceNoVat);
            }
            set
            {
                // nothing to set
            }
        }
        public string BasePriceWithCurrencyWithVat
        {
            get
            {
                return PriceUtil.GetPriceString(this.BasePriceWithVat);
            }
            set
            {
                // nothing to set
            }
        }
        public string BasePriceWithPercVatPerc
        {
            get
            {
                return string.Format("{0} %", this.BasePriceVatPerc);
            }
            set
            {
                // nothing to set
            }
        }

        public decimal TotalPriceNoVat
        {
            get
            {
                return VatUtil.CalculatePriceWithoutVat(this.TotalPriceWithVat, this.BasePriceVatPerc);
            }
        }
        public decimal TotalPriceWithVat
        {
            get
            {
                return this.BasePriceWithVat * PriceUtil.NumberFromEditorString(this.ItemPcs);
            }
        }
        public string TotalPriceWithCurrencyNoVat
        {
            get
            {
                return PriceUtil.GetPriceString(this.TotalPriceNoVat);
            }
            set
            {
                // nothing to set
            }
        }
        public string TotalPriceWithCurrencyWithVat
        {
            get
            {
                return PriceUtil.GetPriceString(this.TotalPriceWithVat);
            }
            set
            {
                // nothing to set
            }
        }

        public bool IsProductItem
        {
            get
            {
                return this.PkProduct != null && this.PkProduct != Guid.Empty;
            }
        }


        public void CopyDataFrom(Product2Quote src)
        {
            this.pk = src.pk;
            this.PkQuote = src.PkQuote;
            this.PkProduct = src.PkProduct;
            this.NonProductId = src.NonProductId;
            this.ItemOrder = src.ItemOrder;
            this.ItemPcs = PriceUtil.NumberToEditorString(src.ItemPcs);
            this.ItemCode = src.ItemCode;
            this.ItemName = src.ItemName;
            this.UnitName = src.UnitName;
            this.UnitTypeId = src.UnitTypeId;
            this.UnitWeight = PriceUtil.NumberToEditorString(src.UnitWeight);
            this.UnitPriceNoVat = PriceUtil.NumberToEditorString(src.UnitPriceNoVat);
            this.UnitPriceWithVat = PriceUtil.NumberToEditorString(src.UnitPriceWithVat);
            this.UnitPriceVatPerc = PriceUtil.NumberToEditorString(src.VatPerc);
        }

        public void CopyDataTo(Product2Quote trg)
        {
            trg.pk = this.pk;
            trg.PkQuote = this.PkQuote;
            trg.PkProduct = this.PkProduct;
            trg.NonProductId = this.NonProductId;
            trg.ItemOrder = this.ItemOrder;
            trg.ItemPcs = PriceUtil.NumberFromEditorString(this.ItemPcs);
            trg.ItemCode = this.ItemCode;
            trg.ItemName = this.ItemName;
            trg.UnitName = this.UnitName;
            trg.UnitTypeId = this.UnitTypeId;
            trg.UnitWeight = PriceUtil.NumberFromEditorString(this.UnitWeight);
            trg.UnitPriceNoVat = PriceUtil.NumberFromEditorString(this.UnitPriceNoVat);
            trg.UnitPriceWithVat = PriceUtil.NumberFromEditorString(this.UnitPriceWithVat);
            trg.VatPerc = PriceUtil.NumberFromEditorString(this.UnitPriceVatPerc);
        }

        //public void UpdateBeforeEdit()
        //{
        //    this.ProductCollectionKey = this.PkProduct.ToString();
        //}

        //public void UpdateAfterEdit()
        //{
        //    this.PkProduct = new Guid(this.ProductCollectionKey);
        //}

        //public void UpdateDropDownsBeforeEdit()
        //{
        //    //this.DropDowns = new QuoteItemDropDowns();

        //    //// item state
        //    //CmpDropDownItem ddi = this.DropDowns.ItemStateCollection.GetItemForName(this.ItemState, true);
        //    //if (ddi != null)
        //    //{
        //    //    this.ItemStateCollectionKey = ddi.DataKey;
        //    //}
        //}

        //public void UpdateDropDownsAfterEdit()
        //{
        //    //this.DropDowns = new QuoteItemDropDowns();
        //    //// item state
        //    //CmpDropDownItem ddi = this.DropDowns.ItemStateCollection.GetItemForKey(this.ItemStateCollectionKey);
        //    //if (ddi != null)
        //    //{
        //    //    this.ItemState = ddi.Name;
        //    //}
        //}

        public static Product2QuoteModel CreateCopyFrom(Product2Quote src)
        {
            Product2QuoteModel trg = new Product2QuoteModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static Product2Quote CreateCopyFrom(Product2QuoteModel src)
        {
            Product2Quote trg = new Product2Quote();
            src.CopyDataTo(trg);

            return trg;
        }

        public string GetProductCollectionName()
        {
            if (this.ProductCollection == null)
            {
                return string.Empty;
            }

            return this.ProductCollection.GetItemNameForKey(this.ProductCollectionKey);
        }

        public string GetIncCnt()
        {
            decimal cnt = 1;
            switch ((ProductUnit.UnitType)this.UnitTypeId)
            {
                case ProductUnit.UnitType.UT_PCS:
                    cnt = 1;
                    break;
                case ProductUnit.UnitType.UT_KG:
                    cnt = 0.05M;
                    break;
            }

            return PriceUtil.NumberToEditorString(cnt);
        }
        public string GetDecCnt()
        {
            decimal cnt = -1;
            switch ((ProductUnit.UnitType)this.UnitTypeId)
            {
                case ProductUnit.UnitType.UT_PCS:
                    cnt = -1;
                    break;
                case ProductUnit.UnitType.UT_KG:
                    cnt = -0.05M;
                    break;
            }

            return PriceUtil.NumberToEditorString(cnt);
        }
        public string ItemPcs2DecPlaces()
        {
            string[] items = this.ItemPcs.Split(',');

            string beforeComma = items[0];
            string afterComma = items[1];
            if (afterComma.Length > 2)
            {
                afterComma = afterComma.Substring(0, 2);
            }

            return string.Format("{0},{1}", beforeComma, afterComma);
        }
    }

    public class InsertProduct2QuoteModel
    {
        public Guid PkQuote { get; set; }
        public Guid PkProduct { get; set; }

        [Display(Name = "Kód")]
        public string ItemCode { get; set; }
        [Display(Name = "Názov")]
        public string ItemName { get; set; }
        [Display(Name = "MJ")]
        public string UnitName { get; set; }

        [Display(Name = "Množstvo")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre množstvo")]
        [Required(ErrorMessage = "Množstvo musí byť zadané")]
        public string ItemPcs { get; set; }

        public static EshoppgsoftwebProduct AddProductToQuote(Guid pkQuote, Guid pkProduct, decimal cnt)
        {
            // Get product
            EshoppgsoftwebProductRepository prodRep = new EshoppgsoftwebProductRepository();
            EshoppgsoftwebProduct product = prodRep.Get(pkProduct);
            // Add product to quote
            Product2QuoteRepository rep = new Product2QuoteRepository();
            Product2Quote dataRec = rep.Get(pkQuote, product.pk);
            if (dataRec == null)
            {
                dataRec = new Product2Quote();
                dataRec.PkQuote = pkQuote;
                dataRec.PkProduct = pkProduct;

                EshoppgsoftwebProductPriceInfo productPrice = EshoppgsoftwebProductPriceCache.GetProductPrice(dataRec.PkProduct);
                dataRec.ItemCode = product.ProductCode;
                dataRec.ItemName = product.ProductName;
                dataRec.UnitName = ProductUnit.GetName((ProductUnit.UnitType)product.UnitTypeId);
                dataRec.UnitTypeId = product.UnitTypeId;
                dataRec.UnitWeight = product.ProductUnitWeight;
                dataRec.UnitPriceNoVat = productPrice.GetCurrentPriceNoVat();
                dataRec.UnitPriceWithVat = productPrice.GetCurrentPriceWithVat();
                dataRec.VatPerc = productPrice.GetCurrentPriceVatPerc();

            }
            dataRec.ItemPcs += cnt;
            rep.Save(dataRec);

            return product;
        }
    }
}
