using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class Product2CategoryController : _BaseCategoryController
    {
        #region Products in category
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecordsInCategory(string id, int page = 1, string sort = "ProductOrder", string sortDir = "ASC")
        {
            try
            {
                return GetRecordsInCategoryView(id, page, sort, sortDir);
            }
            catch
            {
                ProductInCategoryFilterModel filter = GetEshoppgsoftwebProductInCategoryFilterForEdit(id);
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProductInCategoryFilterModel.CreateCopyFrom(filter));
                }

                return GetRecordsInCategoryView(id, page, sort, sortDir);
            }
        }
        ActionResult GetRecordsInCategoryView(string id, int page, string sort, string sortDir)
        {
            Guid keyCategory = new Guid(id);
            EshoppgsoftwebProduct2CategoryRepository repository = new EshoppgsoftwebProduct2CategoryRepository();

            ProductInCategoryFilterModel filter = GetEshoppgsoftwebProductInCategoryFilterForEdit(id);

            Product2CategoryItemsModel model = Product2CategoryItemsModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, keyCategory, sort, sortDir,
                    new EshoppgsoftwebProduct2CategoryFilter()
                    {
                        ProductCode = filter.ProductCode,
                        SearchText = filter.SearchText
                    })
                );
            model.CategoryKey = keyCategory;

            if (model.Items.Count > 0)
            {
                EshoppgsoftwebProductRepository prodRep = new EshoppgsoftwebProductRepository();
                model.BindProducts(
                    prodRep.GetPageForCategory(1, _PagingModel.AllItemsPerPage, model.CategoryKey).Items,
                    new ProductModelDropDowns());
            }

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetInCategoryFilter(string id)
        {
            return View(GetEshoppgsoftwebProductInCategoryFilterForEdit(id));
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveInCategoryFilter(ProductInCategoryFilterModel model)
        {
            SetReturnToCategory(model.PkCategory, _BaseCategoryController.ReturnToTabVal_ProdInCat);

            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, ProductInCategoryFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecordInCategoryFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveRecordInCategoryFilter();
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordInCategoryFilter(ProductInCategoryFilterModel rec = null)
        {
            SetEshoppgsoftwebProductInCategoryFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage(GetReturnToCategoryQueryString());
        }
        void SetEshoppgsoftwebProductInCategoryFilterForEdit(ProductInCategoryFilterModel rec = null)
        {
            TempData["naplnspajzuProductInCategoryFilterForEdit"] = rec;
        }
        ProductInCategoryFilterModel GetEshoppgsoftwebProductInCategoryFilterForEdit(string id)
        {
            if (TempData["naplnspajzuProductInCategoryFilterForEdit"] == null)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                TempData["naplnspajzuProductInCategoryFilterForEdit"] = ProductInCategoryFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProductInCategoryFilterModel));
            }

            ProductInCategoryFilterModel ret = (ProductInCategoryFilterModel)TempData["naplnspajzuProductInCategoryFilterForEdit"];
            ret.PkCategory = string.IsNullOrEmpty(id) ? Guid.Empty : new Guid(id);

            return ret;
        }
        #endregion

        #region Products not in category
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecordsNotInCategory(string id, int page = 1, string sort = "ProductOrder", string sortDir = "ASC")
        {
            try
            {
                return GetRecordsNotInCategoryView(id, page, sort, sortDir);
            }
            catch
            {
                ProductNotInCategoryFilterModel filter = GetEshoppgsoftwebProductNotInCategoryFilterForEdit(id);
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProductNotInCategoryFilterModel.CreateCopyFrom(filter));
                }

                return GetRecordsNotInCategoryView(id, page, sort, sortDir);
            }
        }
        ActionResult GetRecordsNotInCategoryView(string id, int page, string sort, string sortDir)
        {
            Guid keyCategory = new Guid(id);
            EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();

            ProductNotInCategoryFilterModel filter = GetEshoppgsoftwebProductNotInCategoryFilterForEdit(id);

            Product2CategoryItemsModel model = Product2CategoryItemsModel.CreateCopyFrom(keyCategory,
                repository.GetPageForNotInCategory(keyCategory, page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshoppgsoftwebProduct2CategoryFilter()
                    {
                        ProductCode = filter.ProductCode,
                        SearchText = filter.SearchText
                    }),
                new ProductModelDropDowns()
                );
            model.CategoryKey = keyCategory;

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetNotInCategoryFilter(string id)
        {
            return View(GetEshoppgsoftwebProductNotInCategoryFilterForEdit(id));
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveNotInCategoryFilter(ProductNotInCategoryFilterModel model)
        {
            SetReturnToCategory(model.PkCategory, _BaseCategoryController.ReturnToTabVal_ProdNotInCat);

            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, ProductNotInCategoryFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecordNotInCategoryFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveRecordNotInCategoryFilter();
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordNotInCategoryFilter(ProductNotInCategoryFilterModel rec = null)
        {
            SetEshoppgsoftwebProductNotInCategoryFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage(GetReturnToCategoryQueryString());
        }
        void SetEshoppgsoftwebProductNotInCategoryFilterForEdit(ProductNotInCategoryFilterModel rec = null)
        {
            TempData["naplnspajzuProductNotInCategoryFilterForEdit"] = rec;
        }
        ProductNotInCategoryFilterModel GetEshoppgsoftwebProductNotInCategoryFilterForEdit(string id)
        {
            if (TempData["naplnspajzuProductNotInCategoryFilterForEdit"] == null)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                TempData["naplnspajzuProductNotInCategoryFilterForEdit"] = ProductNotInCategoryFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProductNotInCategoryFilterModel));
            }

            ProductNotInCategoryFilterModel ret = (ProductNotInCategoryFilterModel)TempData["naplnspajzuProductNotInCategoryFilterForEdit"];
            ret.PkCategory = string.IsNullOrEmpty(id) ? Guid.Empty : new Guid(id);

            return ret;
        }
        #endregion


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertProduct(string id, string prodid)
        {
            SetReturnToCategory(new Guid(id), _BaseCategoryController.ReturnToTabVal_ProdNotInCat);

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceCategoriesFormId, GetReturnToCategoryQueryString());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult DeleteProduct(string id, string prodid)
        {
            SetReturnToCategory(new Guid(id), _BaseCategoryController.ReturnToTabVal_ProdInCat);

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceCategoriesFormId, GetReturnToCategoryQueryString());
        }
    }
}
