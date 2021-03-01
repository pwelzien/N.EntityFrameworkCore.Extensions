﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace N.EntityFrameworkCore.Extensions
{
    internal static class SqlUtil
    {
        internal static int ExecuteSql(string query, DbConnection connection, DbTransaction transaction, BulkOptions options)
        {
            int rowsAffected;
            using (var dbCommand = connection.CreateCommand())
            {
                if (options?.CommandTimeout.HasValue ?? false)
                {
                    dbCommand.CommandTimeout = options.CommandTimeout.Value;
                }
                dbCommand.Transaction = transaction;
                dbCommand.CommandText = query;
                rowsAffected=dbCommand.ExecuteNonQuery();
            }
            return rowsAffected;
        }
        internal static int ExecuteSql(string query, SqlConnection connection, SqlTransaction transaction, BulkOptions options)
        {
            var sqlCommand = new SqlCommand(query, connection, transaction);
            if (options?.CommandTimeout.HasValue ?? false)
            {
                sqlCommand.CommandTimeout = options.CommandTimeout.Value;
            }
            return sqlCommand.ExecuteNonQuery();
        }
        internal static object ExecuteScalar(string query, SqlConnection connection, SqlTransaction transaction, BulkOptions options)
        {
            var sqlCommand = new SqlCommand(query, connection, transaction);
            if (options?.CommandTimeout.HasValue ?? false)
            {
                sqlCommand.CommandTimeout = options.CommandTimeout.Value;
            }
            return sqlCommand.ExecuteScalar();
        }
        internal static int DeleteTable(string tableName, SqlConnection connection, SqlTransaction transaction)
        {
            return ExecuteSql(string.Format("DROP TABLE {0}", tableName), connection, transaction, null);
        }
        internal static int CloneTable(string sourceTable, string destinationTable, string[] columnNames, SqlConnection connection, SqlTransaction transaction, string internalIdColumnName=null)
        {
            string columns = columnNames != null && columnNames.Length > 0 ? string.Join(",", columnNames) : "*";
            columns = !string.IsNullOrEmpty(internalIdColumnName) ? string.Format("{0},CAST( NULL AS INT) AS {1}",columns, internalIdColumnName) : columns;
            return ExecuteSql(string.Format("SELECT TOP 0 {0} INTO {1} FROM {2}", columns, destinationTable, sourceTable), connection, transaction, null);
        }
        internal static string ConvertToColumnString(IEnumerable<string> columnNames)
        {
            return string.Join(",", columnNames);
        }
        internal static int ToggleIdentiyInsert(bool enable, string tableName, SqlConnection dbConnection, SqlTransaction dbTransaction)
        {
            string boolString = enable ? "ON" : "OFF";
            return ExecuteSql(string.Format("SET IDENTITY_INSERT {0} {1}", tableName, boolString), dbConnection, dbTransaction, null);
        }

        internal static bool TableExists(string tableName, SqlConnection dbConnection, SqlTransaction dbTransaction)
        {
            return Convert.ToBoolean(ExecuteScalar(string.Format("SELECT CASE WHEN OBJECT_ID(N'{0}', N'U') IS NOT NULL THEN 1 ELSE 0 END", tableName), 
                dbConnection, dbTransaction, null));
        }
    }
}