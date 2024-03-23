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
    public class PaymentTypeController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "PaymentOrder", string sortDir = "ASC")
        {
            PaymentTypeRepository repository = new PaymentTypeRepository();
            PaymentTypePagingListModel model = PaymentTypePagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir));

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new PaymentTypeModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            PaymentTypeModel model = PaymentTypeModel.CreateCopyFrom(new PaymentTypeRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(PaymentTypeModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            PaymentTypeRepository repository = new PaymentTypeRepository();
            if (!repository.Save(PaymentTypeModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommercePaymentTypesFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            PaymentTypeModel model = PaymentTypeModel.CreateCopyFrom(new PaymentTypeRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(PaymentTypeModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            PaymentTypeRepository repository = new PaymentTypeRepository();
            if (!repository.Delete(PaymentTypeModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommercePaymentTypesFormId);
        }
    }
}
