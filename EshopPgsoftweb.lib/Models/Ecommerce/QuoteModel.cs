using dufeksoft.lib.Mail;
using dufeksoft.lib.Model.Grid;
using dufeksoft.lib.ParamSet;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Controllers;
using eshoppgsoftweb.lib.Pdf;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Tasks.Ecommerce;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Xml;
using TransportType = eshoppgsoftweb.lib.Repositories.TransportType;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class QuoteModel : _BaseModel
    {
        public string SessionId { get; set; }
        public string FinishedSessionId { get; set; }
        public string DateCreateView { get; set; }
        public DateTime DateCreate { get; set; }

        [Display(Name = "Vytvorené")]
        public string DateFinishedView { get; set; }
        public DateTime? DateFinished { get; set; }

        [Display(Name = "Rok objednávky")]
        public int QuoteYear { get; set; }
        [Display(Name = "Poradie objednávky")]
        public int QuoteNumber { get; set; }
        [Display(Name = "Číslo objednávky")]
        public string QuoteId
        {
            get
            {
                return QuoteModel.GetQuoteId(this.QuoteYear, this.QuoteNumber);
            }
        }


        [Display(Name = "Stav objednávky")]
        public string QuoteState { get; set; }
        public string QuoteStateCollectionKey { get; set; }

        [Display(Name = "Cena bez DPH")]
        public string QuotePriceNoVat { get; set; }
        [Display(Name = "Cena s DPH")]
        public string QuotePriceWithVat { get; set; }
        [Display(Name = "Stav úhrady")]
        public string QuotePriceState { get; set; }
        public string QuotePriceStateCollectionKey { get; set; }

        [Display(Name = "Interná poznámka predávajúceho")]
        public string Note { get; set; }

        public decimal TotalPriceNoVat
        {
            get
            {
                return GetTotalPriceNoVat();
            }
        }
        public decimal TotalPriceVat
        {
            get
            {
                return TotalPriceWithVat - TotalPriceNoVat;
            }
        }
        public decimal TotalPriceWithVat
        {
            get
            {
                return GetTotalPriceWithVat();
            }
        }
        public string TotalPriceNoVatWithCurrency
        {
            get
            {
                return PriceUtil.GetPriceString(this.TotalPriceNoVat);
            }
        }
        public string TotalPriceVatWithCurrency
        {
            get
            {
                return PriceUtil.GetPriceString(this.TotalPriceVat);
            }
        }
        public string TotalPriceWithVatWithCurrency
        {
            get
            {
                return PriceUtil.GetPriceString(this.TotalPriceWithVat);
            }
        }

        public bool IsFreeTransportPrice
        {
            get
            {
                return SysConstUtil.IsFreeTransportPriceAvailable && this.PriceToAddForFreeTransport <= 0M;
            }
        }

        public decimal PriceToAddForFreeTransport
        {
            get
            {
                return SysConstUtil.GetFreeTransportPrice(this.GetTotalPriceWithVatWithoutTransport());
            }
        }

        public int ProductItemsCnt
        {
            get
            {
                return GetProductItemsCnt();
            }
        }

        public List<Product2QuoteModel> Items { get; set; }
        public User2QuoteModel User { get; set; }

        public QuoteDropDowns DropDowns { get; set; }

        /// <summary>
        /// Celkova hmotnost objednavky
        /// </summary>
        public decimal QuoteTotalWeight { get; set; }

        public static QuoteModel GetCompleteModel(Guid quotekey)
        {
            QuoteModel quote = QuoteModel.CreateCopyFrom(new QuoteRepository().Get(quotekey));
            quote.LoadProductItems(new ProductModelDropDowns());
            quote.LoadUser();

            return quote;
        }

        public void CopyDataFrom(Quote src)
        {
            this.pk = src.pk;
            this.SessionId = src.SessionId;
            this.FinishedSessionId = src.FinishedSessionId;
            this.DateCreate = src.DateCreate;
            this.DateFinished = src.DateFinished;
            this.QuoteYear = src.QuoteYear;
            this.QuoteNumber = src.QuoteNumber;

            this.QuotePriceNoVat = PriceUtil.NumberToEditorString(src.QuotePriceNoVat);
            this.QuotePriceWithVat = PriceUtil.NumberToEditorString(src.QuotePriceWithVat);
            this.QuoteState = src.QuoteState;
            this.QuotePriceState = src.QuotePriceState;

            this.Note = src.Note;

            this.UpdateBeforeEdit();
        }

        public void CopyDataTo(Quote trg)
        {
            this.UpdateAfterEdit();

            trg.pk = this.pk;
            trg.SessionId = this.SessionId;
            trg.FinishedSessionId = this.FinishedSessionId;
            trg.DateCreate = this.DateCreate;
            trg.DateFinished = this.DateFinished;
            trg.QuoteYear = this.QuoteYear;
            trg.QuoteNumber = this.QuoteNumber;

            trg.QuotePriceNoVat = PriceUtil.NumberFromEditorString(this.QuotePriceNoVat);
            trg.QuotePriceWithVat = PriceUtil.NumberFromEditorString(this.QuotePriceWithVat);
            trg.QuoteState = this.QuoteState;
            trg.QuotePriceState = this.QuotePriceState;

            trg.Note = this.Note;
        }

        public static QuoteModel CreateCopyFrom(Quote src)
        {
            QuoteModel trg = new QuoteModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static Quote CreateCopyFrom(QuoteModel src)
        {
            Quote trg = new Quote();
            src.CopyDataTo(trg);

            return trg;
        }

        public void UpdateBeforeEdit()
        {
            this.DateCreateView = DateTimeUtil.GetDisplayDateTime(this.DateCreate);
            this.DateFinishedView = DateTimeUtil.GetDisplayDateTime(this.DateFinished);
        }

        public void UpdateAfterEdit()
        {
            this.DateCreate = DateTimeUtil.DisplayDataToDateTime(this.DateCreateView, this.DateCreate);
            if (string.IsNullOrEmpty(this.DateFinishedView))
            {
                this.DateFinished = null;
            }
            else
            {
                this.DateFinished = DateTimeUtil.DisplayDataToDateTime(this.DateFinishedView, DateTime.Now);
            }
        }

        public void UpdateDropDownsBeforeEdit()
        {
            this.DropDowns = new QuoteDropDowns();

            // quote state
            CmpDropDownItem ddi = this.DropDowns.QuoteStateCollection.GetItemForName(this.QuoteState, true);
            if (ddi != null)
            {
                this.QuoteStateCollectionKey = ddi.DataKey;
            }
            // price state
            ddi = this.DropDowns.PriceStateCollection.GetItemForName(this.QuotePriceState, true);
            if (ddi != null)
            {
                this.QuotePriceStateCollectionKey = ddi.DataKey;
            }
        }

        public void UpdateDropDownsAfterEdit()
        {
            this.DropDowns = new QuoteDropDowns();
            // quote state
            CmpDropDownItem ddi = this.DropDowns.QuoteStateCollection.GetItemForKey(this.QuoteStateCollectionKey);
            if (ddi != null)
            {
                this.QuoteState = ddi.Name;
            }
            // price state
            ddi = this.DropDowns.PriceStateCollection.GetItemForKey(this.QuotePriceStateCollectionKey);
            if (ddi != null)
            {
                this.QuotePriceState = ddi.Name;
            }
        }

        public void BindProducts(List<EshoppgsoftwebProduct> products, ProductModelDropDowns productModelDropDowns)
        {
            Hashtable htProduct = new Hashtable(products.Count + 1);
            foreach (EshoppgsoftwebProduct product in products)
            {
                if (!htProduct.ContainsKey(product.pk))
                {
                    htProduct.Add(product.pk, ProductModel.CreateCopyFrom(product, productModelDropDowns, loadPrice: true));
                }
            }

            foreach (Product2QuoteModel item in this.Items)
            {
                if (htProduct.ContainsKey(item.PkProduct))
                {
                    item.Product = (ProductModel)htProduct[item.PkProduct];
                }
            }
        }

        public void LoadRefreshedProductItems()
        {
            ProductModelDropDowns dd = new ProductModelDropDowns();
            LoadProductItems(dd, clearNonProductItems: false);
            UpdateTransportAndPaymentPrice();
            LoadProductItems(dd, clearNonProductItems: false);
        }
        public void LoadProductItems(ProductModelDropDowns productModelDropDowns, bool clearNonProductItems = false)
        {
            this.Items = new List<Product2QuoteModel>();
            Product2QuoteRepository rep = new Product2QuoteRepository();
            if (clearNonProductItems)
            {
                rep.DeleteNonProductItems(this.pk);
            }
            this.QuoteTotalWeight = 0;
            // Product items as first
            List<Product2Quote> quoteItems = rep.GetQuoteItems(this.pk);
            foreach (Product2Quote item in quoteItems)
            {
                if (item.IsRealProductItem())
                {
                    this.QuoteTotalWeight += item.TotalWeight();
                    this.Items.Add(Product2QuoteModel.CreateCopyFrom(item));
                }
            }
            // Non product items after product items
            foreach (Product2Quote item in quoteItems)
            {
                if (!item.IsRealProductItem())
                {
                    this.QuoteTotalWeight += item.TotalWeight();
                    this.Items.Add(Product2QuoteModel.CreateCopyFrom(item));
                }
            }
            EshoppgsoftwebProductRepository prodRep = new EshoppgsoftwebProductRepository();
            this.BindProducts(prodRep.GetPageForQuote(1, _PagingModel.AllItemsPerPage, this.pk).Items, productModelDropDowns);
        }

        public void LoadUser()
        {
            this.User = User2QuoteModel.CreateCopyFrom(new User2QuoteRepository().GetForQuote(this.pk), new User2QuoteDropDowns());
        }

        public decimal GetTotalPriceNoVat()
        {
            decimal totalPrice = 0;

            if (this.Items != null)
            {
                foreach (Product2QuoteModel item in this.Items)
                {
                    totalPrice += item.TotalPriceNoVat;
                }
            }

            return totalPrice;
        }
        public decimal GetTotalPriceWithVat()
        {
            decimal totalPrice = 0;

            if (this.Items != null)
            {
                foreach (Product2QuoteModel item in this.Items)
                {
                    totalPrice += item.TotalPriceWithVat;
                }
            }

            return totalPrice;
        }
        public decimal GetTotalPriceWithVatWithoutTransport()
        {
            decimal totalPrice = GetTotalPriceWithVat();

            if (this.Items != null)
            {
                foreach (Product2QuoteModel item in this.Items)
                {
                    if (item.ItemCode == ConfigurationUtil.Ecommerce_Quote_TransportItemCode || item.ItemCode == ConfigurationUtil.Ecommerce_Quote_PaymentItemCode)
                    {
                        totalPrice -= item.TotalPriceWithVat;
                    }
                }
            }

            return totalPrice;
        }

        public int GetProductItemsCnt()
        {
            int cnt = 0;
            if (this.Items != null)
            {
                foreach (Product2QuoteModel item in this.Items)
                {
                    if (item.IsProductItem)
                    {
                        cnt++;
                    }
                }
            }

            return cnt;
        }

        public static string GetQuoteId(int quoteYear, int quoteNumber)
        {
            return string.Format("{0}{1}", quoteYear, quoteNumber);
        }

        public Product2QuoteModel GetTransportTypeItem()
        {
            if (this.Items != null)
            {
                foreach (Product2QuoteModel item in this.Items)
                {
                    if (item.ItemCode == ConfigurationUtil.Ecommerce_Quote_TransportItemCode)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
        public string GetTransportTypeId()
        {
            Product2QuoteModel item = GetTransportTypeItem();
            return item == null ? string.Empty : item.NonProductId;
        }
        public Guid GetTransportTypeKey()
        {
            string id = GetTransportTypeId();
            return string.IsNullOrEmpty(id) ? Guid.Empty : new Guid(id);
        }
        public string GetTransportTypeName()
        {
            Product2QuoteModel item = GetTransportTypeItem();
            return item == null ? string.Empty : item.ItemName;
        }

        public Product2QuoteModel GetPaymentType()
        {
            if (this.Items != null)
            {
                foreach (Product2QuoteModel item in this.Items)
                {
                    if (item.ItemCode == ConfigurationUtil.Ecommerce_Quote_PaymentItemCode)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
        public string GetPaymentTypeId()
        {
            Product2QuoteModel item = GetPaymentType();
            return item == null ? string.Empty : item.NonProductId;
        }
        public Guid GetPaymentTypeKey()
        {
            string id = GetPaymentTypeId();
            return string.IsNullOrEmpty(id) ? Guid.Empty : new Guid(id);
        }
        public string GetPaymentTypeName()
        {
            Product2QuoteModel item = GetPaymentType();
            return item == null ? string.Empty : item.ItemName;
        }

        public void UpdateTransportAndPaymentPrice()
        {
            UpdateTransportAndPayment(GetTransportTypeKey(), GetPaymentTypeKey());
        }
        public void UpdateTransportAndPayment(Guid transportKey, Guid paymentKey)
        {
            // Always set transport type item to remember choise after postback
            TransportTypeRepository transportRep = new TransportTypeRepository();
            TransportType transportType = transportRep.Get(transportKey);
            if (transportType != null)
            {
                Product2QuoteRepository rep = new Product2QuoteRepository();
                rep.DeleteForItemCode(this.pk, ConfigurationUtil.Ecommerce_Quote_TransportItemCode);

                Product2Quote transportItem = new Product2Quote();
                transportItem = new Product2Quote();
                transportItem.NonProductId = transportType.pk.ToString();
                transportItem.PkQuote = this.pk;
                transportItem.ItemCode = ConfigurationUtil.Ecommerce_Quote_TransportItemCode;
                transportItem.ItemPcs = 1;
                transportItem.ItemName = transportType.Name;
                if (this.IsFreeTransportPrice)
                {
                    transportItem.UnitPriceNoVat = 0M;
                    transportItem.UnitPriceWithVat = 0M;
                }
                else
                {
                    //if (this.QuoteTotalWeight > ConfigurationUtil.GetQuoteWeightLimitAbove5kg())
                    //{
                    //    transportItem.UnitPriceNoVat = transportType.PriceNoVatAbove5kg;
                    //    transportItem.UnitPriceWithVat = transportType.PriceWithVatAbove5kg;
                    //}
                    //else
                    {
                        transportItem.UnitPriceNoVat = transportType.PriceNoVat;
                        transportItem.UnitPriceWithVat = transportType.PriceWithVat;
                    }
                    transportItem.VatPerc = transportType.VatPerc;
                }
                rep.Save(transportItem);
            }

            // Always set payment type item to remember choise after postback
            PaymentTypeRepository paymentRep = new PaymentTypeRepository();
            PaymentType paymentType = paymentRep.Get(paymentKey);
            if (paymentType != null)
            {
                Product2QuoteRepository rep = new Product2QuoteRepository();
                rep.DeleteForItemCode(this.pk, ConfigurationUtil.Ecommerce_Quote_PaymentItemCode);

                Product2Quote paymentItem = new Product2Quote();
                paymentItem.NonProductId = paymentType.pk.ToString();
                paymentItem.PkQuote = this.pk;
                paymentItem.ItemCode = ConfigurationUtil.Ecommerce_Quote_PaymentItemCode;
                paymentItem.ItemPcs = 1;
                paymentItem.ItemName = paymentType.Name;
                //if (this.IsFreeTransportPrice)
                //{
                //    paymentItem.UnitPriceNoVat = 0M;
                //    paymentItem.UnitPriceWithVat = 0M;
                //}
                //else
                {
                    paymentItem.UnitPriceNoVat = paymentType.PriceNoVat;
                    paymentItem.UnitPriceWithVat = paymentType.PriceWithVat;
                    paymentItem.VatPerc = paymentType.VatPerc;
                }
                rep.Save(paymentItem);
            }
        }


        public static void RecalcQuoteForSession(string sessionId, string memberId)
        {
        }

        public static void RecalcQuoteDiscountForSession(string sessionId, string memberId)
        {
        }

        public static bool UseQuoteDiscountCouponForSession(string sessionId, string couponCode)
        {
            return false;
        }

        public string GetQuoteDetailUrl()
        {
            return string.Format("{0}?id={1}", ConfigurationUtil.QuoteViewUrl, this.pk);
        }
        public string GetQuoteNotificationUrl()
        {
            return string.Format("{0}/notifikacia?id={1}", ConfigurationUtil.QuoteViewUrl.TrimEnd('/'), this.pk);
        }
    }

    public class QuoteEditModel
    {
        public const string TabStateId = "state";
        public const string TabUserId = "user";
        public const string TabItemsId = "items";
        public const string TabInvoiceId = "invoice";

        public string QuoteId { get; set; }
        public string TabId { get; set; }

        public string TabStateActive
        {
            get
            {
                return this.TabId == QuoteEditModel.TabStateId || string.IsNullOrEmpty(this.TabId) ? "active" : string.Empty;
            }
        }
        public string TabUserActive
        {
            get
            {
                return this.TabId == QuoteEditModel.TabUserId ? "active" : string.Empty;
            }
        }
        public string TabItemsActive
        {
            get
            {
                return this.TabId == QuoteEditModel.TabItemsId ? "active" : string.Empty;
            }
        }
        public string TabInvoiceActive
        {
            get
            {
                return this.TabId == QuoteEditModel.TabInvoiceId ? "active" : string.Empty;
            }
        }
    }

    public class QuoteSendInfoModel : _BaseModel
    {
        public QuoteModel Quote { get; set; }
        public bool SendOk { get; set; }

        [Display(Name = "Poznámka o stave objednávky pre odoslanie zákazníkovi")]
        public string Note { get; set; }
    }

    public class QuoteDropDowns
    {
        public QuoteStateDropDown QuoteStateCollection { get; private set; }
        public PaymentStateDropDown PriceStateCollection { get; private set; }

        public QuoteDropDowns()
        {
            this.QuoteStateCollection = QuoteStateDropDown.CreateFromRepository(false);
            this.PriceStateCollection = PaymentStateDropDown.CreateFromRepository(false);
        }
    }

    public class BasketModel
    {
        public QuoteModel CurrentQuote { get; set; }
        public Guid PrevQuoteKey { get; set; }
        public Guid NextQuoteKey { get; set; }
        public string QuoteDetailUrl { get; set; }

        public Guid TransportTypeKey { get; set; }
        public Guid PaymentTypeKey { get; set; }

        public TransportTypeListModel TransportTypeCollection { get; set; }
        public PaymentTypeListModel PaymentTypeCollection { get; set; }

        public TransportTypeModel GetSelectedTransportType()
        {
            foreach (TransportTypeModel item in this.TransportTypeCollection.Items)
            {
                if (item.pk == this.TransportTypeKey)
                {
                    return item;
                }
            }

            return this.TransportTypeCollection.Items[0];
        }
        public Guid GetToBeSelectedTransportTypeKey()
        {
            Guid key = this.CurrentQuote.GetTransportTypeKey();
            return key == Guid.Empty ? this.TransportTypeCollection.Items[0].pk : key;
        }

        public PaymentTypeModel GetSelectedPaymentType()
        {
            foreach (PaymentTypeModel item in this.PaymentTypeCollection.Items)
            {
                if (item.pk == this.PaymentTypeKey)
                {
                    return item;
                }
            }

            return this.PaymentTypeCollection.Items[0];
        }
        public Guid GetToBeSelectedPaymentTypeKey()
        {
            Guid key = this.CurrentQuote.GetPaymentTypeKey();
            return key == Guid.Empty ? this.PaymentTypeCollection.Items[0].pk : key;
        }

        public string GetFreeTransportMessage()
        {
            if (!SysConstUtil.IsFreeTransportPriceAvailable)
            {
                return string.Empty;
            }
            if (this.CurrentQuote.IsFreeTransportPrice)
            {
                return "DOPRAVU MÁTE ZADARMO";
            }

            //return string.Format("K DOPRAVE ZADARMO VÁM CHÝBA {0}", PriceUtil.GetPriceString(this.CurrentQuote.PriceToAddForFreeTransport, trimDecZeros: true));
            return string.Format("ABY BOLA VAŠA OBJEDNÁVKA VYBAVENÁ CHÝBA EŠTE  {0}", PriceUtil.GetPriceStringWithCurrency(PriceUtil.GetUserFriendlyPriceString(this.CurrentQuote.PriceToAddForFreeTransport)));
        }
    }
    public class BasketReviewAndSendModel
    {
        public bool AgreePersonalDataNewsletter { get; set; }
        public bool AgreeTradeRules { get; set; }
    }

    public class QuoteNotifier
    {
        public static void SendQuoteStateNotification(Guid quoteKey, string imgPath, bool isCardPayment, bool notifyEshoppgsoftweb = true, string moreInfo = "")
        {
            QuoteModel quote = QuoteModel.GetCompleteModel(quoteKey);
            string quoteUrl = string.Format("{0}{1}", new _BaseControllerUtil().SiteRootUrl, quote.GetQuoteDetailUrl());
            string quoteCardPaymentUrl = quoteUrl;

            // Quote payment info
            List<TextTemplateParam> paymentParamList = new List<TextTemplateParam>();
            paymentParamList.Add(new TextTemplateParam("URL_CARD_PAYMENT", quoteCardPaymentUrl));
            paymentParamList.Add(new TextTemplateParam("CARD_MOUNT", quote.TotalPriceWithVatWithCurrency));
            string msgPaymentInfo = isCardPayment ?
                TextTemplate.GetTemplateText("QuotePaymentCard_Sk", paymentParamList) :
                string.Empty;
            //TextTemplate.GetTemplateText("QuotePaymentBank_Sk", paymentParamList);

            // Quote items
            string msgProductHeader = TextTemplate.GetTemplateText("QuoteProductHeader_Sk", new List<TextTemplateParam>());
            string msgProductFooter = TextTemplate.GetTemplateText("QuoteProductFooter_Sk", new List<TextTemplateParam>());
            StringBuilder msgProductBody = new StringBuilder();
            bool isAltRow = false;
            foreach (Product2QuoteModel product in quote.Items)
            {
                List<TextTemplateParam> itemParamList = new List<TextTemplateParam>();
                itemParamList.Add(new TextTemplateParam("BACKGROUND_COLOR", isAltRow ? "#F5F5F5" : "#FFFFFF"));
                itemParamList.Add(new TextTemplateParam("CODE", product.IsProductItem ? string.Format("{0}", product.ProductCode) : string.Empty));
                itemParamList.Add(new TextTemplateParam("NAME", product.ProductName));
                itemParamList.Add(new TextTemplateParam("PCS", product.IsProductItem ? product.ItemPcs.ToString() : string.Empty));
                itemParamList.Add(new TextTemplateParam("PRICE_NO_VAT", product.TotalPriceWithCurrencyNoVat));
                itemParamList.Add(new TextTemplateParam("PRICE_WITH_VAT", product.TotalPriceWithCurrencyWithVat));
                itemParamList.Add(new TextTemplateParam("UNIT", product.UnitName));
                msgProductBody.AppendLine(TextTemplate.GetTemplateText("QuoteProductItem_Sk", itemParamList));

                isAltRow = isAltRow ? false : true;
            }
            // Total price
            List<TextTemplateParam> totalParamList = new List<TextTemplateParam>();
            totalParamList.Add(new TextTemplateParam("BACKGROUND_COLOR", "#E5E5E5"));
            totalParamList.Add(new TextTemplateParam("CODE", string.Empty));
            totalParamList.Add(new TextTemplateParam("NAME", "CENA CELKOM"));
            totalParamList.Add(new TextTemplateParam("PCS", string.Empty));
            totalParamList.Add(new TextTemplateParam("PRICE_NO_VAT", quote.TotalPriceNoVatWithCurrency));
            totalParamList.Add(new TextTemplateParam("PRICE_WITH_VAT", quote.TotalPriceWithVatWithCurrency));
            totalParamList.Add(new TextTemplateParam("UNIT", string.Empty));
            msgProductBody.AppendLine(TextTemplate.GetTemplateText("QuoteProductItem_Sk", totalParamList));
            // Quote items table
            string msgProductList = string.Format("{0}{1}{2}", msgProductHeader, msgProductBody.ToString(), msgProductFooter);

            // Invoice for company
            string invoiceForCompany = string.Empty;
            if (quote.User.IsCompanyInvoice)
            {
                List<TextTemplateParam> invcParamList = new List<TextTemplateParam>();
                invcParamList.Add(new TextTemplateParam("COMPANY", quote.User.CompanyName));
                invcParamList.Add(new TextTemplateParam("ICO", quote.User.CompanyIco));
                invcParamList.Add(new TextTemplateParam("DIC", quote.User.CompanyDic));
                invcParamList.Add(new TextTemplateParam("ICDPH", quote.User.CompanyIcdph));
                invoiceForCompany = TextTemplate.GetTemplateText("QuoteInvoiceCompany_Sk", invcParamList);
            }

            // Different delivery address
            string deliveryAddress = string.Empty;
            if (quote.User.IsDeliveryAddress)
            {
                List<TextTemplateParam> adrParamList = new List<TextTemplateParam>();
                adrParamList.Add(new TextTemplateParam("NAME", quote.User.DeliveryName));
                adrParamList.Add(new TextTemplateParam("STREET", quote.User.DeliveryStreet));
                adrParamList.Add(new TextTemplateParam("CITY", quote.User.DeliveryCity));
                adrParamList.Add(new TextTemplateParam("ZIP", quote.User.DeliveryZip));
                adrParamList.Add(new TextTemplateParam("COUNTRY", quote.User.DeliveryCountry));
                deliveryAddress = TextTemplate.GetTemplateText("QuoteDeliveryAddress_Sk", adrParamList);
            }

            List<TextTemplateParam> paramList = new List<TextTemplateParam>();
            paramList.Add(new TextTemplateParam("QUOTE_ID", quote.QuoteId));
            paramList.Add(new TextTemplateParam("DT_CREATE", quote.DateFinishedView));
            paramList.Add(new TextTemplateParam("QUOTE_STATE", string.IsNullOrEmpty(quote.QuoteState) ? "ZAEVIDOVANÁ" : quote.QuoteState));
            paramList.Add(new TextTemplateParam("MORE_INFO", moreInfo));

            paramList.Add(new TextTemplateParam("TRANSPORT_TYPE", quote.GetTransportTypeName()));
            paramList.Add(new TextTemplateParam("PAYMENT_TYPE", quote.GetPaymentTypeName()));
            paramList.Add(new TextTemplateParam("PAYMENT_INFO", msgPaymentInfo));
            paramList.Add(new TextTemplateParam("NAME", quote.User.InvName));
            paramList.Add(new TextTemplateParam("EMAIL", quote.User.QuoteEmail));
            paramList.Add(new TextTemplateParam("PHONE", quote.User.QuotePhone));
            paramList.Add(new TextTemplateParam("STREET", quote.User.InvStreet));
            paramList.Add(new TextTemplateParam("CITY", quote.User.InvCity));
            paramList.Add(new TextTemplateParam("ZIP", quote.User.InvZip));
            paramList.Add(new TextTemplateParam("COUNTRY", quote.User.InvCountry));
            paramList.Add(new TextTemplateParam("INVOICE_COMPANY", invoiceForCompany));
            paramList.Add(new TextTemplateParam("DELIVERY_ADDRESS", deliveryAddress));

            paramList.Add(new TextTemplateParam("TEXT", quote.User.Note));

            paramList.Add(new TextTemplateParam("PRODUCT_LIST", msgProductList));

            paramList.Add(new TextTemplateParam("QUOTE_URL", quoteUrl));
            //paramList.Add(new TextTemplateParam("CONTRACT_URL_1", string.Format("{0}{1}", new _BaseControllerUtil().SiteRootUrl, ConfigurationUtil.GetCfgValue(ConfigurationUtil.Ecommerce_Document_Contract_1))));
            //paramList.Add(new TextTemplateParam("CONTRACT_URL_2", string.Format("{0}{1}", new _BaseControllerUtil().SiteRootUrl, ConfigurationUtil.GetCfgValue(ConfigurationUtil.Ecommerce_Document_Contract_2))));

            //paramList.Add(new TextTemplateParam("BANK", SysConstUtil.AppConst.Bank));
            //paramList.Add(new TextTemplateParam("IBAN", SysConstUtil.AppConst.Iban));
            //paramList.Add(new TextTemplateParam("VS", quote.QuoteId));
            //paramList.Add(new TextTemplateParam("TOTAL_PRICE", quote.TotalPriceWithCurrency));


            string msgText = TextTemplate.GetTemplateText("QuoteStateNotification_Sk", paramList);

            PdfFilePrintResult pdfResult = QuoteToPdf.GetQuotePdf(quote, imgPath);
            Attachment pdfAttachment = new Attachment(new MemoryStream(pdfResult.FileContent), pdfResult.FileName);

            if (notifyEshoppgsoftweb)
            {
                // Odoslanie uzivatelovi aj spravcovi TAKFAJN
                EshoppgsoftwebMailer.SendMailTemplate(
                    "Vaša pgsoftweb.sk objednávka",
                    msgText,
                    quote.User.QuoteEmailForNotification, pdfAttachment);
            }
            else
            {
                // Odoslanie iba uzivatelovi
                EshoppgsoftwebMailer.SendMailTemplateWithoutBcc(
                    "Vaša pgsoftweb.sk objednávka",
                    msgText,
                    quote.User.QuoteEmailForNotification, pdfAttachment);
            }
        }
    }

    public class QuoteListModel : _PagingModel
    {
        public List<QuoteForList> Items { get; set; }

        public HttpRequest CurrentRequest { get; private set; }
        private const string cPagerParamName = "stranka";
        private GridPagerModel quotesPager;
        public GridPagerModel QuotesPager
        {
            get
            {
                return GetQuotesPager();
            }
        }

        GridPagerModel GetQuotesPager()
        {
            if (this.quotesPager == null || this.quotesPager.ItemCnt != this.TotalItems)
            {
                NameValueCollection queryParams = new NameValueCollection();
                this.quotesPager =
                    new GridPagerModel(this.CurrentRequest, this.TotalItems, this.ItemsPerPage, queryParams,
                        queryPageParam: QuoteListModel.cPagerParamName);
            }

            return this.quotesPager;
        }


        public static QuoteListModel CreateCopyFrom(HttpRequest request, Page<QuoteForList> srcArray)
        {
            QuoteListModel trgArray = new QuoteListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<QuoteForList>(srcArray.Items.Count + 1);
            trgArray.CurrentRequest = request;

            foreach (QuoteForList src in srcArray.Items)
            {
                trgArray.Items.Add(src);
            }

            return trgArray;
        }

        public static int GetCurrentPageNumber(HttpRequest request)
        {
            return GridPagerModel.GetRequestPageNumber(request, queryPageParam: QuoteListModel.cPagerParamName);
        }
    }

    public class QuoteStateItemListModel
    {
        public List<QuoteStateItemModel> Items { get; set; }
        public static QuoteStateItemListModel CreateFromRepository()
        {
            QuoteStateItemListModel ret = new QuoteStateItemListModel();
            ret.Items = new List<QuoteStateItemModel>();

            foreach (string state in new QuoteRepository().GetAllStates())
            {
                if (!string.IsNullOrEmpty(state))
                {
                    ret.Items.Add(new QuoteStateItemModel() { Name = state });
                }
            }

            return ret;
        }
    }
    public class QuoteStateItemModel
    {
        public string Name { get; set; }
    }

    public class QuoteListFilterModel : _BaseUserPropModel
    {

        [Display(Name = "Číslo objednávky")]
        public string QuoteId { get; set; }
        [Display(Name = "Vyhľadávanie (meno, e-mail, telefón ...)")]
        public string SearchText { get; set; }

        [Display(Name = "Od")]
        public string From { get; set; }
        [Display(Name = "Do")]
        public string To { get; set; }

        public string[] QuoteStates { get; set; }
        public QuoteStateItemListModel AllQuoteStates { get; set; }


        public QuoteListFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_QuoteListFilterModel;
        }

        public static QuoteListFilterModel CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            QuoteListFilterModel trg = new QuoteListFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(QuoteListFilterModel src)
        {
            src.UpdateAfterEdit();
            EshoppgsoftwebUserProp trg = new EshoppgsoftwebUserProp();
            src.CopyDataTo(trg);

            return trg;
        }

        public void Clear()
        {
            this.SearchText = null;
            this.From = null;
            this.To = null;
            this.QuoteStates = null;
        }


        public void UpdateBeforeEdit()
        {
            LoadPropValue(this.PropValue);
        }

        public void UpdateAfterEdit()
        {
            this.PropValue = SavePropValue();
        }

        private string SavePropValue()
        {
            // Create XML document
            XmlDocument doc = new XmlDocument();
            // Create main element
            XmlElement mainNode = doc.CreateElement("QuoteListFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            // Quote number
            XmlParamSet.SaveItem(doc, mainNode, "QuoteId", this.QuoteId);
            // Search text
            XmlParamSet.SaveItem(doc, mainNode, "SearchText", this.SearchText);
            // From
            XmlParamSet.SaveItem(doc, mainNode, "From", this.From);
            // To
            XmlParamSet.SaveItem(doc, mainNode, "To", this.To);
            // Quote states
            int cnt = this.QuoteStates == null ? 0 : this.QuoteStates.Length;
            XmlParamSet.SaveIntItem(doc, mainNode, "QuoteStatesCnt", cnt);
            for (int idx = 0; idx < cnt; idx++)
            {
                XmlParamSet.SaveItem(doc, mainNode, string.Format("QuoteStates{0}", idx), this.QuoteStates[idx]);
            }

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "QuoteListFilterModel";

                // Quote number
                this.QuoteId = XmlParamSet.LoadItem(doc, fullParent, "QuoteId", string.Empty);
                // Search text
                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
                // From
                this.From = XmlParamSet.LoadItem(doc, fullParent, "From", string.Empty);
                // To
                this.To = XmlParamSet.LoadItem(doc, fullParent, "To", string.Empty);
                // Quote states
                int cnt = XmlParamSet.LoadIntItem(doc, fullParent, "QuoteStatesCnt", 0);
                if (cnt > 0)
                {
                    this.QuoteStates = new string[cnt];
                    for (int idx = 0; idx < cnt; idx++)
                    {
                        this.QuoteStates[idx] = XmlParamSet.LoadItem(doc, fullParent, string.Format("QuoteStates{0}", idx), null);
                    }
                }
            }
        }

        public bool IsQuoteStateChecked(QuoteStateItemModel state)
        {
            if (this.QuoteStates != null)
            {
                for (int idx = 0; idx < this.QuoteStates.Length; idx++)
                {
                    if (this.QuoteStates[idx] == state.Name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public DateTime? GetDateTimeFrom()
        {
            return DateTimeUtil.DisplayDateToDate(this.From);
        }

        public DateTime? GetDateTimeTo()
        {
            DateTime? dtTo = DateTimeUtil.DisplayDateToDate(this.To);
            if (dtTo != null)
            {
                dtTo = dtTo.Value.AddDays(1).AddMilliseconds(-1);
            }

            return dtTo;
        }
    }
}
