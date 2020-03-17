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
        internal static string GetTableName(Type objectType, out string errorMessage)
        {
            if(objectType == null)
            {
                errorMessage = string.Format("{0} ({1}_", "Geen objecttype meegegeven", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            CustomDatabaseClassAnnotation classAttr = (CustomDatabaseClassAnnotation)objectType.GetCustomAttribute(typeof(CustomDatabaseClassAnnotation), true);
            if (classAttr == null)
            {
                errorMessage = string.Format("{0} ({1}_", "Geen klasse-annotatie bekend", MethodBase.GetCurrentMethod().Name);
                return null;
            }
            errorMessage = null;
            return classAttr.TableName;
        }

        internal static List<PropertyInfo> GetColumns(Type objectType, out string errorMessage)
        {
            if(objectType == null)
            {
                errorMessage = string.Format("{0} ({1}_", "Geen objecttype meegegeven", MethodBase.GetCurrentMethod().Name);
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
            errorMessage = null;
            return result;
        }

        internal static List<PropertyInfo> GetPrimaryKeyColumns(Type objectType, out string errorMessage)
        {
            if (objectType == null)
            {
                errorMessage = string.Format("{0} ({1}_", "Geen objecttype meegegeven", MethodBase.GetCurrentMethod().Name);
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
            errorMessage = null;
            return result;
        }
    }
}
