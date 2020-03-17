using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.CustomDatabase
{
    internal class CustomDatabaseWhereParameterHelper
    {
        internal static string OperatorToString(CustomDatabaseOperator operatorEnum, out string errorMessage)
        {
            errorMessage = null;
            if(operatorEnum == CustomDatabaseOperator.EQUALS)
            {
                return "=";
            }
            else if(operatorEnum == CustomDatabaseOperator.GREATER_THAN)
            {
                return ">";
            }
            else if(operatorEnum == CustomDatabaseOperator.GREATER_THAN_OR_EQUALS)
            {
                return ">=";
            }
            else if(operatorEnum == CustomDatabaseOperator.LESS_THAN)
            {
                return "<";
            }
            else if(operatorEnum == CustomDatabaseOperator.LESS_THAN_OR_EQUALS)
            {
                return "<=";
            }
            else if(operatorEnum == CustomDatabaseOperator.NOT_EQUALS)
            {
                return "<>";
            }
            errorMessage = string.Format("{0} ({1})", "Operator " + operatorEnum.ToString() + " bestaat niet", MethodBase.GetCurrentMethod().Name);
            return null;
        }
    }
}
