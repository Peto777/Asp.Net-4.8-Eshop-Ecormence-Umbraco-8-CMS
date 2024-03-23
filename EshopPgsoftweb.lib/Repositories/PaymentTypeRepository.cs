using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class PaymentTypeRepository : _BaseRepository
    {
        public Page<PaymentType> GetPage(long page, long itemsPerPage, string sortBy = "PaymentOrder", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<PaymentType>(page, itemsPerPage, sql);
        }

        public List<PaymentType> GetRecordsForBasket()
        {
            return Fetch<PaymentType>(GetBaseQuery().Append("ORDER BY PaymentOrder"));
        }

        public PaymentType Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<PaymentType>(sql).FirstOrDefault();
        }

        public PaymentType GetForGatewayType(int gatewayTypeId)
        {
            var sql = GetBaseQuery().Where(GetGatewayTypeWhereClause(), new { GatewayTypeId = gatewayTypeId });

            return Fetch<PaymentType>(sql).FirstOrDefault();
        }

        public bool Save(PaymentType dataRec)
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

        bool Insert(PaymentType dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(PaymentType dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(PaymentType dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", PaymentType.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", PaymentType.DbTableName);
        }
        string GetGatewayTypeWhereClause()
        {
            return string.Format("{0}.gatewayTypeId = @GatewayTypeId", PaymentType.DbTableName);
        }
    }

    [TableName(PaymentType.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class PaymentType : _BaseRepositoryRec
    {
        public const string DbTableName = "epPaymentType";

        public int PaymentOrder { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal PriceNoVat { get; set; }
        public decimal PriceWithVat { get; set; }
        public decimal VatPerc { get; set; }
        public int GatewayTypeId { get; set; }
    }
}
