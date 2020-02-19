using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Annotations
{
    public class CustomDatabaseClassAnnotation : Attribute
    {
        private string tableName;

        public virtual string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }
    }
}
