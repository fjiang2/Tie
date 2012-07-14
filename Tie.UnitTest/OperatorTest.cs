using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    class OperatorTest
    {
        double a;
        double b;

        public OperatorTest(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public static bool operator >(OperatorTest v1, OperatorTest v2)
        {
            return (v1.a * v1.a + v1.b * v1.b) - (v2.a * v2.a + v2.b * v2.b) > 0;
        }

        public static bool operator <(OperatorTest v1, OperatorTest v2)
        {
            return v2 > v1;
        }

        public static bool operator ==(OperatorTest v1, OperatorTest v2)
        {
            return v1.a == v2.a && v1.b == v2.b;
        }

        public static bool operator !=(OperatorTest v1, OperatorTest v2)
        {
            return !(v1 == v2);
        }


        public static explicit operator string(OperatorTest v)
        {
            return string.Format("{0}+{1}i", v.a, v.b);
        }

        public static VAL foo(VAL val)
        {
            return val + new VAL(120);
        }

        public static void main()
        {
            OperatorTest v1 = new OperatorTest(1, 2);
            OperatorTest v2 = new OperatorTest(3, 4);

            VAL x = foo( new VAL(20F));
            string s1 = (string)v1;

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            DS.AddHostObject("v1", v1);
            DS.AddHostObject("v2", v2);
            string code;
            code = @"
a = +1+3;
b1 = v1 > v2;;
b2 = v2 > v1;

b3 = v1 < v2;
b4 = v2 < v1;

b5 = v1 == v2;
b6 = v1 != v2;

s1 = (string)v1;

b7 = !b6;
b8 = !(12 < 3.1);

//x = Math.Sin(30);
";
            //源代码混乱在ASM列表中,因为遇到<, 有回溯代码,目前还没有修正好
            FunctionTest coo = new FunctionTest();
            Script.Execute(code, DS);
            Debug.Assert((bool)DS["b1"] == false);
            Debug.Assert((bool)DS["b2"] == true);
            Debug.Assert((bool)DS["b3"] == true);
            Debug.Assert((bool)DS["b4"] == false);
            Debug.Assert((bool)DS["b5"] == false);
            Debug.Assert((bool)DS["b6"] == true);
            Debug.Assert((bool)DS["b7"] == false);
            Debug.Assert((bool)DS["b8"] == true);
            
            Debug.Assert(DS["s1"].Str  == s1);

            int i1 = 20;
            uint u1 = 40;
            long i2 = i1 + u1;
            long l1 = 20;
            ulong l2 = 30;
            decimal d = 30.0M;

            int xd = (int)(20 - (double)d);

            VAL val = new VAL(30.3);
            VAL nil = new VAL();

            ulong ul = (ulong)val;
            //DateTime dt = (DateTime)val;

            DBNull dbnull = (DBNull)nil;
            nil = VAL.Boxing(DBNull.Value);

            Debug.Assert(nil.ty == VALTYPE.nullcon && nil.value == typeof(DBNull));
        }
    }
}
