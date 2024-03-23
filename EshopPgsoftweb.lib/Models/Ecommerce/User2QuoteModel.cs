using dufeksoft.lib.Mail;
using dufeksoft.lib.Model;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class User2QuoteModel : _BaseModel
    {
        public bool AgreeRules { get; set; }

        public Guid PkQuote { get; set; }
        public Guid PkUser { get; set; }


        [Display(Name = "Chcem fakturovať na firmu")]
        public bool IsCompanyInvoice { get; set; }
        [Display(Name = "Názov firmy")]
        public string CompanyName { get; set; }
        [Ico(ErrorMessage = "Neplatné IČO")]
        [Display(Name = "IČO")]
        public string CompanyIco { get; set; }
        [Dic(ErrorMessage = "Neplatné DIČ")]
        [Display(Name = "DIČ")]
        public string CompanyDic { get; set; }
        [Icdph(ErrorMessage = "Neplatné IČ DPH")]
        [Display(Name = "IČ DPH")]
        public string CompanyIcdph { get; set; }

        [Required(ErrorMessage = "Meno a priezvisko musí byť zadané")]
        [Display(Name = "Meno a priezvisko")]
        public string InvName { get; set; }
        [Required(ErrorMessage = "Ulica a číslo domu musí byť zadané")]
        [Display(Name = "Ulica a číslo domu")]
        public string InvStreet { get; set; }
        [Required(ErrorMessage = "Obec musí byť zadaná")]
        [Display(Name = "Obec")]
        public string InvCity { get; set; }
        [Required(ErrorMessage = "PSČ musí byť zadané")]
        [Display(Name = "PSČ")]
        public string InvZip { get; set; }
        [RequiredGuidDropDown(ErrorMessage = "Krajina musí byť zadaná")]
        [Display(Name = "Krajina")]
        public string InvCountryCollectionKey { get; set; }
        public string InvCountry { get; set; }
        public string InvCountryCode { get; set; }

        [Display(Name = "Doručovacia adresa je iná ako fakturačná")]
        public bool IsDeliveryAddress { get; set; }
        [Display(Name = "Meno a priezvisko")]
        public string DeliveryName { get; set; }
        [Display(Name = "Ulica a číslo domu")]
        public string DeliveryStreet { get; set; }
        [Display(Name = "Obec")]
        public string DeliveryCity { get; set; }
        [Display(Name = "PSČ")]
        public string DeliveryZip { get; set; }
        [Display(Name = "Krajina")]
        public string DeliveryCountryCollectionKey { get; set; }
        public string DeliveryCountry { get; set; }
        public string DeliveryCountryCode { get; set; }


        [Required(ErrorMessage = "E-mail musí byť zadaný")]
        [EmailAddress(ErrorMessage = "Neplatná e-mailová adresa.")]
        [Display(Name = "E-mail")]
        public string QuoteEmail { get; set; }
        [Required(ErrorMessage = "Telefón musí byť zadaný")]
        [Display(Name = "Telefón")]
        public string QuotePhone { get; set; }


        [Display(Name = "Vaša poznámka")]
        public string Note { get; set; }

        public string QuoteEmailForNotification
        {
            get
            {
                return MailAddressHelper.GetFirstEmail(this.QuoteEmail);
            }
        }

        User2QuoteDropDowns m_dropDowns = null;
        public User2QuoteDropDowns DropDowns
        {
            get
            {
                if (this.m_dropDowns == null)
                {
                    this.m_dropDowns = new User2QuoteDropDowns();
                }

                return this.m_dropDowns;
            }
        }

        public bool IsInternationalInvoice
        {
            get
            {
                return CountryModel.IsNotDomesticCountry(this.InvCountryCode);
            }
        }

        public bool IsInternationalTransport
        {
            get
            {
                return CountryModel.IsNotDomesticCountry(this.RealDeliveryCountryCode);
            }
        }
        public string RealDeliveryCountryCode
        {
            get
            {
                return this.IsDeliveryAddress ? this.DeliveryCountryCode : this.InvCountryCode;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return GetIsEmtpy();
            }
        }

        bool GetIsEmtpy()
        {
            if (!string.IsNullOrEmpty(this.InvName))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(this.InvStreet))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(this.InvCity))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(this.InvZip))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(this.QuoteEmail))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(this.QuotePhone))
            {
                return false;
            }

            return true;
        }

        public void CopyDataFrom(User2Quote src, User2QuoteDropDowns dropDowns)
        {
            this.m_dropDowns = dropDowns;

            this.pk = src.pk;
            this.PkQuote = src.PkQuote;
            this.PkUser = src.PkUser;

            this.IsCompanyInvoice = src.IsCompanyInvoice;
            this.CompanyName = src.CompanyName;
            this.CompanyIco = src.CompanyIco;
            this.CompanyDic = src.CompanyDic;
            this.CompanyIcdph = src.CompanyIcdph;

            this.InvName = src.InvName;
            this.InvStreet = src.InvStreet;
            this.InvCity = src.InvCity;
            this.InvZip = src.InvZip;
            // Set country
            this.InvCountry = src.InvCountry;
            this.InvCountryCollectionKey = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForCountryName(this.InvCountry);
                if (ddiCountry != null)
                {
                    this.InvCountryCollectionKey = ddiCountry.DataKey;
                    this.InvCountryCode = (ddiCountry.Data as CountryModel).Code;
                }
            }

            this.IsDeliveryAddress = src.IsDeliveryAddress;
            this.DeliveryName = src.DeliveryName;
            this.DeliveryStreet = src.DeliveryStreet;
            this.DeliveryCity = src.DeliveryCity;
            this.DeliveryZip = src.DeliveryZip;
            // Set delivery country
            this.DeliveryCountry = src.DeliveryCountry;
            this.DeliveryCountryCollectionKey = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForCountryName(this.DeliveryCountry);
                if (ddiCountry != null)
                {
                    this.DeliveryCountryCollectionKey = ddiCountry.DataKey;
                    this.DeliveryCountryCode = (ddiCountry.Data as CountryModel).Code;
                }
            }

            this.QuoteEmail = src.QuoteEmail;
            this.QuotePhone = src.QuotePhone;

            this.Note = src.Note;
        }

        public void CopyDataTo(User2Quote trg, User2QuoteDropDowns dropDowns)
        {
            this.m_dropDowns = dropDowns;

            trg.pk = this.pk;
            trg.PkQuote = this.PkQuote;
            trg.PkUser = this.PkUser;

            trg.IsCompanyInvoice = this.IsCompanyInvoice;
            trg.CompanyName = this.CompanyName;
            trg.CompanyIco = this.CompanyIco;
            trg.CompanyDic = this.CompanyDic;
            trg.CompanyIcdph = this.CompanyIcdph;

            trg.InvName = this.InvName;
            trg.InvStreet = this.InvStreet;
            trg.InvCity = this.InvCity;
            trg.InvZip = this.InvZip;
            // Set country
            trg.InvCountry = this.InvCountry;
            CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForKey(this.InvCountryCollectionKey);
            if (ddiCountry != null)
            {
                trg.InvCountry = ((CountryModel)ddiCountry.Data).Name;
            }

            trg.IsDeliveryAddress = this.IsDeliveryAddress;
            trg.DeliveryName = this.DeliveryName;
            trg.DeliveryStreet = this.DeliveryStreet;
            trg.DeliveryCity = this.DeliveryCity;
            trg.DeliveryZip = this.DeliveryZip;
            // Set delivery country
            trg.DeliveryCountry = this.DeliveryCountry;
            CmpDropDownItem ddiDeliveryCountry = this.DropDowns.CountryCollection.GetItemForKey(this.DeliveryCountryCollectionKey);
            if (ddiDeliveryCountry != null)
            {
                trg.DeliveryCountry = ddiDeliveryCountry.DataKey == Guid.Empty.ToString() ? null : ((CountryModel)ddiDeliveryCountry.Data).Name;
            }

            trg.QuoteEmail = this.QuoteEmail;
            trg.QuotePhone = this.QuotePhone;

            trg.Note = this.Note;
        }

        public static User2QuoteModel CreateCopyFrom(User2Quote src, User2QuoteDropDowns dropDowns)
        {
            User2QuoteModel trg = new User2QuoteModel();
            trg.CopyDataFrom(src, dropDowns);

            return trg;
        }

        public static User2Quote CreateCopyFrom(User2QuoteModel src, User2QuoteDropDowns dropDowns)
        {
            User2Quote trg = new User2Quote();
            src.CopyDataTo(trg, dropDowns);

            return trg;
        }

        public void CopyDataFrom(EshoppgsoftwebCustomer customer, CountryDropDown ddCountry)
        {
            this.QuoteEmail = customer.Email;
            this.QuotePhone = customer.Phone;

            this.InvName = customer.Name;
            this.InvStreet = customer.Street;
            this.InvCity = customer.City;
            this.InvZip = customer.Zip;
            // Set country
            this.InvCountry = string.Empty;
            this.InvCountryCollectionKey = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForKey(customer.CountryKey.ToString());
                if (ddiCountry != null)
                {
                    this.InvCountry = ((CountryModel)ddiCountry.Data).Name;
                    this.InvCountryCollectionKey = ddiCountry.DataKey;
                }
            }

            if (!string.IsNullOrEmpty(customer.Ico))
            {
                this.IsCompanyInvoice = true;
                this.CompanyName = customer.Name;
                this.CompanyIco = customer.Ico;
                this.CompanyDic = customer.Dic;
                this.CompanyIcdph = customer.Icdph;

                this.InvName = customer.ContactName;
            }

            if (customer.IsDeliveryAddress)
            {
                this.IsDeliveryAddress = true;
                this.DeliveryName = customer.DeliveryName;
                this.DeliveryStreet = customer.DeliveryStreet;
                this.DeliveryCity = customer.DeliveryCity;
                this.DeliveryZip = customer.DeliveryZip;
                // Set delivery country
                this.DeliveryCountry = string.Empty;
                this.DeliveryCountryCollectionKey = string.Empty;
                if (this.DropDowns != null)
                {
                    CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForKey(customer.DeliveryCountryKey.ToString());
                    if (ddiCountry != null)
                    {
                        this.DeliveryCountry = ((CountryModel)ddiCountry.Data).Name;
                        this.DeliveryCountryCollectionKey = ddiCountry.DataKey;
                    }
                }
                if (!string.IsNullOrEmpty(customer.DeliveryPhone))
                {
                    this.QuotePhone = customer.DeliveryPhone;
                }
            }
        }

        public string GetInvoiceAddress()
        {
            StringBuilder str = new StringBuilder();

            if (this.IsCompanyInvoice)
            {
                str.AppendLine(this.CompanyName);
            }
            str.AppendLine(this.InvName);
            str.AppendLine(this.InvStreet);
            str.AppendLine(string.Format("{0} {1}", this.InvZip, this.InvCity));
            str.AppendLine(this.InvCountry);

            return str.ToString();
        }
        public string GetDeliveryAddress()
        {
            if (!this.IsDeliveryAddress)
            {
                return GetInvoiceAddress();
            }

            StringBuilder str = new StringBuilder();

            if (this.IsCompanyInvoice)
            {
                str.AppendLine(this.CompanyName);
            }
            str.AppendLine(this.DeliveryName);
            str.AppendLine(this.DeliveryStreet);
            str.AppendLine(string.Format("{0} {1}", this.DeliveryZip, this.DeliveryCity));
            str.AppendLine(this.DeliveryCountry);

            return str.ToString();
        }
        public MvcHtmlString TextToHtml(string text)
        {
            return MvcHtmlString.Create(string.IsNullOrEmpty(text) ? "" : text.Replace("\n", "<br />"));
        }
    }

    public class User2QuoteDropDowns
    {
        public CountryDropDown CountryCollection { get; set; }

        public User2QuoteDropDowns()
        {
            this.CountryCollection = CountryDropDown.CreateFromRepository(true);
        }
    }
}
