using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using Tie;

public partial class SearchEngineDemo : System.Web.UI.Page
{
    public const string connectionString = "Data Source=localhost\\sqlexpress;Initial catalog=demo;Trusted_Connection=True;";


    public string SQL;
    public string FormCode;

    protected void Page_Load(object sender, EventArgs e)
    {
        HostType.Register(typeof(DateTime));
        Logger.Open("c:\\temp\\tie.log");



        StreamReader streamReader = new StreamReader("G:\\devel\\TieDemo\\Tie.Web\\form.tie");
        FormCode = streamReader.ReadToEnd();
        streamReader.Close();

        streamReader = new StreamReader("G:\\devel\\TieDemo\\data\\test.sql");
        SQL = streamReader.ReadToEnd();
        streamReader.Close();


        if (this.IsPostBack && Session["Script"] != null)
        {
            Script script = (Script)Session["Script"];
            script.DS.AddHostObject("page", this);
            script.Execute(this.FormCode);
            //script.Run(0);
            
            Table table = (Table)script.DS["table"].HostValue;
            this.PlaceHolder1.Controls.Clear();
            this.PlaceHolder1.Controls.Add(table);

        }
    }

    public void ShowResult(VAL result)
    {
        DataTable dataTable = FillDataTable(SQL, result);
        this.TextBox1.Text = WriteResult(result, dataTable);
    }
    public void ClearResult()
    {
        this.TextBox1.Text = "";
    }

    private string WriteResult(VAL result, DataTable dataTable)
    {
        //Show Output
        StringWriter sw = new StringWriter();
        sw.WriteLine("<p>SQL Statement</p>");
        sw.WriteLine(SQL);

        sw.WriteLine();
        sw.WriteLine("<p>SQL Criteria Parameter:</p>");
        for (int i = 0; i < result.Size; i++)
            sw.WriteLine(string.Format("<p>@{0}={1}</p>", result[i][0].ToString2(), result[i][1]));

        sw.WriteLine();
        sw.WriteLine("<p>DataTable retrieved from SQL Server:</p>");
        foreach (DataRow dataRow in dataTable.Rows)
        {
            sw.WriteLine("<p>");
            foreach (object obj in dataRow.ItemArray)
            {
                sw.Write("\t");
                sw.Write(obj);
            }
            sw.WriteLine("</p>");
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


     protected void Button1_Click(object sender, EventArgs e)
    {
        VAL initialValue = new VAL();
        initialValue["ID"] = new VAL("500");
        initialValue["Date"] = VAL.Array(2); 
        initialValue["Date"][0] = new VAL(DateTime.Now.AddYears(-15));
        initialValue["Date"][1] = new VAL(DateTime.Now);

        Script script = new Script();
        script.DS.Add("initialValue", initialValue);
        script.DS.AddHostObject("page", this);
        script.Execute(this.FormCode);

        Table table = (Table)script.DS["table"].HostValue;
        this.PlaceHolder1.Controls.Clear();
        this.PlaceHolder1.Controls.Add(table);
        
        Session["Script"] = script;
    }

   
    
}
