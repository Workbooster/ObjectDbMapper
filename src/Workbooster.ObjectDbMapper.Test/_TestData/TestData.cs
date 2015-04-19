﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Workbooster.ObjectDbMapper.Test._TestData
{
    public static class TestData
    {
        // Available Engines
        public enum DatabaseEngine { MSSQL, MySQL }

        // Engine for test
        public static readonly DatabaseEngine DATABASE_ENGINE = DatabaseEngine.MySQL;


        /* Microsoft T-SQL */

        public static readonly string MSSQL_CONNECTION_STRING = @"Data Source=(LocalDB)\v11.0; AttachDbFilename=|DataDirectory|\_TestData\data.mdf; Integrated Security=True;";
        public static readonly string MSSQL_SETUP_SCRIPT = @"_TestData\mssql_setup.sql";

        /* Oracle MySQL */

        public static readonly string MYSQL_CONNECTION_STRING = @"Server=127.0.0.1;Uid=root;Pwd=;Database=unittests;";
        public static readonly string MYSQL_SETUP_SCRIPT = @"_TestData\mysql_setup.sql";

        /// <summary>
        /// Prepares the testdata and creates and opens a connection.
        /// </summary>
        public static DbConnection SetupConnection()
        {
            string setuptScriptFilePath = "";
            string sqlSetupScript = "";
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DbConnection connection = null;

            switch (DATABASE_ENGINE)
            {
                case DatabaseEngine.MSSQL:
                    connection = new SqlConnection(MSSQL_CONNECTION_STRING);
                    setuptScriptFilePath = Path.Combine(currentDirectory, MSSQL_SETUP_SCRIPT);
                    break;
                case DatabaseEngine.MySQL:
                    connection = new MySqlConnection(MYSQL_CONNECTION_STRING);
                    setuptScriptFilePath = Path.Combine(currentDirectory, MYSQL_SETUP_SCRIPT);
                    break;
                default:
                    throw new Exception("Unknown Database Engine");
            }

            sqlSetupScript = File.ReadAllText(setuptScriptFilePath);

            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = sqlSetupScript;
            cmd.ExecuteNonQuery();

            return connection;
        }
    }
}
