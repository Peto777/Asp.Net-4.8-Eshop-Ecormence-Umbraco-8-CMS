using eshoppgsoftweb.lib.Tasks.Ecommerce;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class QuoteRepository : _BaseRepository
    {
        static readonly object _quoteNumberLock = new object();

        public Quote Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<Quote>(sql).FirstOrDefault();
        }

        public Quote GetForSession(string sessionId)
        {
            var sql = GetBaseQuery().Where(GetSessionWhereClause(), new { SessionId = sessionId });

            Quote quote = Fetch<Quote>(sql).FirstOrDefault();
            if (quote == null || quote.pk == Guid.Empty)
            {
                quote = new Quote();
                quote.SessionId = sessionId;
                this.Insert(quote);
            }

            return quote;
        }

        public Quote GetNewestForFinishedSession(string sessionId)
        {
            var sql = GetBaseQuery().Where(GetFinishedSessionWhereClause(), new { FinishedSessionId = sessionId });
            sql.OrderBy(string.Format("{0}.dateFinished DESC", Quote.DbTableName));

            return Fetch<Quote>(sql).FirstOrDefault();
        }

        public Guid GetQuoteIdForSessionId(string sessionId)
        {
            var sql = new Sql(string.Format("SELECT pk FROM {0}", Quote.DbTableName)).Where(GetSessionWhereClause(), new { SessionId = sessionId });
            return Fetch<Guid>(sql).FirstOrDefault();
        }

        public List<string> GetAllStates()
        {
            var sql = new Sql(string.Format("SELECT DISTINCT(quoteState) FROM {0} ORDER BY quoteState", Quote.DbTableName));
            return Fetch<string>(sql);
        }
        public List<string> GetAllPaymentStates()
        {
            var sql = new Sql(string.Format("SELECT DISTINCT(quotePriceState) FROM {0} ORDER BY quotePriceState", Quote.DbTableName));
            return Fetch<string>(sql);
        }

        public bool Save(Quote dataRec)
        {
            bool makeMkSoftExportAfterSave = CheckMkSoftExportBeforeSave(dataRec);

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
                UpdateQuotePrice(dataRec.pk);
                if (makeMkSoftExportAfterSave)
                {
                    QuoteToXml.ExportToFtp(dataRec.pk);
                }
            }

            return result;
        }

        bool Insert(Quote dataRec)
        {
            dataRec.pk = Guid.NewGuid();
            dataRec.DateCreate = DateTime.Now;

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(Quote dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(Quote dataRec)
        {
            return DeleteInstance(dataRec);
        }

        public bool DeleteOldSessionData(DateTime dt)
        {
            new Product2QuoteRepository().DeleteOldSessionData(dt);
            new User2QuoteRepository().DeleteOldSessionData(dt);

            var sql = new Sql();
            sql.Append(string.Format("DELETE FROM {0}", Quote.DbTableName));
            sql.Where(string.Format("{0}.dateCreate < @DateCreate AND {0}.dateFinished IS NULL", Quote.DbTableName), new { DateCreate = dt });
            Execute(sql);

            return true;
        }

        public void UpdateQuotePrice(Guid pk)
        {
            // Calculate total price
            decimal quotePriceNoVat = 0, quotePriceWithVat = 0;
            List<Product2Quote> dataList = new Product2QuoteRepository().GetQuoteItems(pk);
            foreach (Product2Quote dataRec in dataList)
            {
                quotePriceNoVat += dataRec.TotalItemPriceNoVat();
                quotePriceWithVat += dataRec.TotalItemPriceWithVat();
            }

            // Update database
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET quotePriceNoVat=@TotalPriceNoVat, quotePriceWithVat=@TotalPriceWithVat WHERE pk=@Key",
                Quote.DbTableName),
                new { Key = pk, TotalPriceNoVat = quotePriceNoVat, TotalPriceWithVat = quotePriceWithVat });

            Execute(sql);
        }

        public Quote FinishQuote(Guid pkQuote, string sessionId)
        {
            return FinishQuote(Get(pkQuote), sessionId);
        }
        public Quote FinishQuote(string sessionId)
        {
            return FinishQuote(GetForSession(sessionId), sessionId);
        }
        public Quote FinishQuote(Quote quote, string sessionId)
        {
            quote.SessionId = null;
            quote.FinishedSessionId = sessionId;
            quote.DateFinished = DateTime.Now;
            quote.QuoteYear = ((DateTime)quote.DateFinished).Year;
            quote.QuoteState = ConfigurationUtil.InitialQuoteState();
            Save(quote);
            SetQuoteNumber(quote);

            return quote;
        }
        public Quote SetQuotePaied(Guid pk)
        {
            Quote quote = Get(pk);
            quote.QuotePriceState = ConfigurationUtil.PaiedQuotePriceState();
            Save(quote);

            return quote;
        }
        public bool SetQuoteNumber(Quote quote)
        {
            var sql = new Sql();
            sql.Append(string.Format("UPDATE {0} SET quoteNumber = (SELECT MAX(quoteNumber)+1 FROM {0} WHERE quoteYear=@Year) WHERE pk=@Key", Quote.DbTableName),
                new { Key = quote.pk, Year = quote.QuoteYear });

            int cnt = 0;
            lock (_quoteNumberLock)
            {
                cnt = Execute(sql);
            }
            if (cnt > 0)
            {
                return true;
            }

            return false;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", Quote.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", Quote.DbTableName);
        }

        string GetSessionWhereClause()
        {
            return string.Format("{0}.SessionId = @SessionId", Quote.DbTableName);
        }
        string GetFinishedSessionWhereClause()
        {
            return string.Format("{0}.FinishedSessionId = @FinishedSessionId AND {0}.DateFinished IS NOT NULL", Quote.DbTableName);
        }

        bool CheckMkSoftExportBeforeSave(Quote dataRec)
        {
            if (string.IsNullOrEmpty(dataRec.QuoteState))
            {
                return false;
            }
            QuoteState state = new QuoteStateRepository().Get(dataRec.QuoteState);
            if (state == null || !state.ExportToMksoft)
            {
                return false;
            }

            // Quote state is valid for export
            if (IsNew(dataRec))
            {
                // MAKE EXPORT for new quote
                return true;
            }

            Quote oldDataRec = Get(dataRec.pk);
            if (oldDataRec.QuoteState != dataRec.QuoteState)
            {
                // Quote state has changed
                // MAKE EXPORT for existing quote
                return true;
            }

            return false;
        }
    }

    [TableName(Quote.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class Quote : _BaseRepositoryRec
    {
        public const string DbTableName = "epQuote";

        public DateTime DateCreate { get; set; }
        public DateTime? DateFinished { get; set; }
        public string SessionId { get; set; }
        public string FinishedSessionId { get; set; }
        public int QuoteYear { get; set; }
        public int QuoteNumber { get; set; }

        public decimal QuotePriceNoVat { get; set; }
        public decimal QuotePriceWithVat { get; set; }

        public string QuoteState { get; set; }
        public string QuotePriceState { get; set; }
        public string Note { get; set; }
    }
}
