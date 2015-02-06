using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using Tie;

namespace SearchEngine
{

    /// <summary>
    /// User defined Search Engine Example
    ///     given for each searching:
    ///        1.SQL SELECT Statment with parameter defined.
    ///        2.optional, initial values of paramters
    ///        3.User defined Windows Form to input paramters' values
    ///        
    /// all of above are string values which can stored in database
    ///    
    /// 
    /// </summary>
    public partial class SearchEngineDemo : Form
    {

        public const string connectionString = "Data Source=localhost\\sqlexpress;Initial catalog=demo;Trusted_Connection=True;";

        //to retrive data from SQL Server by SELECT Statement with paramters(@ID, @Date1, @Date2)
        public string SQL;

        //define WinForm to get 3 values(Result.ID,Result.Date1, Result.Date2)
        public string FormCode;

        public SearchEngineDemo()
        {
            InitializeComponent();
            Logger.Open("c:\\temp\\tie.log");

            HostType.Register(typeof(DateTime));
            HostType.Register(typeof(DialogResult));

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


        private void button1_Click(object sender, EventArgs e)
        {
 
            /*
             * ---------------------------------------------------------------+--------------------------------------------------
             *            C#                                                  |        Tie
             *  --------------------------------------------------------------+--------------------------------------------------          
             *  VAL initialValue = new VAL();                                 |   initialValue.ID = "00090"
             *  initialValue["ID"] = new VAL("00090");                        |   initialValue.Date1 = HostType(System.DateTime.Now).AddMonths(-12);
             *  initialValue["Date1"] = new VAL(DateTime.Now.AddMonths(-12)); |   initialValue.Date2 = System.DateTime.Now;
             *  initialValue["Date2"] = new VAL(DateTime.Now);                |
             *  --------------------------------------------------------------+--------------------------------------------------          
             *  
             */
                        
            VAL initialValue = new VAL();
            initialValue["ID"] = new VAL("500");
            initialValue["Date"] = VAL.Array(2);
            initialValue["Date"][0] = VAL.Boxing(DateTime.Now.AddYears(-15));
            initialValue["Date"][1] = VAL.Boxing(DateTime.Now);

            Script script = new Script();
            script.DS.Add("initialValue", initialValue);
            script.Execute(this.FormCode);
            
            Form form = (Form)script.DS["form"].HostValue;     
            if (form.ShowDialog() == DialogResult.OK)
            {
                VAL result = script.DS["Result"];              
                if (!result.IsNull)
                {
                    DataTable dataTable = FillDataTable(SQL, result);
                    this.richTextBox1.Text += WriteResult(result, dataTable);
                }

            }
            
            script.Dispose();
        }

   
    }
}
