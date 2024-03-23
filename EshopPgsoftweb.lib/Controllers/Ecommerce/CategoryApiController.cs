using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Repositories;
using System;
using System.Collections.Generic;
using Umbraco.Web.Mvc;

namespace eshoppgsoftweb.lib.Controllers.Ecommerce
{
    [PluginController("Ecommerce")]
    public class CategoryApiController : _BaseApiController
    {
        public const string CategoryOk = "OK";
        public const string ApiError = "Vznikla chyba vo funkcii {0}.";

        public string MoveUpCategory(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkParent = items[0];
                string pkCategory = items[1];

                EshoppgsoftwebCategoryRepository repository = new EshoppgsoftwebCategoryRepository();
                repository.MoveCategoryUp(new Guid(pkParent), new Guid(pkCategory));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "MoveUpCategory"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string MoveDownCategory(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string pkParent = items[0];
                string pkCategory = items[1];

                EshoppgsoftwebCategoryRepository repository = new EshoppgsoftwebCategoryRepository();
                repository.MoveCategoryDown(new Guid(pkParent), new Guid(pkCategory));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "MoveDownCategory"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string CategoryPublicFilterModel_PageSize_Set(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string pageSize = items[1];
                new CategoryPublicFilterModel().SetPageSize(sessionId, int.Parse(pageSize));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_PageSize_Set"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string CategoryPublicFilterModel_ProductView_Set(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string viewType = items[1];
                new CategoryPublicFilterModel().SetProductView(sessionId, int.Parse(viewType));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_ProductView_Set"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string CategoryPublicFilterModel_ProductSort_Set(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string sortType = items[1];
                new CategoryPublicFilterModel().SetProductSort(sessionId, int.Parse(sortType));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_ProductSort_Set"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string CategoryPublicFilterModel_ProducerIsSelected_Set(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string producerKey = items[1];
                string isSelected = items[2];
                new CategoryPublicFilterModel().SetProducerIsSelected(sessionId, producerKey, isSelected == "1");
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_ProducerIsSelected_Set"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }
        public string CategoryPublicFilterModel_ProducerIsSelected_All(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string isSelected = items[1];
                new CategoryPublicFilterModel().SetProducersAllSelected(sessionId, isSelected == "1");
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_ProducerIsSelected_All"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string CategoryPublicFilterModel_ProductAttributeIsSelected_Set(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string attributeKey = items[1];
                string isSelected = items[2];
                new CategoryPublicFilterModel().SetProductAttributeIsSelected(sessionId, attributeKey, isSelected == "1");
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_ProductAttributeIsSelected_Set"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }
        public string CategoryPublicFilterModel_ProductAttributeIsSelected_All(string id)
        {
            try
            {
                string[] items = id.Split('|');
                string sessionId = items[0];
                string isSelected = items[1];
                new CategoryPublicFilterModel().SetProductAttributesAllSelected(sessionId, isSelected == "1");
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_ProductAttributeIsSelected_All"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }

        public string CategoryPublicFilterModel_Submit()
        {
            try
            {
                string sessionId = this.GetRequestParam("SessionId");
                new CategoryPublicFilterModel().SetProductAttributesSelected(sessionId, SplitItems(this.GetRequestParam("AttrSelection"), '|'));
            }
            catch (Exception exc)
            {
                return string.Format("{0}. {1}", string.Format(CategoryApiController.ApiError, "CategoryPublicFilterModel_Submit"), exc.ToString());
            }

            return CategoryApiController.CategoryOk;
        }
        List<string> SplitItems(string str, char sep)
        {
            List<string> ret = new List<string>();
            foreach (string item in str.Split(sep))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    ret.Add(item);
                }
            }

            return ret;
        }
    }
}
