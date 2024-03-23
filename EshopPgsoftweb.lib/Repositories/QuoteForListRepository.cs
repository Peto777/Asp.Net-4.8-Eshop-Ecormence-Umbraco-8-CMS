using eshoppgsoftweb.lib.Models.Ecommerce;
using eshoppgsoftweb.lib.Util;
using NPoco;
using System;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class QuoteForListRepository : _BaseRepository
    {
        public Page<QuoteForList> GetPage(long page, long itemsPerPage, string sortBy = "dateFinished", string sortDir = "DESC", QuoteForListFilter filter = null)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause());
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.QuoteId))
                {
                    sql.Where(GetQuoteIdWhereClause(), new { QuoteYear = filter.QuoteYear, QuoteNumber = filter.QuoteNumber });
                }
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
                if (filter.From != null)
                {
                    sql.Where(GetFromWhereClause(), new { ValidFrom = filter.From });
                }
                if (filter.To != null)
                {
                    sql.Where(GetToWhereClause(), new { ValidTo = filter.To });
                }
                if (filter.QuoteStates != null && filter.QuoteStates.Length > 0)
                {
                    sql.Where(GetQuoteStateInWhereClause(filter.QuoteStates));
                }
            }
            switch (sortBy)
            {
                case "QuoteId":
                    sql.Append(string.Format("ORDER BY QuoteYear {0}, QuoteNumber {0}", sortDir));
                    break;

                default:
                    sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));
                    break;
            }

            return GetPage<QuoteForList>(page, itemsPerPage, sql);
        }

        public Page<QuoteForList> GetForUser(long page, long itemsPerPage, int memberId, string userEmail)
        {
            EshoppgsoftwebCustomer customer = new EshoppgsoftwebCustomerRepository().GetForOwner(memberId);

            var sql = GetBaseQuery().Where(GetBaseWhereClause());
            if (customer == null)
            {
                sql.Where(GetEmailWhereClause(), new { UserEmail = userEmail });
            }
            else
            {
                sql.Where(GetUserWhereClause(), new { UserKey = customer.pk, UserEmail = userEmail });
            }
            sql.Append("ORDER BY QuoteYear DESC, QuoteNumber DESC");

            return GetPage<QuoteForList>(page, itemsPerPage, sql);
        }

        Sql GetBaseQuery()
        {
            var sql = new Sql();
            sql.Append(string.Format("SELECT {0}.pk, {0}.DateFinished, {0}.QuotePriceNoVat, {0}.QuotePriceWithVat, {0}.QuoteYear, {0}.QuoteNumber, {1}.pkUser, {1}.invName, {1}.invCity, {1}.quoteEmail, {1}.quotePhone, {0}.quoteState, {0}.quotePriceState  FROM {0}, {1}", Quote.DbTableName, User2Quote.DbTableName));

            return sql;
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.dateFinished IS NOT NULL AND {0}.pk = {1}.pkQuote", Quote.DbTableName, User2Quote.DbTableName);
        }
        string GetUserWhereClause()
        {
            return string.Format("{0}.pkUser=@UserKey OR {0}.quoteEmail=@UserEmail", User2Quote.DbTableName);
        }
        string GetEmailWhereClause()
        {
            return string.Format("{0}.quoteEmail=@UserEmail", User2Quote.DbTableName);
        }
        string GetQuoteIdWhereClause()
        {
            return string.Format("{0}.quoteYear = @QuoteYear AND {0}.quoteNumber = @QuoteNumber", Quote.DbTableName);
        }
        string GetSearchTextWhereClause(string searchText)
        {
            return string.Format("{0}.invName LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.quoteEmail LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.quotePhone LIKE '%{1}%' collate Latin1_general_CI_AI", User2Quote.DbTableName, searchText);
        }
        string GetFromWhereClause()
        {
            return string.Format("{0}.dateFinished >= @ValidFrom", Quote.DbTableName);
        }
        string GetToWhereClause()
        {
            return string.Format("{0}.dateFinished <= @ValidTo", Quote.DbTableName);
        }
        string GetQuoteStateInWhereClause(string[] quoteStates)
        {
            return string.Format("{0}.quoteState IN ({1})", Quote.DbTableName, GetKeysForInClause(quoteStates.ToList()));
        }
    }

    public class QuoteForList : _BaseRepositoryRec
    {
        public Guid PkUser { get; set; }
        public DateTime DateFinished { get; set; }
        public int QuoteYear { get; set; }
        public int QuoteNumber { get; set; }
        public string QuoteId
        {
            get
            {
                return QuoteModel.GetQuoteId(this.QuoteYear, this.QuoteNumber);
            }
        }

        public string InvName { get; set; }
        public string InvCity { get; set; }
        public string QuoteEmail { get; set; }
        public string QuotePhone { get; set; }

        public decimal QuotePriceNoVat { get; set; }
        public decimal QuotePriceWithVat { get; set; }
        public string QuoteState { get; set; }
        public string QuotePriceState { get; set; }

        public string QuotePriceWithVatView
        {
            get
            {
                return Util.PriceUtil.NumberToEditorString(this.QuotePriceWithVat);
            }
        }
        public string QuotePriceWithVatWithCurrency
        {
            get
            {
                return PriceUtil.GetPriceString(this.QuotePriceWithVat);
            }
        }
        public string DateFinishedView
        {
            get
            {
                return DateTimeUtil.GetDisplayDateTime(this.DateFinished);
            }
        }
    }

    public class QuoteForListFilter
    {
        public string QuoteId { get; set; }
        public string SearchText { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string[] QuoteStates { get; set; }

        public int QuoteYear
        {
            get
            {
                return GetQuoteYear();
            }
        }
        public int QuoteNumber
        {
            get
            {
                return GetQuoteNumber();
            }
        }

        int GetQuoteYear()
        {
            if (string.IsNullOrEmpty(this.QuoteId))
            {
                return 0;
            }

            string strYear = this.QuoteId.Length > 4 ? this.QuoteId.Substring(0, 4) : this.QuoteId;
            int year;
            if (int.TryParse(strYear, out year))
            {
                return year;
            }

            return 0;
        }

        int GetQuoteNumber()
        {
            if (string.IsNullOrEmpty(this.QuoteId))
            {
                return 0;
            }

            string strNumber = this.QuoteId.Length > 4 ? this.QuoteId.Substring(4) : "0";
            int number;
            if (int.TryParse(strNumber, out number))
            {
                return number;
            }

            return 0;
        }
    }
}
