using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class CountryModel : _BaseModel
    {
        [Required(ErrorMessage = "Kód musí byť zadaný")]
        [Display(Name = "Kód")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Názov musí byť zadaný")]
        [Display(Name = "Názov")]
        public string Name { get; set; }

        public void CopyDataFrom(Country src)
        {
            this.pk = src.pk;
            this.Code = src.Code;
            this.Name = src.Name;
        }

        public void CopyDataTo(Country trg)
        {
            trg.pk = this.pk;
            trg.Code = this.Code;
            trg.Name = this.Name;
        }

        public static CountryModel CreateCopyFrom(Country src)
        {
            CountryModel trg = new CountryModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static Country CreateCopyFrom(CountryModel src)
        {
            Country trg = new Country();
            src.CopyDataTo(trg);

            return trg;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Code, this.Name);
        }

        public static bool IsNotDomesticCountry(string countryCode)
        {
            return !string.IsNullOrEmpty(countryCode) && countryCode != "SK";
        }
    }

    public class CountryPagingListModel : _PagingModel
    {
        public List<CountryModel> Items { get; set; }

        public static CountryPagingListModel CreateCopyFrom(Page<Country> srcArray)
        {
            CountryPagingListModel trgArray = new CountryPagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<CountryModel>(srcArray.Items.Count + 1);

            foreach (Country src in srcArray.Items)
            {
                trgArray.Items.Add(CountryModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class CountryListModel
    {
        public List<CountryModel> Items { get; set; }

        public static CountryListModel CreateCopyFrom(List<Country> srcArray)
        {
            CountryListModel trgArray = new CountryListModel();
            trgArray.Items = new List<CountryModel>(srcArray.Count + 1);

            foreach (Country src in srcArray)
            {
                trgArray.Items.Add(CountryModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class CountryDropDown : CmpDropDown
    {
        public const string DefautCountryCode = "SK";

        Hashtable htCountryName;

        public CountryDropDown()
        {
            this.htCountryName = new Hashtable();
        }

        public static CountryDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            CountryRepository repository = new CountryRepository();
            return CountryDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static CountryDropDown CreateCopyFrom(Page<Country> dataList, bool allowNull, string emptyText)
        {
            CountryDropDown ret = new CountryDropDown();

            if (allowNull)
            {
                ret.AddCountryItem(emptyText, Guid.Empty.ToString(), null);
            }
            // Default country
            foreach (Country dataItem in dataList.Items)
            {
                if (dataItem.Code == CountryDropDown.DefautCountryCode)
                {
                    CountryModel dataModel = CountryModel.CreateCopyFrom(dataItem);
                    ret.AddCountryItem(dataModel.ToString(), dataModel.pk.ToString(), dataModel);
                }
            }
            // Other countries
            foreach (Country dataItem in dataList.Items)
            {
                if (dataItem.Code != CountryDropDown.DefautCountryCode)
                {
                    CountryModel dataModel = CountryModel.CreateCopyFrom(dataItem);
                    ret.AddCountryItem(dataModel.ToString(), dataModel.pk.ToString(), dataModel);
                }
            }

            return ret;
        }

        public void AddCountryItem(string name, string key, CountryModel data)
        {
            CmpDropDownItem ddItem = this.AddItem(name, key, data);
            if (data != null)
            {
                if (!this.htCountryName.ContainsKey(data.Name))
                {
                    this.htCountryName.Add(data.Name, ddItem);
                }
            }
        }

        public CmpDropDownItem GetItemForCountryName(string countryName)
        {
            if (string.IsNullOrEmpty(countryName))
            {
                return null;
            }
            return this.htCountryName.ContainsKey(countryName) ? (CmpDropDownItem)this.htCountryName[countryName] : null;
        }
    }
}
