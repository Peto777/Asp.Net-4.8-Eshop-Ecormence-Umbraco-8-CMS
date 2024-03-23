using eshoppgsoftweb.lib.Repositories;
using System;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class Product2CategoryApiController : _BaseApiController
    {
        public const string ProductToCategoryOk = "OK";
        public const string AddProductToCategoryError = "Vznikla chyba pri pridávaní produktu do kategórie.";
        public const string RemoveProductToCategoryError = "Vznikla chyba pri odstraňovaní produktu z kategórie.";
        public const string MoveUpError = "Vznikla chyba pri posúvaní produktu hore.";
        public const string MoveDownError = "Vznikla chyba pri posúvaní produktu dole.";

        public string AddProductToCategory(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkCategory = items[0];
                string pkProduct = items[1];

                EshoppgsoftwebProduct2Category dataRec = new EshoppgsoftwebProduct2Category()
                {
                    PkCategory = new Guid(pkCategory),
                    PkProduct = new Guid(pkProduct),
                };
                EshoppgsoftwebProduct2CategoryRepository repository = new EshoppgsoftwebProduct2CategoryRepository();
                repository.Insert(dataRec);
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", Product2CategoryApiController.AddProductToCategoryError, exc.ToString());
            }

            return Product2CategoryApiController.ProductToCategoryOk;
        }

        public string RemoveProductToCategory(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkCategory = items[0];
                string pkProduct = items[1];

                EshoppgsoftwebProduct2Category dataRec = new EshoppgsoftwebProduct2Category()
                {
                    PkCategory = new Guid(pkCategory),
                    PkProduct = new Guid(pkProduct),
                };
                EshoppgsoftwebProduct2CategoryRepository repository = new EshoppgsoftwebProduct2CategoryRepository();
                repository.Delete(dataRec);
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", Product2CategoryApiController.RemoveProductToCategoryError, exc.ToString());
            }

            return Product2CategoryApiController.ProductToCategoryOk;
        }

        public string MoveUpProductToCategory(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkCategory = items[0];
                string pkProduct = items[1];

                EshoppgsoftwebProduct2CategoryRepository repository = new EshoppgsoftwebProduct2CategoryRepository();
                repository.MoveProductUp(new Guid(pkCategory), new Guid(pkProduct));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", Product2CategoryApiController.MoveUpError, exc.ToString());
            }

            return Product2CategoryApiController.ProductToCategoryOk;
        }

        public string MoveDownProductToCategory(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkCategory = items[0];
                string pkProduct = items[1];

                EshoppgsoftwebProduct2CategoryRepository repository = new EshoppgsoftwebProduct2CategoryRepository();
                repository.MoveProductDown(new Guid(pkCategory), new Guid(pkProduct));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", Product2CategoryApiController.MoveDownError, exc.ToString());
            }

            return Product2CategoryApiController.ProductToCategoryOk;
        }
    }
}
