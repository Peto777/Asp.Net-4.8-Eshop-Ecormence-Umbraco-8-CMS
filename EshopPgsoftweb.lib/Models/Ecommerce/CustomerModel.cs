using dufeksoft.lib.Model;
using dufeksoft.lib.ParamSet;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class CustomerModel : _BaseModel
    {
        [Display(Name = "Užívateľ")]
        public int OwnerCollectionId { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        [Required(ErrorMessage = "Firma (meno a priezvisko) musí byť zadané")]
        [Display(Name = "Firma (meno a priezvisko)")]
        public string Name { get; set; }
        [RequiredGuidDropDown(ErrorMessage = "Krajina musí byť zadaná")]
        [Display(Name = "Krajina")]
        public string CountryCollectionKey { get; set; }
        public Guid CountryKey { get; set; }
        public string CountryName { get; set; }
        [Required(ErrorMessage = "Ulica a číslo domu musí byť zadané")]
        [Display(Name = "Ulica a číslo domu")]
        public string Street { get; set; }
        [Required(ErrorMessage = "Obec musí byť zadaná")]
        [Display(Name = "Obec")]
        public string City { get; set; }
        [Required(ErrorMessage = "PSČ musí byť zadané")]
        [Display(Name = "PSČ")]
        public string Zip { get; set; }
        [Required(ErrorMessage = "Telefón musí byť zadaný")]
        [Display(Name = "Telefón")]
        public string Phone { get; set; }

        [Email(ErrorMessage = "Neplatný tvar e-mailovej adresy")]
        [Required(ErrorMessage = "E-mail musí byť zadaný")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Ico(ErrorMessage = "Neplatné IČO")]
        [Display(Name = "IČO")]
        public string Ico { get; set; }
        [Dic(ErrorMessage = "Neplatné DIČ")]
        [Display(Name = "DIČ")]
        public string Dic { get; set; }
        [Icdph(ErrorMessage = "Neplatné IČ DPH")]
        [Display(Name = "IČ DPH")]
        public string Icdph { get; set; }
        [Display(Name = "Kontaktná osoba")]
        public string ContactName { get; set; }

        [Display(Name = "Adresa doručenia je iná ako fakturačná adresa")]
        public bool IsDeliveryAddress { get; set; }
        [Display(Name = "Doručovacia firma (meno a priezvisko)")]
        public string DeliveryName { get; set; }
        [Display(Name = "Doručovacia krajina")]
        public string DeliveryCountryCollectionKey { get; set; }
        public Guid? DeliveryCountryKey { get; set; }
        public string DeliveryCountryName { get; set; }
        [Display(Name = "Doručovacia ulica a číslo domu")]
        public string DeliveryStreet { get; set; }
        [Display(Name = "Doručovacia obec")]
        public string DeliveryCity { get; set; }
        [Display(Name = "Doručovacie PSČ")]
        public string DeliveryZip { get; set; }
        [Display(Name = "Doručovací telefón")]
        public string DeliveryPhone { get; set; }

        public CustomerDropDowns DropDowns { get; set; }


        public CustomerModel()
        {
        }

        public CustomerModel(CustomerDropDowns dropDowns) : this()
        {
            this.DropDowns = dropDowns;
        }

        public void CopyDataFrom(EshoppgsoftwebCustomer src, CustomerDropDowns dropDowns)
        {
            this.DropDowns = dropDowns;
            if (src == null)
            {
                return;
            }

            this.pk = src.pk;
            this.Name = src.Name;

            // Set country
            this.CountryKey = src.CountryKey;
            this.CountryCollectionKey = string.Empty;
            this.CountryName = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForKey(this.CountryKey.ToString());
                if (ddiCountry != null)
                {
                    this.CountryCollectionKey = ddiCountry.DataKey;
                    this.CountryName = ddiCountry.Name;
                }
            }

            this.Street = src.Street;
            this.City = src.City;
            this.Zip = src.Zip;
            this.Phone = src.Phone;
            this.Email = src.Email;

            this.Ico = src.Ico;
            this.Dic = src.Dic;
            this.Icdph = src.Icdph;
            this.ContactName = src.ContactName;

            this.IsDeliveryAddress = src.IsDeliveryAddress;
            this.DeliveryName = src.DeliveryName;

            // Set delivery country
            this.DeliveryCountryKey = src.DeliveryCountryKey;
            this.DeliveryCountryCollectionKey = string.Empty;
            this.DeliveryCountryName = string.Empty;
            if (this.DropDowns != null)
            {
                CmpDropDownItem ddiDeliveryCountry = this.DropDowns.CountryCollection.GetItemForKey(this.DeliveryCountryKey.ToString());
                if (ddiDeliveryCountry != null)
                {
                    this.DeliveryCountryCollectionKey = ddiDeliveryCountry.DataKey;
                    this.DeliveryCountryName = ddiDeliveryCountry.Name;
                }
            }

            this.DeliveryStreet = src.DeliveryStreet;
            this.DeliveryCity = src.DeliveryCity;
            this.DeliveryZip = src.DeliveryZip;
            this.DeliveryPhone = src.DeliveryPhone;

            // Set owner
            this.OwnerId = src.OwnerId;
            this.OwnerCollectionId = 0;
            this.OwnerName = string.Empty;
            if (this.DropDowns != null)
            {
                if (this.DropDowns.MemberCollection != null)
                {
                    CmpDropDownItem ddiOwner = this.DropDowns.MemberCollection.GetItemForKey(this.OwnerId.ToString());
                    if (ddiOwner != null)
                    {
                        this.OwnerCollectionId = ddiOwner.Id;
                        if (this.OwnerId > 0)
                        {
                            this.OwnerName = ddiOwner.Name;
                        }
                    }
                }
            }
        }

        public void CopyDataTo(EshoppgsoftwebCustomer trg, CustomerDropDowns dropDowns)
        {
            this.DropDowns = dropDowns;
            trg.pk = this.pk;
            trg.Name = this.Name;

            // Set country
            trg.CountryKey = Guid.Empty;
            CmpDropDownItem ddiCountry = this.DropDowns.CountryCollection.GetItemForKey(this.CountryCollectionKey);
            if (ddiCountry != null)
            {
                trg.CountryKey = new Guid(ddiCountry.DataKey);
            }
            trg.Street = this.Street;
            trg.City = this.City;
            trg.Zip = this.Zip;
            trg.Phone = this.Phone;
            trg.Email = this.Email;

            trg.Ico = this.Ico;
            trg.Dic = this.Dic;
            trg.Icdph = this.Icdph;
            trg.ContactName = this.ContactName;

            trg.IsDeliveryAddress = this.IsDeliveryAddress;
            trg.DeliveryName = this.DeliveryName;

            // Set delivery country
            trg.DeliveryCountryKey = null;
            CmpDropDownItem ddiDeliveryCountry = this.DropDowns.CountryCollection.GetItemForKey(this.DeliveryCountryCollectionKey);
            if (ddiDeliveryCountry != null)
            {
                trg.DeliveryCountryKey = new Guid(ddiDeliveryCountry.DataKey);
            }
            trg.DeliveryStreet = this.DeliveryStreet;
            trg.DeliveryCity = this.DeliveryCity;
            trg.DeliveryZip = this.DeliveryZip;
            trg.DeliveryPhone = this.DeliveryPhone;

            // Set owner
            trg.OwnerId = this.OwnerId;
            if (this.DropDowns.MemberCollection != null)
            {
                CmpDropDownItem ddiOwner = this.DropDowns.MemberCollection.GetItemForId(this.OwnerCollectionId);
                if (ddiOwner != null)
                {
                    trg.OwnerId = int.Parse(ddiOwner.DataKey);
                    if (trg.OwnerId > 0)
                    {
                        this.OwnerName = ddiOwner.Name;
                    }
                }
            }
        }

        public static CustomerModel CreateCopyFrom(EshoppgsoftwebCustomer src, CustomerDropDowns dropDowns)
        {
            CustomerModel trg = new CustomerModel();
            trg.CopyDataFrom(src, dropDowns);

            return trg;
        }

        public static EshoppgsoftwebCustomer CreateCopyFrom(CustomerModel src, CustomerDropDowns dropDowns)
        {
            EshoppgsoftwebCustomer trg = new EshoppgsoftwebCustomer();
            src.CopyDataTo(trg, dropDowns);

            return trg;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Name);
        }

        public static CustomerModel GetCurrentCustomerModel(CustomerDropDowns dropDowns)
        {
            return CustomerModel.CreateCopyFrom(GetCurrentCustomer(), dropDowns);
        }

        public static EshoppgsoftwebCustomer GetCurrentCustomer()
        {
            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            int memberId = GetCurrentMemberId();
            EshoppgsoftwebCustomer customer = repository.GetForOwner(memberId);
            if (customer == null)
            {
                customer = new EshoppgsoftwebCustomer();
                customer.OwnerId = memberId;
            }

            return customer;
        }

        public static Guid GetCurrentCustomerKey()
        {
            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            return repository.GetForOwner(GetCurrentMemberId()).pk;
        }

        public static bool IsUserAuthenticated()
        {
            return System.Web.Security.Membership.GetUser() != null;
        }
        public static int GetCurrentMemberId()
        {
            System.Web.Security.MembershipUser user = System.Web.Security.Membership.GetUser();
            return (int)user.ProviderUserKey;
        }
        public static string GetCurrentMemberName()
        {
            System.Web.Security.MembershipUser user = System.Web.Security.Membership.GetUser();
            return user.UserName;
        }
        public static bool VerifyUserPassword(string userName, string userPassword)
        {
            return System.Web.Security.Membership.ValidateUser(userName, userPassword);
        }
    }

    public class CustomerListModel : _PagingModel
    {
        public List<CustomerModel> Items { get; set; }

        public static CustomerListModel CreateCopyFrom(Page<EshoppgsoftwebCustomer> srcArray, CustomerDropDowns dropDowns)
        {
            CustomerListModel trgArray = new CustomerListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<CustomerModel>(srcArray.Items.Count + 1);

            foreach (EshoppgsoftwebCustomer src in srcArray.Items)
            {
                trgArray.Items.Add(CustomerModel.CreateCopyFrom(src, dropDowns));
            }

            return trgArray;
        }
    }

    public class CustomerDropDowns
    {
        public EshoppgsoftwebMemberDropDown MemberCollection { get; set; }
        public CountryDropDown CountryCollection { get; set; }

        public CustomerDropDowns()
        {
            this.MemberCollection = EshoppgsoftwebMemberDropDown.CreateCustomerUserListFromRepository(false);
            this.CountryCollection = CountryDropDown.CreateFromRepository(true);
        }
    }

    public class CustomerFilterModel : _BaseUserPropModel
    {
        [Display(Name = "Vyhľadávanie (Meno, adresa, telefón, e-mail ...)")]
        public string SearchText { get; set; }

        public CustomerFilterModel()
        {
            this.PropId = ConfigurationUtil.PropId_CustomerFilterModel;
        }

        public static CustomerFilterModel CreateCopyFrom(EshoppgsoftwebUserProp src)
        {
            CustomerFilterModel trg = new CustomerFilterModel();
            if (src != null)
            {
                trg.CopyDataFrom(src);
            }
            trg.UpdateBeforeEdit();

            return trg;
        }

        public static EshoppgsoftwebUserProp CreateCopyFrom(CustomerFilterModel src)
        {
            src.UpdateAfterEdit();
            EshoppgsoftwebUserProp trg = new EshoppgsoftwebUserProp();
            src.CopyDataTo(trg);

            return trg;
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
            XmlElement mainNode = doc.CreateElement("CustomerFilterModel");
            mainNode.SetAttribute("version", "1.0");
            doc.AppendChild(mainNode);

            XmlParamSet.SaveItem(doc, mainNode, "SearchText", this.SearchText);

            return doc.InnerXml;
        }

        private void LoadPropValue(string propValue)
        {
            XmlDocument doc = new XmlDocument();
            if (!string.IsNullOrEmpty(propValue))
            {
                doc.LoadXml(propValue);

                string fullParent = "CustomerFilterModel";

                this.SearchText = XmlParamSet.LoadItem(doc, fullParent, "SearchText", string.Empty);
            }
        }
    }

    public class CustomerDropDown : CmpDropDown
    {
        public CustomerDropDown()
        {
        }

        public static CustomerDropDown CreateFromRepository(bool allowNull, string emptyText = "[ žiadny zákazník ]")
        {
            EshoppgsoftwebCustomerRepository repository = new EshoppgsoftwebCustomerRepository();
            return CustomerDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText, new CustomerDropDowns());
        }

        public static CustomerDropDown CreateCopyFrom(Page<EshoppgsoftwebCustomer> dataList, bool allowNull, string emptyText, CustomerDropDowns dropDowns)
        {
            CustomerDropDown ret = new CustomerDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (EshoppgsoftwebCustomer dataItem in dataList.Items)
            {
                CustomerModel dataModel = CustomerModel.CreateCopyFrom(dataItem, dropDowns);
                ret.AddItem(dataModel.Name, dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }
}
