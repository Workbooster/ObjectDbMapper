using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Commands;

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

        public static InsertCommand<T> NewInsert<T>(this DbConnection connection, string tableName = null)
        {
            if (tableName == null)
            {
                return new InsertCommand<T>(connection);
            }
            else
            {

                return new InsertCommand<T>(connection, tableName);
            }
        }

        public static UpdateCommand<T> NewUpdate<T>(this DbConnection connection, string tableName = null)
        {
            if (tableName == null)
            {
                return new UpdateCommand<T>(connection);
            }
            else
            {
                return new UpdateCommand<T>(connection, tableName);
            }
        }
    }
}
