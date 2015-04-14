using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Windows.Forms;
using System.Reflection;

namespace UnitTest
{
    class SubclassTest
    {
        public static void main()
        {

            string code = @"
MyForm1 = class(text)
{

   this.$base = new System.Windows.Forms.Form();
   this.Text = text;
   button = new System.Windows.Forms.Button();  
   button.Text = 'OK';
   button.Location = new System.Drawing.Point(60, 80);
   this.Controls.Add(button);
 
   this.MyProp1 = 'Hello';
};

//form = new MyForm1('my title');
//form.Show();

MyForm2 = class()
{
   this.$base = new MyForm1('2 level subclass');
   button = new System.Windows.Forms.Button();  
   button.Text = 'Cancel';
   button.Location = new System.Drawing.Point(60, 120);
   this.Controls.Add(button);
   
   this.MyProp2 = 20;
};

form = new MyForm2();
form.MyProp1 = 100;
//form.Show();
";
            Tie.Logger.Close();
            Tie.Logger.Open("C:\\temp\\tie.log");
            Memory DS2 = new Memory();
            Script.Execute(code, DS2);

            var _form = DS2["form"];
            System.Diagnostics.Debug.Assert(_form.ToString() ==
                "{{\"$base\",{{\"$base\",new System.Windows.Forms.Form()},{\"MyProp1\",100}}},{\"MyProp2\",20}}", "Subclass");

            VAL form = DS2["form"];
            form["MyProp1"] = new VAL("ready");

            System.Diagnostics.Debug.Assert(DS2["form"].ToString() ==
                "{{\"$base\",{{\"$base\",new System.Windows.Forms.Form()},{\"MyProp1\",\"ready\"}}},{\"MyProp2\",20}}", "Subclass");


            code = @"
MyForm1 = class(text):System.Windows.Forms.Form()
{
   this.Text = text;
   button = new System.Windows.Forms.Button();      //对于第一次出现的变量, 如果写成this.variable那么就是属性,如果不写的话,就是global变量
   button.Text = 'OK';                              //后面的语句写不写this不要紧,因为优先到当前的class中寻找
   button.Location = new System.Drawing.Point(60, 80);
   this.Controls.Add(button);
 
   this.MyProp1 = 'Hello';
   this.List = function(a,b)
   { return {a,b}; 
   };
};

//form = new MyForm1('my title');
//form.Show();

MyForm2 = class():MyForm1('2 level subclass')
{
   button = new System.Windows.Forms.Button();  
   button.Text = 'Cancel';
   button.Location = new System.Drawing.Point(60, 120);
   this.Controls.Add(button);
   
   this.MyProp2 = 20;
};

form = new MyForm2();
form.MyProp1 = 100;
form.MyProp2 = 200;


MyForm3 = class():MyForm1('3 level subclass')
{
   button = new System.Windows.Forms.Button();  
   button.Text = 'Cancel';
   button.Location = new System.Drawing.Point(60, 120);
   this.Controls.Add(button);
   
   this.MyProp2 = 20;
   
   this.plus = function(a,b,c) 
   { 
     //this.add=function(a,b){ return a+b;}; 
     return a+b+c;
    };
   
};

form3 = new MyForm3();
form3.MyProp1 = 'A';
form3.MyProp2 = 'B';

";


            Tie.Logger.Open("C:\\temp\\tie.log");
            Script script = new Script("unknown");
            Memory DS1 = new Memory();
            script.DS = DS1;
            script.Execute(code);

            System.Diagnostics.Debug.Assert(DS1["form"].ToString() ==
                "{{\"$base\",{{\"$base\",new System.Windows.Forms.Form()},{\"MyProp1\",100},{\"List\",$function(\"unknown\",83)}}},{\"MyProp2\",200}}",
                "Subclass");

            System.Diagnostics.Debug.Assert(DS1["form3"].ToString() ==
                "{{\"$base\",{{\"$base\",new System.Windows.Forms.Form()},{\"MyProp1\",\"A\"},{\"List\",$function(\"unknown\",83)}}},{\"MyProp2\",\"B\"},{\"plus\",$function(\"unknown\",251)}}",
                "Subclass same base class");

            form = DS1["form"];
            VAL prop1 = form["MyProp1"];
            VAL prop2 = form["MyProp2"];

            form["MyProp1"] = new VAL("ready");
            form["MyProp2"] = new VAL(-1);

            System.Diagnostics.Debug.Assert(DS1["form"].ToString() ==
                "{{\"$base\",{{\"$base\",new System.Windows.Forms.Form()},{\"MyProp1\",\"ready\"},{\"List\",$function(\"unknown\",83)}}},{\"MyProp2\",-1}}",
                "Subclass");

            VAL clss = DS1["MyForm3"];
            VAL Clss = Script.Evaluate(clss.ToString());
            VAL func = DS1["form3"]["plus"];
            VAL Func = Script.Evaluate(func.ToString());

            VAL instance = script.CreateInstance("MyForm3", new object[]{});
            VAL plus = script.InvokeMethod(instance, "plus", new object[] {1, 2, 3});
            VAL list = script.InvokeMethod(instance, "List", new object[] {10, 21});
            System.Diagnostics.Debug.Assert(plus.ToString() == "6", "method");
            System.Diagnostics.Debug.Assert(list.ToString() == "{10,21}", "method");

            VAL form_title = instance["Text"];

            string s = instance.ToString();
            VAL s1 = Script.Evaluate(s, script.DS);


            VAL v1 = script.InvokeChainedFunction("typeof", new object[] { new object[] { 1, 2, 3, 4 }, "SET"});

            VAL L = VAL.Boxing(new object[] { 1, 2, 3, 4, 5 });
            VAL v2 = script.InvokeChainedFunction("typeof", new object[] {L, "SET"});
            VAL v3 = script.InvokeChainedFunction("typeof", new object[] {L});
            System.Diagnostics.Debug.Assert(v3 == new VAL("SET"));


            SubclassTest sub = new SubclassTest();
            object sum = Script.InvokeHostMethod(sub, "sum",new object[] { 1, 2} );


            //--------------------------------------------
            VAL MyForm3 = DS1["MyForm3"];
            VAL myForm3 = Script.CreateInstance(DS1,  MyForm3, new object[]{});
            VAL method = myForm3["plus"];
            VAL sum123 = Script.InvokeFunction(DS1, myForm3, method, new object[] { 1, 2, 3 });
            System.Diagnostics.Debug.Assert(sum123.Intcon  == 6, "method");

            object d = Script.InvokeHostMethod(typeof(SubclassTest), "Sum", new object[] {20,30});
            System.Diagnostics.Debug.Assert((int)d == 50, "static host method");
        }

        public SubclassTest()
        { }

        private int sum(int a, int b) { return a + b; }
        public static int Sum(int a, int b) { return a+b;}

    }
}

