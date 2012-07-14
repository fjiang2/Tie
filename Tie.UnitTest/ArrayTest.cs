using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class ArrayTest
    {

        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();


            string code = @"
               I0 = {1,2,3}; 
               I1 = new int[]{1,2,3};
               I2 = new string[]; 
               int64 = long[];
               I3 = new int64 {10,20,30};
               I4 = new object[] {'A', 1, 3.0};
               A = new int[][] { {1,2}, {3,4}, {5,6}};
            ";

            DS.Clear();
            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert((string)DS["I0"].Valor == "{1,2,3}");
            System.Diagnostics.Debug.Assert((string)DS["I1"].Valor == "{1,2,3}.typeof(\"System.Int32[]\")");
            System.Diagnostics.Debug.Assert((string)DS["I2"].Valor == "{}.typeof(\"System.String[]\")");

            //System.Diagnostics.Debug.Assert((double)DS["F"].HostValue == 9.0);
            //System.Diagnostics.Debug.Assert(DS["B3"].HostValue == typeof(int[][]));
            //System.Diagnostics.Debug.Assert(DS["B1"].HostValue.GetType() == typeof(int[]));
            //System.Diagnostics.Debug.Assert(DS["D"].HostValue.GetType() == typeof(double[]));

            Logger.Close();
        }
    }
}
