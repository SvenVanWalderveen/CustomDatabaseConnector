using CustomDatabaseConnectorDll.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Database
{
    internal class MySqlDatabase : IDatabase
    {
        public bool DeleteQuery(object obj, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteSql(string sqlStatement)
        {
            throw new NotImplementedException();
        }

        public string GetConnectionString()
        {
            throw new NotImplementedException();
        }

        public bool InsertQuery(object obj, out int newRecordId, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool InsertQuery(object obj, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectQuery(Type classType)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectQuery(Type classType, string whereFilter)
        {
            throw new NotImplementedException();
        }

        public bool UpdateQuery(object obj, out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
