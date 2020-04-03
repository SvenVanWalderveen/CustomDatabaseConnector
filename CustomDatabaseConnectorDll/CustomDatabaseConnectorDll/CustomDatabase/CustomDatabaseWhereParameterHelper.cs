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
        internal static string OperatorToString(CustomDatabaseOperator operatorEnum, out CustomDatabaseErrorMessage errorMessage)
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
            errorMessage = new CustomDatabaseErrorMessage(string.Format("Operator {0} is not defined", operatorEnum.ToString()));
            return null;
        }
    }
}
