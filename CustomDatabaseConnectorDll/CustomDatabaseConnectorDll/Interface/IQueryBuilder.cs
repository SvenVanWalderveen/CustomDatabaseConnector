using CustomDatabaseConnectorDll.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Interface
{
    internal interface IQueryBuilder
    {
        string BuildCreateTable(Type objectType);
        string GetTableName(object obj);
        string GetTableName(Type typeOfObject);
        List<PropertyInfo> GetDbProperties(Type objectType);
        List<string> ConvertClassAttributeToSqlCreate(PropertyInfo propertyInfo, CustomDatabaseColumnAnnotation annotation, out string errorMessage);
        string GetPrimaryKey(Type type);
        string GetSqlDataTypeByNetField(PropertyInfo propertyInfo, int maxLength, out string errorMessage);

    }
}
