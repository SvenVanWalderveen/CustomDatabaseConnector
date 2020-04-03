using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.CustomDatabase
{
    public class CustomDatabaseErrorMessage
    {
        public string CurrentMethod { get; set; }
        public string DotNetProperty { get; set; }
        public string DotNetClass { get; set; }
        public string ErrorMessage { get; set; }

        //public CustomDatabaseErrorMessage(string sqlTable, string sqlTableProperty, string errorMessage)
        //{
        //    MethodBase calledMethod = new StackTrace().GetFrame(1).GetMethod();
        //    this.CurrentMethod = string.Format("{0}.{1}", calledMethod.DeclaringType.FullName, calledMethod.Name);
        //    this.SqlTable = sqlTable;
        //    this.SqlTableProperty = sqlTableProperty;
        //    this.ErrorMessage = errorMessage;
        //}
        public CustomDatabaseErrorMessage(Type objectType, string errorMessage)
        {
            MethodBase calledMethod = new StackTrace().GetFrame(1).GetMethod();
            this.CurrentMethod = string.Format("{0}.{1}", calledMethod.DeclaringType.FullName, calledMethod.Name);
            this.DotNetClass = objectType.FullName;
            this.ErrorMessage = errorMessage;
        }
        public CustomDatabaseErrorMessage(string errorMessage)
        {
            MethodBase calledMethod = new StackTrace().GetFrame(1).GetMethod();
            this.CurrentMethod = string.Format("{0}.{1}", calledMethod.DeclaringType.FullName, calledMethod.Name);
            this.ErrorMessage = errorMessage;
        }

        public CustomDatabaseErrorMessage(PropertyInfo pi, string errorMessage)
        {
            MethodBase calledMethod = new StackTrace().GetFrame(1).GetMethod();
            this.CurrentMethod = string.Format("{0}.{1}", calledMethod.DeclaringType.FullName, calledMethod.Name);
            this.DotNetClass = pi.DeclaringType.FullName;
            this.DotNetProperty = pi.Name;
            this.ErrorMessage = errorMessage;
        }
    }
}
