﻿using CustomDatabaseConnectorDll.Annotations;
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
            TestTable obj = new TestTable();
            adapter.CreateTable(obj, out errorMessage);
        }
        
    }
    [CustomDatabaseClassAnnotation(TableName = "TESTTABLE")]
    public class TestTable
    {

    }
}
