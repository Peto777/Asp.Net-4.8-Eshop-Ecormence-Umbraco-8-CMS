using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class CategoryPublicController : _BaseController
    {
        public ActionResult PromotedCategory(string categoryCode)
        {
            CategoryModel model = CategoryModel.CreateCopyFrom(new EshoppgsoftwebCategoryRepository().GetForCategoryCode(categoryCode));

            return View(model);
        }

        public ActionResult QuickOrder(string categoryCode)
        {
            new CategoryPublicFilterModel().SetProductView(this.CurrentSessionId, CategoryPublicFilterModel_ProductView.ProductView_List);

            return Redirect(CategoryModel.CreateCopyFrom(EshoppgsoftwebCategory.RootEshoppgsoftwebCategory()).Url);
        }

        public ActionResult CategoryDetail()
        {
            if (this.CurrentRequest.Params[CategoryPublicModel.cQuickOrderParamName] == "1")
            {
                // Switch to list view
                new CategoryPublicFilterModel().SetProductView(this.CurrentSessionId, CategoryPublicFilterModel_ProductView.ProductView_List);
            }
            // Load category data
            CategoryPublicModel model = new CategoryPublicModel(this, null);
            // Set current category data for eshop model
            this.SetCurrentProductCategoryModel(model);
            // Set SEO data
            if (model.SeoData != null)
            {
                this.SetSeoModel(model.SeoData);
            }
            // Set eshop global data
            model.EshopData = this.GetCurrentEshopModel();

            return View(model);
        }

        public ActionResult GetProductsFavorite()
        {
            ProductSearchModel searchModel = new ProductSearchModel()
            {
                CustomerKey = CustomerModel.IsUserAuthenticated() ? CustomerModel.GetCurrentCustomer().pk : Guid.Empty,
                Action = ProductSearchModel.ModelType.Favorite
            };
            CategoryPublicModel model = new CategoryPublicModel(this, searchModel);
            // Set eshop global data
            model.EshopData = this.GetCurrentEshopModel();

            return View("CategoryDetail", model);
        }

        public ActionResult GetProductsForSearch()
        {
            ProductSearchModel searchModel = new ProductSearchModel()
            {
                //ProductToSearch = this.CurrentRequest.Params["srchprod"]
                // Don't do any search now
                ProductToSearch = null,
                Action = ProductSearchModel.ModelType.Search
            };
            CategoryPublicModel model = new CategoryPublicModel(this, searchModel);
            // Set eshop global data
            model.EshopData = this.GetCurrentEshopModel();

            return View("CategoryDetail", model);
        }

        public ActionResult GetProductsSearchResult(string id)
        {
            ProductSearchModel searchModel = new ProductSearchModel()
            {
                ProductToSearch = id,
                Action = ProductSearchModel.ModelType.Search,
            };
            CategoryPublicModel model = new CategoryPublicModel(this, searchModel);

            return View(model);
        }

        public ActionResult GetProductsDiscounted()
        {
            CategoryPromoModel model = new CategoryPromoModel(this.CurrentSessionId, CategoryPromoModel.PromoType.Discounted);

            return View("CategoryPromo", model);
        }

        public ActionResult GetProductsBestseller()
        {
            CategoryPromoModel model = new CategoryPromoModel(this.CurrentSessionId, CategoryPromoModel.PromoType.Bestseller);
            model.Title = "Obľúbené produkty";

            return View("CategoryPromo", model);
        }

        public ActionResult GetProductsRecomended()
        {
            CategoryPromoModel model = new CategoryPromoModel(this.CurrentSessionId, "ODPORUCANE", 8);
            model.Title = "Odporúčané produkty";

            return View("CategoryPromo", model);
        }

        public ActionResult GetProductsPromoted(string id)
        {
            CategoryPromoModel model = new CategoryPromoModel(this.CurrentSessionId, id, 4);
            model.Title = model.Category.CategoryName;

            return View("CategoryPromo", model);
        }

        public ActionResult GetCategoriesPromoted(string id)
        {
            CategoryModel model = GetCurrentEshopModel().CategoryTreeData.GetCategoryNode(id);

            return View(model);
        }

        public ActionResult HomeCategoryBanners()
        {
            return View(GetCurrentEshopModel().CategoryTreeData);
        }
    }
}
