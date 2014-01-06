using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{

    public enum EnumTest
    { 
        min = 1,
        max = 2
    }

    public class BaseClassTest
    {
        public int A;
        public EnumTest E;

        public BaseClassTest()
        {
            A = 10;
            E = EnumTest.min;
        }
    }

    public class DerivedClassTest : BaseClassTest 
    {
        public string S;

        public DerivedClassTest()
        {
            S = "ABC";
        }
    
    }


    public class ClassTest
    {
        public static void main()
        {

            /**
             * 
             *  function表达式和event测试
             * 
             * 
             * */
            Script.CommonMemory.Clear();
            Script script = new Script("unknown");
            script.DS = new Memory();

            string code = @"
                a=10;
                a+=20;

                plus=function(a,b)
                  { return a+b;};
                
                c[0]=plus(a,30);    
                c[1]=plus('AAA','VVV');    

                system.math.PI = 3.1415926;
                system.math.plus = function(a,b)
                  { return a+b+PI;};

                d = system.math.plus('XXX','YYY');
                
//                plus2 = system.math.plus;
//                e = plus2('CCC','DDD');

                plus3 = plus;
                f = plus3('CCC','DDD');

                circle = {}.typeof('Circle');
                circle.x = 10;
                circle.y = 20;
                circle.radius = 4;
                circle.Area = function() 
                    { 
                       return radius*radius*3.1415;
                    };
                
                circle.Circumference = function()
                    { return 2*radius*3.1415; };        

                circle.Distance = function(x1,y1)
                    { return Math.Sqrt((x1-x)*(x1-x)+(y1-y)*(y1-y)); };        


                aa[0] = circle.Area();
                cc[0] = circle.Circumference();
                dd[0] = circle.Distance(12,18);
                
                ACD =  { circle.Area(),  circle.Circumference(), circle.Distance(12,18)};
                ";
            HostType.Register(typeof(System.Math), true);
            
            script.Execute(code);
            string circle = "{{\"x\",10},{\"y\",20},{\"radius\",4},{\"Area\",$function(\"unknown\",137)},{\"Circumference\",$function(\"unknown\",157)},{\"Distance\",$function(\"unknown\",177)}}.typeof(\"Circle\")";
            System.Diagnostics.Debug.Assert(script.DS["circle"].ToString() == circle, "User Defined Class");
            System.Diagnostics.Debug.Assert(script.DS["ACD"].ToString() == "{50.264,25.132,2.82842712474619}","User Defined Class");

            string code1 = @"
                #module Shape;      //一定要放在最前面

                Circle = class(x,y, radius)
                {
                    this.x = x;
                    this.y = y;
                    this.radius = radius;
  
                    this.Area = function() 
                    { 
                        return this.radius*this.radius*3.1415;
                    };
                
                    this.Circumference = function()
                    { return 2*this.radius*3.1415; };        

                    this.Distance = function(x1,y1)
                    { return Math.Sqrt((x1-this.x)*(x1-this.x)+(y1-this.y)*(y1-this.y)); };        

                    this.Test = function(arg1) 
                    { 
                        var tb = new System.Windows.Forms.TextBox();
                        tb.Text = 'Hello World';
                        return arg1+tb.Text+this.radius;
                    };


                   //return this;
                };

           
                circle1 = new Circle(0,1,2);
                aa[0] = circle1.Area();
                cc[0] = circle1.Circumference();
                dd[0] = circle1.Distance(3,5);

                circle2 = new Circle(1,2,4);
                aa[1] = circle2.Area();
                cc[1] = circle2.Circumference();
                dd[1] = circle2.Distance(3,5);
  
            ";

            script = new Script("unknown");
            HostType.Register(typeof(System.Windows.Forms.TextBox));
            script.Execute(code1);
            //System.Diagnostics.Debug.Assert(script.DS["Circle"].Class == "Shape", "directive #module");
            System.Diagnostics.Debug.Assert(script.ModuleName == "Shape", "directive #module");

            script.VolatileExecute(@"
                circle = new Circle(10,20,4);
                ACD =  { circle.Area(),  circle.Circumference(), circle.Distance(12,18)};

                x = circle.Test('Arg1'); 
                
                loginfo('x=',x);
            ");

            System.Diagnostics.Debug.Assert(script.DS["ACD"].ToString() == "{50.264,25.132,2.82842712474619}", "User Defined Class");


            //静态属性, 静态函数测试
            HostType.Register(typeof(System.DateTime));
            HostType.Register(typeof(System.Int32));

            DateTime d1 = new DateTime(2011, 1, 1);
            DateTime d2 = new DateTime(2011, 3, 1);
            TimeSpan ts = new System.TimeSpan(10, 0, 0, 0);
            DateTime d4 = d1 + ts;
            DateTime d5 = d2 - ts;

            script.Execute(@"
            d = System.DateTime.Now;
            x = System.Int32.Parse('12');
            d1 = new DateTime(2011,1,1);
            d2 = new DateTime(2011,3,1);
            d3 = new DateTime(2011,3,1);

            ts = new System.TimeSpan(10, 0, 0, 0);
            d4 = d1+ts;
            d5 = d2-ts;

            b1 = d2 > d1;
            b2 = d2 <= d1;
            b3 = d2 == d3;
            ");


            System.Diagnostics.Debug.Assert((bool)script.DS["b1"] == true);
            System.Diagnostics.Debug.Assert((bool)script.DS["b2"] == false);
            System.Diagnostics.Debug.Assert((bool)script.DS["b3"] == true);

            System.Diagnostics.Debug.Assert(((DateTime)script.DS["d4"]).ToString() == d4.ToString());
            System.Diagnostics.Debug.Assert(((DateTime)script.DS["d5"]).ToString() == d5.ToString());
        }



       
    
    }
}
