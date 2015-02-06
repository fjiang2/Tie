using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tie.FormTest
{
    public partial class MainForm0 : Form
    {
        public MainForm0()
        {
            InitializeComponent();

            this.FormClosed += delegate(object sender, FormClosedEventArgs e)
            {
                Logger.Close();
            };
            
            this.Text = "C# Label & Button";

            Logger.Open("c:\\temp\\tie.log");

        }

        private void button0_Click(object sender, EventArgs e)
        {
            new LabelButtonForm0();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new LabelButtonForm1();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new LabelButtonForm2();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new LabelButtonForm3();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new LabelButtonForm4();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new LabelButtonForm5();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            new LabelButtonForm6();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            new LabelButtonForm7();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            new LabelButtonForm8();
        }
    }
}
