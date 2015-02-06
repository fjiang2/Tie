using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;

namespace Tie.FormTest
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
    public partial class SearchMainForm : Form
    {

        public const string connectionString = "data source=EADS-SQL1;initial catalog=Airepairs;password=dev33;user id=TimeAttendance;packet size=4096";
        SearchItem item;

        public SearchMainForm()
        {
            InitializeComponent();
            HostType.Register(typeof(DateTime));
            HostType.Register(typeof(DialogResult));

            Logger.Open("c:\\temp\\tie.log");

        }



       
        private void WriteResult(SearchItem item)
        {
            //Show Output
            StringWriter sw = new StringWriter();
            sw.WriteLine("SQL Statement");
            sw.WriteLine(item.SQL);

            sw.WriteLine();
            sw.WriteLine("SQL Criteria Parameter:");
            for (int i = 0; i < item.Result.Size; i++)
                sw.WriteLine(string.Format("@{0}={1}", item.Result[i][0].ToSimpleString(), item.Result[i][1]));

            sw.WriteLine();
            sw.WriteLine("DataTable retrieved from SQL Server:");
            foreach (DataRow dataRow in item.DataTable.Rows)
            {
                foreach (object obj in dataRow.ItemArray)
                {
                    sw.Write("\t");
                    sw.Write(obj);
                }
                sw.WriteLine();
            }

            this.richTextBox1.Text += sw.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VAL initialValue = new VAL();
            initialValue["ID"] = new VAL("01227");
            initialValue["Date1"] = VAL.Boxing(DateTime.Now.AddMonths(-12));
            initialValue["Date2"] = VAL.Boxing(DateTime.Now);

            if(item!=null)
                initialValue = item.Result;

            item = new SearchItem(1);
            item.InitialValue = initialValue;

            if(SearchCriteriaForm.Search(item))
                WriteResult(item);

            
        }

   
    }
}
