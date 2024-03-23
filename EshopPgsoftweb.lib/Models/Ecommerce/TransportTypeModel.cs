using dufeksoft.lib.Model;
using dufeksoft.lib.UI;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eshoppgsoftweb.lib.Models.Ecommerce
{
    public class TransportTypeModel : _BaseModel
    {
        [Required(ErrorMessage = "Poradie musí byť zadané")]
        [Display(Name = "Poradie")]
        public int TransportOrder { get; set; }
        [Required(ErrorMessage = "Kód musí byť zadaný")]
        [Display(Name = "Kód")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Názov musí byť zadaný")]
        [Display(Name = "Názov")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Cena bez DPH musí byť zadaná")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre cenu bez DPH")]
        [Display(Name = "Cena bez DPH")]
        public string PriceNoVat { get; set; }
        [Required(ErrorMessage = "Cena s DPH musí byť zadaná")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre cenu s DPH")]
        [Display(Name = "Cena s DPH")]
        public string PriceWithVat { get; set; }
        [Required(ErrorMessage = "% DPH musí byť zadané")]
        [DecimalNumber(ErrorMessage = "Neplatná hodnota pre % DPH")]
        [Display(Name = "% DPH")]
        public string VatPerc { get; set; }

        [Required(ErrorMessage = "Druh dopravy musí byť zadaný")]
        [Display(Name = "Druh dopravy")]
        public string GatewayTypeId { get; set; }
        public string GatewayTypeName { get; set; }

        public decimal QuoteTotalWeight { get; set; }

        public string PriceWithCurrency
        {
            get
            {
                return PriceUtil.GetPriceStringWithCurrency(this.PriceWithVat);
            }
        }
        public string ZeroPriceWithCurrency
        {
            get
            {
                return PriceUtil.GetPriceStringWithCurrency("0");
            }
        }

        public TransportGatewayDropDown TransportGatewayCollection { get; set; }

        public TransportTypeModel()
        {
            this.TransportGatewayCollection = new TransportGatewayDropDown();
        }

        public void CopyDataFrom(TransportType src)
        {
            this.pk = src.pk;
            this.TransportOrder = src.TransportOrder;
            this.Code = src.Code;
            this.Name = src.Name;
            this.PriceNoVat = PriceUtil.NumberToEditorString(src.PriceNoVat);
            this.PriceWithVat = PriceUtil.NumberToEditorString(src.PriceWithVat);
            this.VatPerc = PriceUtil.NumberToEditorString(src.VatPerc);
            this.GatewayTypeId = src.GatewayTypeId.ToString();
            this.GatewayTypeName = TransportGateway.GetName((TransportGateway.GatewayType)src.GatewayTypeId);
        }

        public void CopyDataTo(TransportType trg)
        {
            trg.pk = this.pk;
            trg.TransportOrder = this.TransportOrder;
            trg.Code = this.Code;
            trg.Name = this.Name;
            trg.PriceNoVat = PriceUtil.NumberFromEditorString(this.PriceNoVat);
            trg.PriceWithVat = PriceUtil.NumberFromEditorString(this.PriceWithVat);
            trg.VatPerc = PriceUtil.NumberFromEditorString(this.VatPerc);
            trg.GatewayTypeId = TransportGateway.GetValidGatewayTypeId(this.GatewayTypeId);
        }

        public static TransportTypeModel CreateCopyFrom(TransportType src)
        {
            TransportTypeModel trg = new TransportTypeModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static TransportType CreateCopyFrom(TransportTypeModel src)
        {
            TransportType trg = new TransportType();
            src.CopyDataTo(trg);

            return trg;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Name, this.PriceWithCurrency);
        }
    }

    public class TransportTypePagingListModel : _PagingModel
    {
        public List<TransportTypeModel> Items { get; set; }

        public static TransportTypePagingListModel CreateCopyFrom(Page<TransportType> srcArray, decimal quoteTotalWeight = 0M)
        {
            TransportTypePagingListModel trgArray = new TransportTypePagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<TransportTypeModel>(srcArray.Items.Count + 1);

            foreach (TransportType src in srcArray.Items)
            {
                TransportTypeModel trg = TransportTypeModel.CreateCopyFrom(src);
                trg.QuoteTotalWeight = quoteTotalWeight;
                trgArray.Items.Add(trg);
            }

            return trgArray;
        }
    }

    public class TransportTypeListModel
    {
        public List<TransportTypeModel> Items { get; set; }

        public static TransportTypeListModel CreateCopyFrom(List<TransportType> srcArray, decimal quoteTotalWeight = 0M)
        {
            TransportTypeListModel trgArray = new TransportTypeListModel();
            trgArray.Items = new List<TransportTypeModel>(srcArray.Count + 1);

            foreach (TransportType src in srcArray)
            {
                TransportTypeModel trg = TransportTypeModel.CreateCopyFrom(src);
                trg.QuoteTotalWeight = quoteTotalWeight;
                trgArray.Items.Add(trg);
            }

            return trgArray;
        }
    }

    public class TransportTypeDropDown : CmpDropDown
    {
        public TransportTypeDropDown()
        {
        }

        public static TransportTypeDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            TransportTypeRepository repository = new TransportTypeRepository();
            return TransportTypeDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static TransportTypeDropDown CreateCopyFrom(Page<TransportType> dataList, bool allowNull, string emptyText)
        {
            TransportTypeDropDown ret = new TransportTypeDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (TransportType dataItem in dataList.Items)
            {
                TransportTypeModel dataModel = TransportTypeModel.CreateCopyFrom(dataItem);
                ret.AddItem(dataModel.ToString(), dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }

    public class TransportGateway
    {
        public enum GatewayType
        {
            GT_PERSONALLY = 1,
            GT_CARTAGE = 2,
            GT_DPD = 3,
            GT_UPS = 4,
            GT_POSTA = 5,
            GT_PAKETA = 6,

        }

        public string Id { get; set; }
        public string Name { get; set; }

        public static string GetName(GatewayType gatewayType)
        {
            switch (gatewayType)
            {
                case GatewayType.GT_PERSONALLY:
                    return "Osobný odber";
                case GatewayType.GT_CARTAGE:
                    return "Donáška domov";
                case GatewayType.GT_DPD:
                    return "Kuriér DPD";
                case GatewayType.GT_UPS:
                    return "Kuriér UPS";
                case GatewayType.GT_POSTA:
                    return "Kuriér POŠTA";
                case GatewayType.GT_PAKETA:
                    return "Paketa-Zásielkovňa";
            }

            return string.Empty;
        }

        public static int GetValidGatewayTypeId(string key)
        {
            int id;
            if (int.TryParse(key, out id))
            {
                return (int)((GatewayType)id);
            }

            return (int)GatewayType.GT_PERSONALLY;
        }
    }
    public class TransportGatewayDropDown : CmpDropDown
    {
        public TransportGatewayDropDown()
        {
            this.AddItem(TransportGateway.GatewayType.GT_PERSONALLY);
            this.AddItem(TransportGateway.GatewayType.GT_CARTAGE);
            this.AddItem(TransportGateway.GatewayType.GT_DPD);
            this.AddItem(TransportGateway.GatewayType.GT_UPS);
            this.AddItem(TransportGateway.GatewayType.GT_POSTA);
            this.AddItem(TransportGateway.GatewayType.GT_PAKETA);
        }

        private void AddItem(TransportGateway.GatewayType gatewayType)
        {
            TransportGateway item = new TransportGateway();
            item.Id = ((int)gatewayType).ToString();
            item.Name = TransportGateway.GetName(gatewayType);
            this.AddItem(item.Name, item.Id, item);
        }
    }
}
