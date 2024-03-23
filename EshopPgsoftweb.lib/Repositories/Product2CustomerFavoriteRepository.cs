using NPoco;
using System;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class Product2CustomerFavoriteRepository : _BaseRepository
    {
        public bool Add(Guid pkCustomer, Guid pkProduct)
        {
            Product2CustomerFavorite dataRec = Get(pkCustomer, pkProduct);
            if (dataRec == null)
            {
                dataRec = new Product2CustomerFavorite();
                dataRec.PkCustomer = pkCustomer;
                dataRec.PkProduct = pkProduct;
                return Save(dataRec);
            }

            return true;
        }
        public bool Remove(Guid pkCustomer, Guid pkProduct)
        {
            Product2CustomerFavorite dataRec = Get(pkCustomer, pkProduct);
            if (dataRec != null)
            {
                return Delete(dataRec);
            }

            return true;
        }

        public int GetItemsCntForCustomer(Guid pkCustomer)
        {
            var sql = new Sql(string.Format("SELECT COUNT(*) FROM {0}", Product2CustomerFavorite.DbTableName));
            sql.Where(GetCustomerWhereClause(), new { CustomerKey = pkCustomer });
            return Fetch<int>(sql).FirstOrDefault();
        }


        public Product2CustomerFavorite Get(Guid pkCustomer, Guid pkProduct)
        {
            var sql = GetBaseQuery();
            sql.Where(GetCustomerWhereClause(), new { CustomerKey = pkCustomer });
            sql.Where(GetProductWhereClause(), new { ProductKey = pkProduct });

            return Fetch<Product2CustomerFavorite>(sql).FirstOrDefault();
        }

        public Product2CustomerFavorite Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<Product2CustomerFavorite>(sql).FirstOrDefault();
        }

        public bool Save(Product2CustomerFavorite dataRec)
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

        bool Insert(Product2CustomerFavorite dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(Product2CustomerFavorite dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(Product2CustomerFavorite dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            var sql = new Sql(string.Format("SELECT * FROM {0} ", Product2CustomerFavorite.DbTableName));

            return sql;
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", Product2CustomerFavorite.DbTableName);
        }
        string GetCustomerWhereClause()
        {
            return string.Format("{0}.pkCustomer = @CustomerKey", Product2CustomerFavorite.DbTableName);
        }
        string GetProductWhereClause()
        {
            return string.Format("{0}.pkProduct = @ProductKey", Product2CustomerFavorite.DbTableName);
        }
    }

    [TableName(Product2CustomerFavorite.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class Product2CustomerFavorite : _BaseRepositoryRec
    {
        public const string DbTableName = "epProduct2CustomerFavorite";

        public Guid PkCustomer { get; set; }
        public Guid PkProduct { get; set; }
    }
}
