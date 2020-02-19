using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Interface
{
    internal interface IQueryBuilder
    {
        string BuildCreateTable(Type objectType);
        string GetTableName(object obj);
        string GetTableName(Type typeOfObject);
    }
}
