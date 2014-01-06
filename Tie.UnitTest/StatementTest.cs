using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Diagnostics;

namespace UnitTest
{
    class StatementTest
    {
        public static void main()
        {
            Script.CommonMemory.Clear();
            Script script = new Script("UnitTest", 500);
            Memory DS2 = new Memory();
            script.DS = DS2;
            string code = "";

            code = @"
            a=1;
            if(a==1 && b==10) c=10; else c=20;
            if(a==1) d=10; else c=20;
            ";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["c"].Intcon == 20, "statement: if(..).. else ..");
            Debug.Assert(script.DS["d"].Intcon == 10, "statement: if(..).. else ..");
            script.DS.Clear();


            code = @"
            sum=0;
            for(i=0;i<=100;i++)
                sum+=i;
            ";

    
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 5050, "statement: for loop");
            Debug.Assert(script.DS["i"].Intcon == 101, "statement: for loop");
            script.DS.Clear();


            code = @"
            sum=0;
            for(var i=0;i<=100;i++)
                sum+=i;
            ";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 5050, "statement: for loop");
            Debug.Assert(script.DS["i"].Undefined, "statement: for loop");
            script.DS.Clear();



            code = @"
            sum=0;
            i=0;
            while(i<=10)
                sum+=i++;
            ";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 55, "statement: while loop");
            script.DS.Clear();

            code = @"
            sum=0;
            i=0;
            do 
            {
              sum+=i++;
            }
            while(i<=10);";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 55, "statement: do while");
            script.DS.Clear();

            code = @"
            sum=0;
            L={1,2,3,4,5,6,7,8,9,10};
            foreach(l in L) 
            {
               var s1 = 'string';
               sum+=l;
               var s2 = 'str';
            }
            ";

            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 55, "statement: foreach");
            script.DS.Clear();

            string[] array = {"a", "b","c"};
            code = @"
            sum='';
            foreach(var l in L) 
               sum+=l;
            ";

            script.DS.AddHostObject("L", array);
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Str == "abc", "statement: foreach");
            script.DS.Clear();


            code = @"
            switch(ty)
            {
              case 1: sum=100; break;
              case 10: sum=101; break;
              default: sum=102; break;
            };
            ";
            script.DS["ty"] = new VAL(10);
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 101, "statement: switch");

            script.DS["ty"] = new VAL("A");
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 102, "statement: switch");
            script.DS.Clear();


            code = @"
            L={1,2,3,4,5,6,7,8,9,10};
            plus = function(L) 
            {
              var sum=0;
              foreach(l in L)
                 sum+=l;
              
              return sum;
            };
            
            sum= plus(L);  
            ";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 55, "function:");
            script.DS.Clear();


            code = @"
            L={0,1,2,3,4,5,6,7,8,9,10};
            plus = function(L) 
            {
              var sum=0;
              foreach(l in L)
                 sum+=l;
              
              return sum;
            };
            
            sum= plus(L);  
            sum1= plus({1,2,3});  

            merge = function (func, L, A)
            {
               return func(L)+A;
            };
            
             sum= merge(plus, {1,2,3,4,5}, 100);  
            ";


            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 115, "high-level function:");
            script.DS.Clear();


            code = @"
             AddObject = DS.AddObject;
             if(AddObject!=void)
                AddObject(VAR('VAR'),VAL(20));

             memoryAdd = function (func, VAR, VAL)
             {
                var v = new Tie.VAR('@'+VAR);
                return func(v, VAL);
             };
        
            if(AddObject!=void)             
               memoryAdd(AddObject, 'VAR1', 'VAL1'); 

            ";

            Memory DS = new Memory();
            script.DS.AddHostObject("DS", DS);
            script.VolatileExecute(code);
            Debug.Assert(DS["VAR"].Intcon == 20, "HostType function pointer");
            Debug.Assert(DS["@VAR1"].Str == "VAL1", "HostType function pointer");
            script.DS.Clear();


            code = @"
             function plus(a,b)
             {
                return a+b;
             }
             
             sum = plus(20,130); 
            ";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["sum"].Intcon == 150, "function");
            script.DS.Clear();

            HostType.Register(typeof(IFormattable));
           // HostType.Register(typeof(System.Collections.IEnumerable));  //这个可以不注册,因为is指令知道后面跟的一定是Type
            HostType.Register(typeof(List<>));    //GNRC指令会自动搜索Type,所以可以不注册
            List<string> list = new List<string>();

            script.DS.AddHostObject("list", list);
            code = @"
             today = System.DateTime.Now;
             b1 = GetType(today)==System.DateTime;       
             b2 = today is System.DateTime; 
             b3 = today is System.IFormattable;
             b4 = list is System.Collections.IEnumerable;
             b5 = list is System.Collections.Generic.List<string>;
            ";

            script.VolatileExecute(code);
            Debug.Assert(script.DS["b1"] == script.DS["b2"], "exp1 is type");
            Debug.Assert(script.DS["b3"].Boolcon == true);
            Debug.Assert(script.DS["b4"].Boolcon == true);
            Debug.Assert(script.DS["b5"].Boolcon == true);

            script.DS.Clear();

            code = @"
             L = {10, 20, 30};
             if( 20 < L) b1 =1; else b1 =2;       //跟下面的语句是等价的
             if( 20 in L) b2 =1; else b2 =2;
            ";
            script.VolatileExecute(code);
            Debug.Assert(script.DS["b1"] == script.DS["b2"], "element in List");
            script.DS.Clear();
            

            code = @"
             Format = function(prefix, fmt, args)
             { 
                return prefix + System.String.Format(fmt,args);
             };
             stringFormat = Format(':::', '{0}={1}.{2}:{3}','A',20,30,'B');

             f1 = function(args)
             { 
                return args;
             };
             a = 12+f1(1,2,3,4,5);

            if(f1(1,2)) a=1; else a=2;
";
            
            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            HostType.Register(typeof(String));
            script.Execute(code);
            Debug.Assert(script.DS["stringFormat"].Str == ":::A=20.30:B", "unbounded arguments");
            VAL stringFormat = script.InvokeFunction("Format", new object[] { ":::", "{0}={1}.{2}:{3}", "A", 20, 30, "B" });
            Debug.Assert(script.DS["stringFormat"] == stringFormat, "variable arguments");
            script.DS.Clear();
            script.RemoveModule(); 

            //Logger.Close();
            //Logger.Open("c:\\temp\\tie.log");
        }
    }
}
