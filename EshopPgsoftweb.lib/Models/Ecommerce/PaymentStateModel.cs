using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class PaymentStateModel : _BaseModel
    {
        [Required(ErrorMessage = "Kód musí byť zadaný")]
        [Display(Name = "Kód")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Názov musí byť zadaný")]
        [Display(Name = "Názov")]
        public string Title { get; set; }

        public void CopyDataFrom(PaymentState src)
        {
            this.pk = src.pk;
            this.Code = src.Code;
            this.Title = src.Title;
        }

        public void CopyDataTo(PaymentState trg)
        {
            trg.pk = this.pk;
            trg.Code = this.Code;
            trg.Title = this.Title;
        }

        public static PaymentStateModel CreateCopyFrom(PaymentState src)
        {
            PaymentStateModel trg = new PaymentStateModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static PaymentState CreateCopyFrom(PaymentStateModel src)
        {
            PaymentState trg = new PaymentState();
            src.CopyDataTo(trg);

            return trg;
        }
    }

    public class PaymentStateListModel : _PagingModel
    {
        public List<PaymentStateModel> Items { get; set; }

        public static PaymentStateListModel CreateCopyFrom(List<PaymentState> srcArray)
        {
            PaymentStateListModel trgArray = new PaymentStateListModel();
            trgArray.ItemsPerPage = (int)srcArray.Count + 1;
            trgArray.TotalItems = (int)srcArray.Count;
            trgArray.Items = new List<PaymentStateModel>(srcArray.Count + 1);

            foreach (PaymentState src in srcArray)
            {
                trgArray.Items.Add(PaymentStateModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class PaymentStateDropDown : CmpDropDown
    {
        public PaymentStateDropDown()
        {
        }

        public static PaymentStateDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            PaymentStateRepository repository = new PaymentStateRepository();
            return PaymentStateDropDown.CreateCopyFrom(repository.GetRecords(), allowNull, emptyText);
        }

        public static PaymentStateDropDown CreateCopyFrom(List<PaymentState> dataList, bool allowNull, string emptyText)
        {
            PaymentStateDropDown ret = new PaymentStateDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (PaymentState dataItem in dataList)
            {
                PaymentStateModel dataModel = PaymentStateModel.CreateCopyFrom(dataItem);
                ret.AddItem(dataModel.Title, dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }
}
