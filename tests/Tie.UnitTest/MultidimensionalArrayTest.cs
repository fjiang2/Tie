using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Diagnostics;
using System.Windows.Forms;


namespace UnitTest
{
    class MultidimensionalArrayTest
    {


        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();

            int[,,,] A = new int[,,,] { 
                    {
                    {{1,2},   {3,4},    {5,6}},
                    {{7,8},   {9,10},   {11,12}}
                    }, 
                    {
                    {{13,14}, {15,16},  {17,18}},
                    {{19,20}, {21,22},   {23,24}}
                    }
            };

    
            
            VAL A1 = VAL.Boxing(A);
            string A2 = A1.ToString();
            VAL A3 = Script.Evaluate(A2);
            Array A4 = (Array)A1.HostValue;
            Array A5 = (Array) A3.HostValue;
            Debug.Assert(A4.Rank == 4);
            Debug.Assert(A5.Rank == 4);



            string S = @"{ 
                    {
                    {{1,2},   {3,4},    {5,6}},
                    {{7,8},   {9,10},   {11,12}}
                    }, 
                    {
                    {{13,14}, {15,16},  {17,18}},
                    {{19,20}, {21,22},   {23,24}}
                    }
                }";
            //无cast: {... }
            VAL A6 = Script.Evaluate(S);
            Array A7 = (Array)A6.HostValue;
            Debug.Assert(A7.Rank == 1);

            //cast: new int[,,,]{....}
            VAL V8 = Script.Evaluate("new int[,,,]" + S);
            Array A9 = (Array)V8.HostValue;
            Debug.Assert(A9.Rank == 4);


         //   HostType.Register("DevExpress.XtraGrid.v10.2, Version=10.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
         //   System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(@"C:\Program Files (x86)\DevExpress 2010.2\Components\Sources\DevExpress.DLL\DevExpress.XtraGrid.v10.2.dll");
            Logger.Close();
        }
    }
}
