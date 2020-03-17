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

        public bool CreateObject(object obj, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return false;
            }
            return DatabaseController.Instance.CreateObject(obj, out errorMessage);
        }
        public bool CreateObject(object obj, out int newRecordId, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                newRecordId = -1;
                errorMessage = "Geen database geïnitialiseerd";
                return false;
            }
            return DatabaseController.Instance.CreateObject(obj, out newRecordId, out errorMessage);
        }
        public string CreateObjectScript(object obj, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return null;
            }
            return DatabaseController.Instance.CreateObjectScript(obj, out errorMessage);
        }

        public bool CreateTable(Type objectType, out string errorMessage)
        {
            if(DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return false;
            }
            return DatabaseController.Instance.CreateTable(objectType, out errorMessage);
        }

        public string CreateTableScript(Type objectType, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return null;
            }
            return DatabaseController.Instance.CreateTableScript(objectType, out errorMessage);
        }


        public bool CreateTables(Dictionary<int, Type> objectTypes, out string errorMessage, out int typesCompleted)
        {
            errorMessage = null;
            typesCompleted = 0;
            if (objectTypes == null || objectTypes.Count == 0)
            {
                errorMessage = "Geen objecttypes meegegeven";
                return false;
            }
            foreach (var objectType in objectTypes.OrderBy(x => x.Key))
            {
                bool result = CreateTable(objectType.Value, out errorMessage);
                if (!result)
                {
                    break;
                }
                typesCompleted++;
            }
            return true;
        }

        public bool DeleteObject(object obj, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return false;
            }
            return DatabaseController.Instance.DeleteObject(obj, out errorMessage);
        }

        public bool DropTable(Type objectType, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return false;
            }
            return DatabaseController.Instance.DropTable(objectType, out errorMessage);
        }
        public string DropTableScript(Type objectType, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return null;
            }
            return DatabaseController.Instance.DropTableScript(objectType, out errorMessage);
        }

        public bool DropTables(Dictionary<int, Type> objectTypes, out string errorMessage, out int typesCompleted)
        {
            errorMessage = null;
            typesCompleted = 0;
            if (objectTypes == null || objectTypes.Count == 0)
            {
                errorMessage = "Geen objecttypes meegegeven";
                return false;
            }
            foreach (var objectType in objectTypes.OrderByDescending(x => x.Key))
            {
                bool result = DropTable(objectType.Value, out errorMessage);
                if (!result)
                {
                    break;
                }
                typesCompleted++;
            }
            return true;
        }

        public bool UpdateObject(object obj, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return false;
            }
            return DatabaseController.Instance.UpdateObject(obj, out errorMessage);
        }
        public DataTable QueryData(Type objectType, out string errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = "Geen database geïnitialiseerd";
                return null;
            }
            return DatabaseController.Instance.QueryData(objectType, out errorMessage);
        }


        //public DataTable SelectQuery(Type classType)
        //{
        //    return null;
        //}
        //public DataTable SelectQuery(Type classType, string whereFilter)
        //{
        //    return null;
        //}
        //public bool ExecuteSql(string sqlStatement)
        //{
        //    return false;
        //}
    }
}
