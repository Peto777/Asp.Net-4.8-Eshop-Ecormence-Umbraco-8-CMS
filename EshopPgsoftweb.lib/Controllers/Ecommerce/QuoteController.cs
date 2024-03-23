using dufeksoft.lib.Model;
using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Tasks.Ecommerce;
using eshoppgsoftweb.lib.Util;
using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class QuoteController : _BaseController
    {
        public ActionResult Basket()
        {
            QuoteModel.RecalcQuoteDiscountForSession(this.CurrentSessionId, EshoppgsoftwebCustomerCache.CurrentMemberId);

            BasketModel model = new BasketModel();
            model.CurrentQuote = GetCurrentQuoteWithProducts(true);

            return View(model);
        }

        public ActionResult BasketDeliveryData()
        {
            QuoteModel.RecalcQuoteDiscountForSession(this.CurrentSessionId, EshoppgsoftwebCustomerCache.CurrentMemberId);

            BasketModel model = new BasketModel();
            model.CurrentQuote = GetCurrentQuoteWithProducts(false);
            model.CurrentQuote.User = GetQuoteUser(model.CurrentQuote);
            if ((model.CurrentQuote.User.IsNew || model.CurrentQuote.User.IsEmpty) && EshoppgsoftwebCustomerCache.IsCustomerAuthenticated)
            {
                EshoppgsoftwebCustomer customer = EshoppgsoftwebCustomerCache.GetCurrentCustomer();
                if (customer != null)
                {
                    model.CurrentQuote.User.CopyDataFrom(customer, CountryDropDown.CreateFromRepository(false));
                }
            }

            TransportTypeRepository transportRep = new TransportTypeRepository();
            model.TransportTypeCollection = TransportTypeListModel.CreateCopyFrom(transportRep.GetRecordsForBasket(), model.CurrentQuote.QuoteTotalWeight);
            model.TransportTypeKey = model.CurrentQuote.GetTransportTypeKey();
            PaymentTypeRepository paymentRep = new PaymentTypeRepository();
            model.PaymentTypeCollection = PaymentTypeListModel.CreateCopyFrom(paymentRep.GetRecordsForBasket());
            model.PaymentTypeKey = model.CurrentQuote.GetPaymentTypeKey();

            return View(model);
        }
        [HttpPost]
        public ActionResult SubmitBasketDeliveryData(BasketModel model)
        {
            QuoteModel quote = null;

            if (ModelState.IsValid)
            {
                if (model.CurrentQuote.User.IsCompanyInvoice)
                {
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.CompanyName))
                    {
                        ModelState.AddModelError("CurrentQuote.User.CompanyName", "Názov firmy musí byť zadaný");
                    }
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.CompanyIco))
                    {
                        ModelState.AddModelError("CurrentQuote.User.CompanyIco", "IČO musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.CompanyDic))
                    {
                        ModelState.AddModelError("CurrentQuote.User.CompanyDic", "DIČ musí byť zadané");
                    }
                }
                if (model.CurrentQuote.User.IsDeliveryAddress)
                {
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.DeliveryName))
                    {
                        ModelState.AddModelError("CurrentQuote.User.DeliveryName", "Meno a priezvisko pre adresu doručenia musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.DeliveryStreet))
                    {
                        ModelState.AddModelError("CurrentQuote.User.DeliveryStreet", "Ulica a číslo domu pre adresu doručenia musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.DeliveryZip))
                    {
                        ModelState.AddModelError("CurrentQuote.User.DeliveryZip", "PSČ pre adresu doručenia musí byť zadané");
                    }
                    if (string.IsNullOrEmpty(model.CurrentQuote.User.DeliveryCity))
                    {
                        ModelState.AddModelError("CurrentQuote.User.DeliveryCity", "Obec pre adresu doručenia musí byť zadaná");
                    }
                    if (!RequiredGuidDropDownAttribute.IsValidKey(model.CurrentQuote.User.DeliveryCountryCollectionKey))
                    {
                        ModelState.AddModelError("CurrentQuote.User.DeliveryCountryCollectionKey", "Krajina pre adresu doručenia musí byť zadaná");
                    }
                }

                // Load quote data for later checks
                quote = QuoteModel.CreateCopyFrom(new QuoteRepository().Get(model.CurrentQuote.pk));
                quote.LoadProductItems(new ProductModelDropDowns(), clearNonProductItems: false);

                // Always set transport type item to remember choise after postback
                TransportTypeRepository transportRep = new TransportTypeRepository();
                TransportType transportType = transportRep.Get(model.TransportTypeKey);
                if (transportType != null)
                {
                    Product2QuoteRepository rep = new Product2QuoteRepository();
                    Product2Quote transportItem = rep.GetForItemCode(model.CurrentQuote.pk, ConfigurationUtil.Ecommerce_Quote_TransportItemCode);
                    if (transportItem == null)
                    {
                        transportItem = new Product2Quote();
                    }
                    transportItem.NonProductId = transportType.pk.ToString();
                    transportItem.PkQuote = model.CurrentQuote.pk;
                    transportItem.ItemCode = ConfigurationUtil.Ecommerce_Quote_TransportItemCode;
                    transportItem.ItemPcs = 1;
                    transportItem.ItemName = transportType.Name;
                    if (quote.IsFreeTransportPrice)
                    {
                        transportItem.UnitPriceNoVat = 0M;
                        transportItem.UnitPriceWithVat = 0M;
                        transportItem.VatPerc = 0M;
                    }
                    else
                    {
                        //if (quote.QuoteTotalWeight > ConfigurationUtil.GetQuoteWeightLimitAbove5kg())
                        //{
                        //    transportItem.UnitPriceNoVat = transportType.PriceNoVatAbove5kg;
                        //    transportItem.UnitPriceWithVat = transportType.PriceWithVatAbove5kg;
                        //}
                        //else
                        {
                            transportItem.UnitPriceNoVat = transportType.PriceNoVat;
                            transportItem.UnitPriceWithVat = transportType.PriceWithVat;
                            transportItem.VatPerc = transportType.VatPerc;
                        }
                    }
                    rep.Save(transportItem);
                }

                // Always set payment type item to remember choise after postback
                PaymentTypeRepository paymentRep = new PaymentTypeRepository();
                PaymentType paymentType = paymentRep.Get(model.PaymentTypeKey);
                if (paymentType != null)
                {
                    Product2QuoteRepository rep = new Product2QuoteRepository();
                    Product2Quote paymentItem = rep.GetForItemCode(model.CurrentQuote.pk, ConfigurationUtil.Ecommerce_Quote_PaymentItemCode);
                    if (paymentItem == null)
                    {
                        paymentItem = new Product2Quote();
                    }
                    paymentItem.NonProductId = paymentType.pk.ToString();
                    paymentItem.PkQuote = model.CurrentQuote.pk;
                    paymentItem.ItemCode = ConfigurationUtil.Ecommerce_Quote_PaymentItemCode;
                    paymentItem.ItemPcs = 1;
                    paymentItem.ItemName = paymentType.Name;
                    if (quote.IsFreeTransportPrice)
                    {
                        paymentItem.UnitPriceNoVat = 0M;
                        paymentItem.UnitPriceWithVat = 0M;
                        paymentItem.VatPerc = 0M;
                    }
                    else
                    {
                        paymentItem.UnitPriceNoVat = paymentType.PriceNoVat;
                        paymentItem.UnitPriceWithVat = paymentType.PriceWithVat;
                        paymentItem.VatPerc = paymentType.VatPerc;
                    }
                    rep.Save(paymentItem);
                }

                if (transportType != null && paymentType != null)
                {
                    if (transportType.GatewayTypeId == (int)TransportGateway.GatewayType.GT_DPD)
                    {
                        // Kurier DPD vyzaduje platbu na dobierku
                        if (paymentType.GatewayTypeId != (int)PaymentGateway.GatewayType.GT_ON_DELIVERY)
                        {
                            ModelState.AddModelError("TransportTypeKey", string.Format("Pre spôsob dodania '{0}' musíte vybrať spôsob platby '{1}'.",
                                transportType.Name,
                                paymentRep.GetForGatewayType((int)PaymentGateway.GatewayType.GT_ON_DELIVERY).Name));
                        }
                    }
                    if (paymentType.GatewayTypeId == (int)PaymentGateway.GatewayType.GT_ON_DELIVERY)
                    {
                        // Platba na dobierku vyzaduje dopravu Kurier DPD
                        if (transportType.GatewayTypeId != (int)TransportGateway.GatewayType.GT_DPD)
                        {
                            ModelState.AddModelError("PaymentTypeKey", string.Format("Pre spôsob platby '{0}' musíte vybrať spôsob dodania '{1}'.",
                                paymentType.Name,
                                transportRep.GetForGatewayType((int)TransportGateway.GatewayType.GT_DPD).Name));
                        }
                    }
                }
            }

            User2QuoteDropDowns ddUser2Quote = null;
            if (ModelState.IsValid)
            {
                ddUser2Quote = new User2QuoteDropDowns();
                // Create temp user with correct country codes
                User2QuoteModel tmpUser = User2QuoteModel.CreateCopyFrom(User2QuoteModel.CreateCopyFrom(model.CurrentQuote.User, ddUser2Quote), ddUser2Quote);
                // Check user country codes
                if (tmpUser.IsInternationalInvoice || tmpUser.IsInternationalTransport)
                {
                    PaymentType bankTransfer = new PaymentTypeRepository().GetForGatewayType((int)PaymentGateway.GatewayType.GT_BANK_TRANSFER);
                    if (model.PaymentTypeKey != bankTransfer.pk)
                    {
                        ModelState.AddModelError("PaymentTypeKey", string.Format("Musíte zvoliť spôsob platby '{0}'.", bankTransfer.Name));
                    }
                }
                //if (tmpUser.IsInternationalTransport)
                //{
                //    TransportType internationalTransport = new TransportTypeRepository().GetForGatewayType((int)TransportGateway.GatewayType.GT_OUTSIDE_SLOVAKIA);
                //    if (model.TransportTypeKey != internationalTransport.pk)
                //    {
                //        ModelState.AddModelError("TransportTypeKey", string.Format("Musíte zvoliť spôsob dodania '{0}'.", internationalTransport.Name));
                //    }
                //}
            }

            if (ModelState.IsValid)
            {
                if (EshoppgsoftwebCustomerCache.IsCustomerAuthenticated)
                {
                    EshoppgsoftwebCustomer customer = EshoppgsoftwebCustomerCache.GetCurrentCustomer();
                    if (customer != null)
                    {
                        model.CurrentQuote.User.PkUser = customer.pk;
                    }
                }
                model.CurrentQuote.User.PkQuote = model.CurrentQuote.pk;
                if (!new User2QuoteRepository().Save(User2QuoteModel.CreateCopyFrom(model.CurrentQuote.User, ddUser2Quote)))
                {
                    ModelState.AddModelError("", "Nastala chyba pri zápise záznamu do systému. Skúste akciu zopakovať a ak sa chyba vyskytne znovu, kontaktujte nás prosím.");
                }
            }

            if (ModelState.IsValid)
            {
                //TransportType dhlTransport = new TransportTypeRepository().GetForGatewayType((int)TransportGateway.GatewayType.GT_DHL);
                //if (model.TransportTypeKey == dhlTransport.pk)
                //{
                //    return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_Gateway_DhlParcelShopPageId);
                //}
                //TransportType spsTransport = new TransportTypeRepository().GetForGatewayType((int)TransportGateway.GatewayType.GT_SPS);
                //if (model.TransportTypeKey == spsTransport.pk)
                //{
                //    return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_Gateway_SpsParcelShopPageId);
                //}
                //else
                {
                    return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_ReviewAndSendPageId);
                }
            }

            return CurrentUmbracoPage();
        }


        public ActionResult BasketDetail()
        {
            BasketModel model = new BasketModel();
            model.CurrentQuote = GetCurrentQuoteWithProducts(false);
            model.CurrentQuote.User = GetQuoteUser(model.CurrentQuote);

            return View(model);
        }
        public ActionResult BasketReviewAndSend()
        {
            BasketReviewAndSendModel model = new BasketReviewAndSendModel();
            model.AgreePersonalDataNewsletter = true;
            model.AgreeTradeRules = false;

            return View(model);
        }
        [HttpPost]
        public ActionResult SubmitBasketReviewAndSend(BasketReviewAndSendModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.AgreeTradeRules)
                {
                    ModelState.AddModelError("AgreeTradeRules", "Musíte potvrdiť súhlas s obchodnými podmienkami.");
                }
            }
            if (ModelState.IsValid)
            {
                return BasketFinalize();
            }

            return CurrentUmbracoPage();
        }
        ActionResult BasketFinalize()
        {
            QuoteModel quoteModel = QuoteModel.GetCompleteModel(FinishCurrentQuote().pk);
            Guid quotePaymenTypeKey = quoteModel.GetPaymentTypeKey();

            bool isCardPayment = false;
            PaymentType cardPayment = new PaymentTypeRepository().GetForGatewayType((int)PaymentGateway.GatewayType.GT_GP_WEBPAY);
            if (cardPayment != null)
            {
                //Is card payment?
               isCardPayment = cardPayment.pk == quotePaymenTypeKey;
            }

            bool isSporopayPayment = false;
            PaymentType sporopayPayment = new PaymentTypeRepository().GetForGatewayType((int)PaymentGateway.GatewayType.GT_SPOROPAY);
            if (sporopayPayment != null)
            {
                // Is sporopay payment ?
                isSporopayPayment = sporopayPayment.pk == quotePaymenTypeKey;
            }

            bool isVubeplatbyPayment = false;
            PaymentType vubeplatbyPayment = new PaymentTypeRepository().GetForGatewayType((int)PaymentGateway.GatewayType.GT_VUBEPLATBY);
            if (vubeplatbyPayment != null)
            {
                // Is VUB E-PLATBY payment ?
                isVubeplatbyPayment = vubeplatbyPayment.pk == quotePaymenTypeKey;
            }

            QuoteNotifier.SendQuoteStateNotification(quoteModel.pk, this.DefaultImgPath, isCardPayment);


            //if (isCardPayment)
            //{
            //    return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_CardPayPageId, string.Format("id={0}", quoteModel.pk.ToString()));
            //}
            //if (isSporopayPayment)
            //{
            //    return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_SporopayPageId, string.Format("id={0}", quoteModel.pk.ToString()));
            //}
            //if (isVubeplatbyPayment)
            //{
            //    return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_VubeplatbyPageId, string.Format("id={0}", quoteModel.pk.ToString()));
            //}

            return this.RedirectToEshoppgsoftwebUmbracoPage(ConfigurationUtil.Ecommerce_Basket_FinishedPageId);
        }

        public ActionResult BasketFinished()
        {
            BasketModel model = new BasketModel();
            model.CurrentQuote = GetCurrentFinishedQuote();
            return View(model);
        }
        public ActionResult QuoteDetail(string id)
        {
            BasketModel model = new BasketModel();
            model.CurrentQuote = GetQuoteWithProducts(new Guid(id));
            model.CurrentQuote.User = GetQuoteUser(model.CurrentQuote);

            return View(model);
        }
        public ActionResult QuoteSendStateNotification(string id)
        {
            BasketModel model = new BasketModel();
            model.CurrentQuote = GetQuoteWithProducts(new Guid(id));
            model.CurrentQuote.User = GetQuoteUser(model.CurrentQuote);
            QuoteNotifier.SendQuoteStateNotification(model.CurrentQuote.pk, this.DefaultImgPath, false, notifyEshoppgsoftweb: false);

            return View(model);
        }

        public ActionResult GetQuotePdf(string id)
        {
            PdfFilePrintResult pdfResult = QuoteToPdf.GetQuotePdf(new Guid(id), this.DefaultImgPath);

            ActionResult ret = PdfDownloadResult.GetActionResult(pdfResult.FileContent, pdfResult.FileName);
            if (ret == null)
            {
                throw new HttpException(404, "Error generating PDF");
            }

            return ret;
        }



        Quote FinishCurrentQuote()
        {
            QuoteRepository quoteRep = new QuoteRepository();
            return quoteRep.FinishQuote(this.CurrentSessionId);
        }

        QuoteModel GetCurrentQuote()
        {
            return GetCurrentQuote(this);
        }

        QuoteModel GetCurrentFinishedQuote()
        {
            QuoteRepository quoteRep = new QuoteRepository();
            return QuoteModel.CreateCopyFrom(quoteRep.GetNewestForFinishedSession(this.CurrentSessionId));
        }

        QuoteModel GetCurrentQuoteWithProducts(bool clearNonProductItems)
        {
            return GetCurrentQuoteWithProducts(this, clearNonProductItems);
        }


        public static QuoteModel GetCurrentQuote(_BaseController ctrl)
        {
            QuoteRepository quoteRep = new QuoteRepository();
            return QuoteModel.CreateCopyFrom(quoteRep.GetForSession(ctrl.CurrentSessionId));
        }
        public static QuoteModel GetCurrentQuoteWithProducts(_BaseController ctrl, bool clearNonProductItems = false)
        {
            QuoteModel model = GetCurrentQuote(ctrl);
            model.LoadProductItems(new ProductModelDropDowns(), clearNonProductItems);

            return model;
        }
        public static QuoteModel GetQuoteWithProducts(Guid quoteKey)
        {
            QuoteRepository rep = new QuoteRepository();
            QuoteModel model = QuoteModel.CreateCopyFrom(rep.Get(quoteKey));
            model.LoadProductItems(new ProductModelDropDowns());

            return model;
        }
        public static User2QuoteModel GetQuoteUser(QuoteModel quote)
        {
            User2QuoteModel model = new User2QuoteModel();
            User2QuoteRepository userRep = new User2QuoteRepository();
            User2Quote user = userRep.GetForQuote(quote.pk);
            if (user != null)
            {
                model.CopyDataFrom(user, new User2QuoteDropDowns());
            }

            return model;
        }
    }
}
