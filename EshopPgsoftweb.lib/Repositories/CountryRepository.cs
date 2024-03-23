using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class CountryRepository : _BaseRepository
    {
        public Page<Country> GetPage(long page, long itemsPerPage, string sortBy = "Code", string sortDir = "ASC")
        {
            var sql = GetBaseQuery();
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<Country>(page, itemsPerPage, sql);
        }

        public List<Country> GetRecordsForBasket()
        {
            return Fetch<Country>(GetBaseQuery().Append("ORDER BY code"));
        }

        public Country Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<Country>(sql).FirstOrDefault();
        }

        public bool Save(Country dataRec)
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

        bool Insert(Country dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(Country dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(Country dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", Country.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", Country.DbTableName);
        }
    }

    [TableName(Country.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class Country : _BaseRepositoryRec
    {
        public const string DbTableName = "epCountry";

        public string Code { get; set; }
        public string Name { get; set; }
    }
}
