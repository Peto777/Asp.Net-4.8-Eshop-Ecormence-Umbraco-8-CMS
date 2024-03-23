using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebProductRepository : _BaseRepository
    {
        static readonly object _productOrderLock = new object();

        public Page<EshoppgsoftwebProduct> GetPage(long page, long itemsPerPage, string sortBy = null, string sortDir = null, EshoppgsoftwebProductFilter filter = null)
        {
            var sql = GetBaseQuery();
            if (filter != null)
            {
                if (filter.OnlyIsVisible)
                {
                    sql.Where(GetOnlyVisibleWhereClause());
                }
                if (!string.IsNullOrEmpty(filter.ProductCode))
                {
                    sql.Where(GetCodeWhereClause(), new { Code = filter.ProductCode });
                }
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
                if (filter.FavoriteProductsCustomerKey != null && filter.FavoriteProductsCustomerKey != Guid.Empty)
                {
                    sql.Where(GetFavoriteWhereClause(), new { FavoriteProductsCustomerKey = filter.FavoriteProductsCustomerKey });
                }
                if (filter.ProductKeyList != null && filter.ProductKeyList.Count > 0)
                {
                    sql.Where(GetProductInWhereClause(filter.ProductKeyList));
                }
                if (filter.ProductCategoryKeyList != null && filter.ProductCategoryKeyList.Count > 0)
                {
                    sql.Where(GetProductCategoryInWhereClause(filter.ProductCategoryKeyList));
                }
                if (filter.ProducerKeyList != null && filter.ProducerKeyList.Count > 0)
                {
                    sql.Where(GetProducerInWhereClause(filter.ProducerKeyList));
                }
                if (filter.ProductAttributeKeyList != null && filter.ProductAttributeKeyList.Count > 0)
                {
                    sql.Where(GetProductAttributeInWhereClause(filter.ProductAttributeKeyList));
                    //foreach (string productAttributeKey in filter.ProductAttributeKeyList)
                    //{
                    //    sql.Where(GetProductAttributeWhereClause(productAttributeKey));
                    //}
                }
            }
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = "ProductOrder";
            }
            if (string.IsNullOrEmpty(sortDir))
            {
                sortDir = "ASC";
            }
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshoppgsoftwebProduct>(page, itemsPerPage, sql);
        }

        public Page<EshoppgsoftwebProduct> GetPageForCategory(long page, long itemsPerPage, Guid categoryKey)
        {
            var sql = GetProductsInCategoryQuery(categoryKey);

            return GetPage<EshoppgsoftwebProduct>(page, itemsPerPage, sql);
        }

        public Page<EshoppgsoftwebProduct> GetPageForNotInCategory(Guid categoryKey, long page, long itemsPerPage, string sortBy = "ProductOrder", string sortDir = "ASC", EshoppgsoftwebProduct2CategoryFilter filter = null)
        {
            var sql = GetProductsNotInCategoryQuery();
            sql.Where(GetNotInCategoryWhereClause(), new { Key = categoryKey });
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.ProductCode))
                {
                    sql.Where(GetCodeWhereClause(), new { Code = filter.ProductCode });
                }
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
            }
            sql.Append(string.Format("ORDER BY {0}.{1} {2}", EshoppgsoftwebProduct.DbTableName, sortBy, sortDir));

            return GetPage<EshoppgsoftwebProduct>(page, itemsPerPage, sql);
        }

        public Page<EshoppgsoftwebProduct> GetPageForQuote(long page, long itemsPerPage, Guid quoteKey)
        {
            var sql = GetProductsInQuoteQuery(quoteKey);

            return GetPage<EshoppgsoftwebProduct>(page, itemsPerPage, sql);
        }

        public Page<EshoppgsoftwebProduct> GetPageForSearch(long page, long itemsPerPage, string searchText)
        {
            var sql = GetProductsForSearch(searchText);

            return GetPage<EshoppgsoftwebProduct>(page, itemsPerPage, sql);
        }

        public EshoppgsoftwebProduct Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshoppgsoftwebProduct>(sql).FirstOrDefault();
        }
        public EshoppgsoftwebProduct GetForProductCode(string code)
        {
            var sql = GetBaseQuery().Where(GetCodeWhereClause(), new { Code = code });

            return Fetch<EshoppgsoftwebProduct>(sql).FirstOrDefault();
        }
        public EshoppgsoftwebProduct GetForProductUrl(string url)
        {
            var sql = GetBaseQuery().Where(GetUrlWhereClause(), new { Url = url });

            return Fetch<EshoppgsoftwebProduct>(sql).FirstOrDefault();
        }
        public List<Guid> GetProducerKeysForProductCategories(List<string> productCategoryKeyList)
        {
            var sql = new Sql(string.Format("SELECT DISTINCT(producerKey) FROM {0}", EshoppgsoftwebProduct.DbTableName));
            if (productCategoryKeyList != null)
            {
                sql.Where(GetProductCategoryInWhereClause(productCategoryKeyList));
            }
            sql.Where(GetOnlyVisibleWhereClause());

            return Fetch<Guid>(sql);
        }

        public bool Save(EshoppgsoftwebProduct dataRec)
        {
            if (IsNew(dataRec))
            {
                return Insert(dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(EshoppgsoftwebProduct dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                if ((Guid)result == dataRec.pk)
                {
                    // Record saved
                    // Set product order
                    bool isOk = SetNewProductOrder(dataRec);

                    return isOk;
                }
                return (bool)result;
            }

            return false;
        }

        bool Update(EshoppgsoftwebProduct dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshoppgsoftwebProduct dataRec)
        {            // Delete attribute links
            DeleteProduct2Attribute(dataRec.pk);
            // Delete product category links
            DeleteProduct2Category(dataRec.pk);
            // Delete product price links
            DeleteProductPrice(dataRec.pk);
            // Delete product relation links
            DeleteProductRelation(dataRec.pk);
            // Delete product
            if (DeleteInstance(dataRec))
            {
                return ReorderProducts(dataRec.ProductOrder, -1);
            }

            return false;
        }
        public bool DeleteProduct2Attribute(Guid productKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("DELETE {0} WHERE {1}=@ProductKey", "epProduct2Attribute", "pkProduct"),
                new { ProductKey = productKey });

            Execute(sql);

            return true;
        }
        public bool DeleteProduct2Category(Guid productKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("DELETE {0} WHERE {1}=@ProductKey", "epProduct2Category", "pkProduct"),
                new { ProductKey = productKey });

            Execute(sql);

            return true;
        }
        public bool DeleteProductPrice(Guid productKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("DELETE {0} WHERE {1}=@ProductKey", "epProductPrice", "productKey"),
                new { ProductKey = productKey });

            Execute(sql);

            return true;
        }
        public bool DeleteProductRelation(Guid productKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("DELETE {0} WHERE {1}=@ProductKey OR {2}=@ProductKey", "epProductRelation", "pkProductMain", "pkProductRelated"),
                new { ProductKey = productKey });

            Execute(sql);

            return true;
        }

        public bool SetNewProductOrder(EshoppgsoftwebProduct dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1} = (SELECT MAX({1})+1 FROM {0}) WHERE {2}=@Key",
                EshoppgsoftwebProduct.DbTableName, "ProductOrder", "pk"),
                new { Key = dataRec.pk });

            int cnt = 0;
            lock (_productOrderLock)
            {
                cnt = Execute(sql);
            }
            if (cnt > 0)
            {
                return true;
            }

            return false;
        }
        public bool ReorderProducts(int orderFrom, int orderOffset)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}={1}+@OrderOffset WHERE {1}>=@OrderFrom",
                EshoppgsoftwebProduct.DbTableName, "ProductOrder"),
                new { OrderFrom = orderFrom, OrderOffset = orderOffset });

            lock (_productOrderLock)
            {
                Execute(sql);
            }

            return true;
        }

        public bool SetProductOrder(Guid productKey, int order)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@Order WHERE {2}=@Key",
                EshoppgsoftwebProduct.DbTableName, "ProductOrder", "pk"),
                new { Order = order, Key = productKey });

            return Execute(sql) > 0;
        }
        public bool SetProductOrder(int oldOrder, int newOrder)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@NewOrder WHERE {1}=@OldOrder",
                EshoppgsoftwebProduct.DbTableName, "ProductOrder"),
                new { NewOrder = newOrder, OldOrder = oldOrder });

            return Execute(sql) > 0;
        }

        public int GetProductOrder(Guid productKey)
        {
            var sql = new Sql();
            sql.Append(
                string.Format("SELECT ProductOrder FROM {0} WHERE pk=@ProductKey",
                EshoppgsoftwebProduct.DbTableName),
                new { ProductKey = productKey });
            return ExecuteScalar<int>(sql);
        }

        public int GetMaxOrder()
        {
            var sql = new Sql();
            sql.Append(
                string.Format("SELECT MAX(ProductOrder) FROM {0}",
                EshoppgsoftwebProduct.DbTableName));
            return ExecuteScalar<int>(sql);
        }
        public bool MoveProductUp(Guid productKey)
        {
            lock (_productOrderLock)
            {
                int oldOrder = GetProductOrder(productKey);
                if (oldOrder > 1)
                {
                    SetProductOrder(oldOrder - 1, oldOrder);
                    SetProductOrder(productKey, oldOrder - 1);
                }
            }

            return true;
        }
        public bool MoveProductDown(Guid productKey)
        {
            lock (_productOrderLock)
            {
                int oldOrder = GetProductOrder(productKey);
                if (oldOrder < GetMaxOrder())
                {
                    SetProductOrder(oldOrder + 1, oldOrder);
                    SetProductOrder(productKey, oldOrder + 1);
                }
            }

            return true;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebProduct.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshoppgsoftwebProduct.DbTableName);
        }
        string GetUrlWhereClause()
        {
            return string.Format("{0}.productUrl = @Url", EshoppgsoftwebProduct.DbTableName);
        }
        string GetCodeWhereClause()
        {
            return string.Format("{0}.productCode = @Code", EshoppgsoftwebProduct.DbTableName);
        }
        string GetOnlyVisibleWhereClause()
        {
            return string.Format("{0}.productIsVisible = 1", EshoppgsoftwebProduct.DbTableName);
        }
        string GetRelatedProductWhereClause()
        {
            return string.Format("{0}.pk IN (SELECT {1}.pkProductRelated FROM {1} WHERE {1}.pkProductMain = @KeyProductMain)", EshoppgsoftwebProduct.DbTableName, EshoppgsoftwebProductRelation.DbTableName);
        }
        string GetProductInWhereClause(List<string> productKeyList)
        {
            return string.Format("{0}.pk IN ({1})", EshoppgsoftwebProduct.DbTableName, GetKeysForInClause(productKeyList));
        }
        string GetProductCategoryInWhereClause(List<string> productCategoryKeyList)
        {
            return string.Format("{0}.pk IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkCategory IN ({2}))", EshoppgsoftwebProduct.DbTableName, EshoppgsoftwebProduct2Category.DbTableName, GetKeysForInClause(productCategoryKeyList));
        }
        string GetProducerInWhereClause(List<string> producerKeyList)
        {
            return string.Format("{0}.producerKey IN ({1})", EshoppgsoftwebProduct.DbTableName, GetKeysForInClause(producerKeyList));
        }
        string GetProductAttributeInWhereClause(List<string> productAttributeKeyList)
        {
            return string.Format("{0}.pk IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkAttribute IN ({2}))", EshoppgsoftwebProduct.DbTableName, EshoppgsoftwebProduct2Attribute.DbTableName, GetKeysForInClause(productAttributeKeyList));
        }
        string GetProductAttributeWhereClause(string productAttribute)
        {
            return string.Format("{0}.pk IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkAttribute='{2}')", EshoppgsoftwebProduct.DbTableName, EshoppgsoftwebProduct2Attribute.DbTableName, productAttribute);
        }
        string GetSearchTextWhereClause(string searchText)
        {
            string[] items = searchText.Split(' ');
            StringBuilder str = new StringBuilder();
            foreach (string searchItem in items)
            {
                if (str.Length > 0)
                {
                    str.Append(" AND ");
                }
                str.Append(string.Format("({0}.productCode LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productName LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productDescription LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productImg LIKE '%{1}%' collate Latin1_general_CI_AI)", EshoppgsoftwebProduct.DbTableName, searchItem));
            }

            return string.Format("({0})", str.ToString());
        }
        string GetFavoriteWhereClause()
        {
            return string.Format("{0}.pk IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkCustomer = @FavoriteProductsCustomerKey)", EshoppgsoftwebProduct.DbTableName, Product2CustomerFavorite.DbTableName);
        }


        Sql GetProductsInCategoryQuery(Guid categoryKey)
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.* FROM {0}, {1} WHERE {0}.pk = {1}.pkProduct AND {1}.pkCategory = @Key ORDER BY {1}.productOrder", EshoppgsoftwebProduct.DbTableName, "epProduct2Category"), new { Key = categoryKey });

            return sql;
        }

        Sql GetProductsNotInCategoryQuery()
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.* FROM {0}", EshoppgsoftwebProduct.DbTableName));

            return sql;
        }
        string GetNotInCategoryWhereClause()
        {
            return string.Format("{0}.pk NOT IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkCategory = @Key)", EshoppgsoftwebProduct.DbTableName, EshoppgsoftwebProduct2Category.DbTableName);
        }

        Sql GetProductsInQuoteQuery(Guid quoteKey)
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.*, {1}.itemOrder FROM {0}, {1} WHERE {0}.pk = {1}.pkProduct AND {1}.pkQuote = @Key ORDER BY {1}.itemOrder", EshoppgsoftwebProduct.DbTableName, "epProduct2Quote"), new { Key = quoteKey });

            return sql;
        }

        Sql GetProductsForSearch(string searchText)
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.* FROM {0} WHERE {0}.ProductCode LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.ProductName LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.ProductDescription LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.ProductImg LIKE '%{1}%' collate Latin1_general_CI_AI ORDER BY {0}.productOrder", EshoppgsoftwebProduct.DbTableName, searchText));

            return sql;
        }
    }

    [TableName(EshoppgsoftwebProduct.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshoppgsoftwebProduct : _BaseRepositoryRec
    {
        public const string DbTableName = "epProduct";

        public bool ProductIsVisible { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductText { get; set; }
        public string ProductDescription { get; set; }
        public int ProductOrder { get; set; }
        public string ProductImg { get; set; }

        public string ProductUrl { get; set; }
        public string ProductMetaTitle { get; set; }
        public string ProductMetaKeywords { get; set; }
        public string ProductMetaDescription { get; set; }

        public Guid ProducerKey { get; set; }
        public Guid AvailabilityKey { get; set; }

        public int UnitTypeId { get; set; }
        public bool ProductIsNew { get; set; }
        public bool ProductIsSale { get; set; }
        public string ProductDurability { get; set; }
        public decimal ProductUnitWeight { get; set; }
        public decimal ProductUnitsInPckg { get; set; }
        public string ProductCountry { get; set; }

        public static string GetFileUploadCategory(Guid productKey)
        {
            return string.Format(@"Product/{0}", productKey);
        }
    }

    public class EshoppgsoftwebProductFilter
    {
        public bool OnlyIsVisible { get; set; }
        public string ProductCode { get; set; }
        public string SearchText { get; set; }
        public Guid FavoriteProductsCustomerKey { get; set; }

        public List<string> ProductKeyList { get; set; }
        public List<string> ProductCategoryKeyList { get; set; }
        public List<string> ProducerKeyList { get; set; }
        public List<string> ProductAttributeKeyList { get; set; }
    }
}
