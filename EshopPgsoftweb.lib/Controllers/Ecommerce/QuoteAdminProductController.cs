using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    [Authorize(Roles = "EcommerceAdmin")]
    public class QuoteAdminProductController : _BaseController
    {
        public ActionResult GetRecords(string id)
        {
            QuoteRepository repository = new QuoteRepository();
            QuoteModel model = QuoteModel.CreateCopyFrom(repository.Get(new Guid(id)));
            model.LoadProductItems(new ProductModelDropDowns());
            model.LoadUser();

            return View(model);
        }

        public ActionResult InsertRecord(string id)
        {
            InsertProduct2QuoteModel model = new InsertProduct2QuoteModel();
            model.PkQuote = new Guid(id);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveNewRecord(InsertProduct2QuoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            Product2QuoteRepository repository = new Product2QuoteRepository();
            Product2Quote prodCheck = repository.Get(model.PkQuote, model.PkProduct);
            if (prodCheck != null)
            {
                ModelState.AddModelError("", "Vybraný produkt je v objednávke vybraný v inej položke. Vyberte iný produkt.");
            }
            if (ModelState.IsValid)
            {
                InsertProduct2QuoteModel.AddProductToQuote(model.PkQuote, model.PkProduct, PriceUtil.NumberFromEditorString(model.ItemPcs));
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuotesEditFormId, string.Format("id={0}&tab={1}", model.PkQuote, QuoteEditModel.TabItemsId));
        }

        public ActionResult EditRecord(string quoteId, string itemId)
        {
            Product2QuoteModel model = GetQuoteItemForEdit(quoteId, itemId);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRecord(Product2QuoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            Product2QuoteRepository repository = new Product2QuoteRepository();
            if (model.IsProductItem)
            {
                Product2Quote prodCheck = repository.Get(model.PkQuote, model.PkProduct);
                if (prodCheck != null && prodCheck.pk != model.pk)
                {
                    ModelState.AddModelError("", "Vybraný produkt je v objednávke vybraný v inej položke. Vyberte iný produkt.");
                }
            }
            if (ModelState.IsValid)
            {
                if (!repository.Save(Product2QuoteModel.CreateCopyFrom(model)))
                {
                    ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuotesEditFormId, string.Format("id={0}&tab={1}", model.PkQuote, QuoteEditModel.TabItemsId));
        }


        public ActionResult ConfirmDeleteRecord(string quoteId, string itemId)
        {
            Product2QuoteModel model = GetQuoteItemForEdit(quoteId, itemId);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(Product2QuoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            Product2QuoteRepository repository = new Product2QuoteRepository();
            if (!repository.Delete(Product2QuoteModel.CreateCopyFrom(model)))
            {
                ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuotesEditFormId, string.Format("id={0}&tab={1}", model.PkQuote, QuoteEditModel.TabItemsId));
        }

        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecord(Product2QuoteModel rec = null)
        {
            SetQuoteItemForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetQuoteItemForEdit(Product2QuoteModel rec = null)
        {
            TempData["QuoteItemForEdit"] = rec;
        }
        Product2QuoteModel GetQuoteItemForEdit(string quoteId, string itemId)
        {
            Product2QuoteModel model;

            if (TempData["QuoteItemForEdit"] == null)
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    model = new Product2QuoteModel();
                    model.PkQuote = new Guid(quoteId);
                }
                else
                {
                    Product2QuoteRepository rep = new Product2QuoteRepository();
                    model = Product2QuoteModel.CreateCopyFrom(rep.Get(new Guid(itemId)));
                    if (model.IsProductItem)
                    {
                        EshoppgsoftwebProductRepository prodRep = new EshoppgsoftwebProductRepository();
                        model.Product = ProductModel.CreateCopyFrom(prodRep.Get(model.PkProduct), new ProductModelDropDowns(), loadPrice: true);
                    }
                }
            }
            else
            {
                model = (Product2QuoteModel)TempData["QuoteItemForEdit"];
            }
            if (model.ProductCollection == null)
            {
                model.ProductCollection = ProductDropDown.CreateFromRepository(true);
            }

            return model;
        }
    }
}
