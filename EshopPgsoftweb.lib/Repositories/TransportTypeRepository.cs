using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class TransportTypeRepository : _BaseRepository
    {
        public Page<TransportType> GetPage(long page, long itemsPerPage, string sortBy = "TransportOrder", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<TransportType>(page, itemsPerPage, sql);
        }

        public List<TransportType> GetRecordsForBasket()
        {
            return Fetch<TransportType>(GetBaseQuery().Append("ORDER BY TransportOrder"));
        }

        public TransportType Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<TransportType>(sql).FirstOrDefault();
        }

        public TransportType GetForGatewayType(int gatewayTypeId)
        {
            var sql = GetBaseQuery().Where(GetGatewayTypeWhereClause(), new { GatewayTypeId = gatewayTypeId });

            return Fetch<TransportType>(sql).FirstOrDefault();
        }

        public bool Save(TransportType dataRec)
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

        bool Insert(TransportType dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(TransportType dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(TransportType dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", TransportType.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", TransportType.DbTableName);
        }
        string GetGatewayTypeWhereClause()
        {
            return string.Format("{0}.gatewayTypeId = @GatewayTypeId", TransportType.DbTableName);
        }
    }

    [TableName(TransportType.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class TransportType : _BaseRepositoryRec
    {
        public const string DbTableName = "epTransportType";

        public int TransportOrder { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal PriceNoVat { get; set; }
        public decimal PriceWithVat { get; set; }
        public decimal VatPerc { get; set; }
        public int GatewayTypeId { get; set; }
    }
}
