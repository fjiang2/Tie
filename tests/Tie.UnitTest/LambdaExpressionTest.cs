using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    class LambdaExpressionTest
    {
        public static void main()
        {
            Memory DS = new Memory();
            string code = @"
              plus1 = (x,y,z) => x+y+z;   
              sum1 = plus1(20,30,40);

              len = (A) => size(A);
              L1 = len({1,2,3,4});

              len = () => L1+10;
              L2 = len();

              plus2 = (x,y,z) => { return x+y+z; };
              sum2 = plus2(20,30,40);

              plus20 = x => x+20;
              sum3 = plus20(40);

              list4 = (x,y,z) => { x , y, z , 100 };
              L4 = list4(20,30,40);

              list5 = (x,y,z) => { x , y, z , 100 }.typeof('SET');
              L5 = list5(20,30,40);


";

            Tie.Logger.Open("C:\\temp\\tie.log");
            Script.Execute(code, DS);

            System.Diagnostics.Debug.Assert(DS["sum1"].Intcon == 90);
            System.Diagnostics.Debug.Assert(DS["sum2"].Intcon == 90);
            System.Diagnostics.Debug.Assert(DS["sum3"].Intcon == 60);
            System.Diagnostics.Debug.Assert(DS["L1"].Intcon == 4);
            System.Diagnostics.Debug.Assert(DS["L2"].Intcon == 14);
            System.Diagnostics.Debug.Assert(DS["L4"].ToString() == "{20,30,40,100}");
            System.Diagnostics.Debug.Assert(DS["L5"].ToString() == "{20,30,40,100}.typeof(\"SET\")");
        }
            
    }
}
