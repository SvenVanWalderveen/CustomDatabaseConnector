using CustomDatabaseConnectorDll.Annotations;
using CustomDatabaseConnectorDll.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Database
{
    internal class MySqlQueryBuilder
    {
        public static List<string> errorMessages = new List<string>();
        public string BuildCreateTable(Type objectType)
        {
            List<string> columns = new List<string>();
            string queryFormat = "CREATE TABLE IF NOT EXISTS {0} ({1})";
            string tableName = GetTableName(objectType);
            var dbColumns = GetDbProperties(objectType);
            foreach(var dbProperty in dbColumns)
            {
                string errorMessage = null;
                CustomDatabaseColumnAnnotation attr = (CustomDatabaseColumnAnnotation)dbProperty.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                List<string> columns1 = ConvertClassAttributeToSqlCreate(dbProperty, attr, out errorMessage);
                if(columns1 != null)
                {
                    columns.AddRange(columns1);
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    errorMessages.Add(errorMessage);
                }
            }
            if (errorMessages.Count == 0 && !string.IsNullOrEmpty(tableName))
            {
                return string.Format(queryFormat, tableName, string.Join(",", columns));
            }
            else
            {
                return null;
            }
        }

        public string BuildInsertRow(object obj, out string errorMessage)
        {
            errorMessage = null;
            List<string> columns = new List<string>();
            string queryFormat = "INSERT INTO {0} ({1}) VALUES ({2})";
            string tableName = GetTableName(obj.GetType());
            var dbColumns = GetDbProperties(obj.GetType());
            
            List<string> columnValues = new List<string>();

            foreach (var dbProperty in dbColumns)
            {
                string propName = dbProperty.Name;
                CustomDatabaseColumnAnnotation attr = (CustomDatabaseColumnAnnotation)dbProperty.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (attr == null)
                {
                    continue;
                }
                if (!(attr.IsPrimaryKey && attr.IsAutoIncrement))
                {
                    //Add column to list
                    columns.Add(attr.ColumnName);
                    //Add value to list
                    object propValue = obj.GetType().GetProperty(propName).GetValue(obj, null);
                    string dbValue = ConvertValueToDbValue(dbProperty, propValue);
                    if(CheckColumnDbValue(dbProperty, attr, dbValue))
                    {
                        columnValues.Add(dbValue);
                    }
                    else
                    {
                        errorMessage = "Veld " + propName + " niet gevuld";
                        return null;
                    }
                }
            }
            string query = string.Format(queryFormat, tableName, string.Join(",", columns), string.Join(",", columnValues));
            return query;
        }


        public bool CheckColumnDbValue(PropertyInfo pi, CustomDatabaseColumnAnnotation annotation, string dbValue)
        {
            if(annotation.IsUpdatable && !annotation.IsNullable)
            {
                if(dbValue.Equals("null"))
                {
                    return false;
                }
            }
            return true;
        }

        public List<string> ConvertClassAttributeToSqlCreate(PropertyInfo propertyInfo, CustomDatabaseColumnAnnotation annotation, out string errorMessage)
        {
            errorMessage = null;
            if(string.IsNullOrEmpty(annotation.ColumnName))
            {
                errorMessage = "Eén of meer velden bevat geen kolomnaam";
                return null;
            }


            if (annotation.IsAutoIncrement)
            {
                //Autoincrement field
                if (annotation.IsPrimaryKey)
                {
                    return new List<string> { string.Format("{0} INT NOT NULL AUTO_INCREMENT PRIMARY KEY", annotation.ColumnName) };
                }
                else
                {
                    return new List<string> { string.Format("{0} INT NOT NULL AUTO_INCREMENT", annotation.ColumnName) };
                }
            }
            else
            {
                //Normal field
                string format = "{0} {1} {2}";
                string dataType = GetSqlDataTypeByNetField(propertyInfo, annotation.MaxLength, out errorMessage);
                if (string.IsNullOrEmpty(dataType))
                {
                    return null;
                }
                if (annotation.ForeignKeyTable != null)
                {
                    //Find table name and column of FK
                    string tableName = GetTableName(annotation.ForeignKeyTable);
                    string columName = GetPrimaryKey(annotation.ForeignKeyTable);
                    if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(columName))
                    {
                        List<string> array = new List<string>();
                        string nullableType = annotation.IsNullable ? "NULL" : "NOT NULL";
                        array.Add(string.Format(format, annotation.ColumnName, dataType, nullableType));
                        string fkFormat = "FOREIGN KEY ({0})REFERENCES {1}({2}) ON UPDATE RESTRICT ON DELETE CASCADE";
                        array.Add(string.Format(fkFormat, annotation.ColumnName, tableName, columName));
                        return array;
                    }
                    else
                    {
                        errorMessage = string.Format("Foreign key reference not defined for column {0}", propertyInfo.Name);
                    }

                }
                else
                {
                    string nullableType = annotation.IsNullable ? "NULL" : "NOT NULL";
                    return new List<string> { string.Format(format, annotation.ColumnName, dataType, nullableType) };
                }
            }
            return null;
        }

        public string BuildUpdateRow(object obj, out string errorMessage)
        {
            errorMessage = null;
            string queryFormat = "UPDATE {0} SET {1} WHERE {2}";
            string tableName = GetTableName(obj);
            var dbColumns = GetDbProperties(obj.GetType());
            string whereClause = "";
            string setClause = "";

            foreach(var dbProperty in dbColumns)
            {
                string propName = dbProperty.Name;
                CustomDatabaseColumnAnnotation attr = (CustomDatabaseColumnAnnotation)dbProperty.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (attr.IsPrimaryKey)
                {
                    string dbValue = ConvertValueToDbValue(dbProperty, obj.GetType().GetProperty(propName).GetValue(obj, null));
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        whereClause = string.Format("{0} = {1}", attr.ColumnName, dbValue);
                    }
                    else
                    {
                        whereClause += string.Format("AND {0} = {1}", attr.ColumnName, dbValue);
                    }
                }
                if (attr.IsUpdatable)
                {
                    string dbValue = ConvertValueToDbValue(dbProperty, obj.GetType().GetProperty(propName).GetValue(obj, null));
                    if (string.IsNullOrEmpty(setClause))
                    {
                        setClause = string.Format("{0} = {1}", attr.ColumnName, dbValue);
                    }
                    else
                    {
                        setClause += string.Format(",{0} = {1}", attr.ColumnName, dbValue);
                    }
                }
            }

            if(string.IsNullOrEmpty(tableName))
            {
                errorMessage = "Tabelnaam is leeg";
                return null;
            }
            if(string.IsNullOrEmpty(setClause))
            {
                errorMessage = "Set-clause is leeg";
                return null;
            }
            if(string.IsNullOrEmpty(whereClause))
            {
                errorMessage = "Where-clause is leeg";
                return null;
            }
            string query = string.Format(queryFormat, tableName, setClause, whereClause);
            return query;
        }

        public string ConvertValueToDbValue(PropertyInfo info, object value)
        {
            if (info.PropertyType == typeof(string))
            {
                if (value != null)
                {
                    return string.Format("'{0}'", value.ToString());
                }
                else
                {
                    return "null";
                }
            }
            else if (info.PropertyType == typeof(DateTime))
            {

                return string.Format("'{0}'", DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                return value.ToString();
            }
        }


        public List<PropertyInfo> GetDbProperties(Type objectType)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            PropertyInfo[] properties = objectType.GetProperties();
            foreach(PropertyInfo pi in properties)
            {
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if(annotation != null)
                {
                    result.Add(pi);
                }
            }
            return result;
        }

        public string GetPrimaryKey(Type type)
        {
            if (type == null)
            {
                return null;
            }
            foreach (PropertyInfo pi in type.GetProperties())
            {
                string propName = pi.Name;
                CustomDatabaseColumnAnnotation attr = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (attr == null)
                {
                    continue;
                }
                if (attr.IsPrimaryKey)
                {
                    return attr.ColumnName;
                }
            }
            return null;
        }

        public string GetSqlDataTypeByNetField(PropertyInfo propertyInfo, int maxLength, out string errorMessage)
        {
            Type propertyType = propertyInfo.PropertyType;
            errorMessage = null;
            string dataType = null;
            if (propertyType == typeof(string))
            {
                if (maxLength < 256)
                {
                    dataType = "VARCHAR(255)";
                }
                else
                {
                    dataType = "TEXT";
                }
            }
            else if (propertyType == typeof(DateTime))
            {
                dataType = "DATETIME";
            }
            else if (propertyType == typeof(int))
            {
                dataType = "INTEGER";
            }
            else if (propertyType == typeof(int?))
            {
                dataType = "INTEGER";
            }
            if (dataType == null)
            {
                errorMessage = string.Format("Datatype {0} not defined for Access DB", propertyType.ToString());
                return null;
            }
            else
            {
                return dataType;
            }
        }

        public string GetTableName(object obj)
        {
            CustomDatabaseClassAnnotation classAttr = (CustomDatabaseClassAnnotation)obj.GetType().GetCustomAttribute(typeof(CustomDatabaseClassAnnotation), true);
            if (classAttr == null)
            {
                return null;
            }
            return classAttr.TableName;
        }

        public string GetTableName(Type typeOfObject)
        {
            if (typeOfObject == null)
            {
                return null;
            }
            CustomDatabaseClassAnnotation classAttr = (CustomDatabaseClassAnnotation)typeOfObject.GetCustomAttribute(typeof(CustomDatabaseClassAnnotation), true);
            if (classAttr == null)
            {
                return null;
            }
            return classAttr.TableName;
        }
    }
}
