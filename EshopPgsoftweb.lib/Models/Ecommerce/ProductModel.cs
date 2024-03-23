using dufeksoft.lib.Model;
using dufeksoft.lib.Model.Grid;
using dufeksoft.lib.ParamSet;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class ProductModel : _BaseModel
    {
        public const string EmptyImgUrl = "/Styles/Images/img-add.png";

        [Display(Name = "Zobrazovať")]
        public bool ProductIsVisible { get; set; }
        public string ProductIsVisibleText { get; set; }

        [RequiredGuidDropDown(ErrorMessage = "Výrobca musí byť zadaný")]
        [Display(Name = "Výrobca")]
        public string ProducerCollectionKey { get; set; }
        public Guid ProducerKey { get; set; }
        public string ProducerName { get; set; }

        [Required(ErrorMessage = "Kód produktu musí byť zadaný")]
        [Display(Name = "Kód produktu")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Názov produktu musí byť zadaný")]
        [Display(Name = "Názov produktu")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Hlavný text produktu musí byť zadaný")]
        [Display(Name = "Hlavný text produktu")]
        public string ProductText { get; set; }
        [AllowHtml]
        [Display(Name = "Popis produktu")]
        public string ProductDescription { get; set; }

        [Display(Name = "Poradie")]
        public int ProductOrder { get; set; }
        [Display(Name = "Obrázok")]
        public string ProductImg { get; set; }

        public string AdminImgUrl
        {
            get
            {
                return string.IsNullOrEmpty(this.ProductImg) ? ProductModel.EmptyImgUrl : this.ProductImg;
            }
        }

        [Required(ErrorMessage = "URL musí byť zadané")]
        [Display(Name = "URL")]
        public string ProductUrl { get; set; }
        [Required(ErrorMessage = "META TITLE musí byť zadané")]
        [Display(Name = "META TITLE")]
        public string ProductMetaTitle { get; set; }
        [Required(ErrorMessage = "META KEYWORDS musí byť zadané")]
        [Display(Name = "META KEYWORDS")]
        public string ProductMetaKeywords { get; set; }
        [Required(ErrorMessage = "META DESCRIPTION musí byť zadané")]
        [Display(Name = "META DESCRIPTION")]
        public string ProductMetaDescription { get; set; }

        [RequiredGuidDropDown(ErrorMessage = "Dostupnosť musí byť zadaná")]
        [Display(Name = "Dostupnosť")]
        public string AvailabilityCollectionKey { get; set; }
        public Guid AvailabilityKey { get; set; }
        public string AvailabilityName { get; set; }

        [Required(ErrorMessage = "Merná jednotka musí byť zadaná")]
        [Display(Name = "Merná jednotka")]
        public string UnitTypeId { get; set; }
        public string UnitTypeName { get; set; }

        [Display(Name = "Novinka")]
        public bool ProductIsNew { get; set; }
        [Display(Name = "Výpredaj")]
        public bool ProductIsSale { get; set; }
        [Display(Name = "Trvanlivosť")]
        public string ProductDurability { get; set; }
        [Required(ErrorMessage = "Hmotnosť jedného kusu musí byť zadaná")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre hmotnosť jedného kusu")]
        [Display(Name = "Hmotnosť jedného kusu")]
        public string ProductUnitWeight { get; set; }
        [Required(ErrorMessage = "Počet kusov v kartóne musí byť zadany")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre počet kusov v kartóne")]
        [Display(Name = "Počet kusov v kartóne")]
        public string ProductUnitsInPckg { get; set; }
        [Display(Name = "Krajina pôvodu")]
        public string ProductCountry { get; set; }

        public string Url
        {
            get
            {
                return string.Format("{0}{1}", ProductContentFinder.ProductPath, this.ProductUrl);
            }
        }


        public Product2AttributeModel ProductAttributes { get; set; }
        public ProductRelationModel ProductRelations { get; set; }
        public Product2CategoryEditModel ProductCategories { get; set; }


        public ProductModelDropDowns DropDowns { get; set; }
        public EshoppgsoftwebProductPriceInfo PriceInfo { get; private set; }

        public ProductAttrHelper SizeHelper { get; set; }
        public string ProductAttrString
        {
            get
            {
                return this.SizeHelper.GetProductAttrString(this.pk.ToString());
            }
        }

        public bool IsFavorite { get; set; }


        public ProductModel()
        {
            this.ProductUnitWeight = "0";
            this.ProductUnitsInPckg = "1";

            this.ProductAttributes = new Product2AttributeModel();
            this.ProductRelations = new ProductRelationModel();
            this.ProductCategories = new Product2CategoryEditModel();
        }

        public void CopyDataFrom(EshoppgsoftwebProduct src, ProductModelDropDowns dropDowns)
        {
            this.DropDowns = dropDowns;

            this.pk = src.pk;
            this.ProductIsVisible = src.ProductIsVisible;
            this.ProductIsVisibleText = this.ProductIsVisible ? "ÁNO" : "NIE";
            this.ProductCode = src.ProductCode;
            this.ProductName = src.ProductName;
            this.ProductText = src.ProductText;
            this.ProductDescription = src.ProductDescription;
            this.ProductOrder = src.ProductOrder;
            this.ProductImg = src.ProductImg;

            this.ProductUrl = src.ProductUrl;
            this.ProductMetaTitle = src.ProductMetaTitle;
            this.ProductMetaKeywords = src.ProductMetaKeywords;
            this.ProductMetaDescription = src.ProductMetaDescription;

            // Set producer
            this.ProducerKey = src.ProducerKey;
            this.ProducerCollectionKey = string.Empty;
            this.ProducerName = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiProducer = this.DropDowns.ProducerCollection.GetItemForKey(this.ProducerKey.ToString());
                if (ddiProducer != null)
                {
                    this.ProducerCollectionKey = ddiProducer.DataKey;
                    this.ProducerName = ddiProducer.Name;
                }
            }

            // Set availability
            this.AvailabilityKey = src.AvailabilityKey;
            this.AvailabilityCollectionKey = string.Empty;
            this.AvailabilityName = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiAvailability = this.DropDowns.AvailabilityCollection.GetItemForKey(this.AvailabilityKey.ToString());
                if (ddiAvailability != null)
                {
                    this.AvailabilityCollectionKey = ddiAvailability.DataKey;
                    this.AvailabilityName = ddiAvailability.Name;
                }
            }

            this.UnitTypeId = src.UnitTypeId.ToString();
            this.UnitTypeName = ProductUnit.GetName((ProductUnit.UnitType)src.UnitTypeId);
            this.ProductIsNew = src.ProductIsNew;
            this.ProductIsSale = src.ProductIsSale;
            this.ProductDurability = src.ProductDurability;
            this.ProductUnitWeight = PriceUtil.NumberToEditorString(src.ProductUnitWeight);
            this.ProductUnitsInPckg = PriceUtil.NumberToEditorString(src.ProductUnitsInPckg);
            this.ProductCountry = src.ProductCountry;
        }

        public void CopyDataTo(EshoppgsoftwebProduct trg, ProductModelDropDowns dropDowns)
        {
            this.DropDowns = dropDowns;

            trg.pk = this.pk;
            trg.ProductIsVisible = this.ProductIsVisible;
            trg.ProductCode = this.ProductCode;
            trg.ProductName = this.ProductName;
            trg.ProductText = this.ProductText;
            trg.ProductDescription = this.ProductDescription;
            trg.ProductOrder = this.ProductOrder;
            trg.ProductImg = this.ProductImg;

            trg.ProductUrl = this.ProductUrl;
            trg.ProductMetaTitle = this.ProductMetaTitle;
            trg.ProductMetaKeywords = this.ProductMetaKeywords;
            trg.ProductMetaDescription = this.ProductMetaDescription;

            // Set producer
            trg.ProducerKey = Guid.Empty;
            CmpDropDownItem ddiProducer = this.DropDowns.ProducerCollection.GetItemForKey(this.ProducerCollectionKey);
            if (ddiProducer != null)
            {
                trg.ProducerKey = new Guid(ddiProducer.DataKey);
            }

            // Set availability
            trg.AvailabilityKey = Guid.Empty;
            CmpDropDownItem ddiAvailability = this.DropDowns.AvailabilityCollection.GetItemForKey(this.AvailabilityCollectionKey);
            if (ddiAvailability != null)
            {
                trg.AvailabilityKey = new Guid(ddiAvailability.DataKey);
            }

            trg.UnitTypeId = ProductUnit.GetValidUnitTypeId(this.UnitTypeId);
            trg.ProductIsNew = this.ProductIsNew;
            trg.ProductIsSale = this.ProductIsSale;
            trg.ProductDurability = this.ProductDurability;
            trg.ProductUnitWeight = PriceUtil.NumberFromEditorString(this.ProductUnitWeight);
            trg.ProductUnitsInPckg = PriceUtil.NumberFromEditorString(this.ProductUnitsInPckg);
            trg.ProductCountry = this.ProductCountry;
        }

        public static ProductModel CreateCopyFrom(EshoppgsoftwebProduct src, ProductModelDropDowns dropDowns, bool loadPrice = false)
        {
            ProductModel trg = new ProductModel();
            trg.CopyDataFrom(src, dropDowns);
            if (loadPrice)
            {
                trg.PriceInfo = EshoppgsoftwebProductPriceCache.GetProductPrice(trg.pk);
            }

            return trg;
        }

        public static EshoppgsoftwebProduct CreateCopyFrom(ProductModel src, ProductModelDropDowns dropDowns)
        {
            EshoppgsoftwebProduct trg = new EshoppgsoftwebProduct();
            src.CopyDataTo(trg, dropDowns);

            return trg;
        }


        public decimal GetCurrentPrice_NoVat()
        {
            return this.PriceInfo.CurrentPrice.Price_1_NoVat;
        }
        public decimal GetCurrentPrice_WithVat()
        {
            return this.PriceInfo.CurrentPrice.Price_1_WithVat;
        }
        public decimal GetStandardPrice_WithVat()
        {
            return this.PriceInfo.StandardPrice.Price_1_WithVat;
        }
        public bool IsDiscount()
        {
            if (this.PriceInfo == null)
            {
                return false;
            }
            if (this.PriceInfo.ActionPrice == null)
            {
                return false;
            }
            decimal standardPriceWithVat = GetStandardPrice_WithVat();
            return GetCurrentPrice_WithVat() < standardPriceWithVat ? true : false;
        }
        public string GetBasePriceIfDiscount()
        {
            return this.IsDiscount() ? GetPriceString(GetStandardPrice_WithVat()) : string.Empty;
        }
        public string GetBasePriceWithVatIfDiscount()
        {
            return this.IsDiscount() ? GetPriceStringWithVat(GetStandardPrice_WithVat()) : string.Empty;
        }
        public string GetPriceString(decimal price)
        {
            return PriceUtil.GetPriceString(price);
        }
        public string GetPriceStringNoVat(decimal price)
        {
            return SysConstUtil.IsVatPaier ? string.Format("{0} bez DPH", GetPriceString(price)) : GetPriceString(price);
        }
        public string GetPriceStringWithVat(decimal price)
        {
            return SysConstUtil.IsVatPaier ? string.Format("{0} s DPH", GetPriceString(price)) : GetPriceString(price);
        }
        public string GetDiscountPercString()
        {
            if (!this.IsDiscount())
            {
                return string.Empty;
            }

            decimal standardPrice = GetStandardPrice_WithVat();
            decimal currentPrice = GetCurrentPrice_WithVat();

            int discountPerc = (int)(100M - (100M * currentPrice / standardPrice));

            return PriceUtil.GetPercString(discountPerc);
        }
        public string GetIncCnt()
        {
            decimal cnt = 1;
            switch ((ProductUnit.UnitType)ProductUnit.GetValidUnitTypeId(this.UnitTypeId))
            {
                case ProductUnit.UnitType.UT_PCS:
                    cnt = 1;
                    break;
                case ProductUnit.UnitType.UT_KG:
                    cnt = 0.25M;
                    break;
            }

            return PriceUtil.NumberToEditorString(cnt);
        }
        public string GetDecCnt()
        {
            decimal cnt = -1;
            switch ((ProductUnit.UnitType)ProductUnit.GetValidUnitTypeId(this.UnitTypeId))
            {
                case ProductUnit.UnitType.UT_PCS:
                    cnt = -1;
                    break;
                case ProductUnit.UnitType.UT_KG:
                    cnt = -0.25M;
                    break;
            }

            return PriceUtil.NumberToEditorString(cnt);
        }
        public string GetInitialPcs()
        {
            decimal cnt = 1;
            switch ((ProductUnit.UnitType)ProductUnit.GetValidUnitTypeId(this.UnitTypeId))
            {
                case ProductUnit.UnitType.UT_PCS:
                    cnt = 1;
                    break;
                case ProductUnit.UnitType.UT_KG:
                    cnt = 0.25M;
                    break;
            }

            return PriceUtil.NumberToEditorString(cnt);
        }
    }

    public class ProductSessionModel
    {
        public ProductSessionModel(ProductModel product, string sessionId)
        {
            this.ProductData = product;
            this.SessionId = sessionId;
            this.MemberId = EshoppgsoftwebCustomerCache.CurrentMemberId;
        }
        public ProductModel ProductData { get; set; }
        public string SessionId { get; set; }
        public string MemberId { get; private set; }
    }

    public class ProductModelDropDowns
    {
        public ProducerDropDown ProducerCollection { get; set; }
        public AvailabilityDropDown AvailabilityCollection { get; set; }
        public ProductUnitDropDown ProductUnitCollection { get; set; }

        public ProductModelDropDowns()
        {
            this.ProducerCollection = ProducerDropDown.CreateFromRepository(true);
            this.AvailabilityCollection = AvailabilityDropDown.CreateFromRepository(true);
            this.ProductUnitCollection = new ProductUnitDropDown();
        }
    }

    public class ProductDropDown : CmpDropDown
    {
        public ProductDropDown()
        {
        }

        public static ProductDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
            return ProductDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static ProductDropDown CreateCopyFrom(Page<EshoppgsoftwebProduct> dataList, bool allowNull, string emptyText)
        {
            ProductModelDropDowns dd = new ProductModelDropDowns();
            ProductDropDown ret = new ProductDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (EshoppgsoftwebProduct dataItem in dataList.Items)
            {
                ProductModel dataModel = ProductModel.CreateCopyFrom(dataItem, dd);
                ret.AddItem(dataModel.ToString(), dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }

    public class ProductListModel : List<ProductModel>
    {
        public HttpRequest CurrentRequest { get; private set; }
        public string SessionId { get; set; }
        public int PageSize { get; private set; }

        private GridPagerModel currentPager;
        public GridPagerModel Pager
        {
            get
            {
                return GetPager();
            }
        }

        public ProductListModel(HttpRequest request, int pageSize = 25)
        {
            this.CurrentRequest = request;
            this.PageSize = pageSize;
        }

        public List<ProductModel> GetPageItems()
        {
            GridPageInfo cpi = this.Pager.GetCurrentPageInfo();

            List<ProductModel> resultList = new List<ProductModel>();
            for (int i = cpi.FirsItemIndex; i < this.Count && i < cpi.LastItemIndex + 1; i++)
            {
                resultList.Add(this[i]);
            }

            return resultList;
        }

        GridPagerModel GetPager()
        {
            if (this.currentPager == null || this.currentPager.ItemCnt != this.Count)
            {
                this.currentPager = new GridPagerModel(this.CurrentRequest, this.Count, this.PageSize);
            }

            return this.currentPager;
        }
    }

    public class ProductPagingListModel : _PagingModel
    {
        public List<ProductModel> Items { get; set; }

        public static ProductPagingListModel CreateCopyFrom(Page<EshoppgsoftwebProduct> srcArray, ProductModelDropDowns dropDowns, bool loadPrices = false)
        {
            ProductPagingListModel trgArray = new ProductPagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<ProductModel>(srcArray.Items.Count + 1);

            foreach (EshoppgsoftwebProduct src in srcArray.Items)
            {
                trgArray.Items.Add(ProductModel.CreateCopyFrom(src, dropDowns, loadPrice: loadPrices));
            }

            return trgArray;
        }

        public void BindProductAttrs()
        {
            ProductAttrHelper helper = new ProductAttrHelper();
            helper.AddToLoad(this.Items);
        }
    }

    public class ProductFilterModel : _BaseUserPropModel
    {

        [Display(Name = "Kód produktu")]
        public string ProductCode { get; set; }
        [Display(Name = "Vyhľadávanie (ID, kód, názov, popis ...)")]
        public string SearchText { get; set; }


        public ProductFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_ProductFilterModel;
        }

        public static ProductFilterModel CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            ProductFilterModel trg = new ProductFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(ProductFilterModel src)
        {
            src.UpdateAfterEdit();
            EshoppgsoftwebUserProp trg = new EshoppgsoftwebUserProp();
            src.CopyDataTo(trg);

            return trg;
        }


        public void UpdateBeforeEdit()
        {
            LoadPropValue(this.PropValue);
        }

        public void UpdateAfterEdit()
        {
            this.PropValue = SavePropValue();
        }

        private string SavePropValue()
        {
            // Create XML document
            XmlDocument doc = new XmlDocument();
            // Create main element
            XmlElement mainNode = doc.CreateElement("ProductFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            // Product code
            XmlParamSet.SaveItem(doc, mainNode, "ProductCode", this.ProductCode);
            // Search text
            XmlParamSet.SaveItem(doc, mainNode, "SearchText", this.SearchText);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "ProductFilterModel";

                // Product code
                this.ProductCode = XmlParamSet.LoadItem(doc, fullParent, "ProductCode", string.Empty);
                // Search text
                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
            }
        }
    }

    public class ProductPagerModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ProductSearchModel
    {
        public enum ModelType
        {
            Search = 0,
            Favorite = 1,
        }
        public ModelType Action { get; set; }
        public string ProductToSearch { get; set; }
        public Guid CustomerKey { get; set; }

        public ProductListModel ProductList { get; set; }
    }

    public class ProductModelPriceAscComparer : IComparer<ProductModel>
    {
        EshoppgsoftwebProductPriceComparer comparer = new EshoppgsoftwebProductPriceComparer();

        public int Compare(ProductModel x, ProductModel y)
        {
            return comparer.Compare(x.PriceInfo.CurrentPrice, y.PriceInfo.CurrentPrice);
        }
    }
    public class ProductModelPriceDescComparer : IComparer<ProductModel>
    {
        EshoppgsoftwebProductPriceComparer comparer = new EshoppgsoftwebProductPriceComparer();

        public int Compare(ProductModel x, ProductModel y)
        {
            return comparer.Compare(y.PriceInfo.CurrentPrice, x.PriceInfo.CurrentPrice);
        }
    }
    public class ProductModelRecomendationComparer : IComparer<ProductModel>
    {
        Hashtable ht;
        public ProductModelRecomendationComparer(EshoppgsoftwebProductFilter filter)
        {
            List<EshoppgsoftwebProduct2Category> dataList = new EshoppgsoftwebProduct2CategoryRepository().GetList(new EshoppgsoftwebProduct2CategoryFilter() { ProductCategoryKeyList = filter.ProductCategoryKeyList });
            ht = new Hashtable(dataList.Count + 1);

            foreach (EshoppgsoftwebProduct2Category dataRec in dataList)
            {
                if (!ht.ContainsKey(dataRec.PkProduct))
                {
                    ht.Add(dataRec.PkProduct, dataRec);
                }
            }
        }

        int GetProductOrder(ProductModel product)
        {
            return ht.ContainsKey(product.pk) ? (ht[product.pk] as EshoppgsoftwebProduct2Category).ProductOrder : 0;
        }

        public int Compare(ProductModel x, ProductModel y)
        {
            return GetProductOrder(x) - GetProductOrder(y);
        }
    }

    public class ProductAttrHelper
    {
        Hashtable htProductAttrStr = new Hashtable();
        bool needsReload = false;

        public void AddToLoad(List<ProductModel> dataList)
        {
            foreach (ProductModel dataRec in dataList)
            {
                AddToLoad(dataRec);
            }
        }
        public void AddToLoad(ProductModel dataRec)
        {
            string key = GetKey(dataRec);
            if (!htProductAttrStr.ContainsKey(key))
            {
                dataRec.SizeHelper = this;
                htProductAttrStr.Add(key, "");
                this.needsReload = true;
            }
        }

        string GetKey(ProductModel dataRec)
        {
            return dataRec.pk.ToString();
        }

        void Reload()
        {
            if (htProductAttrStr.Count > 0)
            {
                List<string> productKeyList = new List<string>(htProductAttrStr.Count);
                foreach (string key in htProductAttrStr.Keys)
                {
                    productKeyList.Add(key); // add for SQL WHERE clause
                }
                foreach (string key in productKeyList)
                {
                    htProductAttrStr[key] = ""; // clear old value
                }
                List<EshoppgsoftwebProduct2AttributeEx> dataList = new EshoppgsoftwebProduct2AttributeRepository().GetForProducts(productKeyList);
                foreach (EshoppgsoftwebProduct2AttributeEx dataRec in dataList)
                {
                    string productKey = dataRec.PkProduct.ToString();
                    string str = (string)htProductAttrStr[productKey];
                    htProductAttrStr[productKey] = string.IsNullOrEmpty(str) ? dataRec.ProductAttributeName : string.Format("{0},{1}", str, dataRec.ProductAttributeName);
                }
            }

            this.needsReload = false;
        }

        public string GetProductAttrString(string productKey)
        {
            if (!this.htProductAttrStr.ContainsKey(productKey))
            {
                throw new ApplicationException(string.Format("Specified product key {0} is not in this collection", productKey));
            }
            if (this.needsReload)
            {
                Reload();
            }

            return (string)this.htProductAttrStr[productKey];
        }
    }

    public class ProductImagesModel : _BaseModel
    {
        [Display(Name = "Produkt")]
        public string ProductName { get; set; }

        [Display(Name = "Hlavný obrázok")]
        public string ProductImg { get; set; }

        public string AdminImgUrl
        {
            get
            {
                return string.IsNullOrEmpty(this.ProductImg) ? ProductModel.EmptyImgUrl : this.ProductImg;
            }
        }

        public string FileUploadCategory
        {
            get
            {
                return EshoppgsoftwebProduct.GetFileUploadCategory(this.pk);
            }
        }

        public static ProductImagesModel LoadModel(Guid productKey)
        {
            return LoadModel(ProductModel.CreateCopyFrom(new EshoppgsoftwebProductRepository().Get(productKey), new ProductModelDropDowns()));
        }
        public static ProductImagesModel LoadModel(ProductModel product)
        {
            ProductImagesModel model = new ProductImagesModel();
            model.pk = product.pk;
            model.ProductName = string.Format("{0}, {1}", product.ProductCode, product.ProductName);
            model.ProductImg = product.ProductImg;

            return model;
        }

        public void Save()
        {
            EshoppgsoftwebProductRepository rep = new EshoppgsoftwebProductRepository();
            EshoppgsoftwebProduct product = rep.Get(this.pk);
            product.ProductImg = this.ProductImg;
            rep.Save(product);
        }

    }

    public class ProductUnit
    {
        public enum UnitType
        {
            UT_PCS = 0,
            UT_KG = 1,
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public static string GetName(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.UT_PCS:
                    return "ks";
                case UnitType.UT_KG:
                    return "kg";
            }

            return string.Empty;
        }

        public static int GetValidUnitTypeId(string key)
        {
            int id;
            if (int.TryParse(key, out id))
            {
                return (int)((UnitType)id);
            }

            return (int)UnitType.UT_PCS;
        }
    }
    public class ProductUnitDropDown : CmpDropDown
    {
        public ProductUnitDropDown()
        {
            this.AddItem(ProductUnit.UnitType.UT_PCS);
            this.AddItem(ProductUnit.UnitType.UT_KG);
        }

        private void AddItem(ProductUnit.UnitType gatewayType)
        {
            ProductUnit item = new ProductUnit();
            item.Id = ((int)gatewayType).ToString();
            item.Name = ProductUnit.GetName(gatewayType);
            this.AddItem(item.Name, item.Id, item);
        }
    }

}
