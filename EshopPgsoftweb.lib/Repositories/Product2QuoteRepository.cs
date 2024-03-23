using eshoppgsoftweb.lib.Models;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class Product2QuoteRepository : _BaseRepository
    {
        static readonly object _productOrderLock = new object();

        public List<Product2Quote> GetQuoteItems(Guid pkQuote)
        {
            return GetPage(1, _PagingModel.AllItemsPerPage, pkQuote).Items;
        }
        public List<Product2Quote> GetQuoteProductItems(Guid pkQuote)
        {
            var sql = GetBaseQuery(false).Where(GetQuoteWhereClause(), new { KeyQuote = pkQuote });
            sql.Where(GetIsProductWhereClause(), new { EmptyKey = Guid.Empty });

            return Fetch<Product2Quote>(sql);
        }

        public int GetQuoteItemsCnt(Guid keyQuote)
        {
            //var sql = new Sql(string.Format("SELECT COALESCE(SUM(itemPcs),0) FROM {0}", Product2Quote.DbTableName));
            var sql = new Sql(string.Format("SELECT COUNT(*) FROM {0}", Product2Quote.DbTableName));
            sql.Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });
            sql.Where(GetIsProductWhereClause(), new { EmptyKey = Guid.Empty });
            return Fetch<int>(sql).FirstOrDefault();
        }

        public Page<Product2QuotePcs> GetProduct2QuotePcsPage(long page, long itemsPerPage)
        {
            // Nacitaj pocet predanych produktov
            // - pre odoslane objednavky
            // - pre produkty, ktore su stale v predaji
            //var sql = new Sql("select pkProduct, sum(itemPcs) as pcs from epProduct2Quote where pkQuote in (SELECT pk from epQuote where dateFinished is not null and sessionId is null) and pkProduct is not null and pkProduct != '00000000-0000-0000-0000-000000000000' and pkProduct in (SELECT pk from epProduct where productIsVisible=1) group by pkProduct order by pcs desc");
            var sql = new Sql("select pkProduct, count(itemPcs) as pcs from epProduct2Quote where pkQuote in (SELECT pk from epQuote where dateFinished is not null and sessionId is null) and pkProduct is not null and pkProduct != '00000000-0000-0000-0000-000000000000' and pkProduct in (SELECT pk from epProduct where productIsVisible=1) group by pkProduct order by pcs desc");

            return GetPage<Product2QuotePcs>(page, itemsPerPage, sql);
        }

        public Page<Product2Quote> GetPage(long page, long itemsPerPage, Guid keyQuote, string sortBy = "ItemOrder", string sortDir = "ASC")
        {
            var sql = GetBaseQuery(false).Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<Product2Quote>(page, itemsPerPage, sql);
        }

        public Product2Quote Get(Guid key)
        {
            var sql = GetBaseQuery(false).Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<Product2Quote>(sql).FirstOrDefault();
        }

        public Product2Quote GetForItemCode(Guid keyQuote, string itemCode)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });
            sql.Where(GetItemCodeWhereClause(), new { ItemCode = itemCode });

            return Fetch<Product2Quote>(sql).FirstOrDefault();
        }

        public void DeleteForItemCode(Guid keyQuote, string itemCode)
        {
            var sql = new Sql(string.Format("DELETE {0}", Product2Quote.DbTableName));
            sql.Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });
            sql.Where(GetItemCodeWhereClause(), new { ItemCode = itemCode });

            Execute(sql);
        }

        public Product2Quote Get(Guid keyQuote, Guid keyProduct)
        {
            var sql = GetBaseQuery(false).Where(GetQuoteProductWhereClause(), new { KeyQuote = keyQuote, KeyProduct = keyProduct });

            return Fetch<Product2Quote>(sql).FirstOrDefault();
        }

        public bool Save(Product2Quote dataRec)
        {
            bool result;
            if (IsNew(dataRec))
            {
                result = Insert(dataRec);
            }
            else
            {
                result = Update(dataRec);
            }

            if (result)
            {
                QuoteRepository rep = new QuoteRepository();
                rep.UpdateQuotePrice(dataRec.PkQuote);
            }

            return result;
        }

        bool Insert(Product2Quote dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                if ((Guid)result == dataRec.pk)
                {
                    // Record saved
                    // Set product order
                    bool isOk = SetItemOrder(dataRec);

                    return isOk;
                }
                return (bool)result;
            }

            return false;
        }

        bool Update(Product2Quote dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(Product2Quote dataRec)
        {
            if (DeleteInstance(dataRec))
            {
                new QuoteRepository().UpdateQuotePrice(dataRec.PkQuote);

                return true;
            }

            return false;
        }

        public bool DeleteOldSessionData(DateTime dt)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE FROM {0}", Product2Quote.DbTableName));
            sql.Where(string.Format("{0}.PkQuote IN (SELECT {1}.pk FROM {1} WHERE {1}.dateCreate < @DateCreate AND {1}.dateFinished IS NULL)", Product2Quote.DbTableName, Quote.DbTableName), new { DateCreate = dt });
            Execute(sql);

            return true;
        }

        public void DeleteNonProductItems(Guid keyQuote)
        {
            if (keyQuote != null && keyQuote != Guid.Empty)
            {
                var sql = new Sql(string.Format("DELETE {0}", Product2Quote.DbTableName));
                sql.Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });
                sql.Where(GetNonProductWhereClause(), new { EmptyKey = Guid.Empty });
                sql.Where(GetNotItemCodeWhereClause(), new { NotItemCode = ConfigurationUtil.Ecommerce_Quote_DiscountItemCode });

                Execute(sql);
                new QuoteRepository().UpdateQuotePrice(keyQuote);
            }
        }

        public void DeleteNonProductItem(Guid keyQuote, string itemCode)
        {
            if (keyQuote != null && keyQuote != Guid.Empty)
            {
                var sql = new Sql(string.Format("DELETE {0}", Product2Quote.DbTableName));
                sql.Where(GetNonProductWhereClause(), new { EmptyKey = Guid.Empty });
                sql.Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });
                sql.Where(GetItemCodeWhereClause(), new { ItemCode = itemCode });

                Execute(sql);
                new QuoteRepository().UpdateQuotePrice(keyQuote);
            }
        }

        public bool SetItemOrder(Product2Quote dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET {1} = (SELECT MAX({1})+1 FROM {0} WHERE {2}=@PkQuote) WHERE {3}=@Key",
                Product2Quote.DbTableName, "ItemOrder", "PkQuote", "pk"),
                new { PkQuote = dataRec.PkQuote, Key = dataRec.pk });

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

        Sql GetBaseQuery(bool isCount)
        {
            return new Sql(string.Format("SELECT * FROM {0}", Product2Quote.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", Product2Quote.DbTableName);
        }

        string GetItemCodeWhereClause()
        {
            return string.Format("{0}.ItemCode = @ItemCode", Product2Quote.DbTableName);
        }
        string GetNotItemCodeWhereClause()
        {
            return string.Format("{0}.ItemCode <> @NotItemCode", Product2Quote.DbTableName);
        }

        string GetQuoteProductWhereClause()
        {
            return string.Format("{0}.PkQuote = @KeyQuote AND {0}.PkProduct = @KeyProduct", Product2Quote.DbTableName);
        }

        string GetQuoteWhereClause()
        {
            return string.Format("{0}.PkQuote = @KeyQuote", Product2Quote.DbTableName);
        }

        string GetIsProductWhereClause()
        {
            return string.Format("{0}.pkProduct IS NOT NULL AND {0}.pkProduct<>@EmptyKey", Product2Quote.DbTableName);
        }

        string GetNonProductWhereClause()
        {
            return string.Format("{0}.pkProduct IS NULL OR {0}.pkProduct=@EmptyKey", Product2Quote.DbTableName);
        }
    }

    [TableName(Product2Quote.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class Product2Quote : _BaseRepositoryRec
    {
        public const string DbTableName = "epProduct2Quote";

        public Guid PkQuote { get; set; }
        public Guid PkProduct { get; set; }
        public string NonProductId { get; set; }
        public int ItemOrder { get; set; }
        public decimal ItemPcs { get; set; }
        public decimal UnitWeight { get; set; }
        public decimal UnitPriceNoVat { get; set; }
        public decimal UnitPriceWithVat { get; set; }
        public decimal VatPerc { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public int UnitTypeId { get; set; }

        public bool IsRealProductItem()
        {
            return this.PkProduct != null && this.PkProduct != Guid.Empty;
        }

        public decimal TotalWeight()
        {
            return this.UnitWeight * this.ItemPcs;
        }
        public decimal TotalItemPriceNoVat()
        {
            return VatUtil.CalculatePriceWithoutVat(TotalItemPriceWithVat(), this.VatPerc);
        }
        public decimal TotalItemPriceWithVat()
        {
            return this.UnitPriceWithVat * this.ItemPcs;
        }
    }

    public class Product2QuotePcs
    {
        public Guid PkProduct { get; set; }
        public int Pcs { get; set; }
    }
}
