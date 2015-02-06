using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tie;

namespace WpfApplicationDemo
{
    /// <summary>
    /// Interaction logic for SearchEngineDemo.xaml
    /// </summary>
    public partial class SearchEngineDemo : Window
    {
        public const string connectionString = "Data Source=localhost\\sqlexpress;Initial catalog=demo;Trusted_Connection=True;";    
        
        /*
        * 
        * Example of Searching
        *    given:
        *      1.SQL statement = string SQL1
        *      2.Window Form = string Code1
        * 
        * */
        //to retrive data from SQL Server by SELECT Statement with paramters(@ID, @Date1, @Date2)
        public string SQL;
         
        //define WinForm to get 3 values(Result.ID,Result.Date1, Result.Date2)
        public string FormCode;

        public SearchEngineDemo()
        {
            InitializeComponent();
            HostType.Register(typeof(DateTime));
            HostType.Register(typeof(System.Windows.VerticalAlignment));
            Logger.Open("c:\\temp\\tie.log");

            StreamReader streamReader = new StreamReader("..\\..\\form.tie");
            FormCode = streamReader.ReadToEnd();
            streamReader.Close();

            streamReader = new StreamReader("..\\..\\..\\data\\test.sql");
            SQL = streamReader.ReadToEnd();
            streamReader.Close();

        }

   
        private string WriteResult(VAL result, DataTable dataTable)
        {
            //Show Output
            StringWriter sw = new StringWriter();
            sw.WriteLine("SQL Statement");
            sw.WriteLine(SQL);

            sw.WriteLine();
            sw.WriteLine("SQL Criteria Parameter:");
            for (int i = 0; i < result.Size; i++)
                sw.WriteLine(string.Format("@{0}={1}", result[i][0].ToString2(), result[i][1]));

            sw.WriteLine();
            sw.WriteLine("DataTable retrieved from SQL Server:");
            foreach (DataRow dataRow in dataTable.Rows)
            {
                foreach (object obj in dataRow.ItemArray)
                {
                    sw.Write("\t");
                    sw.Write(obj);
                }
                sw.WriteLine();
            }

             return sw.ToString();
        }


        private DataTable FillDataTable(string SQL, VAL Result)
        {
            DataTable dataTable = new DataTable();

            SqlCommand sqlCommand = new SqlCommand(SQL, new SqlConnection(connectionString));
            for (int i = 0; i < Result.Size; i++)
            {
                string parameterName = Result[i][0].Str;
                object obj = Result[i][1].HostValue;
                sqlCommand.Parameters.AddWithValue("@" + parameterName, obj);
            }

            sqlCommand.Connection.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            adapter.Fill(dataTable);
            sqlCommand.Connection.Close();

            return dataTable;
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            VAL initialValue = new VAL();
            initialValue["ID"] = new VAL("500");
            initialValue["Date"] = VAL.Array(2);
            initialValue["Date"][0] = VAL.Boxing(DateTime.Now.AddYears(-15));
            initialValue["Date"][1] = VAL.Boxing(DateTime.Now);

            Script script = new Script();
            script.DS.Add("initialValue", initialValue);
            script.DS.AddHostObject("TRUE", (Nullable<bool>)true);
            script.DS.AddHostObject("FALSE", (Nullable<bool>)false);
            script.Execute(this.FormCode);


            Window form = (Window)script.DS["form"].HostValue;
            bool? r = form.ShowDialog();
            if (r == true)
            {
                VAL result = script.DS["Result"];
                if (!result.IsNull)
                {
                    DataTable dataTable = FillDataTable(SQL, result);
                    this.textBox1.Text += WriteResult(result, dataTable);
                }

            }

            script.Dispose();
        }


    }
}
