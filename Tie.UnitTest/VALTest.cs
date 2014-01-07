using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Diagnostics;

namespace UnitTest
{
	class VALTest
	{
        public VALTest()
        { }

        public int sum(int[] A)
        {
            int s=0;
            foreach (int a in A)
                s += a;
            return s;
        }


        public static void main()
        {

            VAL json = new VAL();
            json["A"] = new VAL(1);
            json["B"] = new VAL(2);
            json.Add("C", 3);
            Debug.Assert(json["C"].Intcon == 3);
            Debug.Assert(json.Count == 3);
            json.Remove("B");
            Debug.Assert(json.Count == 2);

            string code = @"
SET='SET';
a.b.c.d1=12;
a.b.c.d2[1]='A';
a.b.c.d2[2].e='B';
A={1,2,3,4}.typeof(SET);
B={1,3,5,6}.typeof(SET).push('A');
C= A+B;
ty = A.typeof();
  

  var parameters;
  parameters.Zero  = '0000'; 
  form.Entry = parameters;

word = 'Help';
word5 = '<' + word*5 + '>';


E={10,11,12,13,14,15};
E3 = E.pop(3);

sum = VALTest.sum(E);

i=12;
byte = i.ctype(System.Byte);
E4 = E.ctype(ObjectArray);
    ";

            HostType.Register(typeof(Byte));
            HostType.Register("ObjectArray", typeof(System.Object[]));
            HostType.Register(typeof(string));

            Tie.Logger.Open("C:\\temp\\tie.log");
            Memory DS1 = new Memory();

            DS1.Add("VALTest", VAL.NewHostType(new VALTest()));
            Script.Execute(code, DS1);

            System.Diagnostics.Debug.Assert(DS1["a"].ToString() == "{{\"b\",{{\"c\",{{\"d1\",12},{\"d2\",{null,\"A\",{{\"e\",\"B\"}}}}}}}}}", "VAL");
            VAL A = DS1["A"];
            VAL ty = DS1["ty"];
            System.Diagnostics.Debug.Assert(DS1["form"].ToString() == "{{\"Entry\",{{\"Zero\",\"0000\"}}}}", "VAL 测试局部变量初始化为null");

            VAL E = DS1["E"];
            object host = E.HostValue;
            System.Diagnostics.Debug.Assert(DS1["sum"].Intcon == 62, "自动cast成为C#的整形数组");

            object E4 = DS1["E4"].HostValue;
            System.Diagnostics.Debug.Assert(E4 is object[], "CAST object[]测试");


            Tie.Logger.Open("C:\\temp\\tie.log");
            string THIS="States.S2";
            Memory memory = new Memory();
            Script.Evaluate(THIS, "this.from(base.S1) && base.S1.Completed)", memory, new WorkflowFunction());
            VAL nodes = Script.Evaluate("States", memory);
            System.Diagnostics.Debug.Assert(nodes.ToExJson() =="{\r\n  S2 : null,\r\n  S1 : {\r\n    Completed : void\r\n  }\r\n}", "void测试");

            code = @"
                addreference(Assembly.Load('Tie.UnitTest'));
                //import('Tie.UnitTest');
                A = typeof(UnitTest.VALTest).plus(20,30);
                register(typeof(UnitTest.VALTest));
                B = UnitTest.VALTest.plus(30,40);
                
                C1 = typeof(""UnitTest.MyColor"").red;
                C2 = typeof(UnitTest.MyColor).black;
                import(UnitTest);
                C3 = typeof(MyColor).black;

                addreference(Assembly.Load('System.Data, PublicKeyToken=B77A5C561934E089, Culture=neutral, Version=2.0.0.0'));
                Data1 = typeof(System.Data.DataTable);
                import(""System.Data"");
                import(System.Data);
                import(System.Data.SqlClient);
                Data2 = typeof(DataTable);
                data = new Data2();

                import('SqlNs',System.Data.SqlClient);
                cmd = new SqlNs.SqlCommand();
";
            memory = new Memory();
            Script.Execute(code, memory);
            Debug.Assert(memory["A"].Intcon == 50);
            Debug.Assert(memory["B"].Intcon == 70);
            Debug.Assert((MyColor)(memory["C1"].HostValue) == MyColor.red);
            Debug.Assert((MyColor)(memory["C2"].HostValue) == MyColor.black);
            Debug.Assert((MyColor)(memory["C3"].HostValue) == MyColor.black);

            Debug.Assert((Type)(memory["Data1"].HostValue) == typeof(System.Data.DataTable));
            Debug.Assert((Type)(memory["Data2"].HostValue) == typeof(System.Data.DataTable));
            Debug.Assert(memory["data"].HostValue is System.Data.DataTable);
            Debug.Assert(memory["cmd"].HostValue is System.Data.SqlClient.SqlCommand);

            //VAL operator overloading
            VAL x = new VAL(12);                     //等价于:  VAL x= new VAL(12);
            int a = (int)memory["A"];       //等价于:  int a = memory["A"].Intcon;
        }


        public static int plus(int a, int b)
        {
            return a + b;
        }
	}

    public enum MyColor { red, green, black };


    class WorkflowFunction : IUserDefinedFunction
    {
        public WorkflowFunction()
        { }

        public VAL Function(string func, VAL parameters, Memory DS)
        {

            VAL V1, V2;
            switch (func)
            {
                case "from":
                    if (parameters.Size == 2)
                    {
                        if (parameters[0].IsNull)
                            return new VAL(false);      //use for building transitions

                        V1 = parameters[0]["PS"];
                        V2 = parameters[1];
                        if (V1.Size > 0 && V1[0] == V2)
                            return new VAL(true);
                        else
                            return new VAL(false);
                    }
                    break;
            }

            return null;
        }
    }

}
