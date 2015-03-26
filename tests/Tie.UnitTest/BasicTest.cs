using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Windows.Forms;
using System.Diagnostics;

namespace UnitTest
{
    class BasicTest
    {
        public BasicTest()
        {}

        public void DataPrint(string[] header, string[] footer)
        {
            Console.WriteLine(header);
            if (footer != null)
                Console.WriteLine(footer);
        }

        public void DataPrint(string[] header, int[] footer)
        {
            Console.WriteLine(header);
            if (footer != null)
                Console.WriteLine(footer);
        }

        public void DataPrint(string[] header)
        {
            Console.WriteLine(header);
        }

        public static void main()
        {

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Script.CommonMemory.RemoveAll();
            HostType.Register(typeof(AnchorStyles));

            TextBox textBox = new TextBox();

            Form form = new Form();
            form.Size = new System.Drawing.Size(40, 60);
            form.Dock = DockStyle.Fill;
            Memory DS = new Memory();
            DS.Add("textBox", VAL.Boxing(textBox));
            
            Script.Execute("", @"
            {


//form = new System.Windows.Forms.Form();
//form.Size = new System.Drawing.Size(40,60);
//form.Dock = DockStyle.Fill;

              System1.Math.x='xxx'; System1.Math.y='yyy'; 
              #scope System1.Math; 
              write(30);
              a= {32.0, this.x, true}.typeof('SIN'); 
              b=this.write(15)[0].x.write(16)+{1}; 
              c='System'; 
              d=(*c).Math.y+2000;
              //this=100;
              #scope AAA.BBB.CCC;
              this.x = 'xxx';
              base.Draw=1000;
              z1={1,2,3,4,5}[2];
              z2={1,2,3,4,5};
              z3=z2.echo()[0].echo(10,11);
              //d[1].x = {z1,z2};

              textBox.Anchor  =   
                          System.Windows.Forms.AnchorStyles.Top 
                        | System.Windows.Forms.AnchorStyles.Bottom
                        | System.Windows.Forms.AnchorStyles.Left
                        | System.Windows.Forms.AnchorStyles.Right;
             }

              test = new UnitTest.BasicTest();
                
              test.DataPrint({'A','B'}, null.ctype('System.String[]'));
              test.DataPrint({'A','B'}, {}.ctype(typeof('System.Int32[]')));
              test.DataPrint({'A','B'}, {null}.typeof('System.String[]'));
              test.DataPrint({'A','B'}, {}.typeof('System.Int32[]'));
             ", DS);

            
            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");

            Console.WriteLine("a=" + DS["a"]);
            Console.WriteLine("b=" + DS["b"]);


            System.Diagnostics.Debug.Assert(DS["a"].ToString() == "{32.0,\"xxx\",true}.typeof(\"SIN\")", "#scope");
            System.Diagnostics.Debug.Assert(DS["b"].ToString() == "{\"xxx\",16,1}", "#scope");

            Script.Execute("G", "{a.b=1; a.b+={32};}", DS);


            string AssessmentPlace =
                   "AssessmentPlace ={"
                   + "{ \"Child home\",                   1},"
                   + "{ \"Child Foster Care setting\",    2},"
                   + "{ \"Friend/Relative home\",         3},"
                   + "{ \"School\",                       4},"
                   + "{ \"Child care setting\",           5},"
                   + "{ \"ICF/MR;Hospital\",              6},"
                   + "{ \"Nursing Facility\",             7},"
                   + "{ \"Non-Certified Boarding Care\",  8},"
                   + "{ \"Other:___\",                    9}"
                   + "}";

            VAL v;


            DS.Add("a", new VAL(100));
            // Computer.DS.Add("b", new VAL(100));

            v = Script.Execute("My.money=100;", DS);


            v = Script.Execute("{Mike={{\"street\", \"3650 Street\"},{\"zip\", 70809*2},{\"phone\",\"225-341-7488\"},{\"apt\",2213}};"
                + " write(\"Mike.street=\",Mike.street);"
                + " Mike.street=\"new street\";  write(\"Mike=\",Mike); write(\"Mike[street]=\",Mike[\"street\"]);"
                + " Mike.money=100;"
                + "}", DS);

            v = Script.Evaluate("{{\"street\", \"3650 Street\"},{\"zip\", 70809}}");

            v["street"] = new VAL("Street");
            v["email"] = new VAL("fjiang@xxxx.com");


            v = Script.Evaluate("");
            v = Script.Evaluate("{\"Home Depot\",\"Office Max\",\"Staples\"}");
            v = Script.Evaluate("{1}");
            bool b = new VAL(0) < v;
            b = new VAL("Home Depot") < new VAL();

            v = Script.Evaluate(AssessmentPlace);
            Console.WriteLine(v);

            v = Script.Evaluate("a={40,30},30<a");
            Debug.Assert(v.Boolcon == true);
            Console.WriteLine(v);

            Script.Execute("{var a=3; if(a>10) b=3; else b=5; write(\"inside b=\",b);}", DS);
            Console.WriteLine("a={0}", DS["a"][0]);
            Console.WriteLine("b={0}", DS["b"]);
            Console.WriteLine("AssessmentPlace[2][1]={0}", DS["AssessmentPlace"][2][1]);

            v = Script.Evaluate("AssessmentPlace[-1]");
            Console.WriteLine("AssessmentPlace[-1]={0}", v);

            String x = v.ToString();
            Console.WriteLine("encode AssessmentPlace[-1]={0}", x);


            Script.CommonMemory.RemoveAll();


            VAL dt = Script.Evaluate("write(DateTime(2000,12,31,6,59,59))");
            VAL dt2 = Script.Evaluate("write(DateTime(2000,12,31,6,59,59))");

            Tie.Logger.Open("C:\\temp\\tie.log");
            VAL A = Script.Execute("{ A=3; A[0][0]=null; A[1][0]=2; A[0][1]=3; A[1][1]=4;A[5]=-5+12; A[4].City = \"XXX\"; A[4].Age=3; B={2,3}; write(A); }", DS);
            VAL B = Script.Execute("{ A={10,20,30}; B=null; write(A,-A, A+B, A*B, A-B); }", DS);

            A = Script.Execute("{A={{\"State\",{\"Ohio\"}},\"A\"}; write(A.State); write(A[-1]);}", DS);

            Console.WriteLine("Date = {0}", dt);
            Script.Execute("{var i; write(\"xx\",\"yy\");}" ,DS);
            Script.Execute(" {var i; for(i=0; i<3; i++) write(\"key\"+i,\"message\");}", DS);

            A = Script.Execute("{ X[\"City\"]=\"BTR\"; write(X[\"City\"]);}" ,DS);

            Script.Execute("G", "{i=1; A[i][3]=2; this.a=10; this.a = this.a+30;   $bx=$x+3;i.j.k=120; this.j2=130;}", DS);

           

            //测试.Net Object的Serialization
            HostType.Register(typeof(DateTime));
            HostType.Register(typeof(VAL));

            v = VAL.NewHostType(new object[] { "中文", DateTime.Now, 123 });
            v = VAL.NewHostType(DateTime.Now);
            string o = v.ToString();
            //string o2 = HostType.EncodeSOAP(v.value);

            //VAL y1 = Coding.Decode(o, CodeType.expression);
            //VAL y2 = Coding.Decode(o, CodeType.expression);

            string d = "{{{\"Text\",\"输入任意值\"}},{}}";
            VAL D = Script.Evaluate(d);
            VAL P = D["NOTEXISTED"];

            VAL dict = new VAL();
            dict["A"] = new VAL();
            dict["A"]["a"] = new VAL(12);
            dict["B"] = new VAL(20);

            string S = "";
            Memory M = new Memory();
            Script.Execute(S, M); 
        }

    }
}
