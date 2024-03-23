using NPoco;
using System;
using System.Linq;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebProducerRepository : _BaseRepository
    {
        public Page<EshoppgsoftwebProducer> GetPage(long page, long itemsPerPage, string sortBy = "ProducerName", string sortDir = "ASC", EshoppgsoftwebProducerFilter filter = null)
        {
            var sql = GetBaseQuery();
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchText))
                {
                    sql.Where(GetSearchTextWhereClause(filter.SearchText), new { SearchText = filter.SearchText });
                }
            }
            sql.Append(string.Format("ORDER BY {0} {1}", sortBy, sortDir));

            return GetPage<EshoppgsoftwebProducer>(page, itemsPerPage, sql);
        }

        public EshoppgsoftwebProducer Get(Guid key)
        {
            var sql = GetBaseQuery().Where(GetBaseWhereClause(), new { Key = key });

            return Fetch<EshoppgsoftwebProducer>(sql).FirstOrDefault();
        }

        public bool Save(EshoppgsoftwebProducer dataRec)
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

        bool Insert(EshoppgsoftwebProducer dataRec)
        {
            dataRec.pk = Guid.NewGuid();

            object result = InsertInstance(dataRec);
            if (result is Guid)
            {
                return (Guid)result == dataRec.pk;
            }

            return false;
        }

        bool Update(EshoppgsoftwebProducer dataRec)
        {
            return UpdateInstance(dataRec);
        }

        public bool Delete(EshoppgsoftwebProducer dataRec)
        {
            return DeleteInstance(dataRec);
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebProducer.DbTableName));
        }

        string GetBaseWhereClause()
        {
            return string.Format("{0}.pk = @Key", EshoppgsoftwebProducer.DbTableName);
        }
        string GetSearchTextWhereClause(string searchText)
        {
            return string.Format("{0}.producerName LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.producerDescription LIKE '%{1}%' collate Latin1_general_CI_AI OR {0}.producerWeb LIKE '%{1}%' collate Latin1_general_CI_AI", EshoppgsoftwebProducer.DbTableName, searchText);
        }
    }


    [TableName(EshoppgsoftwebProducer.DbTableName)]
    [PrimaryKey("pk", AutoIncrement = false)]
    public class EshoppgsoftwebProducer : _BaseRepositoryRec
    {
        public const string DbTableName = "epProducer";

        public string ProducerName { get; set; }
        public string ProducerDescription { get; set; }
        public string ProducerWeb { get; set; }
    }

    public class EshoppgsoftwebProducerFilter
    {
        public string SearchText { get; set; }
    }
}
