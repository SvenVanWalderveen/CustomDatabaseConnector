using CustomDatabaseConnectorDll.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomDatabaseConnectorDll.CustomDatabaseAdapter adapter = new CustomDatabaseConnectorDll.CustomDatabaseAdapter();
            string errorMessage = null;
            adapter.CreateTable(typeof(TestTable), out errorMessage);
       
        }
        
    }
    [CustomDatabaseClassAnnotation(TableName = "TESTTABLE")]
    public class TestTable
    {
        [CustomDatabaseColumnAnnotation(ColumnName = "ID", IsPrimaryKey = true, IsAutoIncrement = true)]
        public int Id { get; set; }
    }
    [CustomDatabaseClassAnnotation(TableName = "TESTTABLE2")]
    public class TestTable2
    {
        [CustomDatabaseColumnAnnotation(ColumnName = "ID", IsPrimaryKey = true, IsAutoIncrement = true)]
        public int Id { get; set; }
        [CustomDatabaseColumnAnnotation(ColumnName = "NAME", IsNullable = false, IsUpdatable = true)]
        public string Name { get; set; }
    }
    [CustomDatabaseClassAnnotation(TableName = "TESTTABLE1")]
    public class TestTable1
    {
        [CustomDatabaseColumnAnnotation(ColumnName = "ID", IsPrimaryKey = true, IsAutoIncrement = true)]
        public int Id { get; set; }
        [CustomDatabaseColumnAnnotation(ColumnName = "TEST_ID", ForeignKeyTable = typeof(TestTable))]
        public int TestId { get; set; }
    }
}
