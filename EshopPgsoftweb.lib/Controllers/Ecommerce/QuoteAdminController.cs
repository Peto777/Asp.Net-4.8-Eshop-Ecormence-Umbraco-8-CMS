using dufeksoft.lib.Model;
using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Tasks;
using eshoppgsoftweb.lib.Tasks.Ecommerce;
using eshoppgsoftweb.lib.Util;
using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    [Authorize(Roles = "EcommerceAdmin")]
    public class QuoteAdminController : _BaseController
    {
        public ActionResult GetRecords(int page = 1, string sort = "DateFinished", string sortDir = "DESC")
        {
            try
            {
                return GetRecordsView(page, sort, sortDir);
            }
            catch
            {
                QuoteListFilterModel filter = GetQuoteFilterForEdit();
                if (filter != null)
                {
                    filter.Clear();
                    EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                    repository.Save(this.CurrentSessionId, QuoteListFilterModel.CreateCopyFrom(filter));
                }
                return GetRecordsView(page, sort, sortDir);
            }
        }
        ActionResult GetRecordsView(int page, string sort, string sortDir)
        {
            QuoteListFilterModel filter = GetQuoteFilterForEdit();

            QuoteForListRepository repository = new QuoteForListRepository();
            QuoteListModel model = QuoteListModel.CreateCopyFrom(null,
                repository.GetPage(page, _PagingModel.DefaultItemsPerPage, sort, sortDir,
                    new QuoteForListFilter()
                    {
                        QuoteId = filter.QuoteId,
                        SearchText = filter.SearchText,
                        From = filter.GetDateTimeFrom(),
                        To = filter.GetDateTimeTo(),
                        QuoteStates = filter.QuoteStates,
                    }
                ));

            return View(model);
        }
        public ActionResult GetFilter()
        {
            return View(GetQuoteFilterForEdit());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFilter(QuoteListFilterModel model)
        {
            model.ModelErrors.Clear();
            if (model.ModelErrors.Count == 0)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                if (!repository.Save(this.CurrentSessionId, QuoteListFilterModel.CreateCopyFrom(model)))
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
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecordFilter(QuoteListFilterModel rec = null)
        {
            SetQuoteFilterForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetQuoteFilterForEdit(QuoteListFilterModel rec = null)
        {
            TempData["QuoteListFilterForEdit"] = rec;
        }
        QuoteListFilterModel GetQuoteFilterForEdit()
        {
            if (TempData["QuoteListFilterForEdit"] == null)
            {
                EshoppgsoftwebUserPropRepository repository = new EshoppgsoftwebUserPropRepository();
                TempData["QuoteListFilterForEdit"] = QuoteListFilterModel.CreateCopyFrom(repository.Get(this.CurrentSessionId, ConfigurationUtil.PropId_QuoteListFilterModel));
            }

            QuoteListFilterModel model = (QuoteListFilterModel)TempData["QuoteListFilterForEdit"];
            model.AllQuoteStates = QuoteStateItemListModel.CreateFromRepository();

            return model;
        }



        public ActionResult ConfirmDeleteRecord(string id)
        {
            QuoteModel model = GetQuoteForEdit(id);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(QuoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();

            if (model.ModelErrors.Count == 0)
            {
                QuoteRepository repository = new QuoteRepository();
                if (!repository.Delete(QuoteModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuotesFormId);
        }

        public ActionResult EditRecord(string id, string tab)
        {
            return View(new QuoteEditModel() { QuoteId = id, TabId = tab });
        }

        public ActionResult EditState(string id)
        {
            QuoteModel model = GetQuoteForEdit(id);
            model.UpdateDropDownsBeforeEdit();

            return View(model);
        }
        [HttpPost]
        public ActionResult SaveState(QuoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();

            if (model.ModelErrors.Count == 0)
            {
                model.UpdateDropDownsAfterEdit();
                QuoteRepository repository = new QuoteRepository();
                if (!repository.Save(QuoteModel.CreateCopyFrom(model)))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuotesEditFormId, string.Format("id={0}&tab={1}", model.pk, QuoteEditModel.TabStateId));
        }


        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecord(QuoteModel rec = null)
        {
            SetQuoteForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetQuoteForEdit(QuoteModel rec = null)
        {
            TempData["QuoteModel"] = rec;
        }
        QuoteModel GetQuoteForEdit(string id)
        {
            QuoteModel model;
            if (string.IsNullOrEmpty(id))
            {
                model = (QuoteModel)TempData["QuoteModel"];
            }
            else
            {
                QuoteRepository repository = new QuoteRepository();
                model = QuoteModel.CreateCopyFrom(repository.Get(new Guid(id)));
            }

            model.LoadUser();
            model.LoadProductItems(new ProductModelDropDowns());

            return model;
        }



        public ActionResult EditUser(string id)
        {
            User2QuoteModel model = GetUserForEdit(id);

            return View(model);
        }
        [HttpPost]
        public ActionResult SaveUser(User2QuoteModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsCompanyInvoice)
                {
                    if (string.IsNullOrEmpty(model.CompanyName))
                    {
                        ModelState.AddModelError("CompanyName", "Názov firmy musí byť zadaný");
                    }
                    if (string.IsNullOrEmpty(model.CompanyIco))
                    {
                        ModelState.AddModelError("CompanyIco", "IČO musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.CompanyDic))
                    {
                        ModelState.AddModelError("CompanyDic", "DIČ musí byť zadané");
                    }
                }
                if (model.IsDeliveryAddress)
                {
                    if (string.IsNullOrEmpty(model.DeliveryName))
                    {
                        ModelState.AddModelError("DeliveryName", "Meno a priezvisko pre adresu doručenia musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryStreet))
                    {
                        ModelState.AddModelError("DeliveryStreet", "Ulica a číslo domu pre adresu doručenia musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryZip))
                    {
                        ModelState.AddModelError("DeliveryZip", "PSČ pre adresu doručenia musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.DeliveryCity))
                    {
                        ModelState.AddModelError("DeliveryCity", "Obec pre adresu doručenia musí byť zadaná");
                    }
                    if (!RequiredGuidDropDownAttribute.IsValidKey(model.DeliveryCountryCollectionKey))
                    {
                        ModelState.AddModelError("DeliveryCountryCollectionKey", "Krajina pre adresu doručenia musí byť zadaná");
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();

            if (model.ModelErrors.Count == 0)
            {
                User2QuoteRepository repository = new User2QuoteRepository();
                if (!repository.Save(User2QuoteModel.CreateCopyFrom(model, new User2QuoteDropDowns())))
                {
                    model.ModelErrors.Add("Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }
            if (model.ModelErrors.Count > 0)
            {
                return RedirectToCurrentUmbracoPageAfterSaveRecord(model);
            }

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.EcommerceQuotesEditFormId, string.Format("id={0}&tab={1}", model.PkQuote, QuoteEditModel.TabUserId));
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSaveRecord(User2QuoteModel rec = null)
        {
            SetUserForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetUserForEdit(User2QuoteModel rec = null)
        {
            TempData["User2QuoteModel"] = rec;
        }
        User2QuoteModel GetUserForEdit(string id)
        {
            User2QuoteModel model;
            if (string.IsNullOrEmpty(id))
            {
                model = (User2QuoteModel)TempData["User2QuoteModel"];
            }
            else
            {
                User2QuoteRepository repository = new User2QuoteRepository();
                model = User2QuoteModel.CreateCopyFrom(repository.GetForQuote(new Guid(id)), new User2QuoteDropDowns());
            }

            return model;
        }



        public ActionResult ConfirmSendInfo(string id)
        {
            return View(GetQuoteInfoForEdit(id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendInfo(QuoteSendInfoModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            model.ModelErrors.Clear();

            try
            {
                // Odoslanie zakaznikovi
                QuoteNotifier.SendQuoteStateNotification(model.pk, this.DefaultImgPath, false, notifyEshoppgsoftweb: false, moreInfo: model.Note);

                model.SendOk = true;
            }
            catch (Exception exc)
            {
                model.ModelErrors.Add(exc.ToString());
            }

            return RedirectToCurrentUmbracoPageAfterSendInfo(model);
        }
        RedirectToUmbracoPageResult RedirectToCurrentUmbracoPageAfterSendInfo(QuoteSendInfoModel rec = null)
        {
            SetQuoteInfoForEdit(rec);
            return RedirectToCurrentUmbracoPage();
        }
        void SetQuoteInfoForEdit(QuoteSendInfoModel rec = null)
        {
            TempData["QuoteSendInfoModel"] = rec;
        }
        QuoteSendInfoModel GetQuoteInfoForEdit(string id)
        {
            QuoteSendInfoModel model;
            if (TempData["QuoteSendInfoModel"] != null)
            {
                model = (QuoteSendInfoModel)TempData["QuoteSendInfoModel"];
            }
            else
            {
                model = new QuoteSendInfoModel();
                model.pk = new Guid(id);
            }
            if (model.Quote == null)
            {
                model.Quote = GetQuoteForEdit(model.pk.ToString());
            }

            return model;
        }



        public ActionResult GetQuoteListPdf()
        {
            PdfFilePrintResult pdfResult = new QuoteListToPdf(this).GetPdf();

            ActionResult ret = PdfDownloadResult.GetActionResult(pdfResult.FileContent, pdfResult.FileName);
            if (ret == null)
            {
                throw new HttpException(404, "Error generating PDF");
            }

            return ret;
        }

        public ActionResult GetQuotePcsListPdf()
        {
            PdfFilePrintResult pdfResult = new QuotePcsListToPdf(this).GetPdf();

            ActionResult ret = PdfDownloadResult.GetActionResult(pdfResult.FileContent, pdfResult.FileName);
            if (ret == null)
            {
                throw new HttpException(404, "Error generating PDF");
            }

            return ret;
        }
    }
}
