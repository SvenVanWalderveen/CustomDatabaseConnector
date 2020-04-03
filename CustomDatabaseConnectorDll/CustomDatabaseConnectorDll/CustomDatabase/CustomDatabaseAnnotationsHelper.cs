using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.CustomDatabase
{
    internal class CustomDatabaseAnnotationsHelper
    {
        internal static string GetTableName(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            errorMessage = null;
            if (objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.NO_OBJECTTYPE_PASSED);
                return null;
            }
            CustomDatabaseClassAnnotation classAttr = (CustomDatabaseClassAnnotation)objectType.GetCustomAttribute(typeof(CustomDatabaseClassAnnotation), true);
            if (classAttr == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.ANN_CLASS_NO_ANNOTATION);
                return null;
            }
            if(string.IsNullOrEmpty(classAttr.TableName))
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.ANN_NO_CLASSNAME_INSERTED);
                return null;
            }
            return classAttr.TableName;
        }

        internal static List<PropertyInfo> GetColumns(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if(objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.NO_OBJECTTYPE_PASSED);
                return null;
            }
        
            List<PropertyInfo> result = new List<PropertyInfo>();
            PropertyInfo[] properties = objectType.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (annotation != null)
                {
                    result.Add(pi);
                }
            }
            if(result.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.ANN_CLASS_NO_COLUMNS);
                return null;
            }
            errorMessage = null;
            return result;
        }

        internal static List<PropertyInfo> GetPrimaryKeyColumns(Type objectType, out CustomDatabaseErrorMessage errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.NO_OBJECTTYPE_PASSED);
                return null;
            }

            List<PropertyInfo> result = new List<PropertyInfo>();
            PropertyInfo[] properties = objectType.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                CustomDatabaseColumnAnnotation annotation = (CustomDatabaseColumnAnnotation)pi.GetCustomAttribute(typeof(CustomDatabaseColumnAnnotation));
                if (annotation != null && annotation.IsPrimaryKey)
                {
                    result.Add(pi);
                }
            }
            if (result.Count == 0)
            {
                errorMessage = new CustomDatabaseErrorMessage(objectType, ErrorMessages.ANN_NO_PRIMARY_KEY);
                return null;
            }
            errorMessage = null;
            return result;
        }
    }
}
