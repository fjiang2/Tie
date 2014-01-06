using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Tie;
using EasyWay.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

//测试
namespace UnitTest
{
    class UnitTest
    {
        public static void test1()
        {
            DataRow row = SQLCommand.FillDataRow("SELECT * FROM workflows");
            DataTable dt = SQLCommand.FillDataTable("SELECT * FROM Workflows");

//            DataRow row = SQLCommand.FillDataRow("SELECT * FROM Security..Commands");
//            DataTable dt = SQLCommand.FillDataTable("SELECT * FROM Security..Commands");
            Memory myDS = new Memory();

            decimal dec = 1m;

            myDS.Add("dec", VAL.NewBestVAL(dec));
            myDS.Add("row", VAL.NewHostType(row));
            myDS.Add("dt", VAL.NewHostType(dt));
            myDS.Add("tbEdit", VAL.NewHostType(new TextBox()));

            HostType.Register(typeof(System.Reflection.MemberTypes));
            HostType.Register(typeof(System.Drawing.Color));
            HostType.Register(typeof(System.Drawing.Font));
            HostType.Register(typeof(System.Drawing.FontStyle));
            HostType.Register(typeof(System.Drawing.GraphicsUnit));



            //HostType.Register(DateTime.Now);
            HostType.Register(new Label());
            HostType.Register(dt.NewRow());

            dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
            string primaryKey = dt.PrimaryKey[0].ColumnName;

            string code = @"{
                  color = System.Drawing.Color.Red;
                  a1= row['Label'];  

                  a2= row.Description;  
                  a3 = row.Table.Rows.Count;
                  row['Description'] = 'xxxx';
                  a4= row.Description;  

                  a5 = dt.Rows[1].Label;  
                  primaryKey = dt.PrimaryKey[0].ColumnName;
                  
                  newRow = dt.NewRow();
                  newRow['Workflow_ID'] = '200000';       
                  //newRow['ID'] =20000;
                  newRow['Label'] = 'New Row Label';
                  
                  a1= newRow['Description'];
                  dt.Rows.Add(newRow);


                  descIsNull = dt.Rows[1].IsNull('Description');  

                  setting.Rows[0] =  {{'Description','1111Desc'}, {'Label','1111Labe'}};
                  setting.Rows[1] =  {{'Description','2222Desc'}, {'Label','2222Labe'}};
                  setting.Rows[2] =  {{'Description', null}, {'Label',null}};

                  textbox =  {{'Class','System.Windows.Forms.Label'},{'Text','LC#'}, {'Position',{2,1}} };
                   
                  combo = {textbox,setting};

                  label1 = new System.Windows.Forms.Label();
                  label1.Text = 'Hello World';
                  label1.ForeColor = System.Drawing.Color.Red;
                  label1.Font = new System.Drawing.Font('Arial Black', float(11.25), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, byte(0));
                  
//                  a1 = System.Reflection.MemberTypes.All;

                  //func = function(sender,e){a1=1;a2=2;});
                  //tbEdit.Click.AddListener(function(sender,e){a1=1;a2=2;});
            }";


            Coding.Decode(code, CodeType.statement, myDS);
            Label label1 = (Label)myDS["label1"].value;

            string x = myDS["label1"].ToString();
            x = myDS["color"].ToString();
            x = myDS["row"].ToString();
            TextBox tb = new TextBox();

            // HostType.SetObjectProperties(dt, myDS["setting"]);

            VAL before = HostType.GetObjectProperties(tb, myDS["textbox"]);
            HostType.SetObjectProperties(tb, myDS["textbox"]);
            VAL after = HostType.GetObjectProperties(tb, myDS["textbox"]);

            after = HostType.GetObjectProperties(new object[] { tb, dt }, myDS["combo"]);


            myDS.Clear();
            HostType.Register(new TextBox());
            code = @"{
                            //tb = new System.Windows.Forms.TextBox();
                            tb.Name = 'tbEdit';
                            tb.Text= 'Hello World';
                            tb.WordWrap= true;
                            tb.Padding.Bottom=111;
                        }";
            Coding.Decode(code, CodeType.statement, myDS);
            VAL properties = myDS["tb"];
            TextBox textBox = new TextBox();
            HostType.SetObjectProperties(textBox, properties);

            //VAL pro = HostType.GetObjectProperties(new TextBox());
            //string o = pro.ToString();

            return;
        }



        public static void test2()
        {
            Memory myDS = new Memory();
            TextBox tbEdit = new TextBox();
            tbEdit.Text = "Hello world";
            tbEdit.ForeColor = Color.Red;

            myDS.Add("tbEdit", VAL.NewHostType(tbEdit));

            HostType.Register(typeof(System.Reflection.MemberTypes));
            HostType.Register(typeof(System.Drawing.Color));
            HostType.Register(typeof(System.Drawing.Font));
            HostType.Register(typeof(System.Drawing.FontStyle));
            HostType.Register(typeof(System.Drawing.GraphicsUnit));

            VAL x = HostType.GetObjectProperties(tbEdit);
        }
    }
}
