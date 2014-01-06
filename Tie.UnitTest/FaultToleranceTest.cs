using System;
using System.Collections.Generic;
using System.Text;
using Tie;
using System.Windows.Forms;

namespace UnitTest
{
    class FaultToleranceTest
    {
        TextBox textBox = new System.Windows.Forms.TextBox();
        private int count = 40;
        public  int count2 = 10;

        int Size { get { return 40; } }
        string log1;
        int log2;

        void WriteLine(string value)
        {
            log1 = value;
        }

        //不支持private method的重载,因为HostOperation.HostFunction没有search private的方法
        //void WriteLine(int value)
        //{
        //    log2 = value;
        //}

        public FaultToleranceTest()
        {
            Script script = new Script("unknown");
            Memory DS1 = new Memory();
            script.DS = DS1;


            string code = @"
            
            prop = { Text: 'Hello',
                     Location : new System.Drawing.Point(60, 40),
                     My : count
                   };
            
            textBox += prop;                     
            count += 10 + Size;      

            WriteLine('Call MethodInfo');      
            x = this.count2;           //count2 是public,可以调用
            y = this.count;           //count 是private,不能调用,返回void

            ";

            VAL x = VAL.NewHostType(new System.Drawing.Point(60, 40));
            string s = x.ToString();
 
            Tie.Logger.Open("C:\\temp\\tie.log");
            //script.Execute(code);

            script.Execute(code, this);
 
            //TextBox textBox = DS1["textBox"].value as TextBox;

            System.Diagnostics.Debug.Assert(textBox.Text == "Hello");
            System.Diagnostics.Debug.Assert(count == 90);
        }
    }
}
