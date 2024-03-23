using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Models
{
    public class _BaseModel
    {
        [Display(Name = "PK")]
        public Guid pk { get; set; }
        public List<string> ModelErrors = new List<string>();

        public _BaseModel()
        {
        }

        public virtual bool IsNew
        {
            get
            {
                return pk == null || pk == Guid.Empty;
            }
        }
    }

    public class _BaseUserPropModel : _BaseModel
    {
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public string PropId { get; set; }
        public string PropValue { get; set; }

        public _BaseUserPropModel()
        {
        }

        public void CopyDataFrom(EshoppgsoftwebUserProp src)
        {
            this.pk = src.pk;
            this.UserId = src.UserId;
            this.SessionId = src.SessionId;
            this.PropId = src.PropId;

            LoadProperties(src.PropValue);
        }

        public void CopyDataTo(EshoppgsoftwebUserProp trg)
        {
            trg.pk = this.pk;
            trg.UserId = this.UserId;
            trg.SessionId = this.SessionId;
            trg.PropId = this.PropId;
            trg.PropValue = SaveProperties();
        }

        protected virtual void LoadProperties(string propValue)
        {
            this.PropValue = propValue;
        }

        protected virtual string SaveProperties()
        {
            return this.PropValue;
        }
    }

    public class _PagingModel
    {
        public const int DefaultItemsPerPage = 20;
        public const int AllItemsPerPage = 100000000;

        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }

        public _PagingModel()
        {
            ItemsPerPage = DefaultItemsPerPage;
        }
    }

    public class _SeoModel
    {
        // SEO
        public const string TemDataKey = "_seo_model_";

        public string MenuTitle { get; set; }

        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }

        public string Og_Url { get; set; }
        public string Og_Type { get; set; }
        public string Og_Title { get; set; }
        public string Og_Description { get; set; }
        public string Og_Image { get; set; }
    }

    public class _EshopModel
    {
        // ESHOP
        public const string TemDataKey = "_eshop_model_";

        public CategoryTree CategoryTreeData { get; private set; }
        public CategoryPublicModel CurrentProductCategory { get; set; }

        public _EshopModel()
        {
            this.CategoryTreeData = new CategoryTree();
        }

        //public const string TabId_PckgSmall = "PckgSmall";
        //public const string TabId_PckgBig = "PckgBig";
        //public const string TabId_PckgContainers = "PckgContainers";

        //public bool IsCategoryMenuTabActive(string tabId)
        //{
        //    if (this.CurrentProductCategory == null || this.CurrentProductCategory.CategoryData.IsRoot)
        //    {
        //        // Root category page
        //        // Or no category info available (current page is not category detail page)
        //        switch (tabId)
        //        {
        //            case TabId_PckgSmall:
        //                return true;
        //            case TabId_PckgBig:
        //                return false;
        //            case TabId_PckgContainers:
        //                return false;
        //        }
        //    }

        //    CategoryModel tabCategory = null;
        //    switch (tabId)
        //    {
        //        case TabId_PckgSmall:
        //            tabCategory = this.CategoryTreeData.Root.Children[0];
        //            break;
        //        case TabId_PckgBig:
        //            tabCategory = this.CategoryTreeData.Root.Children[1];
        //            break;
        //        case TabId_PckgContainers:
        //            tabCategory = this.CategoryTreeData.Root.Children[2];
        //            break;
        //    }

        //    CategoryModel node = this.CategoryTreeData.GetCategoryNode(this.CurrentProductCategory.CategoryData.pk);
        //    for (; ; )
        //    {
        //        if (node.pk == tabCategory.pk)
        //        {
        //            return true;
        //        }
        //        node = node.Parent;
        //        if (node == null)
        //        {
        //            break;
        //        }
        //    }

        //    return false;
        //}

        //public bool IsCategorySubmenuActive(Guid categoryKey)
        //{
        //    if (this.CurrentProductCategory == null)
        //    {
        //        // No category info available
        //        // Current page is not category detail page
        //        return false;
        //    }

        //    if (categoryKey == this.CurrentProductCategory.CategoryData.pk)
        //    {
        //        // Current category is active
        //        return true;
        //    }

        //    CategoryModel node = this.CategoryTreeData.GetCategoryNode(this.CurrentProductCategory.CategoryData.pk);
        //    if (node != null && node.Parent != null)
        //    {
        //        if (categoryKey == node.Parent.pk)
        //        {
        //            // Current category is child
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }

    public class _SeoViewPage : UmbracoViewPage
    {
        public override void Execute()
        {
        }

        public _SeoModel GetCurrentSeoModel()
        {
            if (this.TempData.ContainsKey(_SeoModel.TemDataKey))
            {
                return (_SeoModel)this.TempData[_SeoModel.TemDataKey];
            }
            else
            {
                IPublishedContent model = this.Model;

                return new _SeoModel()
                {
                    MenuTitle = model.Value("menuTitle").ToString(),
                    MetaTitle = model.Value("pageTitle").ToString(),
                    MetaDescription = model.Value("metaDescription").ToString(),
                };
            }
        }
    }
    public class _EshoppgsoftwebViewPage : _SeoViewPage
    {
        public override void Execute()
        {
        }

        public _EshopModel GetCurrentEshopModel()
        {
            if (!this.TempData.ContainsKey(_EshopModel.TemDataKey))
            {
                this.TempData[_EshopModel.TemDataKey] = new _EshopModel() { CurrentProductCategory = null };
            }

            return (_EshopModel)this.TempData[_EshopModel.TemDataKey];
        }
    }
}
