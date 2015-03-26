using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class ParamsTest
    {
        public static string Format(string fmt, params int[] A)
        { 
            object[] args = new object[A.Length];
            for(int i=0;i<args.Length;i++)
                args[i]= A[i];

            return string.Format("Test:" + fmt, args);
        }

        //可变参数测试

        public static void main()
        {
            Memory DS = new Memory();
            HostType.Register(typeof(System.String));
            
            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");

            //HOST类型的可变参数不支持
            string code = @"
                s1 = System.String.Format('{0}={1}+{2}+{3}', 'A', 100, 200,300);
            ";

            //Coding.Execute(code, DS);
            //System.Diagnostics.Debug.Assert(DS["s1"].Str == "A=100+200+300");

            code= @"
                foo = function(fmt,A)
                {
                    var sum=0;
                    
                    if(size(A) >= 0 )
                    //if(type(A)==TYPE.LIST)  //TIE 方法, TYPE.LIST==64
                    {
                       foreach(var a in A)
                         sum+=a;
                    }
                    else
                       sum = A;
      
                    return fmt+sum;
                };

                
                s1= foo('Sum=');    //参数个数少一个, A={}
                s2= foo('Sum=', {1,2,3,4,5}); //参数个数相同, 不压制为数组, 原来是什么样子就是什么样子
                s3= foo('Sum=', 1,2,3,4,5); //参数个数多了, 最后1个参数压制为array
                s4= foo('Sum=', 125);   //参数个数相同: A=125, 而不是A={125}
                s5 = 'Sum='.foo(1,2,3);  
                s6 = 'Sum='.foo();  
            ";

            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert(DS["s1"].Str == "Sum=0");
            System.Diagnostics.Debug.Assert(DS["s2"].Str == "Sum=15");
            System.Diagnostics.Debug.Assert(DS["s3"].Str == "Sum=15");
            System.Diagnostics.Debug.Assert(DS["s4"].Str == "Sum=125");
            System.Diagnostics.Debug.Assert(DS["s5"].Str == "Sum=6");
            System.Diagnostics.Debug.Assert(DS["s6"].Str == "Sum=0");


            //构造overloading函数模板
            code = @"
                foo = function(L)
                {
                    len = size(L);
                    if(len == -1 )
                    {
                       L = {L};
                       len = 1;
                    }    

                    switch(len)
                    {
                      case 0:
                        return 0;

                      case 1:  
                        if(L[0] is int)
                           return L[0];
                        else if(L[0] is string)
                            return 'Hello: ' + L[0];
                        break; 
                      case 2:  
                        if(type(L[0])==TYPE.INT && type(L[1])==TYPE.INT)
                            return L[0]+L[1];
                        break;
                    }
                    
                    return null;
                };

                
                s1= foo();  
                s2= foo(1); 
                s3= foo(1,2);
                s4= foo('Me');
            ";
            DS.RemoveAll();
            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert(DS["s1"].Intcon == 0);
            System.Diagnostics.Debug.Assert(DS["s2"].Intcon == 1);
            System.Diagnostics.Debug.Assert(DS["s3"].Intcon == 3);

            string s2 = UnitTest.ParamsTest.Format("{0}+{1}+{2}",1,2,3);
            code = @"
                s1 = UnitTest.ParamsTest.Format('{0}+{1}+{2}',{1,2,3});
                s2 = UnitTest.ParamsTest.Format('{0}+{1}+{2}',1,2,3);
                s3 = string.Format('{0}+{1}+{2}',1,2,3);
            ";
            DS.RemoveAll();
            HostType.Register(typeof(ParamsTest));
            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert(DS["s1"].Str == "Test:1+2+3");
            System.Diagnostics.Debug.Assert(DS["s2"].Str == s2);
            System.Diagnostics.Debug.Assert(DS["s3"].Str == "1+2+3");
        }
    }
}
