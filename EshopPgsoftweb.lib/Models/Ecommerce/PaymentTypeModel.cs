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
    public class PaymentTypeModel : _BaseModel
    {
        [Required(ErrorMessage = "Poradie musí byť zadané")]
        [Display(Name = "Poradie")]
        public int PaymentOrder { get; set; }
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

        [Required(ErrorMessage = "Platobná brána musí byť zadaná")]
        [Display(Name = "Platobná brána")]
        public string GatewayTypeId { get; set; }
        public string GatewayTypeName { get; set; }
        public string GatewayTypeImg { get; set; }


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

        public PaymentGatewayDropDown PaymentGatewayCollection { get; set; }

        public PaymentTypeModel()
        {
            this.PaymentGatewayCollection = new PaymentGatewayDropDown();
        }

        public void CopyDataFrom(PaymentType src)
        {
            this.pk = src.pk;
            this.PaymentOrder = src.PaymentOrder;
            this.Code = src.Code;
            this.Name = src.Name;
            this.PriceNoVat = PriceUtil.NumberToEditorString(src.PriceNoVat);
            this.PriceWithVat = PriceUtil.NumberToEditorString(src.PriceWithVat);
            this.VatPerc = PriceUtil.NumberToEditorString(src.VatPerc);
            this.GatewayTypeId = src.GatewayTypeId.ToString();
            this.GatewayTypeName = PaymentGateway.GetName((PaymentGateway.GatewayType)src.GatewayTypeId);
            this.GatewayTypeImg = PaymentGateway.GetImgUrl((PaymentGateway.GatewayType)src.GatewayTypeId);
        }

        public void CopyDataTo(PaymentType trg)
        {
            trg.pk = this.pk;
            trg.PaymentOrder = this.PaymentOrder;
            trg.Code = this.Code;
            trg.Name = this.Name;
            trg.PriceNoVat = PriceUtil.NumberFromEditorString(this.PriceNoVat);
            trg.PriceWithVat = PriceUtil.NumberFromEditorString(this.PriceWithVat);
            trg.VatPerc = PriceUtil.NumberFromEditorString(this.VatPerc);
            trg.GatewayTypeId = PaymentGateway.GetValidGatewayTypeId(this.GatewayTypeId);
        }

        public static PaymentTypeModel CreateCopyFrom(PaymentType src)
        {
            PaymentTypeModel trg = new PaymentTypeModel();
            trg.CopyDataFrom(src);

            return trg;
        }

        public static PaymentType CreateCopyFrom(PaymentTypeModel src)
        {
            PaymentType trg = new PaymentType();
            src.CopyDataTo(trg);

            return trg;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Name, this.PriceWithCurrency);
        }
    }

    public class PaymentTypePagingListModel : _PagingModel
    {
        public List<PaymentTypeModel> Items { get; set; }

        public static PaymentTypePagingListModel CreateCopyFrom(Page<PaymentType> srcArray)
        {
            PaymentTypePagingListModel trgArray = new PaymentTypePagingListModel();
            trgArray.ItemsPerPage = (int)srcArray.ItemsPerPage;
            trgArray.TotalItems = (int)srcArray.TotalItems;
            trgArray.Items = new List<PaymentTypeModel>(srcArray.Items.Count + 1);

            foreach (PaymentType src in srcArray.Items)
            {
                trgArray.Items.Add(PaymentTypeModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class PaymentTypeListModel
    {
        public List<PaymentTypeModel> Items { get; set; }

        public static PaymentTypeListModel CreateCopyFrom(List<PaymentType> srcArray)
        {
            PaymentTypeListModel trgArray = new PaymentTypeListModel();
            trgArray.Items = new List<PaymentTypeModel>(srcArray.Count + 1);

            foreach (PaymentType src in srcArray)
            {
                trgArray.Items.Add(PaymentTypeModel.CreateCopyFrom(src));
            }

            return trgArray;
        }
    }

    public class PaymentTypeDropDown : CmpDropDown
    {
        public PaymentTypeDropDown()
        {
        }

        public static PaymentTypeDropDown CreateFromRepository(bool allowNull, string emptyText = "[ nezadané ]")
        {
            PaymentTypeRepository repository = new PaymentTypeRepository();
            return PaymentTypeDropDown.CreateCopyFrom(repository.GetPage(1, _PagingModel.AllItemsPerPage), allowNull, emptyText);
        }

        public static PaymentTypeDropDown CreateCopyFrom(Page<PaymentType> dataList, bool allowNull, string emptyText)
        {
            PaymentTypeDropDown ret = new PaymentTypeDropDown();

            if (allowNull)
            {
                ret.AddItem(emptyText, Guid.Empty.ToString(), null);
            }
            foreach (PaymentType dataItem in dataList.Items)
            {
                PaymentTypeModel dataModel = PaymentTypeModel.CreateCopyFrom(dataItem);
                ret.AddItem(dataModel.ToString(), dataModel.pk.ToString(), dataModel);
            }

            return ret;
        }
    }

    public class PaymentGateway
    {
        public enum GatewayType
        {
            GT_NO_GATEWAY = 1,
            GT_ON_DELIVERY = 2,
            GT_SPOROPAY = 3,
            GT_VUBEPLATBY = 4,
            GT_BANK_TRANSFER = 5,
            GT_OUTSIDE_SLOVAKIA = 6,
            GT_GP_WEBPAY = 7,
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public static string GetName(GatewayType gatewayType)
        {
            switch (gatewayType)
            {
                case GatewayType.GT_NO_GATEWAY:
                    return "Žiadna platobná brána";
                case GatewayType.GT_ON_DELIVERY:
                    return "Dobierka";
                case GatewayType.GT_SPOROPAY:
                    return "SPOROPAY";
                case GatewayType.GT_VUBEPLATBY:
                    return "VUBEPLATBY";
                case GatewayType.GT_BANK_TRANSFER:
                    return "Karta";
                case GatewayType.GT_GP_WEBPAY:
                    return "WEBPAY";
            }

            return string.Empty;
        }

        public static string GetImgUrl(GatewayType gatewayType)
        {
            //switch (gatewayType)
            //{
            //    case GatewayType.GT_SPOROPAY:
            //        return "/Styles/images/slovenska-sporitelna-sporopay.png";
            //    case GatewayType.GT_VUBEPLATBY:
            //        return "/Styles/images/platba-kartou.jpg";
            //    case GatewayType.GT_BANK_TRANSFER:
            //        return "";
            //    case GatewayType.GT_GP_WEBPAY:
            //        return "";
            //}

            return string.Empty;
        }

        public static int GetValidGatewayTypeId(string key)
        {
            int id;
            if (int.TryParse(key, out id))
            {
                return (int)((GatewayType)id);
            }

            return (int)GatewayType.GT_NO_GATEWAY;
        }
    }
    public class PaymentGatewayDropDown : CmpDropDown
    {
        public PaymentGatewayDropDown()
        {
            this.AddItem(PaymentGateway.GatewayType.GT_NO_GATEWAY);
            this.AddItem(PaymentGateway.GatewayType.GT_ON_DELIVERY);
            this.AddItem(PaymentGateway.GatewayType.GT_SPOROPAY);
            this.AddItem(PaymentGateway.GatewayType.GT_VUBEPLATBY);
            this.AddItem(PaymentGateway.GatewayType.GT_BANK_TRANSFER);
            this.AddItem(PaymentGateway.GatewayType.GT_GP_WEBPAY);
        }

        private void AddItem(PaymentGateway.GatewayType gatewayType)
        {
            PaymentGateway item = new PaymentGateway();
            item.Id = ((int)gatewayType).ToString();
            item.Name = PaymentGateway.GetName(gatewayType);
            this.AddItem(item.Name, item.Id, item);
        }
    }
}
