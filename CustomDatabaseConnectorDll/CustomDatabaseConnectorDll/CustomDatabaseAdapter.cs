using CustomDatabaseConnectorDll.Controller;
using CustomDatabaseConnectorDll.CustomDatabase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll
{
    public class CustomDatabaseAdapter
    {
        public bool Init(string connectionString, out CustomDatabaseErrorMessage errorMessage)
        {
            errorMessage = null;
            if(string.IsNullOrEmpty(connectionString))
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_EMPTY_CONNECTIONSTRING);
                return false;
            }
            CacheClass.ConnectionString = connectionString;
            CacheClass.Environment = 1;
            return true;
        }
        public bool CreateObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            errorMessage = null;
            if(DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(obj.GetType(), ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.CreateObject(obj, out errorMessage);
        }
        public bool CreateObject(object obj, out int newRecordId, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                newRecordId = -1;
                errorMessage = new CustomDatabaseErrorMessage(obj.GetType(), ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.CreateObject(obj, out newRecordId, out errorMessage);
        }
        public string CreateObjectScript(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(obj.GetType(), ErrorMessages.DB_NOT_INITIALIZED);
                return null;
            }
            return DatabaseController.Instance.CreateObjectScript(obj, out errorMessage);
        }
        public bool CreateTable(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.CreateTable(objectType, out errorMessage);
        }
        public string CreateTableScript(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.DB_NOT_INITIALIZED);
                return null;
            }
            return DatabaseController.Instance.CreateTableScript(objectType, out errorMessage);
        }
        public bool CreateTables(Dictionary<int, Type> objectTypes, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            errorMessage = null;
            if (objectTypes == null || objectTypes.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
                return false;
            }
            foreach (var objectType in objectTypes.OrderBy(x => x.Key))
            {
                bool result = CreateTable(objectType.Value, out errorMessage);
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }
        public bool DeleteObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.DeleteObject(obj, out errorMessage);
        }
        public bool DropTable(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.DropTable(objectType, out errorMessage);
        }
        public string DropTableScript(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return null;
            }
            return DatabaseController.Instance.DropTableScript(objectType, out errorMessage);
        }
        public bool DropTables(Dictionary<int, Type> objectTypes, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            errorMessage = null;
            if (objectTypes == null || objectTypes.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.NO_OBJECT_PASSED);
                return false;
            }
            foreach (var objectType in objectTypes.OrderByDescending(x => x.Key))
            {
                bool result = DropTable(objectType.Value, out errorMessage);
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }

        public bool UpdateObject(object obj, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.UpdateObject(obj, out errorMessage);
        }
        public DataTable QueryData(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return null;
            }
            return DatabaseController.Instance.QueryData(objectType, out errorMessage);
        }
        public DataTable QueryData(Type objectType, List<CustomDatabaseWhereParameter> whereParameters, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return null;
            }
            return DatabaseController.Instance.QueryData(objectType, whereParameters, out errorMessage);
        }
        public bool ExecuteSql(string sqlStatement, out CustomDatabaseErrorMessage errorMessage)
        {
            if (DatabaseController.Instance == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(ErrorMessages.DB_NOT_INITIALIZED);
                return false;
            }
            return DatabaseController.Instance.ExecuteSqlStatement(sqlStatement, out errorMessage);
        }
    }
}
