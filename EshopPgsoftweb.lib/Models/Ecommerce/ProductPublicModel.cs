using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections.Generic;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class ProductPublicModel
    {
        public string SessionId { get; set; }
        public string MemberId { get; private set; }
        public ProductModel ProductData { get; set; }
        public List<ProductModel> RelatedProducts { get; set; }
        public _SeoModel SeoData { get; set; }

        public CategoryModel ProductCategory { get; set; }
        public CategoryPublicModel CategoryForFilterMenu { get; set; }
        public ProductPublicImagesModel ProductImages { get; set; }

        public ProductPublicModel(string url)
        {
            this.MemberId = EshoppgsoftwebCustomerCache.CurrentMemberId;

            EshoppgsoftwebProductRepository rep = new EshoppgsoftwebProductRepository();
            EshoppgsoftwebProduct product = rep.GetForProductUrl(url);

            if (product != null)
            {
                ProductModelDropDowns dropDowns = new ProductModelDropDowns();

                this.ProductData = ProductModel.CreateCopyFrom(product, dropDowns, loadPrice: true);

                this.SeoData = new _SeoModel();
                this.SeoData.MenuTitle = this.ProductData.ProductMetaTitle;
                this.SeoData.MetaTitle = this.ProductData.ProductMetaTitle;
                this.SeoData.MetaKeywords = this.ProductData.ProductMetaKeywords;
                this.SeoData.MetaDescription = this.ProductData.ProductMetaDescription;

                _BaseControllerUtil urlHelper = new _BaseControllerUtil();
                this.SeoData.Og_Title = this.ProductData.ProductMetaTitle;
                this.SeoData.Og_Description = this.ProductData.ProductMetaDescription;
                this.SeoData.Og_Type = "website";
                this.SeoData.Og_Url = urlHelper.CurrentRequest.Url.ToString();
                if (!string.IsNullOrEmpty(this.ProductData.ProductImg))
                {
                    this.SeoData.Og_Image = urlHelper.GetAbsoluteUrl(this.ProductData.ProductImg);
                }


                this.RelatedProducts = new List<ProductModel>();
                foreach (ProductRelationItem relationItem in ProductRelationModel.LoadItems(this.ProductData.pk))
                {
                    if (relationItem.RelatedProduct.ProductIsVisible)
                    {
                        this.RelatedProducts.Add(ProductModel.CreateCopyFrom(relationItem.RelatedProduct, dropDowns, loadPrice: true));
                    }
                }

                // Product category
                List<EshoppgsoftwebProduct2Category> p2cDataList = new EshoppgsoftwebProduct2CategoryRepository().GetForProduct(product.pk);
                if (p2cDataList != null && p2cDataList.Count > 0)
                {
                    EshoppgsoftwebCategoryRepository categoryRep = new EshoppgsoftwebCategoryRepository();
                    this.ProductCategory = CategoryModel.CreateCopyFrom(categoryRep.Get(p2cDataList[0].PkCategory));
                    this.ProductCategory.LoadRelatives(categoryRep);
                }

                // Product images
                this.ProductImages = new ProductPublicImagesModel(this.ProductData.pk);
            }
        }
    }

    public class ProductPublicPropModel
    {
        public string PropLabel { get; set; }
        public string PropValue { get; set; }
        public string CssClass { get; set; }

        public ProductPublicPropModel(string label, string value, string css = "")
        {
            this.PropLabel = label;
            this.PropValue = value;
            this.CssClass = css;
        }
    }

    public class ProductPublicImagesModel
    {
        public List<FileUploadInfo> Items { get; set; }

        public ProductPublicImagesModel(Guid productKey)
        {
            this.Items = new FileUploadRepository().GetFiles(EshoppgsoftwebProduct.GetFileUploadCategory(productKey));
        }
    }
}
