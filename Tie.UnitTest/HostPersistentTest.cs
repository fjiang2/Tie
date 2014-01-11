using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace UnitTest
{
    enum HostEnum
    { 
        Laptop,
        Desktop,
        Nettop
    }
    
    class HostDemoClass1
    {
        public int a { get; set; }
        public int b { get; set; }

        public HostDemoClass1()
        {
            this.a = 20;
            this.b = 30;
        }
    }

    class HostDemoClass2 : IValizable
    {
        public int a { get; set; }
        public int b { get; set; }

        public HostDemoClass2(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        //a class, which implemented IValable, must have constructor(VAL val)  
        public HostDemoClass2(VAL val)
        {
            this.a = val["a"].Intcon;
            this.b = val["b"].Intcon;
        }

        public VAL GetValData()
        { 
            VAL val = new VAL();
            val["a"] = new VAL(a);
            val["b"] = new VAL(b);
            return val;
        }
    }

    class HostDemoClass
    {
        public HostDemoClass1 class1 { get; set; }
        public HostDemoClass2 class2 { get; set; }

        public HostEnum he = HostEnum.Desktop;
        public int a { get; set; }
        
        [NonValized]  public string b;
        
        public double d;
        public DateTime e;

        public int[] Array;

        private Color color1;


        [Valizable("this.GetType().FullName + '.' + this.Name")]            //System.Drawing.Color.Red
        //[Valizable]
        public Color Color1
        {
            get
            {
                return this.color1;
            }
            set
            {
                this.color1 = value;
            }
        }

        //[Valizable("this.GetType().FullName + '.' + (this.Name=='0'?'Black':this.Name)")]            //System.Drawing.Color.Red
        public Color color2 { get; set; }

        [Valizable("this.GetType().FullName + '.' + this.Name")]            //System.Drawing.Color.Red
        public Color color3;


        [Valizable("{Text: this.Text}")]
        public TextBox textBox1 { get; set; }

        [Valizable(new string[] { "Text", "ReadOnly", "Visible", "Font" })]
        public TextBox textBox2;// { get; set; }

        [Valizable]
        public string B
        {
            get
            {
                return b;
            }
            set
            {
                this.b = value;
            }
        }

        public Rectangle rect { get; set; }
        public Guid guid { get; set; }


        public HostDemoClass()
        {
            this.class1 = new HostDemoClass1();
            this.class2 = new HostDemoClass2(100,200);
            this.a = 20;
            this.b = "AB";
            this.d = 30.0;
            this.e = DateTime.Today;
            this.Array = new int[] { 1, 2, 3, 4 };
            this.color1 = Color.AliceBlue;
            this.color2 = Color.FromArgb(1, 2, 3);
            this.color3 = Color.White;
            this.textBox1 = new TextBox();
            this.textBox1.Text = "Hello World";
            this.textBox2 = new TextBox();
            this.textBox2.Text = "Good Morning";
            this.textBox2.Font = new System.Drawing.Font("MS Gothic", 11.25F, System.Drawing.FontStyle.Bold| FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            this.rect = new Rectangle(10, 20, 30, 40);
            this.guid = Guid.NewGuid();
        }

    }
    
    class HostPersistentTest
    {


        public static void main()
        {
            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();

            var s = new UnitTest.HostDemoClass();
            VAL v= VAL.Boxing(s);
            DS.Add("v", v);
            VAL val = Script.Evaluate("v.classof()", DS);
            VAL valable = Script.Evaluate("v.valize()", DS);
            
            VAL font = val["textBox2"]["Font"];
            Debug.Assert(font.HostValue is Font && ((Font)font.HostValue).Name == "MS Gothic");

            string fontString = font.Valor;
            VAL fontStyle = font["Style"];
            Debug.Assert(fontStyle.HostValue is FontStyle && (FontStyle)fontStyle.HostValue == (FontStyle.Bold | FontStyle.Italic));

            string fontStyleString = fontStyle.Valor;
            VAL fontUnit = font["Unit"];
            Debug.Assert(fontUnit.HostValue is GraphicsUnit && (GraphicsUnit)fontUnit.HostValue == GraphicsUnit.Point);

            string fontUnitString = fontUnit.Valor;

            VAL xxx = v.GetValData();

            string persistent = v.Valor;
            string json = v.ToExJson();


            string size = VAL.Boxing(new Size(200, 300)).Valor;

            HostType.Register(typeof(Color));

            VAL p = Script.Evaluate(persistent);
            VAL j = Script.Evaluate(json);
            Debug.Assert(p.Valor == j.Valor);

            p["a"] = new VAL(1000);
            p["Color1"] = VAL.Boxing(Color.SaddleBrown);
            p["textBox1"]["Visible"] = new VAL(false);
            HostDemoClass obj = (HostDemoClass)HostType.NewInstance(p, new object[]{});
            Debug.Assert(obj is HostDemoClass);
        

            //HOST类型的可变参数不支持
            string code = @"
               textBox = (new System.Windows.Forms.TextBox()).classof({Text:'Hello World', ReadOnly:true});
            ";

           
            HostType.Register(typeof(System.String));

            DS.Clear();
            Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert(
                   DS["textBox"]["Text"].Str == "Hello World"
                && DS["textBox"]["ReadOnly"].Boolcon == true);

            Logger.Close();
            code = @"
                A=1;
                B=3;
                return {A,B};
                C=1;
            ";
            VAL result = Script.Execute(code, DS);
            System.Diagnostics.Debug.Assert(result.ToString() == "{1,3}");
            System.Diagnostics.Debug.Assert(DS["C"].ty == VALTYPE.voidcon);
            Logger.Close();
        
        }




        public static void main2()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            string code1, code2;

            code1 = @"
listBox  = new System.Windows.Forms.ListBox();
listBox.Items.Add(1234);
listBox.Items[0] = 'XXXX';
";
            Script.Execute(code1, DS);
            ListBox lb = (ListBox)DS["listBox"].value;
            object obj = lb.Items[0];
            Debug.Assert(obj.Equals("XXXX"), "Collection");


            code2 = @"
{
  Items : 
        ['red', 'green', 'white']
}
";

            DS.Clear();
            ListBox listBox1 = new ListBox();
            VAL val = Script.Evaluate(code2);
            HostType.SetObjectProperties(listBox1, val);

            
            obj = listBox1.Items[2];
            Debug.Assert(listBox1.Items[0].Equals("red") && listBox1.Items[2].Equals("white"), "用VAL值,自动增长Collection值");

            code2 = @"
{
  Items : 
        ['red', 'green', 'black', 'yellow','blue']
}
";
            val = Script.Evaluate(code2);

            string str = "";
            foreach (VAL x in val["Items"])
            {
                str += x.Str +".";
            }

            Debug.Assert(str == "red.green.black.yellow.blue.", "测试VAL实现的ICollection<VAL>");

            HostType.SetObjectProperties(listBox1, val);
            Debug.Assert(listBox1.Items[0].Equals("red") && listBox1.Items[2].Equals("black") && listBox1.Items[4].Equals("blue"), "用VAL值,修改Collection值");


            List<string> list = new List<string>();
            list.Add("A");
            list.Add("B");
            list.Add("C");
            list.Add("D");
            list.Add("E");
            list.Add("F");

            val = new VAL();
            val.Add("Items", VAL.NewHostType(list));
            HostType.SetObjectProperties(listBox1, val);
            string s = "";
            foreach (object x in listBox1.Items)
                s += x.ToString();

            Debug.Assert(s == "ABCDEF", "用Host值,修改Collection值");

            Valizer.Register<ListBox>(new string[] { "Items" });
            val = VAL.NewHostType(listBox1);
            string valor = val.Valor;
            string json = val.ToExJson();
            DS.Clear();

            
        }
    }
}
