using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class QuoteStateRepository : _BaseRepository
    {
        public List<QuoteState> GetRecords(string sort = "code", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sort, sortDir));

            return Fetch<QuoteState>(sql);
        }

        public QuoteState Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<QuoteState>(sql).FirstOrDefault();
        }

        public QuoteState Get(string title)
        {
            var sql = GetBaseQuery().Where(GetTitleWhereClause(), new { Title = title });

            return Fetch<QuoteState>(sql).FirstOrDefault();
        }

        public bool Save(QuoteState dataRec)
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

        bool Insert(QuoteState dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(QuoteState dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(QuoteState dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            var sql = new Sql(string.Format("SELECT * FROM {0} ", QuoteState.DbTableName));

            return sql;
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", QuoteState.DbTableName);
        }

        string GetTitleWhereClause()
        {
            return string.Format("{0}.title = @Title", QuoteState.DbTableName);
        }
    }

    [TableName(QuoteState.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class QuoteState : _BaseRepositoryRec
    {
        public const string DbTableName = "epQuoteState";

        public string Code { get; set; }
        public string Title { get; set; }
        public bool ExportToMksoft { get; set; }
    }
}
