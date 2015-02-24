using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workbooster.ObjectDbMapper
{
    public static class ConnectionExcetions
    {
        public static IList<T> Select<T>(this DbConnection connection, string sql, DbParameter[] parameters = null) where T : new()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            DbCommand sqlCmd = connection.CreateCommand();
            sqlCmd.CommandText = sql;
            
            if (parameters != null)
            {
                sqlCmd.Parameters.AddRange(parameters);
            }

            DbDataReader reader = sqlCmd.ExecuteReader();
            
            return reader.Read<T>();
        }
    }
}
