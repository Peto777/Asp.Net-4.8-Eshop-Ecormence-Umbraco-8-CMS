using eshoppgsoftweb.lib.Models;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebProduct2CategoryRepository : _BaseRepository
    {
        static readonly object _productOrderLock = new object();

        public List<EshoppgsoftwebProduct2Category> GetCategoryItems(Guid pkCategory)
        {
            return GetPage(1, _PagingModel.AllItemsPerPage, pkCategory).Items;
        }

        public Page<EshoppgsoftwebProduct2Category> GetPage(long page, long itemsPerPage, Guid keyCategory, string sortBy = "ProductOrder", string sortDir = "ASC", EshoppgsoftwebProduct2CategoryFilter filter = null)
        {
            var sql = GetBaseQuery().Where(GetCategoryWhereClause(), new { KeyCategory = keyCategory });
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.ProductCode))
                {
                    sql.Where(GetCodeWhereClause(), new { Code = filter.ProductCode });
                }
                if (filter.ProductCategoryKeyList != null && filter.ProductCategoryKeyList.Count > 0)
                {
                    sql.Where(GetProductCategoryInWhereClause(filter.ProductCategoryKeyList));
                }
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
            }
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshoppgsoftwebProduct2Category>(page, itemsPerPage, sql);
        }

        public List<EshoppgsoftwebProduct2Category> GetList(EshoppgsoftwebProduct2CategoryFilter filter = null)
        {
            var sql = GetBaseQuery();
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.ProductCode))
                {
                    sql.Where(GetCodeWhereClause(), new { Code = filter.ProductCode });
                }
                if (filter.ProductCategoryKeyList != null && filter.ProductCategoryKeyList.Count > 0)
                {
                    sql.Where(GetProductCategoryInWhereClause(filter.ProductCategoryKeyList));
                }
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
            }

            return Fetch<EshoppgsoftwebProduct2Category>(sql);
        }

        public EshoppgsoftwebProduct2Category Get(Guid keyCategory, Guid keyProduct)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { KeyCategory = keyCategory, KeyProduct = keyProduct });

            return Fetch<EshoppgsoftwebProduct2Category>(sql).FirstOrDefault();
        }

        public List<EshoppgsoftwebProduct2Category> GetForProduct(Guid keyProduct)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { KeyProduct = keyProduct });

            return Fetch<EshoppgsoftwebProduct2Category>(sql);
        }

        public bool Insert(EshoppgsoftwebProduct2Category dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("INSERT INTO {0} (PkCategory, PkProduct, ProductOrder) VALUES (@PkCategory, @PkProduct, @ProductOrder)",
                EshoppgsoftwebProduct2Category.DbTableName),
                new { PkCategory = dataRec.PkCategory, PkProduct = dataRec.PkProduct, ProductOrder = dataRec.ProductOrder });

            if (Execute(sql) > 0)
            {
                return SetNewProductOrder(dataRec);
            }

            return false;
        }

        public bool Delete(EshoppgsoftwebProduct2Category dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE {0} WHERE {1}=@PkCategory AND {2}=@PkProduct",
                EshoppgsoftwebProduct2Category.DbTableName, "PkCategory", "PkProduct"),
                new { PkCategory = dataRec.PkCategory, PkProduct = dataRec.PkProduct });

            lock (_productOrderLock)
            {
                if (Execute(sql) > 0)
                {
                    ReorderProducts(dataRec.PkCategory);
                }
            }

            return true;
        }

        public bool DeleteForProduct(Guid productKey)
        {
            bool isOK = true;
            List<EshoppgsoftwebProduct2Category> dataList = GetForProduct(productKey);
            foreach (EshoppgsoftwebProduct2Category dataRec in dataList)
            {
                if (!Delete(dataRec))
                {
                    isOK = false;
                }
            }

            return isOK;
        }


        public bool SetNewProductOrder(EshoppgsoftwebProduct2Category dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1} = (SELECT MAX({1})+1 FROM {0} WHERE {2}=@PkCategory) WHERE {2}=@PkCategory AND {3}=@PkProduct",
                EshoppgsoftwebProduct2Category.DbTableName, "ProductOrder", "PkCategory", "PkProduct"),
                new { PkCategory = dataRec.PkCategory, PkProduct = dataRec.PkProduct });

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

        public bool SetProductOrder(Guid categoryKey, Guid productKey, int order)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@Order WHERE {2}=@CategoryKey AND {3}=@ProductKey",
                EshoppgsoftwebProduct2Category.DbTableName, "ProductOrder", "PkCategory", "PkProduct"),
                new { Order = order, CategoryKey = categoryKey, ProductKey = productKey });

            return Execute(sql) > 0;
        }
        public bool SetProductOrder(Guid categoryKey, int oldOrder, int newOrder)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1}=@NewOrder WHERE {2}=@CategoryKey AND {1}=@OldOrder",
                EshoppgsoftwebProduct2Category.DbTableName, "ProductOrder", "PkCategory"),
                new { CategoryKey = categoryKey, NewOrder = newOrder, OldOrder = oldOrder });

            return Execute(sql) > 0;
        }

        public bool ReorderProducts(Guid categoryKey)
        {
            if (categoryKey == null)
            {
                categoryKey = Guid.Empty;
            }

            var sql = new Sql();
            sql.Append(
                string.Format("SELECT PkProduct FROM {0} WHERE PkCategory=@CategoryKey ORDER BY ProductOrder ASC",
                EshoppgsoftwebProduct2Category.DbTableName),
                new { CategoryKey = categoryKey });
            List<Guid> guidList = Fetch<Guid>(sql);

            int i = 0;
            foreach (Guid productKey in guidList)
            {
                SetProductOrder(categoryKey, productKey, ++i);
            }

            return true;
        }

        public int GetProductOrder(Guid categoryKey, Guid productKey)
        {
            if (categoryKey == null)
            {
                categoryKey = Guid.Empty;
            }

            var sql = new Sql();
            sql.Append(
                string.Format("SELECT ProductOrder FROM {0} WHERE PkCategory=@CategoryKey AND PkProduct=@ProductKey",
                EshoppgsoftwebProduct2Category.DbTableName),
                new { CategoryKey = categoryKey, ProductKey = productKey });
            return ExecuteScalar<int>(sql);
        }

        public int GetMaxOrder(Guid categoryKey)
        {
            if (categoryKey == null)
            {
                categoryKey = Guid.Empty;
            }

            var sql = new Sql();
            sql.Append(
                string.Format("SELECT MAX(ProductOrder) FROM {0} WHERE PkCategory=@CategoryKey",
                EshoppgsoftwebProduct2Category.DbTableName),
                new { CategoryKey = categoryKey });
            return ExecuteScalar<int>(sql);
        }
        public bool MoveProductUp(Guid categoryKey, Guid productKey)
        {
            if (categoryKey == null)
            {
                categoryKey = Guid.Empty;
            }

            lock (_productOrderLock)
            {
                int oldOrder = GetProductOrder(categoryKey, productKey);
                if (oldOrder > 1)
                {
                    SetProductOrder(categoryKey, oldOrder - 1, oldOrder);
                    SetProductOrder(categoryKey, productKey, oldOrder - 1);
                }
            }

            return true;
        }

        public bool MoveProductDown(Guid categoryKey, Guid productKey)
        {
            if (categoryKey == null)
            {
                categoryKey = Guid.Empty;
            }

            lock (_productOrderLock)
            {
                int oldOrder = GetProductOrder(categoryKey, productKey);
                if (oldOrder < GetMaxOrder(categoryKey))
                {
                    SetProductOrder(categoryKey, oldOrder + 1, oldOrder);
                    SetProductOrder(categoryKey, productKey, oldOrder + 1);
                }
            }

            return true;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebProduct2Category.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.PkCategory = @KeyCategory AND {0}.PkProduct = @KeyProduct", EshoppgsoftwebProduct2Category.DbTableName);
        }

        string GetCategoryWhereClause()
        {
            return string.Format("{0}.PkCategory = @KeyCategory", EshoppgsoftwebProduct2Category.DbTableName);
        }
        string GetProductCategoryInWhereClause(List<string> productCategoryKeyList)
        {
            return string.Format("{0}.PkCategory IN ({1})", EshoppgsoftwebProduct2Category.DbTableName, GetKeysForInClause(productCategoryKeyList));
        }
        string GetProductWhereClause()
        {
            return string.Format("{0}.PkProduct = @KeyProduct", EshoppgsoftwebProduct2Category.DbTableName);
        }
        string GetCodeWhereClause()
        {
            return string.Format("{0}.PkProduct IN (SELECT pk FROM {1} WHERE {1}.productCode = @Code)", EshoppgsoftwebProduct2Category.DbTableName, EshoppgsoftwebProduct.DbTableName);
        }
        string GetSearchTextWhereClause(string searchText)
        {
            string textCondition = string.Format("{0}.productCode LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productName LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productDescription LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productUnit LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.productImg LIKE '%{1}%' collate Latin1_general_CI_AI", EshoppgsoftwebProduct.DbTableName, searchText);
            int num;
            string condition = string.Format("{0}{1}", textCondition, int.TryParse(searchText, out num) ? string.Format(" OR {0}.productOrder={1}", EshoppgsoftwebProduct.DbTableName, searchText) : string.Empty);

            return string.Format("{0}.PkProduct IN (SELECT pk FROM {1} WHERE {2})", EshoppgsoftwebProduct2Category.DbTableName, EshoppgsoftwebProduct.DbTableName, condition);
        }
    }

    [TableName(EshoppgsoftwebProduct2Category.DbTableName)]
    public class EshoppgsoftwebProduct2Category : _BaseRepositoryRec
    {
        public const string DbTableName = "epProduct2Category";

        public Guid PkCategory { get; set; }
        public Guid PkProduct { get; set; }
        public int ProductOrder { get; set; }
    }

    public class EshoppgsoftwebProduct2CategoryFilter
    {
        public string ProductCode { get; set; }
        public string SearchText { get; set; }
        public List<string> ProductCategoryKeyList { get; set; }
    }
}
