using CustomDatabaseConnectorDll.Database;
using CustomDatabaseConnectorDll.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Controller
{
    internal class DatabaseController
    {
        internal static IDatabase Instance
        {
            get
            {
                if (CacheClass.Environment == 1)
                {
                    return new MySqlDatabase();
                }
                return null;
            }
        }
    }
}
