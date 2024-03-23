using dufeksoft.lib.Model.Grid;
using dufeksoft.lib.ParamSet;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Xml;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class CategoryPublicModel
    {
        public CategoryModel CategoryData { get; set; }
        public _SeoModel SeoData { get; set; }
        public _EshopModel EshopData { get; set; }

        public HttpRequest CurrentRequest { get; private set; }
        public string SessionId { get; set; }
        public int ProductsPageSize { get; private set; }

        public ProductPagingListModel Products { get; set; }

        public const string cQuickOrderParamName = "rychlaobj";

        private const string cPagerParamName = "stranka";
        private GridPagerModel productsPager;
        public GridPagerModel ProductsPager
        {
            get
            {
                return GetProductsPager();
            }
        }
        public GridSorterModel ProductsSorter { get; set; }

        public CategoryPublicFilterModel Filter { get; set; }
        public ProductSearchModel SearchModel { get; private set; }

        ///// <summary>
        ///// To display only filter menu
        ///// </summary>
        ///// <param name="sessionId">Session id</param>
        //public CategoryPublicModel(string sessionId)
        //{
        //    this.SessionId = sessionId;

        //    this.Filter = new CategoryPublicFilterModel();
        //    this.Filter.LoadFilters(this.SessionId);
        //}

        /// <summary>
        /// To display category with products
        /// </summary>
        /// <param name="ctrl">Controller</param>
        /// <param name="searchModel">Search model if creating product list for search request</param>
        public CategoryPublicModel(_BaseController ctrl, ProductSearchModel searchModel)
        {
            this.CurrentRequest = ctrl.CurrentRequest;
            this.SessionId = ctrl.CurrentSessionId;
            this.SearchModel = searchModel;

            string categoryUrl = CategoryContentFinder.GetCategoryUrl(this.CurrentRequest.Url);
            string categoryKeyForFilters = "null";

            if (this.SearchModel == null)
            {
                EshoppgsoftwebCategoryRepository rep = new EshoppgsoftwebCategoryRepository();
                EshoppgsoftwebCategory category;
                if (categoryUrl == CategoryContentFinder.CategoryUrl_All)
                {
                    category = EshoppgsoftwebCategory.RootEshoppgsoftwebCategory();
                }
                else
                {
                    category = rep.GetForCategoryUrl(categoryUrl);
                }
                if (category != null)
                {
                    this.CategoryData = CategoryModel.CreateCopyFrom(category);
                    this.CategoryData.LoadRelatives(rep);

                    this.SeoData = new _SeoModel();
                    this.SeoData.MenuTitle = this.CategoryData.CategoryMetaTitle;
                    this.SeoData.MetaTitle = this.CategoryData.CategoryMetaTitle;
                    this.SeoData.MetaKeywords = this.CategoryData.CategoryMetaKeywords;
                    this.SeoData.MetaDescription = this.CategoryData.CategoryMetaDescription;

                    this.SeoData.Og_Title = this.CategoryData.CategoryMetaTitle;
                    this.SeoData.Og_Description = this.CategoryData.CategoryMetaDescription;
                    this.SeoData.Og_Type = "website";
                    this.SeoData.Og_Url = this.CurrentRequest.Url.ToString();
                    if (!string.IsNullOrEmpty(this.CategoryData.CategoryImg))
                    {
                        _BaseControllerUtil urlHelper = new _BaseControllerUtil();
                        this.SeoData.Og_Image = urlHelper.GetAbsoluteUrl(this.CategoryData.CategoryImg);
                    }

                    categoryKeyForFilters = this.CategoryData.pk.ToString();
                }
            }
            else
            {
                switch (this.SearchModel.Action)
                {
                    case ProductSearchModel.ModelType.Search:
                        categoryKeyForFilters = "search";
                        this.CategoryData = new CategoryModel();
                        this.CategoryData.CategoryName = string.Format("VYHĽADÁVANIE", this.SearchModel.ProductToSearch);
                        break;
                    case ProductSearchModel.ModelType.Favorite:
                        categoryKeyForFilters = "favorite";
                        this.CategoryData = new CategoryModel();
                        this.CategoryData.CategoryName = "OBĽÚBENÉ PRODUKTY";
                        break;
                }
            }

            this.Filter = new CategoryPublicFilterModel();
            this.Filter.LoadFilters(this.SessionId, categoryKeyForFilters);
            this.ProductsPageSize = this.Filter.PageSizeFilter.PageSize;

            if (this.CategoryData != null)
            {
                bool isEmptySearch = false;
                if (this.SearchModel == null)
                {
                    isEmptySearch = false;
                }
                else
                {
                    switch (this.SearchModel.Action)
                    {
                        case ProductSearchModel.ModelType.Search:
                            isEmptySearch = string.IsNullOrEmpty(this.SearchModel.ProductToSearch);
                            break;
                        case ProductSearchModel.ModelType.Favorite:
                            isEmptySearch = false;
                            break;
                    }
                }

                if (!isEmptySearch)
                {
                    LoadProducts(ctrl, GridPagerModel.GetRequestPageNumber(this.CurrentRequest, queryPageParam: CategoryPublicModel.cPagerParamName));
                    //this.Products.BindProductSizes();
                }
            }
        }

        void LoadProducts(_BaseController ctrl, int page)
        {
            bool isPriceSort = IsPriceSort();
            bool isRecomendationSort = IsRecomendationSort();

            EshoppgsoftwebProductFilter filter = new EshoppgsoftwebProductFilter();
            filter.OnlyIsVisible = true;
            if (this.SearchModel == null)
            {
                filter.ProductCategoryKeyList = CategoryTree.GetDeepCategoryList(ctrl, this.CategoryData.pk.ToString(), onlyVisible: true);
                this.Filter.ProducersFilter.SetEnabledProducersForProductCategories(filter.ProductCategoryKeyList);
                this.Filter.ProductAttributesFilter.SetEnabledAttributesForProductCategories(filter.ProductCategoryKeyList);
            }
            else
            {
                switch (this.SearchModel.Action)
                {
                    case ProductSearchModel.ModelType.Search:
                        filter.SearchText = this.SearchModel.ProductToSearch;
                        break;
                    case ProductSearchModel.ModelType.Favorite:
                        filter.FavoriteProductsCustomerKey = this.SearchModel.CustomerKey;
                        break;
                }
            }
            if (!this.Filter.ProducersFilter.AllProducersSelected(true))
            {
                filter.ProducerKeyList = this.Filter.ProducersFilter.GetSelectedProducers(true);
            }
            if (!this.Filter.ProductAttributesFilter.AllAttributesSelected(true))
            {
                filter.ProductAttributeKeyList = this.Filter.ProductAttributesFilter.GetSelectedAttributes(true);
            }

            EshoppgsoftwebProductRepository rep = new EshoppgsoftwebProductRepository();

            string sortField = GetSortField();
            string sortDirection = GetSortDirection();
            long pageNum = (isPriceSort | isRecomendationSort) ? 1 : page;
            long pageSize = (isPriceSort | isRecomendationSort) ? _PagingModel.AllItemsPerPage : this.ProductsPageSize;

            // Load requested page
            Page<EshoppgsoftwebProduct> productsPage = rep.GetPage(
                        page: pageNum,
                        itemsPerPage: pageSize,
                        sortBy: sortField,
                        sortDir: sortDirection,
                        filter: filter);

            if (productsPage.Items.Count <= 0 && productsPage.TotalItems > 0)
            {
                // Check last page request
                long lastPageNum = productsPage.TotalItems / this.ProductsPageSize;
                if (lastPageNum * this.ProductsPageSize < productsPage.TotalItems)
                {
                    lastPageNum++;
                }
                if (lastPageNum < pageNum)
                {
                    // Load last page
                    pageNum = lastPageNum;
                    productsPage = rep.GetPage(
                            page: pageNum,
                            itemsPerPage: pageSize,
                            sortBy: sortField,
                            sortDir: sortDirection,
                            filter: filter);
                }
            }


            this.Products = ProductPagingListModel.CreateCopyFrom(
                productsPage,
                new ProductModelDropDowns(),
                loadPrices: true);

            if (isPriceSort)
            {
                SortProductsByPrice((int)page);
            }
            if (isRecomendationSort)
            {
                SortProductsByRecomendation((int)page, filter);
            }

            if (this.SearchModel != null && this.SearchModel.Action == ProductSearchModel.ModelType.Favorite)
            {
                // Set favorite tag
                foreach (ProductModel product in this.Products.Items)
                {
                    product.IsFavorite = true;
                }
            }
        }
        void SortProductsByPrice(int page)
        {
            // Sort by price
            if (this.Filter.ProductSortFilter.ProductSortType == CategoryPublicFilterModel_ProductSort.ProductSort_PriceAsc)
            {
                // Ascending
                this.Products.Items.Sort(new ProductModelPriceAscComparer());
            }
            else
            {
                // Descending
                this.Products.Items.Sort(new ProductModelPriceDescComparer());
            }

            CreateSortedProductsPage(page);
        }
        void SortProductsByRecomendation(int page, EshoppgsoftwebProductFilter filter)
        {
            // Sort by recomendation
            this.Products.Items.Sort(new ProductModelRecomendationComparer(filter));

            CreateSortedProductsPage(page);
        }
        void CreateSortedProductsPage(int page)
        {
            // Get products for requested page number
            int startIdx = (page - 1) * this.ProductsPageSize;
            if (startIdx > this.Products.Items.Count - 1)
            {
                int lastPageNum = this.Products.Items.Count / this.ProductsPageSize;
                if (lastPageNum * this.ProductsPageSize < this.Products.Items.Count)
                {
                    lastPageNum++;
                }
                startIdx = (lastPageNum - 1) * this.ProductsPageSize;
            }
            if (startIdx < 0)
            {
                startIdx = 0;
            }
            int endIdx = startIdx + this.ProductsPageSize;

            ProductPagingListModel sortedPage = new ProductPagingListModel();
            sortedPage.ItemsPerPage = (int)this.ProductsPageSize;
            sortedPage.TotalItems = (int)this.Products.Items.Count;
            sortedPage.Items = new List<ProductModel>(endIdx - startIdx + 1);

            for (int idx = startIdx; idx < endIdx; idx++)
            {
                if (idx < sortedPage.TotalItems)
                {
                    sortedPage.Items.Add(this.Products.Items[idx]);
                }
            }

            this.Products = sortedPage;
        }
        bool IsPriceSort()
        {
            return
                this.Filter.ProductSortFilter.ProductSortType == CategoryPublicFilterModel_ProductSort.ProductSort_PriceAsc
                ||
                this.Filter.ProductSortFilter.ProductSortType == CategoryPublicFilterModel_ProductSort.ProductSort_PriceDesc;
        }
        bool IsRecomendationSort()
        {
            return
                this.Filter.ProductSortFilter.ProductSortType == CategoryPublicFilterModel_ProductSort.ProductSort_Recomendation;
        }
        string GetSortField()
        {
            switch (this.Filter.ProductSortFilter.ProductSortType)
            {
                case CategoryPublicFilterModel_ProductSort.ProductSort_Newest:
                    return "ProductOrder";
                case CategoryPublicFilterModel_ProductSort.ProductSort_NameAsc:
                    return "ProductName";
                case CategoryPublicFilterModel_ProductSort.ProductSort_NameDesc:
                    return "ProductName";
            }

            return string.Empty;
        }
        string GetSortDirection()
        {
            switch (this.Filter.ProductSortFilter.ProductSortType)
            {
                case CategoryPublicFilterModel_ProductSort.ProductSort_Recomendation:
                    return "ASC";
                case CategoryPublicFilterModel_ProductSort.ProductSort_Newest:
                    return "DESC";
                case CategoryPublicFilterModel_ProductSort.ProductSort_NameAsc:
                    return "ASC";
                case CategoryPublicFilterModel_ProductSort.ProductSort_NameDesc:
                    return "DESC";
                case CategoryPublicFilterModel_ProductSort.ProductSort_PriceAsc:
                    return "ASC";
                case CategoryPublicFilterModel_ProductSort.ProductSort_PriceDesc:
                    return "DESC";
            }

            return string.Empty;
        }

        GridPagerModel GetProductsPager()
        {
            if (this.productsPager == null || this.productsPager.ItemCnt != this.Products.TotalItems)
            {
                NameValueCollection queryParams = new NameValueCollection();
                //queryParams.Add(PublicSupplierDetailModel.ActiveTabParam, PublicSupplierDetailModel.ActiveTabValue_Products);
                this.productsPager =
                    new GridPagerModel(this.CurrentRequest, this.Products.TotalItems, this.ProductsPageSize, queryParams,
                        queryPageParam: CategoryPublicModel.cPagerParamName, showPrevNext: true);
            }

            return this.productsPager;
        }
    }

    public class CategoryPublicFilterModel
    {
        public CategoryPublicFilterModel_PageSize PageSizeFilter { get; private set; }
        public CategoryPublicFilterModel_ProductView ProductViewFilter { get; private set; }
        public CategoryPublicFilterModel_ProductSort ProductSortFilter { get; private set; }


        public CategoryPublicFilterModel_CurrentCategory CurrentCategory { get; private set; }
        public CategoryPublicFilterModel_Producers ProducersFilter { get; private set; }
        public CategoryPublicFilterModel_ProductAttributes ProductAttributesFilter { get; private set; }

        public void LoadFilters(string sessionId, string categoryKey)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();

            LoadPageSize(repository, sessionId);
            LoadProductView(repository, sessionId);
            LoadProductSort(repository, sessionId);

            LoadCurrentCategory(repository, sessionId);
            if (this.CurrentCategory.CategoryKey != categoryKey)
            {
                this.CurrentCategory.CategoryKey = categoryKey;
                repository.Save(sessionId, CategoryPublicFilterModel_CurrentCategory.CreateCopyFrom(this.CurrentCategory));
                repository.Delete(sessionId, new CategoryPublicFilterModel_Producers().PropId);
                repository.Delete(sessionId, new CategoryPublicFilterModel_ProductAttributes().PropId);
            }

            LoadProducers(repository, sessionId);
            LoadProductAttributes(repository, sessionId);
        }


        public void SetPageSize(string sessionId, int pageSize)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadPageSize(repository, sessionId);
            this.PageSizeFilter.PageSize = pageSize;
            repository.Save(sessionId, CategoryPublicFilterModel_PageSize.CreateCopyFrom(this.PageSizeFilter));
        }
        void LoadPageSize(EshoppgsoftwebUserPropRepository repository, string sessionId)
        {
            this.PageSizeFilter = CategoryPublicFilterModel_PageSize.CreateCopyFrom(repository.Get(sessionId, new CategoryPublicFilterModel_PageSize().PropId));
        }

        public void SetProductView(string sessionId, int productViewType)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProductView(repository, sessionId);
            this.ProductViewFilter.ProductViewType = productViewType;
            repository.Save(sessionId, CategoryPublicFilterModel_ProductView.CreateCopyFrom(this.ProductViewFilter));
        }
        void LoadProductView(EshoppgsoftwebUserPropRepository repository, string sessionId)
        {
            this.ProductViewFilter = CategoryPublicFilterModel_ProductView.CreateCopyFrom(repository.Get(sessionId, new CategoryPublicFilterModel_ProductView().PropId));
        }

        public void SetProductSort(string sessionId, int productSortType)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProductSort(repository, sessionId);
            this.ProductSortFilter.ProductSortType = productSortType;
            repository.Save(sessionId, CategoryPublicFilterModel_ProductSort.CreateCopyFrom(this.ProductSortFilter));
        }
        void LoadProductSort(EshoppgsoftwebUserPropRepository repository, string sessionId)
        {
            this.ProductSortFilter = CategoryPublicFilterModel_ProductSort.CreateCopyFrom(repository.Get(sessionId, new CategoryPublicFilterModel_ProductSort().PropId));
        }

        void LoadCurrentCategory(EshoppgsoftwebUserPropRepository repository, string sessionId)
        {
            this.CurrentCategory = CategoryPublicFilterModel_CurrentCategory.CreateCopyFrom(repository.Get(sessionId, new CategoryPublicFilterModel_CurrentCategory().PropId));
        }


        public void SetProducerIsSelected(string sessionId, string producerKey, bool isSelected)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProducers(repository, sessionId);
            this.ProducersFilter.SetProducerSelected(producerKey, isSelected);
            repository.Save(sessionId, CategoryPublicFilterModel_Producers.CreateCopyFrom(this.ProducersFilter));
        }
        public void SetProducersAllSelected(string sessionId, bool isSelected)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProducers(repository, sessionId);
            this.ProducersFilter.SetProducersAllSelected(isSelected);
            repository.Save(sessionId, CategoryPublicFilterModel_Producers.CreateCopyFrom(this.ProducersFilter));
        }
        void LoadProducers(EshoppgsoftwebUserPropRepository repository, string sessionId)
        {
            this.ProducersFilter = CategoryPublicFilterModel_Producers.CreateCopyFrom(repository.Get(sessionId, new CategoryPublicFilterModel_Producers().PropId));
        }

        public void SetProductAttributeIsSelected(string sessionId, string attributeKey, bool isSelected)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProductAttributes(repository, sessionId);
            this.ProductAttributesFilter.SetProductAttributeSelected(attributeKey, isSelected);
            repository.Save(sessionId, CategoryPublicFilterModel_ProductAttributes.CreateCopyFrom(this.ProductAttributesFilter));
        }
        public void SetProductAttributesAllSelected(string sessionId, bool isSelected)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProductAttributes(repository, sessionId);
            this.ProductAttributesFilter.SetProductAttributesAllSelected(isSelected);
            repository.Save(sessionId, CategoryPublicFilterModel_ProductAttributes.CreateCopyFrom(this.ProductAttributesFilter));
        }
        public void SetProductAttributesSelected(string sessionId, List<string> attrKeys)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            LoadProductAttributes(repository, sessionId);
            this.ProductAttributesFilter.SetProductAttributesAllSelected(false);
            foreach (string attrKey in attrKeys)
            {
                this.ProductAttributesFilter.SetProductAttributeSelected(attrKey, true);
            }
            repository.Save(sessionId, CategoryPublicFilterModel_ProductAttributes.CreateCopyFrom(this.ProductAttributesFilter));
        }
        void LoadProductAttributes(EshoppgsoftwebUserPropRepository repository, string sessionId)
        {
            this.ProductAttributesFilter = CategoryPublicFilterModel_ProductAttributes.CreateCopyFrom(repository.Get(sessionId, new CategoryPublicFilterModel_ProductAttributes().PropId));
        }
    }

    public class CategoryPublicFilterModel_PageSize : _BaseUserPropModel
    {
        public const int PageSize_Size_1 = 20;
        public const int PageSize_Size_2 = 40;
        public const int PageSize_Size_All = _PagingModel.AllItemsPerPage;

        public int PageSize { get; set; }

        public CategoryPublicFilterModel_PageSize()
        {
            this.PropId = ConfigurationUtil.PropId_CategoryPublicFilterModel_PageSize;
            this.PageSize = CategoryPublicFilterModel_PageSize.PageSize_Size_1;
        }

        public static CategoryPublicFilterModel_PageSize CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CategoryPublicFilterModel_PageSize trg = new CategoryPublicFilterModel_PageSize();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CategoryPublicFilterModel_PageSize src)
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
            XmlElement mainNode = doc.CreateElement("CategoryPublicFilterModel_PageSize");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveIntItem(doc, mainNode, "PageSize", this.PageSize);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CategoryPublicFilterModel_PageSize";

                this.PageSize = XmlParamSet.LoadIntItem(doc, fullParent, "PageSize", CategoryPublicFilterModel_PageSize.PageSize_Size_1);
            }
        }
    }

    public class CategoryPublicFilterModel_ProductView : _BaseUserPropModel
    {
        public const int ProductView_Image = 0;
        public const int ProductView_List = 1;

        public int ProductViewType { get; set; }

        public CategoryPublicFilterModel_ProductView()
        {
            this.PropId = ConfigurationUtil.PropId_CategoryPublicFilterModel_ProductView;
            this.ProductViewType = CategoryPublicFilterModel_ProductView.ProductView_Image;
        }

        public static CategoryPublicFilterModel_ProductView CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CategoryPublicFilterModel_ProductView trg = new CategoryPublicFilterModel_ProductView();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CategoryPublicFilterModel_ProductView src)
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
            XmlElement mainNode = doc.CreateElement("CategoryPublicFilterModel_ProductView");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveIntItem(doc, mainNode, "ProductViewType", this.ProductViewType);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CategoryPublicFilterModel_ProductView";

                this.ProductViewType = XmlParamSet.LoadIntItem(doc, fullParent, "ProductViewType", CategoryPublicFilterModel_ProductView.ProductView_Image);
            }
        }
    }
    public class CategoryPublicFilterModel_ProductViewDropDown : CmpDropDown
    {
        public CategoryPublicFilterModel_ProductViewDropDown()
        {
        }

        public static CategoryPublicFilterModel_ProductViewDropDown CreateFromRepository()
        {
            CategoryPublicFilterModel_ProductViewDropDown ret = new CategoryPublicFilterModel_ProductViewDropDown();

            AddItem(ret, CategoryPublicFilterModel_ProductView.ProductView_Image);
            AddItem(ret, CategoryPublicFilterModel_ProductView.ProductView_List);

            return ret;
        }

        static void AddItem(CategoryPublicFilterModel_ProductViewDropDown ret, int viewType)
        {
            ret.AddItem(GetViewTypeName(viewType), viewType.ToString(), null);
        }

        public static string GetViewTypeName(int viewType)
        {
            switch (viewType)
            {
                case CategoryPublicFilterModel_ProductView.ProductView_Image:
                    return "Obrázkové";
                case CategoryPublicFilterModel_ProductView.ProductView_List:
                    return "Zoznam";
            }

            return string.Empty;
        }
    }

    public class CategoryPublicFilterModel_ProductSort : _BaseUserPropModel
    {
        public const int ProductSort_NameAsc = 0;
        public const int ProductSort_NameDesc = 1;
        public const int ProductSort_PriceAsc = 2;
        public const int ProductSort_PriceDesc = 3;
        public const int ProductSort_Newest = 4;
        public const int ProductSort_Recomendation = 5;

        public int ProductSortType { get; set; }

        public CategoryPublicFilterModel_ProductSort()
        {
            this.PropId = ConfigurationUtil.PropId_CategoryPublicFilterModel_ProductSort;
            this.ProductSortType = CategoryPublicFilterModel_ProductSort.ProductSort_Recomendation;
        }

        public static CategoryPublicFilterModel_ProductSort CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CategoryPublicFilterModel_ProductSort trg = new CategoryPublicFilterModel_ProductSort();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CategoryPublicFilterModel_ProductSort src)
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
            XmlElement mainNode = doc.CreateElement("CategoryPublicFilterModel_ProductSort");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveIntItem(doc, mainNode, "ProductSortType", this.ProductSortType);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CategoryPublicFilterModel_ProductSort";

                this.ProductSortType = XmlParamSet.LoadIntItem(doc, fullParent, "ProductSortType", CategoryPublicFilterModel_ProductSort.ProductSort_Recomendation);
            }
        }
    }
    public class CategoryPublicFilterModel_ProductSortDropDown : CmpDropDown
    {
        public CategoryPublicFilterModel_ProductSortDropDown()
        {
        }

        public static CategoryPublicFilterModel_ProductSortDropDown CreateFromRepository()
        {
            CategoryPublicFilterModel_ProductSortDropDown ret = new CategoryPublicFilterModel_ProductSortDropDown();

            AddItem(ret, CategoryPublicFilterModel_ProductSort.ProductSort_Recomendation);
            AddItem(ret, CategoryPublicFilterModel_ProductSort.ProductSort_Newest);
            AddItem(ret, CategoryPublicFilterModel_ProductSort.ProductSort_NameAsc);
            AddItem(ret, CategoryPublicFilterModel_ProductSort.ProductSort_NameDesc);
            AddItem(ret, CategoryPublicFilterModel_ProductSort.ProductSort_PriceAsc);
            AddItem(ret, CategoryPublicFilterModel_ProductSort.ProductSort_PriceDesc);

            return ret;
        }

        static void AddItem(CategoryPublicFilterModel_ProductSortDropDown ret, int viewType)
        {
            ret.AddItem(GetViewTypeName(viewType), viewType.ToString(), null);
        }

        public static string GetViewTypeName(int viewType)
        {
            switch (viewType)
            {
                case CategoryPublicFilterModel_ProductSort.ProductSort_Recomendation:
                    return "Odporúčané";
                case CategoryPublicFilterModel_ProductSort.ProductSort_Newest:
                    return "Od najnovšieho";
                case CategoryPublicFilterModel_ProductSort.ProductSort_NameAsc:
                    return "Od A po Z";
                case CategoryPublicFilterModel_ProductSort.ProductSort_NameDesc:
                    return "Od Z po A";
                case CategoryPublicFilterModel_ProductSort.ProductSort_PriceAsc:
                    return "Od najlacnejšieho";
                case CategoryPublicFilterModel_ProductSort.ProductSort_PriceDesc:
                    return "Od najdrahšieho";
            }

            return string.Empty;
        }
    }

    public class CategoryPublicFilterModel_CurrentCategory : _BaseUserPropModel
    {
        public string CategoryKey { get; set; }

        public CategoryPublicFilterModel_CurrentCategory()
        {
            this.PropId = ConfigurationUtil.PropId_CategoryPublicFilterModel_CurrentCategory;
        }

        public static CategoryPublicFilterModel_CurrentCategory CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CategoryPublicFilterModel_CurrentCategory trg = new CategoryPublicFilterModel_CurrentCategory();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CategoryPublicFilterModel_CurrentCategory src)
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
            XmlElement mainNode = doc.CreateElement("CategoryPublicFilterModel_CurrentCategory");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveItem(doc, mainNode, "CategoryKey", this.CategoryKey);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CategoryPublicFilterModel_CurrentCategory";
                this.CategoryKey = XmlParamSet.LoadItem(doc, fullParent, "CategoryKey", null);
            }
        }
    }

    public class CategoryPublicFilterModel_Producers : _BaseUserPropModel
    {
        public List<CategoryPublicFilterModel_ProducerItem> Producers { get; set; }
        Hashtable htProducers;
        Hashtable htEnabledProducers;

        public CategoryPublicFilterModel_Producers()
        {
            this.PropId = ConfigurationUtil.PropId_CategoryPublicFilterModel_Producer;
        }

        void LoadProducers()
        {
            this.Producers = new List<CategoryPublicFilterModel_ProducerItem>();
            this.htProducers = new Hashtable();
            this.htEnabledProducers = new Hashtable();

            EshoppgsoftwebProducerRepository repository = new EshoppgsoftwebProducerRepository();
            foreach (EshoppgsoftwebProducer producer in repository.GetPage(1, _PagingModel.AllItemsPerPage).Items)
            {
                CategoryPublicFilterModel_ProducerItem item = new CategoryPublicFilterModel_ProducerItem()
                {
                    ProducerData = ProducerModel.CreateCopyFrom(producer),
                    IsChecked = false
                };
                string producerKey = item.ProducerData.pk.ToString();
                this.htProducers.Add(producerKey, item);
                this.htEnabledProducers.Add(producerKey, null);
                this.Producers.Add(item);
            }
        }

        public bool IsProducerEnabled(string producerKey)
        {
            return this.htEnabledProducers.ContainsKey(producerKey);
        }
        public void SetEnabledProducersForProductCategories(List<string> productCategoryKeyList)
        {
            this.htEnabledProducers.Clear();
            foreach (Guid producerKey in new EshoppgsoftwebProductRepository().GetProducerKeysForProductCategories(productCategoryKeyList))
            {
                this.htEnabledProducers.Add(producerKey.ToString(), null);
            }
        }

        public void SetProducerSelected(string producerKey, bool isSelected)
        {
            ((CategoryPublicFilterModel_ProducerItem)this.htProducers[producerKey]).IsChecked = isSelected;
        }
        public void SetProducersAllSelected(bool isSelected)
        {
            foreach (CategoryPublicFilterModel_ProducerItem item in this.Producers)
            {
                item.IsChecked = isSelected;
            }
        }

        List<CategoryPublicFilterModel_ProducerItem> GetEnabledProducers()
        {
            List<CategoryPublicFilterModel_ProducerItem> ret = new List<CategoryPublicFilterModel_ProducerItem>(this.Producers.Count + 1);
            foreach (CategoryPublicFilterModel_ProducerItem producerItem in this.Producers)
            {
                if (IsProducerEnabled(producerItem.ProducerData.pk.ToString()))
                {
                    ret.Add(producerItem);
                }
            }

            return ret;
        }
        public List<string> GetSelectedProducers(bool onlyEnabledProducers)
        {
            List<CategoryPublicFilterModel_ProducerItem> producerList = onlyEnabledProducers ? GetEnabledProducers() : this.Producers;
            List<string> selectedProducers = new List<string>();
            foreach (CategoryPublicFilterModel_ProducerItem producerItem in producerList)
            {
                if (producerItem.IsChecked)
                {
                    selectedProducers.Add(producerItem.ProducerData.pk.ToString());
                }
            }
            if (selectedProducers.Count == 0)
            {
                //No producer selected
                //Add fake producer key
                //The result can't be empty
                selectedProducers.Add(Guid.Empty.ToString());
            }

            return selectedProducers;
        }
        public bool AllProducersSelected(bool onlyEnabledProducers)
        {
            List<CategoryPublicFilterModel_ProducerItem> producerList = onlyEnabledProducers ? GetEnabledProducers() : this.Producers;
            if (producerList.Count > 0)
            {
                bool firstChecked = producerList[0].IsChecked;
                foreach (CategoryPublicFilterModel_ProducerItem producerItem in producerList)
                {
                    if (producerItem.IsChecked != firstChecked)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static CategoryPublicFilterModel_Producers CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CategoryPublicFilterModel_Producers trg = new CategoryPublicFilterModel_Producers();
            trg.LoadProducers();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CategoryPublicFilterModel_Producers src)
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
            XmlElement mainNode = doc.CreateElement("CategoryPublicFilterModel_Producer");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveIntItem(doc, mainNode, "Count", this.Producers.Count);
            for (int i = 0; i < this.Producers.Count; i++)
            {
                if (this.Producers[i].IsChecked)
                {
                    // Save only not selected producers
                    XmlParamSet.SaveItem(doc, mainNode, string.Format("Selected{0}", i), this.Producers[i].ProducerData.pk.ToString());
                }
            }

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CategoryPublicFilterModel_Producer";
                int count = XmlParamSet.LoadIntItem(doc, fullParent, "Count", 0);
                for (int i = 0; i < count; i++)
                {
                    string producerKey = XmlParamSet.LoadItem(doc, fullParent, string.Format("Selected{0}", i), null);
                    if (!string.IsNullOrEmpty(producerKey))
                    {
                        SetProducerSelected(producerKey, true);
                    }
                }
            }
        }
    }
    public class CategoryPublicFilterModel_ProducerItem
    {
        public ProducerModel ProducerData { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CategoryPublicFilterModel_ProductAttributes : _BaseUserPropModel
    {
        public List<CategoryPublicFilterModel_ProductAttributeItem> ProductAttributes { get; set; }
        Hashtable htProductAttributes;
        Hashtable htEnabledAttributes;

        public CategoryPublicFilterModel_ProductAttributes()
        {
            this.PropId = ConfigurationUtil.PropId_CategoryPublicFilterModel_ProductAttribute;
        }

        void LoadProductAttributes()
        {
            this.ProductAttributes = new List<CategoryPublicFilterModel_ProductAttributeItem>();
            this.htProductAttributes = new Hashtable();
            this.htEnabledAttributes = new Hashtable();

            EshoppgsoftwebProductAttributeRepository repository = new EshoppgsoftwebProductAttributeRepository();
            foreach (EshoppgsoftwebProductAttribute producer in repository.GetPage(1, _PagingModel.AllItemsPerPage).Items)
            {
                CategoryPublicFilterModel_ProductAttributeItem item = new CategoryPublicFilterModel_ProductAttributeItem()
                {
                    ProductAttributeData = ProductAttributeModel.CreateCopyFrom(producer),
                    IsChecked = false
                };
                string attributeKey = item.ProductAttributeData.pk.ToString();
                this.htProductAttributes.Add(attributeKey, item);
                this.htEnabledAttributes.Add(attributeKey, null);
                this.ProductAttributes.Add(item);
            }
        }

        public List<List<CategoryPublicFilterModel_ProductAttributeItem>> GetAttributesInGroups(bool onlyEnabled)
        {
            List<List<CategoryPublicFilterModel_ProductAttributeItem>> ret = new List<List<CategoryPublicFilterModel_ProductAttributeItem>>();
            Hashtable htGroup = new Hashtable();
            foreach (CategoryPublicFilterModel_ProductAttributeItem attr in this.ProductAttributes)
            {
                if (onlyEnabled)
                {
                    if (!IsAttributeEnabled(attr.ProductAttributeData.pk.ToString()))
                    {
                        // do not get this attribute
                        continue;
                    }
                }
                if (!htGroup.ContainsKey(attr.ProductAttributeData.ProductAttributeGroup))
                {
                    ret.Add(new List<CategoryPublicFilterModel_ProductAttributeItem>());
                    htGroup.Add(attr.ProductAttributeData.ProductAttributeGroup, ret[ret.Count - 1]);
                }
                ((List<CategoryPublicFilterModel_ProductAttributeItem>)htGroup[attr.ProductAttributeData.ProductAttributeGroup]).Add(attr);
            }

            return ret;
        }

        public bool IsAttributeEnabled(string attributeKey)
        {
            return this.htEnabledAttributes.ContainsKey(attributeKey);
        }
        public void SetEnabledAttributesForProductCategories(List<string> productCategoryKeyList)
        {
            this.htEnabledAttributes.Clear();
            foreach (Guid attributeKey in new EshoppgsoftwebProduct2AttributeRepository().GetProductAttributeKeysForProductCategories(productCategoryKeyList))
            {
                this.htEnabledAttributes.Add(attributeKey.ToString(), null);
            }
        }

        public void SetProductAttributeSelected(string attrKey, bool isSelected)
        {
            if (this.htProductAttributes.ContainsKey(attrKey))
            {
                ((CategoryPublicFilterModel_ProductAttributeItem)this.htProductAttributes[attrKey]).IsChecked = isSelected;
            }
        }
        public void SetProductAttributesAllSelected(bool isSelected)
        {
            foreach (CategoryPublicFilterModel_ProductAttributeItem item in this.ProductAttributes)
            {
                item.IsChecked = isSelected;
            }
        }

        List<CategoryPublicFilterModel_ProductAttributeItem> GetEnabledAttributes()
        {
            List<CategoryPublicFilterModel_ProductAttributeItem> ret = new List<CategoryPublicFilterModel_ProductAttributeItem>(this.ProductAttributes.Count + 1);
            foreach (CategoryPublicFilterModel_ProductAttributeItem attributeItem in this.ProductAttributes)
            {
                if (IsAttributeEnabled(attributeItem.ProductAttributeData.pk.ToString()))
                {
                    ret.Add(attributeItem);
                }
            }

            return ret;
        }
        public List<string> GetSelectedAttributes(bool onlyEnabledAttributes)
        {
            List<CategoryPublicFilterModel_ProductAttributeItem> attributeList = onlyEnabledAttributes ? GetEnabledAttributes() : this.ProductAttributes;
            List<string> selectedAttributes = new List<string>();
            foreach (CategoryPublicFilterModel_ProductAttributeItem attributeItem in attributeList)
            {
                if (attributeItem.IsChecked)
                {
                    selectedAttributes.Add(attributeItem.ProductAttributeData.pk.ToString());
                }
            }
            if (selectedAttributes.Count == 0)
            {
                // No attribute selected
                // Add fake attribute key
                // The result can't be empty
                selectedAttributes.Add(Guid.Empty.ToString());
            }

            return selectedAttributes;
        }

        public bool AllAttributesSelected(bool onlyEnabledAttributes)
        {
            List<CategoryPublicFilterModel_ProductAttributeItem> attributeList = onlyEnabledAttributes ? GetEnabledAttributes() : this.ProductAttributes;
            if (attributeList.Count > 0)
            {
                bool firstChecked = attributeList[0].IsChecked;
                foreach (CategoryPublicFilterModel_ProductAttributeItem attributeItem in attributeList)
                {
                    if (attributeItem.IsChecked != firstChecked)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static CategoryPublicFilterModel_ProductAttributes CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CategoryPublicFilterModel_ProductAttributes trg = new CategoryPublicFilterModel_ProductAttributes();
            trg.LoadProductAttributes();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CategoryPublicFilterModel_ProductAttributes src)
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
            XmlElement mainNode = doc.CreateElement("CategoryPublicFilterModel_ProductAttribute");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveIntItem(doc, mainNode, "Count", this.ProductAttributes.Count);
            for (int i = 0; i < this.ProductAttributes.Count; i++)
            {
                if (this.ProductAttributes[i].IsChecked)
                {
                    // Save only not selected attributes
                    XmlParamSet.SaveItem(doc, mainNode, string.Format("Selected{0}", i), this.ProductAttributes[i].ProductAttributeData.pk.ToString());
                }
            }

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CategoryPublicFilterModel_ProductAttribute";
                int count = XmlParamSet.LoadIntItem(doc, fullParent, "Count", 0);
                for (int i = 0; i < count; i++)
                {
                    string producerKey = XmlParamSet.LoadItem(doc, fullParent, string.Format("Selected{0}", i), null);
                    if (!string.IsNullOrEmpty(producerKey))
                    {
                        SetProductAttributeSelected(producerKey, true);
                    }
                }
            }
        }
    }
    public class CategoryPublicFilterModel_ProductAttributeItem
    {
        public ProductAttributeModel ProductAttributeData { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CategoryPromoModel
    {
        public enum PromoType
        {
            Discounted = 0,
            Bestseller = 1,
            CategoryCode = 1,
        }

        public int MaxProductCnt = 4;

        public string Title { get; set; }
        public string SessionId { get; set; }
        public PromoType Type { get; set; }
        public List<ProductModel> Products { get; set; }
        public CategoryModel Category { get; set; }

        public CategoryPromoModel(string sessionId, PromoType type)
        {
            this.SessionId = sessionId;
            this.Type = type;

            List<ProductModel> dataList = null;
            switch (this.Type)
            {
                case PromoType.Bestseller:
                    dataList = GetAllProductsBestseller();
                    break;
                case PromoType.Discounted:
                    dataList = GetAllProductsDiscounted();
                    break;
            }

            List<int> idxList = RandomUtil.GetIntList(1, dataList.Count, MaxProductCnt);

            this.Products = new List<ProductModel>();
            foreach (int idx in idxList)
            {
                this.Products.Add(dataList[idx - 1]);
            }
        }

        public CategoryPromoModel(string sessionId, string categoryCode, int maxCnt)
        {
            this.SessionId = sessionId;
            this.Type = PromoType.CategoryCode;

            this.Products = new List<ProductModel>();
            EshoppgsoftwebCategory category = new EshoppgsoftwebCategoryRepository().GetForCategoryCode(categoryCode);
            if (category != null)
            {
                this.Category = CategoryModel.CreateCopyFrom(category);
                List<ProductModel> dataList = GetAllProductsForCategory(category);
                List<int> idxList = RandomUtil.GetIntList(1, dataList.Count, maxCnt);
                foreach (int idx in idxList)
                {
                    this.Products.Add(dataList[idx - 1]);
                }
            }
        }

        List<ProductModel> GetAllProductsDiscounted()
        {
            EshoppgsoftwebProductRepository rep = new EshoppgsoftwebProductRepository();
            ProductPagingListModel dataList = ProductPagingListModel.CreateCopyFrom(
                rep.GetPage(1, _PagingModel.AllItemsPerPage, filter: new EshoppgsoftwebProductFilter() { OnlyIsVisible = true }),
                new ProductModelDropDowns(),
                loadPrices: true);

            List<ProductModel> discountedList = new List<ProductModel>();
            foreach (ProductModel product in dataList.Items)
            {
                if (product.IsDiscount())
                {
                    discountedList.Add(product);
                }
            }

            return discountedList;
        }

        List<ProductModel> GetAllProductsBestseller()
        {
            Product2QuoteRepository p2qRep = new Product2QuoteRepository();
            Page<Product2QuotePcs> bestList = p2qRep.GetProduct2QuotePcsPage(1, MaxProductCnt);
            if (bestList.Items.Count <= 0)
            {
                return new List<ProductModel>();
            }

            List<string> productKeyList = new List<string>();
            foreach (Product2QuotePcs p2qPcs in bestList.Items)
            {
                productKeyList.Add(p2qPcs.PkProduct.ToString());
            }
            EshoppgsoftwebProductRepository rep = new EshoppgsoftwebProductRepository();
            ProductPagingListModel dataList = ProductPagingListModel.CreateCopyFrom(
                rep.GetPage(1, _PagingModel.AllItemsPerPage,
                filter: new EshoppgsoftwebProductFilter() { OnlyIsVisible = true, ProductKeyList = productKeyList }),
                new ProductModelDropDowns(),
                loadPrices: true);

            return dataList.Items;
        }

        List<ProductModel> GetAllProductsForCategory(EshoppgsoftwebCategory category)
        {
            List<string> categoryKeyList = new List<string>();
            categoryKeyList.Add(category.pk.ToString());
            EshoppgsoftwebProductRepository rep = new EshoppgsoftwebProductRepository();
            ProductPagingListModel dataList = ProductPagingListModel.CreateCopyFrom(
                rep.GetPage(1, _PagingModel.AllItemsPerPage,
                filter: new EshoppgsoftwebProductFilter() { OnlyIsVisible = true, ProductCategoryKeyList = categoryKeyList }),
                new ProductModelDropDowns(),
                loadPrices: true);

            return dataList.Items;
        }

        public string GetTypeName()
        {
            switch (this.Type)
            {
                case PromoType.Bestseller:
                    return "bestseller";
                case PromoType.Discounted:
                    return "discounted";
            }

            return string.Empty;
        }
    }
}
