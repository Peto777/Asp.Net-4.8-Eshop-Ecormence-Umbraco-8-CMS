using NPoco;
using System;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebAvailabilityRepository : _BaseRepository
    {
        public Page<EshoppgsoftwebAvailability> GetPage(long page, long itemsPerPage, string sortBy = "AvailabilityName", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshoppgsoftwebAvailability>(page, itemsPerPage, sql);
        }

        public EshoppgsoftwebAvailability Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshoppgsoftwebAvailability>(sql).FirstOrDefault();
        }

        public bool Save(EshoppgsoftwebAvailability dataRec)
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

        bool Insert(EshoppgsoftwebAvailability dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(EshoppgsoftwebAvailability dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshoppgsoftwebAvailability dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebAvailability.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshoppgsoftwebAvailability.DbTableName);
        }
    }


    [TableName(EshoppgsoftwebAvailability.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshoppgsoftwebAvailability : _BaseRepositoryRec
    {
        public const string DbTableName = "epAvailability";

        public string AvailabilityName { get; set; }
    }
}
