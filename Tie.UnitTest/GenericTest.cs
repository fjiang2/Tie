using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Tie;
using System.Windows.Forms; 

namespace UnitTest
{
    class GenericTest
    {
        public GenericTest()
        { 
        }


        public string GenericMethod<T>(T a, T b)
        {
            return string.Format("{0} {1}", a, b);
        }

        public T GenericMethod1<T>(T a, T b)
        {
            return a;
        }

        public int plus(int a, int b)
        {
            return a + b;
        }


        public static void main()
        {

           // HostType.Register(typeof(Dictionary<,>));

            Type d1 = typeof(Dictionary<,>);
            Type[] typeArgs = { typeof(string), typeof(int) };
            Type constructed = d1.MakeGenericType(typeArgs);

            object dict = Activator.CreateInstance(constructed);
            MethodInfo Add = dict.GetType().GetMethod("Add"); 
            Add.Invoke(dict, new object[]{"two", 2});


            GenericTest gt = new GenericTest();
            MethodInfo method = typeof(GenericTest).GetMethod("GenericMethod");
            MethodInfo generic = method.MakeGenericMethod(new Type[]{ typeof(int) } );
            object ret = generic.Invoke(gt, new object[] { 1,2 });

            string s1 = gt.GenericMethod<int>(20, 30);
            string s2 = gt.GenericMethod(20, 30);
            string s3 = gt.GenericMethod("A", "B");

            Memory DS = new Memory();
            string code = @"
              //  a = new System.Collections.Generic.Dictionary(100,200);
                dict2 = new System.Collections.Generic.Dictionary<string,int>(20);
                dict2.Add('one',1);
                dict2.Add('two',2);
                dict2.Add('ten',10);
                list = new System.Collections.Generic.List<string>();
                list.Add('A');
                list.Add('B');
                dict3 = new System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<string>>();   //注意是> >, 为了不和>>SHR搞混.  7/1/2011起compiler自动拆分SHR为2个GTR
                dict3.Add('comp', list);    
                a = 30;
                A = a < 20;
            ";

            Tie.Logger.Open("C:\\temp\\tie.log");
            Script.Execute(code, DS);
            Dictionary<string, int> dict2 = (Dictionary<string, int>)DS["dict2"].HostValue;
            Dictionary<string, List<string>> dict3 = (Dictionary<string, List<string>>)DS["dict3"].HostValue;

            System.Diagnostics.Debug.Assert(dict2["ten"]==10);
            System.Diagnostics.Debug.Assert(dict3["comp"][1] == "B");

            code = @"
                Int1 = typeof(int[]);
                Type1 = typeof('System.Type[]');
                Type2 = typeof(System.Type[]);
                single = typeof(System.Windows.Forms.BorderStyle).FixedSingle;
                red = typeof('System.Drawing.Color').Red;
                dict = typeof(System.Collections.Generic.Dictionary<,>);
                list = typeof(System.Collections.Generic.List<>);
            ";
            
            DS.RemoveAll();
            Script.Execute(code, DS);

            Debug.Assert(DS["Int1"].value.Equals(typeof(int[])));
            Debug.Assert(DS["Type1"].value.Equals(typeof(Type[])));
            Debug.Assert(DS["Type2"].value.Equals(typeof(Type[])));
            Debug.Assert(DS["single"].value is BorderStyle);
            Debug.Assert(DS["red"].value.Equals(System.Drawing.Color.Red));
            Debug.Assert(DS["dict"].value.Equals(typeof(Dictionary<,>)));
            Debug.Assert(DS["list"].value.Equals(typeof(List<>)));
        
        }
    }
}
