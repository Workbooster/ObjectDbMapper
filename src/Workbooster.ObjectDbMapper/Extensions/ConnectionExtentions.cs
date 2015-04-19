﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Workbooster.ObjectDbMapper.Commands;

namespace Workbooster.ObjectDbMapper
{
    public static class ConnectionExcetions
    {
        /// <summary>
        /// Creates a new SELECT query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Query<T> Select<T>(this DbConnection connection, string sql, DbParameter[] parameters = null) where T : new()
        {
            var query = new Query<T>(connection, sql);

            if (parameters != null) query.Parameters.AddRange(parameters);
            
            return query;
        }

        /// <summary>
        /// Creates a new INSERT command without executing it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new UPDATE command without executing it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the database engne type of the current connection. This is used for database specific commands or behaviour.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DatabaseEngineEnum GetDatabaseType(this DbConnection connection)
        {
            if (connection.GetType().Name == "SqlConnection")
            {
                return DatabaseEngineEnum.MSSQL;
            }
            else if (connection.GetType().Name == "MySqlConnection")
            {
                return DatabaseEngineEnum.MySQL;
            }

            return DatabaseEngineEnum.Unknown;
        }

        /// <summary>
        /// Escapes names of databse objects like tables, columns etc.
        /// Example for Microsoft SQL: SELECT [columnName] FROM [tableName]
        /// Example for MySQL: SELECT `columnName` FROM `tableName`
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string EscapeObjectName(this DbConnection connection, string name)
        {
            switch (connection.GetDatabaseType())
            {
                case DatabaseEngineEnum.MSSQL:
                    return String.Format("[{0}]", name);
                case DatabaseEngineEnum.MySQL:
                    return String.Format("`{0}`", name);
                default:
                    // ANSI SQL default
                    return String.Format("\"{0}\"", name);
            }
        }
    }
}
