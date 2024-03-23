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
    public class CustomerAdminController : _BaseController
    {
        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetRecords(int page = 1, string sort = "Name", string sortDir = "ASC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                CustomerFilterModel filter = GetCustomerFilterForEdit();
                if (filter != null)
                {
                    filter.SearchText = string.Empty;
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, CustomerFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            CustomerFilterModel filter = GetCustomerFilterForEdit();

            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            CustomerListModel model = CustomerListModel.CreateCopyFrom(
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new EshoppgsoftwebCustomerFilter()
                    {
                        SearchText = filter.SearchText,
                    }),
                    new CustomerDropDowns()
                );

            return View(model);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult InsertRecord()
        {
            return View("EditRecord", GetCustomerForEdit());
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult EditRecord(string id)
        {
            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            CustomerModel model = CustomerModel.CreateCopyFrom(repository.Get(new Guid(id)), new CustomerDropDowns());

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsDeliveryAddress)
                {
                    if (string.IsNullOrEmpty(model.DeliveryName))
                    {
                        ModelState.AddModelError("DeliveryName", "Doručovacia firma (meno a priezvisko) musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCountryCollectionKey) || model.DeliveryCountryCollectionKey == Guid.Empty.ToString())
                    {
                        ModelState.AddModelError("DeliveryCountryCollectionKey", "Doručovacia krajina musí byť zadaná.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryStreet))
                    {
                        ModelState.AddModelError("DeliveryStreet", "Doručovacia ulica a číslo domu musí byť zadané.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCity))
                    {
                        ModelState.AddModelError("DeliveryCity", "Doručovacia obec musí byť zadaná.");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryZip))
                    {
                        ModelState.AddModelError("DeliveryZip", "Doručovacie PSČ musí byť zadané.");
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            if (repository.Save(CustomerModel.CreateCopyFrom(model, new CustomerDropDowns())))
            {
                //model.SaveNewsletterSettings();
            }
            else
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceCustomersFormId);
        }


        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult ConfirmDeleteRecord(string id)
        {
            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            CustomerModel model = CustomerModel.CreateCopyFrom(repository.Get(new Guid(id)), new CustomerDropDowns());

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(string keepMember, CustomerModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            if (!repository.Delete(CustomerModel.CreateCopyFrom(model, new CustomerDropDowns())))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (keepMember == "no")
            {
                EshoppgsoftwebMemberRepository memberRep = new EshoppgsoftwebMemberRepository();
                EshoppgsoftwebMember member = memberRep.Get(model.OwnerId);
                if (member != null)
                {
                    memberRep.Delete(member);
                }
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceCustomersFormId);
        }

        [Authorize(Roles = "EcommerceAdmin")]
        public ActionResult GetFilter()
        {
            return View(GetCustomerFilterForEdit());
        }
        [HttpPost]
        [Authorize(Roles = "EcommerceAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(CustomerFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, CustomerFilterModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveFilter(model);
            }

            return RedirectToCurrentUmbracoPageAfterSaveFilter();
        }


        CustomerModel GetCustomerForEdit()
        {
            CustomerModel model = new CustomerModel();
            model.DropDowns = new CustomerDropDowns();

            return model;
        }

        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveFilter(CustomerFilterModel rec = null)
        {
            SetCustomerFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetCustomerFilterForEdit(CustomerFilterModel rec = null)
        {
            TempData["CustomerFilterForEdit"] = rec;
        }
        CustomerFilterModel GetCustomerFilterForEdit()
        {
            if (TempData["CustomerFilterForEdit"] == null)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                TempData["CustomerFilterForEdit"] = CustomerFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_CustomerFilterModel));
            }

            return (CustomerFilterModel)TempData["CustomerFilterForEdit"];
        }
    }
}
