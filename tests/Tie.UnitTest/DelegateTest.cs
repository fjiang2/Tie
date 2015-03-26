using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Tie;

namespace UnitTest
{

    class Impl
    {

        public Impl()
        { }


        public int Sum(int[] A)
        {
            int s = 0;
            foreach (int a in A)
                s += a;
            return s;
        }

        public static int Sum2(int[] A)
        {
            int s = 0;
            foreach (int a in A)
                s += a;
            return s;
        }
    }

    public delegate int Plus0000(int[] a);
    public delegate string Concat0000(string a, string b);


    class DelegateTest
    {

        public int foo(Plus0000 plus, int[] A)
        {

            return plus(A);
        }

        public string foo(Concat0000 concat, string a, string b)
        {
            return concat(a,b);
        }

        public DelegateTest()
        { 
        
        }

        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            string code;

            HostType.Register(typeof(Impl));
 
            DelegateTest test = new DelegateTest();
            Impl impl = new Impl();
            code = @"
                tsum = function(A)
                {
                    var s=0;
                    foreach(var a in A)
                      s += a;
                    
                    return s;  
                };

                concat1 = function(a,b) { return a+b;};
                concat2 = function(a,b) { return a+ '=' + b;};

                sum1 = test.foo(impl.Sum, (int[]){1,2,3,4,5});
                sum2 = test.foo(UnitTest.Impl.Sum, (int[]){1,2,3,4,5});
                sum3 = test.foo(tsum, (int[]){1,2,3,4,5,6});        //把TIE函数作为delegate
                c1 = test.foo(concat1, 'A','B');
                c2 = test.foo(concat2, 'A','B');
                sum4 = test.foo(dPlus, (int[]){1,2,3,4,5});
                sum5 = dPlus((int[]){10,20});
                sum6 = dPlus2((int[]){10,20});
            ";

            Plus0000 dPlus = delegate(int[] A)
            {
                int s = 0;
                foreach (int a in A)
                   s += a;
                return s;
            };

            int x = 300;

            Plus0000 dPlus2 = delegate(int[] A)
            {
                int s = 0;
                foreach (int a in A)
                    s += a;
                return s+x;
            };

            DS.RemoveAll();
            DS.Add("test", VAL.NewHostType(test));
            DS.Add("impl", VAL.NewHostType(impl));
            DS.Add("dPlus", VAL.NewHostType(dPlus));
            DS.Add("dPlus2", VAL.NewHostType(dPlus2));


            int x1 = dPlus2(new int[]{1, 2} );
            int x2 = (int)dPlus2.Method.Invoke(dPlus2.Target, new object[] { new int[] { 1, 2 } });

            //因为有TIE function 作为delegate,所以不能用volatile excute
            //Coding.Execute(code, DS);
            Script script = new Script("unknown");
            script.DS = DS;
            script.Execute(code);

            Debug.Assert(DS["sum1"].Intcon == 15);
            Debug.Assert(DS["sum2"].Intcon == 15);
            Debug.Assert(DS["sum3"].Intcon == 21);
            Debug.Assert(DS["sum4"].Intcon == 15);
            Debug.Assert(DS["sum5"].Intcon == 30);
            Debug.Assert(DS["sum6"].Intcon == 330);
            Debug.Assert(DS["c1"].Str == "AB");
            Debug.Assert(DS["c2"].Str == "A=B");
 
        }
    }
}
