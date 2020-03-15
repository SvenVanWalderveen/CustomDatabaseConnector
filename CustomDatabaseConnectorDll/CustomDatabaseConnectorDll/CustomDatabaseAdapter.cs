using CustomDatabaseConnectorDll.Controller;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll
{
    public class CustomDatabaseAdapter
    {
        public bool Init(string connectionString)
        {
            if(string.IsNullOrEmpty(connectionString))
            {
                return false;
            }
            CacheClass.ConnectionString = connectionString;
            CacheClass.Environment = 1;
            return true;
        }

        public bool CreateTable(Type objectType, out string errorMessage)
        {
            return DatabaseController.Instance.CreateTable(objectType, out errorMessage);
        }

        public bool InsertQuery(object obj, out int newRecordId, out string errorMessage)
        {
            return DatabaseController.Instance.InsertQuery(obj, out newRecordId, out errorMessage);
        }
        public bool InsertQuery(object obj, out string errorMessage)
        {
            return DatabaseController.Instance.InsertQuery(obj, out errorMessage);
        }
        public bool UpdateQuery(object obj, out string errorMessage)
        {
            return DatabaseController.Instance.UpdateQuery(obj, out errorMessage);
        }
        public bool DeleteQuery(object obj, out string errorMessage)
        {
            return DatabaseController.Instance.DeleteQuery(obj, out errorMessage);
        }
        public DataTable SelectQuery(Type classType)
        {
            return null;
        }
        public DataTable SelectQuery(Type classType, string whereFilter)
        {
            return null;
        }
        public bool ExecuteSql(string sqlStatement)
        {
            return false;
        }
    }
}
