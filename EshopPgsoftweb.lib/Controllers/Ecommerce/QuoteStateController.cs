using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class QuoteStateController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(string tableId, string sort = "code", string sortDir = "ASC")
        {
            QuoteStateRepository repository = new QuoteStateRepository();

            return View(QuoteStateListModel.CreateCopyFrom(repository.GetRecords(sort, sortDir)));
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new QuoteStateModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            QuoteStateModel model = QuoteStateModel.CreateCopyFrom(new QuoteStateRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(QuoteStateModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (!new QuoteStateRepository().Save(QuoteStateModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuoteStatesFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            QuoteStateModel model = QuoteStateModel.CreateCopyFrom(new QuoteStateRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(QuoteStateModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (!new QuoteStateRepository().Delete(QuoteStateModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuoteStatesFormId);
        }
    }
}
