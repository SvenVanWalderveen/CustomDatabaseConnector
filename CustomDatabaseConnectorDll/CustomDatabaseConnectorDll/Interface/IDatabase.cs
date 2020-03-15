using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Interface
{
    internal interface IDatabase
    {
        string GetConnectionString();
        bool InsertQuery(object obj, out int newRecordId, out string errorMessage);
        bool InsertQuery(object obj, out string errorMessage);
        bool UpdateQuery(object obj, out string errorMessage);
        bool DeleteQuery(object obj, out string errorMessage);
        DataTable SelectQuery(Type classType);
        DataTable SelectQuery(Type classType, string whereFilter);
        bool ExecuteSql(string sqlStatement, out string errorMessage);
        bool CreateTable(Type objectType, out string errorMessage);
        bool DropTable(Type objectType, out string errorMessage);
    }
}
