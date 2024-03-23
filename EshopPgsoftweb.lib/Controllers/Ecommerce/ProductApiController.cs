using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using eshoppgsoftweb.lib.Util;
using System;
using System.Collections.Generic;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class ProductApiController : _BaseApiController
    {
        public const string ProductOk = "OK";
        public const string ApiError = "Vznikla chyba vo funkcii {0}.";

        public string MoveUpProduct(string id)
        {
            try
            {
                EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
                repository.MoveProductUp(new Guid(id));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(ProductApiController.ApiError, "MoveUpProduct"), exc.ToString());
            }

            return ProductApiController.ProductOk;
        }

        public string MoveDownProduct(string id)
        {
            try
            {
                EshoppgsoftwebProductRepository repository = new EshoppgsoftwebProductRepository();
                repository.MoveProductDown(new Guid(id));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(ProductApiController.ApiError, "MoveDownProduct"), exc.ToString());
            }

            return ProductApiController.ProductOk;
        }

        public string MoveUpProductAttribute(string id)
        {
            try
            {
                EshoppgsoftwebProductAttributeRepository repository = new EshoppgsoftwebProductAttributeRepository();
                repository.MoveProductAttributeUp(new Guid(id));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(ProductApiController.ApiError, "MoveUpProductAttribute"), exc.ToString());
            }

            return ProductApiController.ProductOk;
        }

        public string MoveDownProductAttribute(string id)
        {
            try
            {
                EshoppgsoftwebProductAttributeRepository repository = new EshoppgsoftwebProductAttributeRepository();
                repository.MoveProductAttributeDown(new Guid(id));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(ProductApiController.ApiError, "MoveDownProductAttribute"), exc.ToString());
            }

            return ProductApiController.ProductOk;
        }

        public VatResult CalculatePriceWithoutVat(string id)
        {
            VatResult ret = new VatResult();

            try
            {
                string[] items = id.Split('|');
                decimal vatPrice = PriceUtil.NumberFromEditorString(items[0]);
                decimal vatRate = PriceUtil.NumberFromEditorString(items[1]);

                ret.PriceWithoutVat = PriceUtil.NumberToEditorString(VatUtil.CalculatePriceWithoutVat(vatPrice, vatRate));
                ret.PriceWithVat = PriceUtil.NumberToEditorString(vatPrice);
                ret.Status = ProductApiController.ProductOk;
            }
            catch (Exception exc)
            {
                ret.Status = string.Format("{0}. {1}", string.Format(ProductApiController.ApiError, "CalculatePriceWithoutVat"), exc.ToString());
            }

            return ret;
        }

        public List<ProductSearchItem> SearchProduct(string id)
        {
            List<ProductSearchItem> ret = new List<ProductSearchItem>();
            if (string.IsNullOrEmpty(id))
            {
                return ret;
            }

            try
            {
                List<EshoppgsoftwebProduct> dataList = new EshoppgsoftwebProductRepository().GetPage(1, 20, "ProductName", "ASC", new EshoppgsoftwebProductFilter() { SearchText = id }).Items;
                foreach (EshoppgsoftwebProduct dataRec in dataList)
                {
                    ret.Add(new ProductSearchItem()
                    {
                        Key = dataRec.pk.ToString(),
                        Code = dataRec.ProductCode,
                        Name = dataRec.ProductName,
                        UnitName = ProductUnit.GetName((ProductUnit.UnitType)dataRec.UnitTypeId)
                    });
                }
                if (ret.Count <= 0)
                {
                    ret.Add(
                        new ProductSearchItem()
                        {
                            Msg = "Nič sa nenašlo",
                        });
                }

                return ret;
            }
            catch
            {
                return ret;
            }
        }
    }

    public class VatResult
    {
        public string Status { get; set; }
        public string PriceWithVat { get; set; }
        public string PriceWithoutVat { get; set; }
    }

    public class ProductSearchItem
    {
        public string Key { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string UnitName { get; set; }
        public string Msg { get; set; }
    }
}
