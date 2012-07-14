using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Tie;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

//测试
namespace UnitTest
{
    class HostObjectSearialization
    {



        public static void main()
        {
            Memory DS = new Memory();

            string func = @"
            
            plus = function(a,b)
            {return a+b;};

            c=plus(20,30);
                
            ";

            Script.Execute(func, DS);


            DerivedClassTest clssTest = new DerivedClassTest();

            VAL prop = new VAL();
            prop["A"] = new VAL(300);
            prop["E"] = new VAL(2);
            prop["S"] = new VAL("XXX");

            HostType.SetObjectProperties(clssTest, prop);


            StringWriter o = new StringWriter();
            double d1 = 0.0000031415;

            o.Write("{0}", d1);

            VAL ESC = Script.Evaluate(@"@'ab\times'");

            Memory myDS = new Memory();
            TextBox tbEdit = new TextBox();
            tbEdit.Text = "Hello world";
            tbEdit.ForeColor = Color.Red;

            myDS.Add("tbEdit", VAL.NewHostType(tbEdit));

            HostType.Register(typeof(System.Reflection.MemberTypes));
            HostType.Register(typeof(System.Drawing.Color));
            HostType.Register(typeof(System.Drawing.Font));
            HostType.Register(typeof(System.Drawing.FontStyle));
            HostType.Register(typeof(System.Drawing.GraphicsUnit));

            //VAL x = HostType.GetObjectProperties(tbEdit);
        }
    }
}
