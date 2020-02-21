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
    internal class MySqlQueryBuilder : IQueryBuilder
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
                columns.Add(ConvertClassAttributeToSqlCreate(dbProperty, attr, out errorMessage));
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

        public string ConvertClassAttributeToSqlCreate(PropertyInfo propertyInfo, CustomDatabaseColumnAnnotation annotation, out string errorMessage)
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
                    return string.Format("{0} INT NOT NULL AUTO_INCREMENT PRIMARY KEY", annotation.ColumnName);
                }
                else
                {
                    return string.Format("{0} INT NOT NULL AUTO_INCREMENT", annotation.ColumnName);
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
                        string fkFormat = "REFERENCES {0}({1})";
                        return string.Format(format, annotation.ColumnName, dataType, string.Format(fkFormat, tableName, columName));
                    }
                    else
                    {
                        errorMessage = string.Format("Foreign key reference not defined for column {0}", propertyInfo.Name);
                    }

                }
                else
                {
                    string nullableType = annotation.IsNullable ? "NULL" : "NOT NULL";
                    return string.Format(format, annotation.ColumnName, dataType, nullableType);
                }
            }
            return null;
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
