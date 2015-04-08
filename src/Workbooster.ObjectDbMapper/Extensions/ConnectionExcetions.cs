using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Workbooster.ObjectDbMapper
{
    public static class ConnectionExcetions
    {
        public static Query<T> Select<T>(this DbConnection connection, string sql, DbParameter[] parameters = null) where T : new()
        {
            var query = new Query<T>(connection, sql);

            if (parameters != null) query.Parameters.AddRange(parameters);

            return query;
        }
    }
}
