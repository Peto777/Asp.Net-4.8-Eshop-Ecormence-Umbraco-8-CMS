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
    public class ProducerController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "ProducerName", string sortDir = "ASC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                ProducerFilterModel filter = GetEshoppgsoftwebProducerFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, ProducerFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            ProducerFilterModel filter = GetEshoppgsoftwebProducerFilterForEdit();

            EshoppgsoftwebProducerRepository repository = new EshoppgsoftwebProducerRepository();
            ProducerPagingListModel model = ProducerPagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshoppgsoftwebProducerFilter()
                    {
                        SearchText = filter.SearchText
                    })
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new ProducerModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            ProducerModel model = ProducerModel.CreateCopyFrom(new EshoppgsoftwebProducerRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(ProducerModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (!new EshoppgsoftwebProducerRepository().Save(ProducerModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProducersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            ProducerModel model = ProducerModel.CreateCopyFrom(new EshoppgsoftwebProducerRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(ProducerModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebProducerRepository repository = new EshoppgsoftwebProducerRepository();
            try
            {
                if (!repository.Delete(ProducerModel.CreateCopyFrom(model)))
                {
                    ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            catch (Exception exc)
            {
                ModelState.AddModelError("", "Výrobcu nie je možné odstrániť pretože je priradený k niektorým produktom.");
                this.Logger.Error(typeof(ProducerController), "DeleteRecord error", exc);
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceProducersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetEshoppgsoftwebProducerFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(ProducerFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, ProducerFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecordFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveRecordFilter();
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordFilter(ProducerFilterModel rec = null)
        {
            SetEshoppgsoftwebProducerFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetEshoppgsoftwebProducerFilterForEdit(ProducerFilterModel rec = null)
        {
            TempData["stirilabProducerFilterForEdit"] = rec;
        }
        ProducerFilterModel GetEshoppgsoftwebProducerFilterForEdit()
        {
            if (TempData["stirilabProducerFilterForEdit"] == null)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                TempData["stirilabProducerFilterForEdit"] = ProducerFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_ProducerFilterModel));
            }

            return (ProducerFilterModel)TempData["stirilabProducerFilterForEdit"];
        }
    }
}
