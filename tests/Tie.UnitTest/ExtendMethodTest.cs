using System;
using System.Collections.Generic;
using System.Text;
using Tie;


namespace UnitTest
{
    class ExtendMethodTest
    {
        public static void main()
        {
            Script script = new Script("unknown");
            Memory DS1 = new Memory();
            script.DS = DS1;

            string code = @"
                sum = function(L)
                {  var sum=0;
                   foreach(l in L)
                   sum+=l;
                   return sum;
                };
               L = {1,2,3,4,5};
               a= sum(L);
               b = L.sum();
               c = {2,5,6}.sum(); 

d=['ABC','a'].sum();
t={2,3,6}.typeof();
              Sin = Math.Sin;
sin30 = Sin(3.1415/6);

dtType = DateTime;
               
today = System.DateTime.Now;
if(today.GetType() == System.DateTime)
   todayOK=true;
else
   todayOK = false;

listOK =  L.type() == TYPE.LIST;
            ";

            Tie.Logger.Open("C:\\temp\\tie.log");
            script.Execute(code);
            VAL a = DS1["a"];
            VAL t = DS1["t"];
            VAL Sin = DS1["Sin"];
            string Sinx = Sin.ToString();
            VAL dtType = DS1["dtType"];
            string dtTypex = dtType.ToString();
            System.Diagnostics.Debug.Assert(DS1["b"].Intcon == 15);
            System.Diagnostics.Debug.Assert(DS1["todayOK"].IsTrue);
        }
    }
}
