using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebProduct2AttributeRepository : _BaseRepository
    {
        public List<EshoppgsoftwebProduct2Attribute> GetForProduct(Guid keyProduct)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { KeyProduct = keyProduct });

            return Fetch<EshoppgsoftwebProduct2Attribute>(sql);
        }

        public List<EshoppgsoftwebProduct2AttributeEx> GetForProducts(List<string> productKeyList)
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.PkAttribute, {0}.PkProduct, {1}.ProductAttributeName, {1}.ProductAttributeOrder FROM {0}, {1}", EshoppgsoftwebProduct2Attribute.DbTableName, EshoppgsoftwebProductAttribute.DbTableName));
            sql.Where(GetProductInWhereClause(productKeyList));
            sql.Where(string.Format("{0}.PkAttribute = {1}.pk", EshoppgsoftwebProduct2Attribute.DbTableName, EshoppgsoftwebProductAttribute.DbTableName));
            sql.Append(string.Format("ORDER BY {0}.PkProduct, {1}.ProductAttributeOrder", EshoppgsoftwebProduct2Attribute.DbTableName, EshoppgsoftwebProductAttribute.DbTableName));


            return Fetch<EshoppgsoftwebProduct2AttributeEx>(sql);
        }

        public List<Guid> GetProductAttributeKeysForProductCategories(List<string> productCategoryKeyList)
        {
            var sql = new Sql(string.Format("SELECT DISTINCT({0}.PkAttribute) FROM {0}", EshoppgsoftwebProduct2Attribute.DbTableName));
            if (productCategoryKeyList != null)
            {
                sql.Where(GetProductCategoryInWhereClause(productCategoryKeyList));
            }

            return Fetch<Guid>(sql);
        }


        public EshoppgsoftwebProduct2Attribute Get(Guid keyAttribute, Guid keyProduct)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { KeyAttribute = keyAttribute, KeyProduct = keyProduct });

            return Fetch<EshoppgsoftwebProduct2Attribute>(sql).FirstOrDefault();
        }

        public bool Insert(EshoppgsoftwebProduct2Attribute dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("INSERT INTO {0} (PkAttribute, PkProduct) VALUES (@PkAttribute, @PkProduct)",
                EshoppgsoftwebProduct2Attribute.DbTableName),
                new { PkAttribute = dataRec.PkAttribute, PkProduct = dataRec.PkProduct });

            return Execute(sql) > 0;
        }

        public bool Delete(EshoppgsoftwebProduct2Attribute dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE {0} WHERE {1}=@PkAttribute AND {2}=@PkProduct",
                EshoppgsoftwebProduct2Attribute.DbTableName, "PkAttribute", "PkProduct"),
                new { PkAttribute = dataRec.PkAttribute, PkProduct = dataRec.PkProduct });

            return Execute(sql) > 0;
        }

        public bool DeleteForProduct(Guid productKey)
        {
            bool isOK = true;
            List<EshoppgsoftwebProduct2Attribute> dataList = GetForProduct(productKey);
            foreach (EshoppgsoftwebProduct2Attribute dataRec in dataList)
            {
                if (!Delete(dataRec))
                {
                    isOK = false;
                }
            }

            return isOK;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebProduct2Attribute.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.PkAttribute = @KeyAttribute AND {0}.PkProduct = @KeyProduct", EshoppgsoftwebProduct2Attribute.DbTableName);
        }

        string GetProductWhereClause()
        {
            return string.Format("{0}.PkProduct = @KeyProduct", EshoppgsoftwebProduct2Attribute.DbTableName);
        }
        string GetAttributeWhereClause()
        {
            return string.Format("{0}.PkAttribute = @KeyAttribute", EshoppgsoftwebProduct2Attribute.DbTableName);
        }
        string GetProductInWhereClause(List<string> productKeyList)
        {
            StringBuilder strIn = new StringBuilder();
            foreach (string productKey in productKeyList)
            {
                if (strIn.Length > 0)
                {
                    strIn.Append(",");
                }
                strIn.Append(string.Format("'{0}'", productKey));
            }
            return string.Format("{0}.PkProduct IN ({1})", EshoppgsoftwebProduct2Attribute.DbTableName, strIn.ToString());
        }
        string GetProductCategoryInWhereClause(List<string> productCategoryKeyList)
        {
            return string.Format("{0}.PkProduct IN (SELECT {1}.pkProduct FROM {1} WHERE {1}.pkCategory IN ({2}))", EshoppgsoftwebProduct2Attribute.DbTableName, EshoppgsoftwebProduct2Category.DbTableName, GetKeysForInClause(productCategoryKeyList));
        }
    }

    [TableName(EshoppgsoftwebProduct2Attribute.DbTableName)]
    public class EshoppgsoftwebProduct2Attribute : _BaseRepositoryRec
    {
        public const string DbTableName = "epProduct2Attribute";

        public Guid PkAttribute { get; set; }
        public Guid PkProduct { get; set; }
    }

    public class EshoppgsoftwebProduct2AttributeEx : _BaseRepositoryRec
    {
        public Guid PkAttribute { get; set; }
        public Guid PkProduct { get; set; }
        public string ProductAttributeName { get; set; }
        public int ProductAttributeOrder { get; set; }
    }
}
