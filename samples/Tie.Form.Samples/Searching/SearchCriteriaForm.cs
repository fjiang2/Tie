using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient; 
using Tie;

namespace Tie.FormTest
{
    class SearchCriteriaForm
    {
        Script script;

        private SearchCriteriaForm(string formCode)
        {
            script = new Script();
            script.Execute(formCode);

        }


        private VAL Show(VAL initialValue)
        {
            script.DS.Add("initialValue", initialValue);
            script.VolatileExecute("myForm = new MyForm(initialValue); myForm.InitializeComponent();");

            Form form = (Form)script.DS["myForm"]["form"].HostValue;        // = myForm.form
            if (form.ShowDialog() == DialogResult.OK)
                return script.DS["myForm"]["Result"];                       // = myForm.Result
            else
                return new VAL();
        }

        private DataTable FillDataTable(string SQL, VAL Result)
        {
            DataTable dataTable = new DataTable();

            SqlCommand sqlCommand = new SqlCommand(SQL, new SqlConnection(SearchMainForm.connectionString));
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


        public static bool Search(SearchItem item)
        {

            SearchCriteriaForm searchCriteriaForm = new SearchCriteriaForm(item.FormCode);
            VAL result = searchCriteriaForm.Show(item.InitialValue);
            searchCriteriaForm.script.Dispose();

            if (result.IsNull)
                return false;

            item.Result = result;
            item.DataTable = searchCriteriaForm.FillDataTable(item.SQL , result);
            
            return true;
        }
    }
}
