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
    public class ProductAttributeController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = null, string sortDir = null)
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                ProductAttributeFilterModel filter = GetEshoppgsoftwebProductAttributeFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProductAttributeFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            ProductAttributeFilterModel filter = GetEshoppgsoftwebProductAttributeFilterForEdit();

            EshoppgsoftwebProductAttributeRepository repository = new EshoppgsoftwebProductAttributeRepository();
            ProductAttributePagingListModel model = ProductAttributePagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshoppgsoftwebProductAttributeFilter()
                    {
                        SearchText = filter.SearchText
                    })
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new ProductAttributeModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            ProductAttributeModel model = ProductAttributeModel.CreateCopyFrom(new EshoppgsoftwebProductAttributeRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProductAttributeModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebProductAttributeRepository repository = new EshoppgsoftwebProductAttributeRepository();
            if (!repository.Save(ProductAttributeModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProductAttributesFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            ProductAttributeModel model = ProductAttributeModel.CreateCopyFrom(new EshoppgsoftwebProductAttributeRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProductAttributeModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebProductAttributeRepository repository = new EshoppgsoftwebProductAttributeRepository();
            if (!repository.Delete(ProductAttributeModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProductAttributesFormId);
        }



        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetEshoppgsoftwebProductAttributeFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(ProductAttributeFilterModel model)
        {
            EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
            if (!repository.Save(this.CurrentSessionId, ProductAttributeFilterModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return RedirectToCurrentUmbracoPage();
        }
        ProductAttributeFilterModel GetEshoppgsoftwebProductAttributeFilterForEdit()
        {
            return ProductAttributeFilterModel.CreateCopyFrom(new EshoppgsoftwebUserPropRepository().Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProductAttributeFilterModel));
        }
    }
}
