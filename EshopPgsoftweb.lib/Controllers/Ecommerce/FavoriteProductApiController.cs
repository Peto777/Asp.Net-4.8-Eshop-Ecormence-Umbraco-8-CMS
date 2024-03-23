using eshoppgsoftweb.lib.Repositories;
using System;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class FavoriteProductApiController : _BaseApiController
    {
        public const string ProductToFavoriteOk = "OK";
        public const string AddProductToFavoriteError = "Vznikla chyba pri pridávaní produktu medzi obľúbené.";
        public const string RemoveProductToFavoriteError = "Vznikla chyba pri odstraňovaní produktu z obľúbených.";
        public const string FavoriteInfoError = "Vznikla chyba pri čítaní obsahu obľúbených produktov.";

        public Api_AddToFavoriteInfo AddProductToFavorite(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkProduct = items[0];
                int memberId = int.Parse(items[1]);

                Api_AddToFavoriteInfo ret = new Api_AddToFavoriteInfo();
                EshoppgsoftwebCustomer customer = new EshoppgsoftwebCustomerRepository().GetForOwner(memberId);
                if (customer != null)
                {
                    new Product2CustomerFavoriteRepository().Add(customer.pk, new Guid(pkProduct));
                }

                ret.Result = FavoriteProductApiController.ProductToFavoriteOk;
                return ret;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(FavoriteProductApiController), "AddProductToFavorite error", exc);
                return new Api_AddToFavoriteInfo()
                {
                    Result = string.Format("{0}. {1}", FavoriteProductApiController.AddProductToFavoriteError, exc.ToString())
                };
            }
        }
        public Api_RemoveToFavoriteInfo RemoveProductToFavorite(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkProduct = items[0];
                int memberId = int.Parse(items[1]);

                Api_RemoveToFavoriteInfo ret = new Api_RemoveToFavoriteInfo();
                EshoppgsoftwebCustomer customer = new EshoppgsoftwebCustomerRepository().GetForOwner(memberId);
                if (customer != null)
                {
                    new Product2CustomerFavoriteRepository().Remove(customer.pk, new Guid(pkProduct));
                }

                ret.Result = FavoriteProductApiController.ProductToFavoriteOk;
                return ret;
            }
            catch (Exception exc)
            {
                this.Logger.Error(typeof(FavoriteProductApiController), "RemoveProductToFavorite error", exc);
                return new Api_RemoveToFavoriteInfo()
                {
                    Result = string.Format("{0}. {1}", FavoriteProductApiController.RemoveProductToFavoriteError, exc.ToString())
                };
            }
        }
    }

    public class Api_AddToFavoriteInfo
    {
        public string Result { get; set; }
    }
    public class Api_RemoveToFavoriteInfo
    {
        public string Result { get; set; }
    }
}
