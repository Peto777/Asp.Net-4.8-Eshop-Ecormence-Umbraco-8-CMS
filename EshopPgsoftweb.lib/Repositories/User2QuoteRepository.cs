using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class User2QuoteRepository : _BaseRepository
    {
        static readonly object _productOrderLock = new object();

        public User2Quote GetForQuote(Guid keyQuote)
        {
            var sql = GetBaseQuery().Where(GetQuoteWhereClause(), new { KeyQuote = keyQuote });

            return Fetch<User2Quote>(sql).FirstOrDefault();
        }

        public User2Quote Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<User2Quote>(sql).FirstOrDefault();
        }

        public bool Save(User2Quote dataRec, bool checkDupl = false)
        {
            bool isOk;
            if (IsNew(dataRec))
            {
                isOk = Insert(dataRec);
            }
            else
            {
                isOk = Update(dataRec);
            }

            if (isOk && checkDupl)
            {
                DeleteDuplQuoteData(dataRec);
            }

            return isOk;
        }

        bool Insert(User2Quote dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(User2Quote dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(User2Quote dataRec)
        {
            return DeleteInstance(dataRec);
        }

        public void DeleteDuplQuoteData(User2Quote dataRec)
        {
            var sql = GetBaseQuery().Where(GetQuoteWhereClause(), new { KeyQuote = dataRec.PkQuote });
            List<User2Quote> dataList = Fetch<User2Quote>(sql);

            if (dataList.Count > 1)
            {
                foreach (User2Quote duplRec in dataList)
                {
                    if (duplRec.pk != dataRec.pk)
                    {
                        Delete(duplRec);
                    }
                }
            }
        }

        public bool DeleteOldSessionData(DateTime dt)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE FROM {0}", User2Quote.DbTableName));
            sql.Where(string.Format("{0}.PkQuote IN (SELECT {1}.pk FROM {1} WHERE {1}.dateCreate < @DateCreate AND {1}.dateFinished IS NULL)", User2Quote.DbTableName, Quote.DbTableName), new { DateCreate = dt });
            Execute(sql);

            return true;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", User2Quote.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", User2Quote.DbTableName);
        }

        string GetQuoteWhereClause()
        {
            return string.Format("{0}.PkQuote = @KeyQuote", User2Quote.DbTableName);
        }
    }

    [TableName(User2Quote.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class User2Quote : _BaseRepositoryRec
    {
        public const string DbTableName = "epUser2Quote";

        public Guid PkQuote { get; set; }
        public Guid PkUser { get; set; }

        public bool IsCompanyInvoice { get; set; }
        public string CompanyName { get; set; }
        public string CompanyIco { get; set; }
        public string CompanyDic { get; set; }
        public string CompanyIcdph { get; set; }

        public string InvName { get; set; }
        public string InvStreet { get; set; }
        public string InvCity { get; set; }
        public string InvZip { get; set; }
        public string InvCountry { get; set; }

        public bool IsDeliveryAddress { get; set; }
        public string DeliveryName { get; set; }
        public string DeliveryStreet { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryZip { get; set; }
        public string DeliveryCountry { get; set; }

        public string QuoteEmail { get; set; }
        public string QuotePhone { get; set; }

        public string Note { get; set; }
    }
}
