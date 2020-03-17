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

    public class CustomDatabaseColumnAnnotation : Attribute
    {
        private bool isAutoIncrement;
        private bool isPrimaryKey;
        private string columnName;
        private Type fkTable;
        private bool isNullable;
        private int maxLength;
        private bool updatable;
        public virtual bool IsAutoIncrement
        {
            get { return isAutoIncrement; }
            set { isAutoIncrement = value; }
        }
        public virtual bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set { isPrimaryKey = value; }
        }
        public virtual string ColumnName
        {
            get { return columnName; }
            set { columnName = value; }
        }
        public virtual Type ForeignKeyTable
        {
            get { return fkTable; }
            set { fkTable = value; }
        }
        public virtual bool IsNullable
        {
            get { return isNullable; }
            set { isNullable = value; }
        }
        public virtual bool IsUpdatable
        {
            get { return updatable; }
            set { updatable = value; }
        }

        public virtual int MaxLength
        {
            get { return maxLength; }
            set { maxLength = value; }
        }
    }
}
