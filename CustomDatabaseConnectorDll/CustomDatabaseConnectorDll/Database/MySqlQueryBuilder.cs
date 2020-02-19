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
        public string BuildCreateTable(Type objectType)
        {
            string queryFormat = "CREATE TABLE IF NOT EXISTS {0} ({1})";
            string tableName = GetTableName(objectType);
            return null;
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
