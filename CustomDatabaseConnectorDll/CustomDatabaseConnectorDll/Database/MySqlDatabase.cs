using CustomDatabaseConnectorDll.CustomDatabase;
using CustomDatabaseConnectorDll.Interface;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Database
{
    class MySqlDatabase : IDatabase
    {
        #region adapter methods
        public bool CreateObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            string sqlQuery = BuildCreateObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public bool CreateObject(object obj, out int newRecordId, out CustomDatabaseErrorMessage errorMessage)
        {
            string sqlQuery = BuildCreateObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                newRecordId = -1;
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out newRecordId, out errorMessage);
        }
        public string CreateObjectScript(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            return BuildCreateObject(obj, out errorMessage);
        }
        public bool CreateTable(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            string sqlQuery = BuildCreateStatement(objectType, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public string CreateTableScript(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            return BuildCreateStatement(objectType, out errorMessage);
        }
        public bool DeleteObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            string sqlQuery = BuildDeleteObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public bool DropTable(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            string sqlQuery = BuildDropStatement(objectType, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public string DropTableScript(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            return BuildDropStatement(objectType, out errorMessage);
        }
        public bool UpdateObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            string sqlQuery = BuildUpdateObject(obj, out errorMessage);
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return false;
            }
            return ExecuteSqlStatement(sqlQuery, out errorMessage);
        }
        public DataTable QueryData(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.NO_OBJECTTYPE_PASSED);
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
        public DataTable QueryData(Type objectType, List<CustomDatabaseWhereParameter> whereParameters, out CustomDatabaseErrorMessage errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.NO_OBJECTTYPE_PASSED);
                return null;
            }
            string tableName = CustomDatabaseAnnotationsHelper.GetTableName(objectType, out errorMessage);
            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }
            string result = null;
            string format = "SELECT * FROM {0}";
            result = string.Format(format, tableName);
            if (whereParameters != null && whereParameters.Count > 0)
            {
                format = "{0} WHERE {1}";
                List<string> whereColumns = new List<string>();
                bool loopCompleted = false;
                for (int i = 0; i < whereParameters.Count; i++)
                {
                    CustomDatabaseWhereParameter parameter = whereParameters[i];
                    string whereClause = ConvertWhereParameterToString(parameter, out errorMessage);

                    if (!string.IsNullOrEmpty(whereClause))
                    {
                        whereColumns.Add(whereClause);
                    }
                    if (i == (whereParameters.Count - 1))
                    {
                        loopCompleted = true;
                    }
                }
                if (loopCompleted)
                {
                    result = string.Format(format, result, string.Join(" AND ", whereColumns));
                }
            }
            if (result == null)
            {
                return null;
            }
            return ExecuteSqlStatementToDataTable(result, out errorMessage);
        }
        #endregion

        #region generating SQL query
        public string BuildCreateObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            if (obj == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(obj.GetType(), ErrorMessages.NO_OBJECT_PASSED);
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
                        errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_NO_COLUMNNAME_INSERTED);
                        break;
                    }
                    columnNames.Add(columnName);
                    string dbValue = ConvertNetValueToSqlValue(pi, obj, false, out errorMessage);
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
                errorMessage = new CustomDatabaseErrorMessage(obj.GetType(),ErrorMessages.MISMATCH);
                return null;
            }
            return null;
        }
        public string BuildCreateStatement(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECTTYPE_PASSED);
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
            if (columns == null || columns.Count == 0)
            {
                return null;
            }
            List<string> columnDefinitions = GenerateColumnDefinitions(columns, out errorMessage);
            if (columnDefinitions == null)
            {
                return null;
            }
            result = string.Format(format, result, string.Join(",", columnDefinitions));
            format = "{0},{1}";
            List<PropertyInfo> primaryKeyColumns = CustomDatabaseAnnotationsHelper.GetPrimaryKeyColumns(objectType, out errorMessage);
            if(primaryKeyColumns == null)
            {
                return null;
            }
            string primaryKey = GeneratePrimaryKeyDefinition(primaryKeyColumns, tableName, out errorMessage);
            if (primaryKey == null)
            {
                return null;
            }
            result = string.Format(format, result, primaryKey);
            List<string> uniqueKeys = GenerateUniqueKeyDefinitions(columns, tableName, out errorMessage);
            if(errorMessage != null)
            {
                return null;
            }
            if(uniqueKeys != null && uniqueKeys.Count > 0)
            {
                format = "{0},{1}";
                result = string.Format(format, result, string.Join(",", uniqueKeys));
            }
            List<string> foreignKeys = GenerateForeignKeyDefinitions(columns, tableName, out errorMessage);
            if(errorMessage != null)
            {
                return null;
            }
            if (foreignKeys != null && foreignKeys.Count > 0)
            {
                format = "{0},{1}";
                result = string.Format(format, result, string.Join(",", foreignKeys));
            }
            format = "{0})";
            result = string.Format(format, result);
                return result;
        }

        

        public string BuildDeleteObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            if (obj == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
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
                    errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_NO_COLUMNNAME_INSERTED);
                    break;
                }
                string sqlValue = ConvertNetValueToSqlValue(pi, obj, false, out errorMessage);
                whereClauses.Add(string.Format(format, columnName, sqlValue));
            }
            string queryFormat = "DELETE FROM {0} WHERE {1}";
            errorMessage = null;
            return string.Format(queryFormat, tableName, string.Join(" AND ", whereClauses));
        }
        public string BuildDropStatement(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
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
        public string BuildUpdateObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            if (obj == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
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
                    errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_COLUMN_NO_ANNOTATION);
                    break;
                }
                string sqlValue = ConvertNetValueToSqlValue(pi, obj, false, out errorMessage);
                whereClauses.Add(string.Format(format, columnName, sqlValue));
            }
            if (whereClauses == null || whereClauses.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.SQL_NO_WHERE_CLAUSE);
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
                        errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.ANN_COLUMN_NO_ANNOTATION);
                        break;
                    }
                    string dbValue = ConvertNetValueToSqlValue(pi, obj, false, out errorMessage);
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
        public List<string> GenerateColumnDefinitions(List<PropertyInfo> columns, out CustomDatabaseErrorMessage errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_DOTNET_PROPERTY_PASSED);
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
                    errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_NO_COLUMNNAME_INSERTED);
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
        public string GeneratePrimaryKeyDefinition(List<PropertyInfo> columns, string tableName, out CustomDatabaseErrorMessage errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_DOTNET_PROPERTY_PASSED);
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
                        errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_NO_COLUMNNAME_INSERTED);
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
                    errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.ANN_NO_PRIMARY_KEY);
                    return null;
                }
                return string.Format(primaryKeyFormat, tableName, string.Join(" , ", primaryKeyColumns));
            }
            return null;
        }
        public List<string> GenerateForeignKeyDefinitions(List<PropertyInfo> columns, string tableName, out CustomDatabaseErrorMessage errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_DOTNET_PROPERTY_PASSED);
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
                        errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_NO_COLUMNNAME_INSERTED);
                        break;
                    }
                    List<PropertyInfo> foreignTablePrimaryKey = CustomDatabaseAnnotationsHelper.GetPrimaryKeyColumns(annotation.ForeignKeyTable, out errorMessage);
                    if (foreignTablePrimaryKey == null || foreignTablePrimaryKey.Count == 0)
                    {
                        errorMessage = new CustomDatabaseErrorMessage(pi, string.Format(ErrorMessages.SQL_NO_FKEY_FOUND, annotation.ForeignKeyTable.Name));
                        break;
                    }
                    if (foreignTablePrimaryKey.Count > 1)
                    {
                        errorMessage = new CustomDatabaseErrorMessage(pi, string.Format(ErrorMessages.SQL_FKEY_MULTIPLE_COLUMNS, annotation.ForeignKeyTable.Name));
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
        private List<string> GenerateUniqueKeyDefinitions(List<PropertyInfo> columns, string tableName, out CustomDatabaseErrorMessage errorMessage)
        {
            if (columns == null || columns.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_DOTNET_PROPERTY_PASSED);
                return null;
            }
            errorMessage = null;
            Dictionary<int, List<CustomDatabaseColumnAnnotation>> foundUniqueColumns = new Dictionary<int, List<CustomDatabaseColumnAnnotation>>();
            foreach(PropertyInfo pi in columns)
            {
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if(annotation.IsUnique)
                {
                    int constraintNumber = annotation.UniqueConstraintNumber;
                    if(!foundUniqueColumns.ContainsKey(constraintNumber))
                    {
                        foundUniqueColumns.Add(constraintNumber, new List<CustomDatabaseColumnAnnotation>());
                    }
                    foundUniqueColumns[constraintNumber].Add(annotation);
                }
            }
            
            string uniqueKeyFormat = "CONSTRAINT {0}_{1}_UC UNIQUE ({2})";
            List<string> result = new List<string>();
            bool loopCompleted = false;
            for (int i = 0; i < foundUniqueColumns.Count; i++)
            {
                var constraint = foundUniqueColumns.ElementAt(i);
                List<string> constraintColumnNames = constraint.Value.Select(x => x.ColumnName).ToList();
                string s = "";
                result.Add(string.Format(uniqueKeyFormat, tableName, constraint.Key, string.Join(",", constraintColumnNames)));
                if (i == (foundUniqueColumns.Count - 1))
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
        #endregion

        #region Execute SQL
        public bool ExecuteSqlStatement(string sqlQuery, out CustomDatabaseErrorMessage errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(sqlQuery))
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.SQL_EMPTY_STATEMENT);
            }
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
                errorMessage = new CustomDatabaseErrorMessage(ex.Message);
                return false;
            }
        }
        public bool ExecuteSqlStatement(string sqlQuery, out int newRecordId, out CustomDatabaseErrorMessage errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(sqlQuery))
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.SQL_EMPTY_STATEMENT);
            }
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
                errorMessage = new CustomDatabaseErrorMessage(ex.Message);
                return false;
            }
        }
        public DataTable ExecuteSqlStatementToDataTable(string sqlQuery, out CustomDatabaseErrorMessage errorMessage)
        {
            if (string.IsNullOrEmpty(sqlQuery))
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.SQL_EMPTY_STATEMENT);
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
                errorMessage = new CustomDatabaseErrorMessage(ex.Message);
                return null;
            }
        }
        #endregion
        public string GetConnectionString()
        {
            return CacheClass.ConnectionString;
        }
        public string ConvertNetValueToSqlValue(PropertyInfo pi, object obj, bool isProperty, out CustomDatabaseErrorMessage errorMessage)
        {
            if (pi == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_DOTNET_PROPERTY_PASSED);
                return null;
            }
            if (obj == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
                return null;
            }
            object propertyValue = obj;
            if (!isProperty)
            {
                propertyValue = obj.GetType().GetProperty(pi.Name).GetValue(obj, null);
            }
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
        public string ConvertWhereParameterToString(CustomDatabaseWhereParameter parameter, out CustomDatabaseErrorMessage errorMessage)
        {
            if (parameter == null)
            {
                errorMessage = new CustomDatabaseErrorMessage("No where-parameter(s) passed to method");
                return null;
            }
            PropertyInfo pi = parameter.Property;
            CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
            if (annotation != null)
            {
                //Format: Columnname , operator, value
                string columnName = annotation.ColumnName;
                if (string.IsNullOrEmpty(columnName))
                {
                    errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_NO_COLUMNNAME_INSERTED);
                    return null;
                }
                string format = "{0} {1} {2}";
                string operatorString = CustomDatabaseWhereParameterHelper.OperatorToString(parameter.Operator, out errorMessage);
                if (string.IsNullOrEmpty(operatorString))
                {
                    return null;
                }
                string sqlValue = ConvertNetValueToSqlValue(pi, parameter.Value, true, out errorMessage);
                if (string.IsNullOrEmpty(operatorString))
                {
                    return null;
                }
                return string.Format(format, columnName, operatorString, sqlValue);
            }
            errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.ANN_COLUMN_NO_ANNOTATION);
            return null;
        }
        public string GetSqlDataTypeByNetType(PropertyInfo pi, CustomDatabaseColumnAnnotation annotation, out CustomDatabaseErrorMessage errorMessage)
        {
            if (pi == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_DOTNET_PROPERTY_PASSED);
                return null;
            }
            if (annotation == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
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
            else if (propertyType == typeof(double))
            {
                return "FLOAT";
            }
            errorMessage = new CustomDatabaseErrorMessage(pi, ErrorMessages.DATATYPE_UNKNOWN + " (" + propertyType.ToString() + ")");
            return null;
        }

       
    }
}
