using NPoco;
using System;
using System.Collections.Generic;

namespace eshoppgsoftweb.lib.Repositories
{
    public class EshoppgsoftwebProductRelationRepository : _BaseRepository
    {
        public List<EshoppgsoftwebProductRelation> GetForProduct(Guid productKey)
        {
            var sql = GetBaseQuery().Where(GetProductMainWhereClause(), new { KeyProductMain = productKey });

            return Fetch<EshoppgsoftwebProductRelation>(sql);
        }

        public bool Insert(EshoppgsoftwebProductRelation dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("INSERT INTO {0} (pkProductMain, pkProductRelated) VALUES (@PkProductMain, @PkProductRelated)",
                EshoppgsoftwebProductRelation.DbTableName),
                new { PkProductMain = dataRec.PkProductMain, PkProductRelated = dataRec.PkProductRelated });

            return Execute(sql) > 0;
        }

        public bool Delete(EshoppgsoftwebProductRelation dataRec)
        {
            var sql = new Sql();
            sql.Append(string.Format("DELETE {0} WHERE pkProductMain=@PkProductMain AND pkProductRelated=@PkProductRelated", EshoppgsoftwebProductRelation.DbTableName),
                new { PkProductMain = dataRec.PkProductMain, PkProductRelated = dataRec.PkProductRelated });

            return Execute(sql) > 0;
        }

        public bool DeleteForProduct(Guid productKey)
        {
            bool isOK = true;
            List<EshoppgsoftwebProductRelation> dataList = GetForProduct(productKey);
            foreach (EshoppgsoftwebProductRelation dataRec in dataList)
            {
                if (!Delete(dataRec))
                {
                    isOK = false;
                }
            }

            return isOK;
        }

        Sql GetBaseQuery()
        {
            return new Sql(string.Format("SELECT * FROM {0}", EshoppgsoftwebProductRelation.DbTableName));
        }

        string GetProductMainWhereClause()
        {
            return string.Format("{0}.pkProductMain = @KeyProductMain", EshoppgsoftwebProductRelation.DbTableName);
        }
    }

    [TableName(EshoppgsoftwebProductRelation.DbTableName)]
    public class EshoppgsoftwebProductRelation : _BaseRepositoryRec
    {
        public const string DbTableName = "epProductRelation";

        public Guid PkProductMain { get; set; }
        public Guid PkProductRelated { get; set; }
    }
}
