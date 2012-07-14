using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class TryCatchTest
    {
        public static void main()
        {
            Memory DS1 = new Memory();


            string code = @"
//throw new System.Exception('test my Exception in TIE');
try
{
  i=1;
  //throw new System.Exception('test my Exception in TIE');
  k=20;
}
finally
{
 i=10; 
}
";

          //  Tie.Logger.Open("C:\\temp\\tie.log");
            DS1 = new Memory();
            Script.Execute(code, DS1);
            System.Diagnostics.Debug.Assert(DS1["i"].Intcon == 10);


            code = @"
try
{
  i=1;
  throw new System.Exception('Exception1');
  k=20;
}
catch(e)
{
  exception = e.Message; 
}
catch(e2)       //第二个catch会被忽略掉
{
  exception = e2.Message; 
}
finally
{
 i=10; 
}
";
             Tie.Logger.Open("C:\\temp\\tie.log");
            DS1 = new Memory();
            Script.Execute(code, DS1);
            System.Diagnostics.Debug.Assert(DS1["k"].Undefined);
            System.Diagnostics.Debug.Assert(DS1["exception"].Str == "Exception1");

code=@"
try
{
  try {
     i=1;
     throw new System.Exception('test my Exception in TIE');
  } catch(e)
  {
    e1 = e.Message; 
  }

  k=20;
}
catch(e2)
{
  k=30;
  e2 = e.Message; 
}
finally
{
 i=10; 
}
";

            //Tie.Logger.Open("C:\\temp\\tie.log");
            DS1 = new Memory();
            Script.Execute(code, DS1);
            System.Diagnostics.Debug.Assert(DS1["k"].Intcon == 20);


//            System.Diagnostics.Debug.Assert(DS1["a"].ToString() == "{{\"b\",{{\"c\",{{\"d1\",12},{\"d2\",{null,\"A\",{{\"e\",\"B\"}}}}}}}}}", "VAL");
        }
    }
}
