using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Data;
using Tie;

namespace UnitTest
{
    class HostObjectTest
    {
        public static void main()
        {
            DataTable dt = SqlCmd.FillDataTable("SELECT * FROM Employee");
            DataRow row = dt.Rows[0];

            Memory myDS = new Memory();

            decimal dec = 1m;

            myDS.Add("dec", VAL.Boxing(dec));
            myDS.Add("row", VAL.NewHostType(row));
            myDS.Add("dt", VAL.NewHostType(dt));
            myDS.Add("tbEdit", VAL.NewHostType(new TextBox()));

            HostType.Register(typeof(System.Reflection.MemberTypes));
            HostType.Register(typeof(System.Drawing.Color));
            HostType.Register(typeof(System.Drawing.Font));
            HostType.Register(typeof(System.Drawing.FontStyle));
            HostType.Register(typeof(System.Drawing.GraphicsUnit));
            HostType.Register(typeof(System.Data.DataRow));



            //HostType.Register(DateTime.Now);
            HostType.Register(typeof(Label));
            HostType.Register(dt.NewRow().GetType());

            dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
            string primaryKey = dt.PrimaryKey[0].ColumnName;

            string code = @"{
                  color = System.Drawing.Color.Red;
                  a1= row['First_Name'];  

                  a2= row.Description;  
                  a3 = row.Table.Rows.Count;
                  row['Last_Name'] = 'xxxx';
                  a4= row.First_Name;  

                  a5 = dt.Rows[1].Last_Name;  
                  primaryKey = dt.PrimaryKey[0].ColumnName;
                  
                  newRow = dt.NewRow();
                  newRow['ID'] = '200000';       
                  newRow['Last_Name'] = 'New Row Label';
                  
                  a1= newRow['Last_Name'];
                  dt.Rows.Add(newRow);


                  descIsNull = dt.Rows[1].IsNull('Last_Name');  

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


            Script.Execute(code, myDS);
            Label label1 = (Label)myDS["label1"].Value;

            string x = myDS["label1"].ToString();
            x = myDS["color"].ToString();
            x = myDS["row"].ToString();
            TextBox tb = new TextBox();

            // HostType.SetObjectProperties(dt, myDS["setting"]);

            HostType.SetObjectProperties(tb, myDS["textbox"]);


            myDS.RemoveAll();
            HostType.Register(typeof(TextBox));
            code = @"{
                            //tb = new System.Windows.Forms.TextBox();
                            tb.Name = 'tbEdit';
                            tb.Text= 'Hello World';
                            tb.WordWrap= true;
                            tb.Padding.Bottom=111;
                        }";
            Script.Execute(code, myDS);
            VAL properties = myDS["tb"];
            TextBox textBox = new TextBox();
            HostType.SetObjectProperties(textBox, properties);

            //VAL pro = HostType.GetObjectProperties(new TextBox());
            //string o = pro.ToString();

            return;
        }

    }
}
