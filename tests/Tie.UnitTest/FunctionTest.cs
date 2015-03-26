using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Tie;

namespace UnitTest
{

    public static class ExtendTest
    {
        public static double Area(double radius)
        {
            return 3.0 * radius * radius;
        }

        public static double Area(double radius1, double radius2)
        {
            return 3.0 * radius1 * radius1 - 3.0 * radius2 * radius2;
        }
    
    }
    
    class FunctionTest
    {
        public int PLUS(int a, int b)
        {
            return a + b;
        }

        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            string code;

            HostType.Register(typeof(UnitTest.ExtendTest));
            code = @"
            //register(typeof('UnitTest.ExtendTest'));
            r = 4.0;

            f1 = UnitTest.ExtendTest.Area;
            A1 = f1(r);
            A2 = Area(r);
            A3 = r.Area();
            A4 = r.f1();
            A5 = UnitTest.ExtendTest.Area(r);
            A6 = r.(UnitTest.ExtendTest.Area)();
            A7 = r.Area(3.0);

            L={0,1,2,3,4,5,6,7,8,9,10};
         
            foo = function (func, A, B)
            {
               return func(A,B)+100;
            };
            
             sum = foo(coo.PLUS, 1,2 );   //可以把C#的函数当指针传进去

            A=20;
            sum1 = coo.PLUS(A,40);
            sum2 = A.(coo.PLUS)(40);        //对C# HostType函数,不能用extend method,因为coo已经在ES中了, 6/16/2011, 已经实现


       
            ";

            DS.RemoveAll();
            FunctionTest coo = new FunctionTest();
            DS.Add("coo", VAL.NewHostType(coo));
            Script.Execute(code, DS);
            Debug.Assert(DS["A1"].Doublecon == 48.0);
            Debug.Assert(DS["A2"].Doublecon == 48.0);
            Debug.Assert(DS["A3"].Doublecon == 48.0);
            Debug.Assert(DS["A4"].Doublecon == 48.0);
            Debug.Assert(DS["A5"].Doublecon == 48.0);
            Debug.Assert(DS["A6"].Doublecon == 48.0);
            Debug.Assert(DS["A7"].Doublecon == 21.0);
            
            Debug.Assert(DS["sum"].Intcon == 103);
            Debug.Assert(DS["sum1"].Intcon == 60);
            Debug.Assert(DS["sum2"].Intcon == 60);

            code = @"
            L={0,1,2,3,4,5,6,7,8,9,10};
            A.plus = function(L)        //可以把函数赋值给一个复合变量
            {
              var sum=0;
              foreach(l in L)   sum+=l;
//for(i=0; i<size(L); i++) sum+=L[i];
              
              return sum;
            };

            sum1= A.plus(L);    
            sum2= A.plus({1,2,3});  
            
            plus = A.plus;      //不能对一个复合变量的函数,用extend method
            sum4 = L.plus();
            
            //sum5 = (L.plus)();  //错误:这个语法是cast,不是调用函数
            sum6 = L.(A.plus)();
            sum7 = L.((A).(plus))();
            ";

            DS.RemoveAll();
            Script.Execute(code, DS);
            Debug.Assert(DS["sum1"].Intcon == 55);
            Debug.Assert(DS["sum2"].Intcon == 6);
            Debug.Assert(DS["sum6"].Intcon == 55);
            Debug.Assert(DS["sum7"].Intcon == 55);

        
            code = @"
            L={0,1,2,3,4,5,6,7,8,9,10};
            plus = function(L) 
            {
              var sum=0;
              foreach(l in L)
                 sum+=l;
              
              return sum;
            };
            
            sum1= plus(L);  
            sum2= plus({1,2,3});  

            merge = function (L, func, A)
            {
               return func(L)+A;
            };
            
             sum = merge({1,2,3,4,5}, plus, 100);   //A类函数: user function  
             count  = merge({1,2,3,4,5}, 'size', 100);   //C类函数: system function, size 是TIE的系统函数,用字符串传入,相当于size({1,2,3,4,5}+100
            ";

            DS.RemoveAll();
            Script.Execute(code, DS);
            Debug.Assert(DS["sum"].Intcon == 115, "high-level function:");
            Debug.Assert(DS["count"].Intcon == 105, "high-level function:");

            //把语句当成表达式的方法
            code = @"
               sum1 = 100 + function(a,b) { return a+b; } (20,30) +300; 
               plus = function(a,b) { return a+b; }; 
               sum2 = plus(20,30);

               vec1 = function(vec)
               {
                  for(var i=0; i< vec.size(); i++)
                     vec[i] = 3* vec[i];
                  
                  return vec;
               }
               ({2,4,6}); 


                //下面代码,不能正确的工作,以为TIE传值
               vec2 = function(vec)
               {
                  var L={};
                  foreach(x in vec)
                    L.append(3*x);
                  
                  return L;
               }
               ({2,4,6}); 


              //the expression (car (cons x y)) evaluates to x, and (cdr (cons x y)) evaluates to y.
               x = 10; 
               y = {20,30,40};
               cons = y.insert(0,x); 
               car = cons[0];
               cdr = cons.slice(1,-1); 
              ";
          

            DS.RemoveAll();
            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert(DS["sum1"].Intcon == 450);
            System.Diagnostics.Debug.Assert(DS["sum2"].Intcon == 50);
            System.Diagnostics.Debug.Assert(DS["vec1"].Valor == "{6,12,18}");
            System.Diagnostics.Debug.Assert(DS["vec2"].Valor == "{6,12,18}");


            VAL i6 = Script.InvokeFunction(DS, new VAL(), "function(a,b,c) { return a+b+c;}", new object[] { 1, 2, 3 }, null);
            System.Diagnostics.Debug.Assert(i6.Intcon == 6);

            VAL i7 = Script.InvokeFunction(DS, new VAL(), "function(a,b,c) { return a+b+c;}", new object[] { "A", 2, 3 }, null);
            System.Diagnostics.Debug.Assert(i7.Str == "A23");

            Logger.Close();
        }
    }
}
