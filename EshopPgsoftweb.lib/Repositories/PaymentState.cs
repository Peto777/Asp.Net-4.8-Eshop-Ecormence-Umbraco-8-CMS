using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class PaymentStateRepository : _BaseRepository
    {
        public List<PaymentState> GetRecords(string sort = "code", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sort, sortDir));

            return Fetch<PaymentState>(sql);
        }

        public PaymentState Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<PaymentState>(sql).FirstOrDefault();
        }

        public bool Save(PaymentState dataRec)
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

        bool Insert(PaymentState dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(PaymentState dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(PaymentState dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            var sql = new Sql(string.Format("SELECT * FROM {0} ", PaymentState.DbTableName));

            return sql;
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", PaymentState.DbTableName);
        }
    }

    [TableName(PaymentState.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class PaymentState : _BaseRepositoryRec
    {
        public const string DbTableName = "epPaymentState";

        public string Code { get; set; }
        public string Title { get; set; }
    }
}
