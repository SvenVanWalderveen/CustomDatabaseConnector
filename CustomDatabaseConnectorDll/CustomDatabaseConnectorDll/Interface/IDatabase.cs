using CustomDatabaseConnectorDll.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Interface
{
    internal interface IDatabase
    {
        string BuildCreateObject(object obj, out string errorMessage);
        string BuildCreateStatement(Type objectType, out string errorMessage);
        string BuildDeleteObject(object obj, out string errorMessage);
        string BuildDropStatement(Type objectType, out string errorMessage);
        string BuildUpdateObject(object obj, out string errorMessage);
        string ConvertNetValueToSqlValue(PropertyInfo pi, object obj, out string errorMessage);
        bool CreateObject(object obj, out int newRecordId, out string errorMessage);
        bool CreateObject(object obj, out string errorMessage);
        string CreateObjectScript(object obj, out string errorMessage);
        bool CreateTable(Type objectType, out string errorMessage);
        string CreateTableScript(Type objectType, out string errorMessage);
        bool DeleteObject(object obj, out string errorMessage);
        bool DropTable(Type objectType, out string errorMessage);
        string DropTableScript(Type objectType, out string errorMessage);
        bool ExecuteSqlStatement(string sqlQuery, out int newRecordId, out string errorMessage);
        bool ExecuteSqlStatement(string sqlQuery, out string errorMessage);
        DataTable ExecuteSqlStatementToDataTable(string sqlQuery, out string errorMessage);
        List<string> GenerateColumnDefinitions(List<PropertyInfo> columns, out string errorMessage);
        List<string> GenerateForeignKeyDefinitions(List<PropertyInfo> columns, string tableName, out string errorMessage);
        string GeneratePrimaryKeyDefinition(List<PropertyInfo> columns, string tableName, out string errorMessage);
        string GetConnectionString();
        string GetSqlDataTypeByNetType(PropertyInfo pi, CustomDatabaseColumnAnnotation annotation, out string errorMessage);
        DataTable QueryData(Type objectType, out string errorMessage);
        bool UpdateObject(object obj, out string errorMessage);
    }
}
