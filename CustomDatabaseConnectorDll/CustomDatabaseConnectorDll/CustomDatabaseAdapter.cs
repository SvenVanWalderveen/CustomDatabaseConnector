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
            return false;
        }

        public string GetConnectionString()
        {
            return null;
        }
        public bool InsertQuery(object obj, out int newRecordId, out string errorMessage)
        {
            newRecordId = 0;
            errorMessage = null;
            return false;
        }
        public bool InsertQuery(object obj, out string errorMessage)
        {
            errorMessage = null;
            return false;
        }
        public bool UpdateQuery(object obj, out string errorMessage)
        {
            errorMessage = null;
            return false;
        }
        public bool DeleteQuery(object obj, out string errorMessage)
        {
            errorMessage = null;
            return false;
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
