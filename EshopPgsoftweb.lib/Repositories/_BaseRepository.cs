using NPoco;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;

namespace eshoppgsoftweb.lib.Repositories
{
    public class _BaseRepository
    {
        protected readonly IMemberService MemberService;

        public _BaseRepository()
        {
            MemberService = Current.Services.MemberService;
        }

        public bool IsNew(_BaseRepositoryRec dataRec)
        {
            return dataRec.pk == null || dataRec.pk == Guid.Empty;
        }

        protected Page<T> GetPage<T>(long page, long itemsPerPage, Sql sql)
        {
            Page<T> ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.Page<T>(page, itemsPerPage, sql);

                scope.Complete();
            }

            return ret;
        }
        protected List<T> Fetch<T>(Sql sql)
        {
            List<T> ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.Fetch<T>(sql);

                scope.Complete();
            }

            return ret;
        }
        protected object InsertInstance<T>(T obj)
        {
            object ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.Insert<T>(obj);

                scope.Complete();
            }

            return ret;
        }
        protected bool UpdateInstance<T>(T obj)
        {
            int ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.Update(obj);

                scope.Complete();
            }

            return ret > 0;
        }
        protected bool DeleteInstance<T>(T obj)
        {
            int ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.Delete(obj);

                scope.Complete();
            }

            return ret > 0;
        }
        protected int Execute(Sql sql)
        {
            int ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.Execute(sql);

                scope.Complete();
            }

            return ret;
        }

        protected T ExecuteScalar<T>(Sql sql)
        {
            T ret;

            using (var scope = Current.ScopeProvider.CreateScope())
            {
                ret = scope.Database.ExecuteScalar<T>(sql);

                scope.Complete();
            }

            return ret;
        }

        public string GetKeysForInClause(List<string> keyList)
        {
            StringBuilder strIn = new StringBuilder();
            foreach (string key in keyList)
            {
                if (strIn.Length > 0)
                {
                    strIn.Append(",");
                }
                strIn.Append(string.Format("'{0}'", key));
            }
            return strIn.ToString();
        }
    }

    public class _BaseRepositoryRec
    {
        [Column("pk")]
        public Guid pk { get; set; }
    }
}
