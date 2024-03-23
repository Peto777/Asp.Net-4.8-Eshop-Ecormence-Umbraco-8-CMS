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
    public class AvailabilityController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "AvailabilityName", string sortDir = "ASC")
        {
            EshoppgsoftwebAvailabilityRepository repository = new EshoppgsoftwebAvailabilityRepository();
            AvailabilityPagingListModel model = AvailabilityPagingListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir));

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", new AvailabilityModel());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            AvailabilityModel model = AvailabilityModel.CreateCopyFrom(new EshoppgsoftwebAvailabilityRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(AvailabilityModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebAvailabilityRepository repository = new EshoppgsoftwebAvailabilityRepository();
            if (!repository.Save(AvailabilityModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceAvailabilitiesFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            AvailabilityModel model = AvailabilityModel.CreateCopyFrom(new EshoppgsoftwebAvailabilityRepository().Get(new Guid(id)));

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(AvailabilityModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebAvailabilityRepository repository = new EshoppgsoftwebAvailabilityRepository();
            if (!repository.Delete(AvailabilityModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceAvailabilitiesFormId);
        }
    }
}
