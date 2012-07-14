using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    class MathTest
    {
        public static void main()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("One", 1);
            dict.Add("Two", 2);

            string code = @"
                a= Math.Sin(Math.PI/4.0);
                b = Math.Exp(1.0);
                d = Convert.ToDecimal(20);      //TIE internal is double, see VAL.Boxing
                e1 = Convert.ToByte(20);
                e2 = Convert.ToByte(30);
                e = e1+e2;
                x = dict['Two'];
                dict.Add('Three',3);

                dict2 = new System.Collections.Generic.Dictionary<string, int>();
                dict2.Add('One',1);
                dict2.Add('Three',3);
                dict2.Add('Ten',10);
                
          keys=''; values=0;
                foreach(kvp in dict2)
                {
                   keys += kvp.Key;
                   values += kvp.Value;
                }
                

                today = new System.DateTime(2010,10,26);
                D = {Width:120, Height:24, Caption:'Emp'};
                y=D.Width==120? 10:20;
            ";

             
            Tie.Logger.Open("C:\\temp\\tie.log");
            HostType.Register(typeof(System.Math), true);
            HostType.Register(typeof(System.Convert), true);
           // HostType.Register("DictionaryStringInt32", typeof(Dictionary<string, int>));
            HostType.Register(typeof(System.DateTime), true);

            VAL v = VAL.Array();
 
            Script script = new Script("unknown", 500);
            script.DS.AddObject("dict", dict);
            script.VolatileExecute(code);

            Debug.Assert(script.DS["a"].Doublecon == 0.70710678118654746);
            Debug.Assert(script.DS["x"].Intcon == 2, "Dictionary<string,int> operation");
            Debug.Assert(script.DS["dict2"]["Ten"].Intcon == 10, "Dictionary<string,int> operation");
            Debug.Assert(script.DS["keys"].Str == "OneThreeTen");
            Debug.Assert(script.DS["values"].Intcon == 14);


            Tie.Logger.Close();

        }
    }
}
