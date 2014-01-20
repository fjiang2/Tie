using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class CastTest
    {

        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            
            
            string code = @"
//tt = (this.from(base.S1) && base.S1.Completed) || (this.from(base.S7) && !base.S7.Results.Yes);

               c = char(65);
               AAA = 12 as double;
               S = (System.String[]) null;
               //I1 = new int[] {1,2,3};
               //I2 = new int[3][4]; 

               E = (System.Data.Table)null;
               S1 = (string)('A' + 12);
               S2 = (string)(12 + 12);

               A = (1+2)*3; 
               F = (double)A;
               B1 = (int[]){1,2,3,4};
               B2 = ((int[]){10,20});
               B3 = (((int[][])));
               B4 = (int[])(int[]){10,20};
               
                O = (object[]){1,2,3,4,5,6,7};
                D = (double[]){1,2,3,4,5,6,7};
                D2 = (object[]){{1,2,null},{4,5,6}};
              S = { 'A','B','C'};
            ";

            DS.RemoveAll();
            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert((string)DS["S1"].HostValue == "A12");
            System.Diagnostics.Debug.Assert((string)DS["S2"].HostValue == "24");

            System.Diagnostics.Debug.Assert((double)DS["F"].HostValue == 9.0);
            System.Diagnostics.Debug.Assert(DS["B3"].HostValue == typeof(int[][]));
            System.Diagnostics.Debug.Assert(DS["B1"].HostValue.GetType() == typeof(int[]));
            System.Diagnostics.Debug.Assert(DS["D"].HostValue.GetType() == typeof(double[]));

         //   code = @"((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();";
         //   Script.Execute(code, DS);
            Logger.Close();
        }
    }
}
