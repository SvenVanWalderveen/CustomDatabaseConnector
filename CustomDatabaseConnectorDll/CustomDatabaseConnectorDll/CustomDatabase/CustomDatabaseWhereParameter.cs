using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.CustomDatabase
{
    public class CustomDatabaseWhereParameter
    {
        public PropertyInfo Property { get; set; }
        public CustomDatabaseOperator Operator { get; set; }
        public object Value { get; set; }

    }
    public enum CustomDatabaseOperator 
    {
        EQUALS, GREATER_THAN, LESS_THAN, GREATER_THAN_OR_EQUALS, LESS_THAN_OR_EQUALS, NOT_EQUALS
    }
}
