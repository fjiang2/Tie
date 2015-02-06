using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Tie;

namespace DataDrivenWinForm
{
    public partial class UserDefinedFormTest : Form
    {
        Memory DS = new Memory();

        public UserDefinedFormTest()
        {
            InitializeComponent();

            StreamReader streamReader = new StreamReader("..\\..\\form.tie");
            this.richTextBox1.Text = streamReader.ReadToEnd();
            streamReader.Close();

            //initial value of @Date1 and @Date2
            DS.AddObject("Date1", new DateTime(2010, 2, 1));
            DS.AddObject("Date2", new DateTime(2010, 5, 3));

            HostType.Register(typeof(System.Windows.Forms.MessageBox)); 
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParameterForm form = new ParameterForm(DS, this.richTextBox1.Text);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
            {
                string SQL = this.textBox1.Text;
                this.textBox2.Text = ParameterForm.SetSqlStringParameters(SQL, form.ResultParameters);
            }
            else
                this.textBox2.Text = "";
        }
    }
}
