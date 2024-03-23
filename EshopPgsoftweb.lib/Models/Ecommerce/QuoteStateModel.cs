using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class QuoteStateModel : _BaseModel
    {
        [Required(ErrorMessage = "Kód musí byť zadaný")]
        [Display(Name = "Kód")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Názov musí byť zadaný")]
        [Display(Name = "Názov")]
        public string Title { get; set; }

        [Display(Name = "Export pre MK soft")]
        public bool ExportToMksoft { get; set; }
        public string ExportToMksoftText { get; set; }


        public void CopyDataFrom(QuoteState src)
        {
            this.pk = src.pk;
            this.Code = src.Code;
            this.Title = src.Title;
            this.ExportToMksoft = src.ExportToMksoft;
            this.ExportToMksoftText = this.ExportToMksoft ? "ÁNO" : "NIE";
        }

        public void CopyDataTo(QuoteState trg)
        {
            trg.pk = this.pk;
            trg.Code = this.Code;
            trg.Title = this.Title;
            trg.ExportToMksoft = this.ExportToMksoft;
        }

        public static QuoteStateModel CreateCopyFrom(QuoteState src)
        {
            QuoteStateModel trg = new QuoteStateModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static QuoteState CreateCopyFrom(QuoteStateModel src)
        {
            QuoteState trg = new QuoteState();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class QuoteStateListModel : _PagingModel
    {
        public List<QuoteStateModel> Items { get; set; }

        public static QuoteStateListModel CreateCopyFrom(List<QuoteState> srcArray)
        {
            QuoteStateListModel trgArray = new QuoteStateListModel();
            trgArray.ItemsPerPage = (int)srcArray.Count + 1;
            trgArray.TotalItems = (int)srcArray.Count;
            trgArray.Items = new List<QuoteStateModel>(srcArray.Count + 1);

            foreach (QuoteState src in srcArray)
            {
                trgArray.Items.Add(QuoteStateModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class QuoteStateDropDown : CmpDropDown
    {
        public QuoteStateDropDown()
        {
        }

        public static QuoteStateDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            QuoteStateRepository repository = new QuoteStateRepository();
            return QuoteStateDropDown.CreateCopyFrom(repository.GetRecords(), allowNull, emptyText);
        }

        public static QuoteStateDropDown CreateCopyFrom(List<QuoteState> dataList, bool allowNull, string emptyText)
        {
            QuoteStateDropDown ret = new QuoteStateDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (QuoteState dataItem in dataList)
            {
                QuoteStateModel dataModel = QuoteStateModel.CreateCopyFrom(dataItem);
                ret.AddItem(dataModel.Title, dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }
}
