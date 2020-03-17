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
            adapter.Init("server=ID146926_sven.db.webhosting.be;user id=ID146926_sven;database=ID146926_sven;password=93!Ftg105;persistsecurityinfo=True");
            string errorMessage = null;
            //int num;
            //TestTable obj = new TestTable();
            //obj.Id = 2;
            //obj.Name = "Rnsn";
            //obj.Test = "Test2";
            //bool result = adapter.UpdateObject(obj, out errorMessage);
            //bool result = adapter.DeleteObject(obj, out errorMessage);
            //bool result = adapter.CreateTable(typeof(TestTable1), out errorMessage);v
            var data = adapter.QueryData(typeof(TestTable), out errorMessage);            
        }
        
    }
    [CustomDatabaseClassAnnotation(TableName = "TESTTABLE")]
    public class TestTable
    {
        [CustomDatabaseColumnAnnotation(ColumnName = "ID", IsPrimaryKey = true, IsAutoIncrement = true)]
        public int Id { get; set; }
        [CustomDatabaseColumnAnnotation(ColumnName = "NAME", IsNullable = false, IsUpdatable = true)]
        public string Name { get; set; }
        [CustomDatabaseColumnAnnotation(ColumnName = "TEST2", IsNullable = false, IsUpdatable = true)]
        public string Test { get; set; }

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
