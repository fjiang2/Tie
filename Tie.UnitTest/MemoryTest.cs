using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    class MemoryTest
    {
        public static void main()
        {
            Memory ds = new Memory();

            ds.SetValue("PI", new VAL(3.14));
            ds.SetValue("A.B.C", new VAL(100));

            VAL val = ds.GetValue("A");
            Debug.Assert(val["B"]["C"].Intcon == 100);

            val = ds.GetValue("A.B");
            Debug.Assert(val["C"].Intcon == 100);
            
            val = ds.GetValue("A.B.C");
            Debug.Assert(val.Intcon == 100);

            ds.SetValue("A.B.D", new VAL(200));
            val = ds.GetValue("A");
            Debug.Assert(val["B"]["C"].Intcon == 100);
            Debug.Assert(val["B"]["D"].Intcon == 200);

            val = ds.GetValue("A.B");
            Debug.Assert(val["C"].Intcon == 100);
            Debug.Assert(val["D"].Intcon == 200);

            ds.SetValue("A.B.E", new VAL(22));
            val = ds.GetValue("A.B");

            val = ds.GetValue("C.B");
            Debug.Assert(val.Undefined);


            ds.RemoveValue("A.B.E");
            ds.RemoveValue("A.B.D");
            ds.RemoveValue("A.B");

            ds.SetValue("A", new VAL(200));
            Debug.Assert(ds["A"].Intcon == 200);

            string code = "A.a=1; A.b=2; A.c=void; A.D.a=1; A.D.c=null;";
            Script.Execute(code, ds);
            val=ds["A"];
            Debug.Assert(val.ToString()== "{{\"a\",1},{\"b\",2},{\"c\",void},{\"D\",{{\"a\",1},{\"c\",null}}}}");
            ds.ClearNullorVoid("A");
            val = ds["A"];
            Debug.Assert(val.ToString() == "{{\"a\",1},{\"b\",2},{\"D\",{{\"a\",1}}}}");

            ds.RemoveAll();
            code = "A.B.C = {1, 2, 3}.typeof('SET');";
            //code = "A.B.C = {1, 2, 3};";
            Script.Execute(code, ds);
            //ds.SetValue("A.B.C", new VAL(10));
            ds.SetValue("A.B.C", new VAL(20));
       }
    }
}
