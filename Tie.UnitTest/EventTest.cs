using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Tie;


namespace UnitTest
{
    class EventTest
    {

        public static void eventTest()
        {
            HostType.Register(typeof(DataTable));
            DataTable dt = SqlCmd.FillDataTable("SELECT * FROM Employee");

            Script script = new Script();
            script.DS = new Memory();
            script.DS.Add("dt", VAL.NewHostType(dt));

            string code = @"
                dt.RowChanged += function(sender, e)
                {
                    ID = e.Row['ID'];
                };
                
                ID = dt.Rows[0]['ID'];    
                
                plus = function(a,b) { return a+b;};
                multiple = function(a,b) { return a*b;};
                sum = plus(2,3);
            ";

            script.Execute(code);
            System.Diagnostics.Debug.Assert(script.DS["ID"].Intcon == 1, "event handler");
            dt.Rows[0]["ID"]=20;
            dt.Rows[0].AcceptChanges();
            System.Diagnostics.Debug.Assert(script.DS["ID"].Intcon == 20, "event handler");

            script.Execute("X=plus(2,3);");
            VAL p34 = script.ResidentEvaluate("plus(3,4)");
            VAL p23 = Script.Evaluate("plus(2,3)",  script.DS);

            // or
            // args = Coding.Decode("{20,42}");
            VAL x1 = script.InvokeFunction("plus", new object[] {20,42});
            System.Diagnostics.Debug.Assert(x1.Intcon == 62, "plus");

            code = @" 
              minus = function(a,b) { return a-b;};
            ";

            script.Execute(code);
            VAL x2 = script.InvokeFunction("minus", new object[] {20, 42});
            System.Diagnostics.Debug.Assert(x2.Intcon == -22, "minus");

            VAL x3 = script.InvokeFunction("multiple", new object[] {20, 30});
            System.Diagnostics.Debug.Assert(x3.Intcon == 600, "multiple");

            

        }
    }
}

