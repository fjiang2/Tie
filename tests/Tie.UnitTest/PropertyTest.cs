using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Diagnostics;
using System.Windows.Forms;

namespace UnitTest
{
    class BasePropertyTest
    {
        public int a;
        
        public object SelectedValue
        {
            get
            {
                return 12;
            }
        }

        public BasePropertyTest()
        {
            a = 20;
        }
    }

    class PropertyTest : ListBox
    {
        public PropertyTest()
        {
            this.v = "String";
            this.age = 18;
            this.angle = 3.1415;
        }

        string v;
        public string SelectedValue
        {
            get
            {
                return this.v;
            }
            set
            {
                this.v = value;
            }

        }

        private int age;
        public int Age
        {
            get { return this.age; }
            set { this.age = value + 1; }
        }

        private double angle;
        private double Angle
        {
            get
            {
                return this.angle;
            }
        }

        private int privateValue = 0;


        public int plus(int a, int b)
        {
            return a + b;
        }

        private string plus(string a, int b)
        {
            return a + b;
        }

        public int[] A = new int[] { 0, 1, 2, 3, 4, 5, 6 }; //由于数组会转化为TIE的List, 那么修改List就不会自动的修改.NET对象的变量
        public string[] S = new string[] { "A", "B", "C" };
        public int X99 = 99;

        public int this[int index]
        {
            get
            {
                return this.A[index];
            }
            set 
            {
                this.A[index] = value;
            }
        }

        public int[,,] AA = new int[,,] { {{1,2}, {3,4}, {5,6}}, {{7,8}, {9,10}, {11,12}} };        //2X3X2

        public int this[int i, string j, int k]
        {
            get
            {
                return AA[i, Convert.ToInt32(j), k];
            }
            set
            {
                AA[i, Convert.ToInt32(j), k ] = value;
            }
        }

        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();

            PropertyTest p = new PropertyTest();
            DS.Add("p", VAL.Boxing(p));
            
            
            string code = @"
             //属性
              p.A[0] =10000;
              p.A = p.A;

              p[1,'1',1] = 500;
              A121 = p[1,'2',1];      //==12
              p.X99 = 9999;
              
             //Field变量
              a010 = p.AA[0,1,0];    //==3
              p.AA[0,1,1] = 4444;       //这条语句不会改变.net对象的值, 因为p.AA转化为TIE的LIST, LIST中的值被修改为44,但是.net 对象没有修改, 主要是因为这个是ValueType的

              ret = p.propertyof(typeof(string), 'SelectedValue');
              p.propertyof(typeof(string), 'SelectedValue', 'Hello World');
              age = p.propertyof('Age');
              p.propertyof('Age', 20);
              angle = p.propertyof('Angle');
              p.fieldof('privateValue', 10000);
              privateValue = p.fieldof('privateValue');

              plus = p.methodof(typeof(int), 'plus', {typeof(int), typeof(int)});
              sum20 = plus(12,8);
              plus = p.methodof(typeof(string), 'plus', {typeof(string), typeof(int)});
              sum128 = plus('12',8);

              func = p.plus;
              sum50 = func(20,30);
              //sum130 = func('1',30);        //private method, you will see error
            ";

            Script.Execute(code, DS);
            Debug.Assert(DS["ret"].Str == "String");
            Debug.Assert(DS["age"].Intcon == 18);
            Debug.Assert(DS["privateValue"].Intcon == 10000);
            Debug.Assert(DS["sum20"].Intcon == 20);
            Debug.Assert(DS["sum128"].Str == "128");
            Debug.Assert(DS["sum50"].Intcon == 50);
            Debug.Assert(DS["a010"].Intcon == 3);
            Debug.Assert(DS["A121"].Intcon == 12);

            Debug.Assert(p.SelectedValue == "Hello World");
            Debug.Assert(p.Age == 21);
            Debug.Assert(p.privateValue == 10000);
            Debug.Assert(p.AA[1,1,1] == 500);

            Debug.Assert(p.A[0] == 10000);
            Debug.Assert(p.AA[0,1,1] == 4444);
            
            Logger.Close();
        }
    }
}
