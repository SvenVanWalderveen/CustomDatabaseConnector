using CustomDatabaseConnectorDll.Annotations;
using CustomDatabaseConnectorDll.Interface;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace CustomDatabaseConnectorDll.Database
{

    internal class MySqlDatabase : IDatabase
    {
        #region Adapter methods 
        public bool CreateObject(object obj, out string errorMessage)
        {
            string sqlQuery = BuildCreateObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public bool CreateObject(object obj, out int newRecordId, out string errorMessage)
        {
            string sqlQuery = BuildCreateObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                newRecordId = -1;
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out newRecordId, out errorMessage);
        }
        public string CreateObjectScript(object obj, out string errorMessage)
        {
            return BuildCreateObject(obj, out errorMessage);
        }
        public bool CreateTable(Type objectType, out string errorMessage)
        {
            string sqlQuery = BuildCreateStatement(objectType, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public string CreateTableScript(Type objectType, out string errorMessage)
        {
            return BuildCreateStatement(objectType, out errorMessage);
        }
        public bool DeleteObject(object obj, out string errorMessage)
        {
            string sqlQuery = BuildDeleteObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public bool DropTable(Type objectType, out string errorMessage)
        {
            string sqlQuery = BuildDropStatement(objectType, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public string DropTableScript(Type objectType, out string errorMessage)
        {
            return BuildDropStatement(objectType, out errorMessage);
        }
        public bool UpdateObject(object obj, out string errorMessage)
        {
            string sqlQuery = BuildUpdateObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public DataTable QueryData(Type objectType, out string errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen objecttype meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            string queryFormat = "SELECT * FROM {0}";
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(objectType, out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            string query = string.Format(queryFormat, tableName);
            return ExecuteSqlStatementToDataTable(query, out errorMessage);
        }

        #endregion
        public string BuildUpdateObject(object obj, out string errorMessage)
        {
            if (obj == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen object meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            //Get tablename
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(obj.GetType(), out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            //Build where clause
            List<PropertyInfo> primaryKeyColumns = CustomDatabaseAnnotationsHelper.GetPrimaryKeyColumns(obj.GetType(), out errorMessage);
            if (primaryKeyColumns == null || primaryKeyColumns.Count == 0)
            {
                errorMessage = string.Format("{0} ({1})", "Geen primary keys gevonden", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            List<string> whereClauses = new List<string>();
            foreach (PropertyInfo pi in primaryKeyColumns)
            {
                string format = "{0} = {1}";
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                string columnName = annotation.ColumnName;
                if (string.IsNullOrEmpty(columnName))
                {
                    errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                    break;
                }
                string sqlValue = ConvertNetValueToSqlValue(pi, obj, out errorMessage);
                whereClauses.Add(string.Format(format, columnName, sqlValue));
            }
            if (whereClauses == null || whereClauses.Count == 0)
            {
                errorMessage = string.Format("{0} ({1})", "Geen where clause", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            List<PropertyInfo> columns = CustomDatabaseAnnotationsHelper.GetColumns(obj.GetType(), out errorMessage);
            if (columns == null || columns.Count == 0)
            {
                return null;
            }
            List<string> setColumns = new List<string>();
            bool loopCompleted = false;
            for (int i = 0; i < columns.Count; i++)
            {
                string format = "{0} = {1}";
                PropertyInfo pi = columns[i];
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (!annotation.IsPrimaryKey)
                {
                    //Auto-increment values can be skipped.
                    string columnName = annotation.ColumnName;
                    if (string.IsNullOrEmpty(columnName))
                    {
                        errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                        break;
                    }
                    string dbValue = ConvertNetValueToSqlValue(pi, obj, out errorMessage);
                    setColumns.Add(string.Format(format, columnName, dbValue));
                }
                if (i == (columns.Count - 1))
                {
                    loopCompleted = true;
                }
            }
            if (loopCompleted)
            {
                string queryFormat = "UPDATE {0} SET {1} WHERE {2}";
                return string.Format(queryFormat, tableName, string.Join(",", setColumns), string.Join(" AND ", whereClauses));
            }
            return null;
        }
        public string BuildCreateObject(object obj, out string errorMessage)
        {
            if (obj == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen object meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(obj.GetType(), out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            string queryFormat = "INSERT INTO {0}({1}) VALUES ({2})";
            List<PropertyInfo> columns = CustomDatabaseAnnotationsHelper.GetColumns(obj.GetType(), out errorMessage);
            if (columns == null || columns.Count == 0)
            {
                return null;
            }
            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            bool loopCompleted = false;
            for (int i = 0; i < columns.Count; i++)
            {
                PropertyInfo pi = columns[i];
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (!annotation.IsAutoIncrement)
                {
                    //Auto-increment values can be skipped.
                    string columnName = annotation.ColumnName;
                    if (string.IsNullOrEmpty(columnName))
                    {
                        errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                        break;
                    }
                    columnNames.Add(columnName);
                    string dbValue = ConvertNetValueToSqlValue(pi, obj, out errorMessage);
                    columnValues.Add(dbValue);
                }
                if (i == (columns.Count - 1))
                {
                    loopCompleted = true;
                }
            }
            if (loopCompleted)
            {
                if (columnValues.Count == columnNames.Count)
                {
                    return string.Format(queryFormat, tableName, string.Join(",", columnNames), string.Join(",", columnValues));
                }
                errorMessage = string.Format("{0} ({1})", "Aantal kolomnamen en waardes matcht niet", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            return null;
        }
        public string BuildDropStatement(Type objectType, out string errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen objecttype meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            string format = "DROP TABLE IF EXISTS {0}";
            //Get tablename
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(objectType, out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            return string.Format(format, tableName);
        }
        public DataTable ExecuteSqlStatementToDataTable(string sqlQuery, out string errorMessage)
        {

            if (string.IsNullOrEmpty(sqlQuery))
            {
                errorMessage = string.Format("{0} ({1})", "Geen sql statement meegegeven", MethodBase.GetCurrentMethod().Name);
            }
            errorMessage = null;
            try
            {
                DataTable table = new DataTable();
                MySqlConnection con = new MySqlConnection(GetConnectionString());
                con.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
                cmd.ExecuteNonQuery();
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(table);
                con.Close();
                return table;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return null;
            }
        }
        public bool ExecuteSqlStatement(string sqlQuery, out string errorMessage)
        {
            if (string.IsNullOrEmpty(sqlQuery))
            {
                errorMessage = string.Format("{0} ({1})", "Geen sql statement meegegeven", MethodBase.GetCurrentMethod().Name);
            }
            errorMessage = null;
            try
            {
                MySqlConnection con = new MySqlConnection(GetConnectionString());
                con.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
        public bool ExecuteSqlStatement(string sqlQuery, out int newRecordId, out string errorMessage)
        {
            if (string.IsNullOrEmpty(sqlQuery))
            {
                errorMessage = string.Format("{0} ({1})", "Geen sql statement meegegeven", MethodBase.GetCurrentMethod().Name);
            }
            errorMessage = null;
            try
            {
                MySqlConnection con = new MySqlConnection(GetConnectionString());
                con.Open();
                MySqlCommand cmd = new MySqlCommand(sqlQuery, con);
                cmd.ExecuteNonQuery();
                newRecordId = Convert.ToInt32(cmd.LastInsertedId);
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                newRecordId = -1;
                errorMessage = ex.Message;
                return false;
            }
        }
        public string GetConnectionString()
        {
            return CacheClass.ConnectionString;
        }
        public string BuildCreateStatement(Type objectType, out string errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen objecttype meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            string result = null;
            string format = "CREATE TABLE IF NOT EXISTS {0}";
            //Get tablename
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(objectType, out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            result = string.Format(format, tableName);
            format = "{0} ({1}";
            //Get columns
            List<PropertyInfo> columns = CustomDatabaseAnnotationsHelper.GetColumns(objectType, out errorMessage);
            List<string> columnDefinitions = GenerateColumnDefinitions(columns, out errorMessage);
            if (columnDefinitions == null)
            {
                return null;
            }
            result = string.Format(format, result, string.Join(",", columnDefinitions));
            format = "{0},{1}";
            List<PropertyInfo> primaryKeyColumns = CustomDatabaseAnnotationsHelper.GetPrimaryKeyColumns(objectType, out errorMessage);
            string primaryKey = GeneratePrimaryKeyDefinition(primaryKeyColumns, tableName, out errorMessage);
            if (primaryKey == null)
            {
                return null;
            }
            result = string.Format(format, result, primaryKey);
            List<string> foreignKeys = GenerateForeignKeyDefinitions(columns, tableName, out errorMessage);
            if (foreignKeys != null && foreignKeys.Count > 0)
            {
                format = "{0},{1}";
                result = string.Format(format, result, string.Join(",", foreignKeys));
                string s = "";
            }
            format = "{0})";
            result = string.Format(format, result);
            return result;
        }
        public List<string> GenerateForeignKeyDefinitions(List<PropertyInfo> columns, string tableName, out string errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = string.Format("{0} ({1})", "Geen kolommen meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            errorMessage = null;
            string foreignKeyFormat = "CONSTRAINT {0}_{1}_FK FOREIGN KEY ({2}) REFERENCES {3}({4}) ON UPDATE RESTRICT ON DELETE CASCADE";
            List<string> result = new List<string>();
            bool loopCompleted = false;
            for (int i = 0; i < columns.Count; i++)
            {
                PropertyInfo pi = columns[i];
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (annotation.ForeignKeyTable != null)
                {
                    string columnName = annotation.ColumnName;
                    if (string.IsNullOrEmpty(columnName))
                    {
                        errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                        break;
                    }
                    List<PropertyInfo> foreignTablePrimaryKey = CustomDatabaseAnnotationsHelper.GetPrimaryKeyColumns(annotation.ForeignKeyTable, out errorMessage);
                    if (foreignTablePrimaryKey == null || foreignTablePrimaryKey.Count == 0)
                    {
                        errorMessage = string.Format("Geen primary key gevonden voor foreign key {0} ({1})", annotation.ForeignKeyTable.Name, MethodBase.GetCurrentMethod().Name);
                        break;
                    }
                    if (foreignTablePrimaryKey.Count > 1)
                    {
                        errorMessage = string.Format("Meerdere primary keys gevonden voor foreign key {0} ({1})", annotation.ForeignKeyTable.Name, MethodBase.GetCurrentMethod().Name);
                        break;
                    }
                    string fkTable = CustomDatabaseAnnotationsHelper.GetTableName(annotation.ForeignKeyTable, out errorMessage);
                    if (string.IsNullOrEmpty(fkTable))
                    {
                        break;
                    }
                    CustomDatabaseColumnAnnotation foreignKeyAnnotation = (CustomDatabaseColumnAnnotation)foreignTablePrimaryKey[0].GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                    result.Add(string.Format(foreignKeyFormat, tableName, fkTable, columnName, fkTable, foreignKeyAnnotation.ColumnName));
                }
                if (i == (columns.Count - 1))
                {
                    loopCompleted = true;
                }
            }
            if (loopCompleted)
            {
                return result;
            }
            return null;
        }
        public string GeneratePrimaryKeyDefinition(List<PropertyInfo> columns, string tableName, out string errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = string.Format("{0} ({1})", "Geen primary key-kolommen meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            errorMessage = null;
            string primaryKeyFormat = "CONSTRAINT {0}_PK PRIMARY KEY ({1})";
            List<string> primaryKeyColumns = new List<string>();
            bool loopCompleted = false;
            for (int i = 0; i < columns.Count; i++)
            {
                PropertyInfo pi = columns[i];
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (annotation.IsPrimaryKey)
                {
                    string columnName = annotation.ColumnName;
                    if (string.IsNullOrEmpty(columnName))
                    {
                        errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                        break;
                    }
                    primaryKeyColumns.Add(annotation.ColumnName);
                }
                if (i == (columns.Count - 1))
                {
                    loopCompleted = true;
                }
            }
            if (loopCompleted)
            {
                if (primaryKeyColumns.Count == 0)
                {
                    errorMessage = string.Format("{0} ({1})", "Geen primary key kolommen gevonden", MethodBase.GetCurrentMethod().Name);
                    return null;
                }
                return string.Format(primaryKeyFormat, tableName, string.Join(" , ", primaryKeyColumns));
            }
            return null;
        }
        public List<string> GenerateColumnDefinitions(List<PropertyInfo> columns, out string errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = string.Format("{0} ({1})", "Geen kolommen meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            errorMessage = null;
            List<string> result = new List<string>();
            //Format: ColumnName DataType Nullable AutoIncrement
            string columnFormat = "{0} {1} {2}{3}";
            bool loopCompleted = false;
            for (int i = 0; i < columns.Count; i++)
            {
                PropertyInfo pi = columns[i];
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                string columnName = annotation.ColumnName;
                string dataType = GetSqlDataTypeByNetType(pi, annotation, out errorMessage);
                string nullable = annotation.IsNullable ? "NULL" : "NOT NULL";
                string autoIncrement = annotation.IsAutoIncrement ? " AUTO_INCREMENT" : "";
                if (string.IsNullOrEmpty(columnName))
                {
                    errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                    break;
                }
                if (string.IsNullOrEmpty(dataType))
                {
                    break;
                }
                string output = string.Format(columnFormat, columnName, dataType, nullable, autoIncrement);
                result.Add(output);
                if (i == (columns.Count - 1))
                {
                    loopCompleted = true;
                }
            }
            if (loopCompleted)
            {
                return result;
            }
            return null;
        }
        public string GetSqlDataTypeByNetType(PropertyInfo pi, CustomDatabaseColumnAnnotation annotation, out string errorMessage)
        {
            if (pi == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen .NET-attribuut meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            if (annotation == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen attribuut-annotatie meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            errorMessage = null;
            Type propertyType = pi.PropertyType;
            if (propertyType == typeof(string))
            {
                int maxLength = annotation.MaxLength;
                if (maxLength <= 0)
                {
                    return "VARCHAR(255)";
                }
                else if (maxLength < 256)
                {
                    return string.Format("VARCHAR({0})", maxLength);
                }
                else
                {
                    return "TEXT";
                }
            }
            else if (propertyType == typeof(DateTime))
            {
                return "DATETIME";
            }
            else if (propertyType == typeof(int))
            {
                return "INTEGER";
            }
            else if (propertyType == typeof(int?))
            {
                return "INTEGER";
            }
            errorMessage = string.Format("Datatype {0} is niet gedefinieerd ({1})", propertyType.ToString(), MethodBase.GetCurrentMethod().Name);
            return null;
        }
        public string BuildDeleteObject(object obj, out string errorMessage)
        {
            if (obj == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen object meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(obj.GetType(), out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            List<PropertyInfo> primaryKeyColumns = CustomDatabaseAnnotationsHelper.GetPrimaryKeyColumns(obj.GetType(), out errorMessage);
            if (primaryKeyColumns == null || primaryKeyColumns.Count == 0)
            {
                errorMessage = string.Format("{0} ({1})", "Geen primary keys gevonden", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            List<string> whereClauses = new List<string>();
            foreach (PropertyInfo pi in primaryKeyColumns)
            {
                string format = "{0} = {1}";
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                string columnName = annotation.ColumnName;
                if (string.IsNullOrEmpty(columnName))
                {
                    errorMessage = string.Format("Attribuut {0} bevat geen kolomnaam ({1})", pi.Name, MethodBase.GetCurrentMethod().Name);
                    break;
                }
                string sqlValue = ConvertNetValueToSqlValue(pi, obj, out errorMessage);
                whereClauses.Add(string.Format(format, columnName, sqlValue));
            }
            string queryFormat = "DELETE FROM {0} WHERE {1}";
            errorMessage = null;
            return string.Format(queryFormat, tableName, string.Join(" AND ", whereClauses));
        }
        public string ConvertNetValueToSqlValue(PropertyInfo pi, object obj, out string errorMessage)
        {
            if (pi == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen .NET-attribuut meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            if (obj == null)
            {
                errorMessage = string.Format("{0} ({1})", "Geen object meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            object propertyValue = obj.GetType().GetProperty(pi.Name).GetValue(obj, null);
            errorMessage = null;
            if (pi.PropertyType == typeof(string))
            {
                if (propertyValue != null)
                {
                    return string.Format("'{0}'", propertyValue.ToString());
                }
                else
                {
                    return "null";
                }
            }
            else if (pi.PropertyType == typeof(DateTime))
            {

                return string.Format("'{0}'", DateTime.Parse(propertyValue.ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                return propertyValue.ToString();
            }
        }
    }
}
