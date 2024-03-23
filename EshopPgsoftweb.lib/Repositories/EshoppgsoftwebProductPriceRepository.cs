using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebProductPriceRepository : _BaseRepository
    {
        public List<EshoppgsoftwebProductPrice> GetForProduct(Guid productKey)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { ProductKey = productKey });
            sql.Append("ORDER BY ValidFrom DESC, ValidTo DESC");

            return Fetch<EshoppgsoftwebProductPrice>(sql);
        }

        public List<EshoppgsoftwebProductPrice> GetForDate(DateTime dateAt)
        {
            var sql = GetBaseQuery().Where(GetValidAtWhereClause(), new { ValidAt = dateAt });

            return Fetch<EshoppgsoftwebProductPrice>(sql);
        }

        public EshoppgsoftwebProductPrice Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshoppgsoftwebProductPrice>(sql).FirstOrDefault();
        }

        public EshoppgsoftwebProductPrice GetStandardPrice(Guid productKey)
        {
            var sql = GetBaseQuery().Where(GetProductWhereClause(), new { ProductKey = productKey });
            sql.Where(GetStandardPriceWhereClause());

            return Fetch<EshoppgsoftwebProductPrice>(sql).FirstOrDefault();
        }

        public bool Save(EshoppgsoftwebProductPrice dataRec)
        {
            EshoppgsoftwebProductPriceCache.ClearCache();

            if (IsNew(dataRec))
            {
                return Insert(dataRec);
            }
            else
            {
                return Update(dataRec);
            }
        }

        bool Insert(EshoppgsoftwebProductPrice dataRec)
        {
            bool finalResult = false;

            dataRec.pk = Guid.NewGuid();

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                bool transOk = false;
                try
                {
                    scope.Database.BeginTransaction();
                    object result = scope.Database.Insert<EshoppgsoftwebProductPrice>(dataRec);

                    if (result is Guid)
                    {
                        if ((Guid)result == dataRec.pk)
                        {
                            if (dataRec.ValidTo == null)
                            {
                                // New standard price
                                // Set valid to date for previous standard price
                                var sql = new Sql(string.Format("UPDATE {0} SET validTo=@ValidTo", EshoppgsoftwebProductPrice.DbTableName),
                                    new { ValidTo = dataRec.ValidFrom.AddDays(-1) });
                                sql.Where(GetNegativeBaseWhereClause(), new { Key = dataRec.pk });
                                sql.Where(GetProductWhereClause(), new { ProductKey = dataRec.ProductKey });
                                sql.Where(GetStandardPriceWhereClause());
                                scope.Database.Execute(sql);
                            }
                            scope.Database.CompleteTransaction();
                            transOk = true;
                            finalResult = true;
                        }
                    }

                }
                finally
                {
                    if (!transOk)
                    {
                        scope.Database.AbortTransaction();
                    }
                    scope.Complete();
                }
            }

            return finalResult;
        }

        bool Update(EshoppgsoftwebProductPrice dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshoppgsoftwebProductPrice dataRec)
        {
            EshoppgsoftwebProductPriceCache.ClearCache();

            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebProductPrice.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshoppgsoftwebProductPrice.DbTableName);
        }
        string GetNegativeBaseWhereClause()
        {
            return string.Format("{0}.pk <> @Key", EshoppgsoftwebProductPrice.DbTableName);
        }
        string GetProductWhereClause()
        {
            return string.Format("{0}.productKey = @ProductKey", EshoppgsoftwebProductPrice.DbTableName);
        }
        string GetStandardPriceWhereClause()
        {
            return string.Format("{0}.validTo IS NULL", EshoppgsoftwebProductPrice.DbTableName);
        }
        string GetValidAtWhereClause()
        {
            return string.Format("{0}.validFrom <= @ValidAt AND ({0}.validTo IS NULL OR {0}.validTo >= @ValidAt)", EshoppgsoftwebProductPrice.DbTableName);
        }
    }

    [TableName(EshoppgsoftwebProductPrice.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshoppgsoftwebProductPrice : _BaseRepositoryRec
    {
        public const string DbTableName = "epProductPrice";

        public Guid ProductKey { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal VatRate { get; set; }
        public decimal Price_1_NoVat { get; set; }
        public decimal Price_1_WithVat { get; set; }

        public static EshoppgsoftwebProductPrice CreateUnknownPrice(Guid procutKey)
        {
            return new EshoppgsoftwebProductPrice()
            {
                ProductKey = procutKey,
                ValidFrom = new DateTime(2019, 1, 1),
                Price_1_NoVat = 1000000,
                Price_1_WithVat = 1000000,
            };
        }
    }

    public class EshoppgsoftwebProductPriceCache
    {
        static readonly object _cacheLock = new object();
        private static Hashtable htCache = null;
        private static DateTime htCreatedAt;

        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                EshoppgsoftwebProductPriceCache.htCache = null;
            }
        }

        public static EshoppgsoftwebProductPriceInfo GetProductPrice(Guid productKey)
        {
            EshoppgsoftwebProductPricesAtDate pricesCollection;

            lock (_cacheLock)
            {
                if (htCache != null && htCreatedAt < DateTime.Now.AddDays(-1))
                {
                    // Clear cache older than 24 hours
                    htCache = null;
                }
                if (htCache == null)
                {
                    htCache = new Hashtable();
                    htCreatedAt = DateTime.Now;
                }

                DateTime today = DateTime.Today;
                string key = DateTimeUtil.GetDateId(today);
                if (!htCache.ContainsKey(key))
                {
                    htCache.Add(key, new EshoppgsoftwebProductPricesAtDate(today));
                }

                pricesCollection = (EshoppgsoftwebProductPricesAtDate)htCache[key];
            }

            return pricesCollection.GetProductPrice(productKey);
        }
    }

    public class EshoppgsoftwebProductPricesAtDate
    {
        private Hashtable htProduct;

        public EshoppgsoftwebProductPricesAtDate(DateTime dateAt)
        {
            this.htProduct = new Hashtable();
            foreach (EshoppgsoftwebProductPrice price in new EshoppgsoftwebProductPriceRepository().GetForDate(dateAt))
            {
                EshoppgsoftwebProductPriceInfo priceInfo;
                string key = price.ProductKey.ToString();
                if (!this.htProduct.ContainsKey(key))
                {
                    this.htProduct.Add(key, priceInfo = new EshoppgsoftwebProductPriceInfo(dateAt));
                }
                else
                {
                    priceInfo = (EshoppgsoftwebProductPriceInfo)htProduct[key];
                }

                priceInfo.SetPrice(price);
            }
        }

        public EshoppgsoftwebProductPriceInfo GetProductPrice(Guid productKey)
        {
            string key = productKey.ToString();

            EshoppgsoftwebProductPriceInfo productPriceInfo = null;
            if (!htProduct.ContainsKey(key))
            {
                this.htProduct.Add(key, productPriceInfo = new EshoppgsoftwebProductPriceInfo(DateTime.Today));
                productPriceInfo.SetPrice(EshoppgsoftwebProductPrice.CreateUnknownPrice(productKey));
            }

            productPriceInfo = (EshoppgsoftwebProductPriceInfo)htProduct[key];
            productPriceInfo.EnsureStandardPrice();

            return productPriceInfo;
        }
    }

    public class EshoppgsoftwebProductPriceInfo
    {
        public DateTime ValidAt { get; private set; }
        public EshoppgsoftwebProductPrice StandardPrice { get; private set; }
        public ProductPriceModel StandardPriceModel
        {
            get
            {
                return ProductPriceModel.CreateCopyFrom(this.StandardPrice);
            }
        }
        public EshoppgsoftwebProductPrice ActionPrice { get; private set; }
        public ProductPriceModel ActionPriceModel
        {
            get
            {
                return ProductPriceModel.CreateCopyFrom(this.ActionPrice);
            }
        }
        public EshoppgsoftwebProductPrice CurrentPrice
        {
            get
            {
                return this.ActionPrice != null ? this.ActionPrice : this.StandardPrice;
            }
        }
        public ProductPriceModel CurrentPriceModel
        {
            get
            {
                return ProductPriceModel.CreateCopyFrom(this.CurrentPrice);
            }
        }

        public EshoppgsoftwebProductPriceInfo(DateTime dateAt)
        {
            this.ValidAt = dateAt;
        }

        public void SetPrice(EshoppgsoftwebProductPrice price)
        {
            if (price.ValidTo == null)
            {
                // Standard price
                if (this.StandardPrice == null || this.StandardPrice.ValidFrom < price.ValidFrom)
                {
                    this.StandardPrice = price;
                }
            }
            else
            {
                // Action price
                if (this.ActionPrice == null || this.ActionPrice.ValidFrom < price.ValidFrom)
                {
                    this.ActionPrice = price;
                }
            }
        }
        public void EnsureStandardPrice()
        {
            if (this.StandardPrice == null)
            {
                if (this.ActionPrice == null)
                {
                    this.StandardPrice = new EshoppgsoftwebProductPrice();
                }
                else
                {
                    this.StandardPrice = this.ActionPrice;
                }
            }
        }

        public decimal GetCurrentPriceNoVat()
        {
            return this.CurrentPrice.Price_1_NoVat;
        }
        public decimal GetCurrentPriceVatPerc()
        {
            return this.CurrentPrice.VatRate;
        }

        public decimal GetCurrentPriceWithVat()
        {
            return this.CurrentPrice.Price_1_WithVat;
        }
    }

    public class EshoppgsoftwebProductPriceComparer : IComparer<EshoppgsoftwebProductPrice>
    {
        public int Compare(EshoppgsoftwebProductPrice x, EshoppgsoftwebProductPrice y)
        {
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            return decimal.Compare(x.Price_1_NoVat, y.Price_1_NoVat);
        }
    }
}
