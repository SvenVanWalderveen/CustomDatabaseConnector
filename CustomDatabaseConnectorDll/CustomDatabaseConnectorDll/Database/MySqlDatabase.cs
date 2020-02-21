using CustomDatabaseConnectorDll.Interface;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDatabaseConnectorDll.Database
{
    internal class MySqlDatabase : IDatabase
    {
        public bool CreateTable(object obj, out string errorMessage)
        {
            MySqlQueryBuilder builder = new MySqlQueryBuilder();
            string sql = builder.BuildCreateTable(obj.GetType());
            return ExecuteSql(sql, out errorMessage);
        }

        public bool DeleteQuery(object obj, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteSql(string sqlStatement, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                MySqlConnection con = new MySqlConnection(GetConnectionString());
                con.Open();
                MySqlCommand cmd = new MySqlCommand(sqlStatement, con);
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public string GetConnectionString()
        {
            return CacheClass.ConnectionString;
        }

        public bool InsertQuery(object obj, out int newRecordId, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public bool InsertQuery(object obj, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectQuery(Type classType)
        {
            throw new NotImplementedException();
        }

        public DataTable SelectQuery(Type classType, string whereFilter)
        {
            throw new NotImplementedException();
        }

        public bool UpdateQuery(object obj, out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
