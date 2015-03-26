using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UnitTest
{
    class SqlCmd
    {
        public const string connectionString = "Data Source=localhost\\sqlexpress;Initial catalog=demo;Trusted_Connection=True;";


        public static DataTable FillDataTable(string SQL)
        {
            DataTable dataTable = new DataTable();

            SqlCommand sqlCommand = new SqlCommand(SQL, new SqlConnection(connectionString));
            sqlCommand.Connection.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            adapter.Fill(dataTable);
            sqlCommand.Connection.Close();

            return dataTable;
        }
    }
}
