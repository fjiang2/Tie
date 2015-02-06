using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tie.FormTest
{
    public partial class MainForm1: Form
    {
        Script script;
        public MainForm1()
        {
            InitializeComponent();

            this.FormClosed += delegate(object sender, FormClosedEventArgs e)
            {
                script.Dispose(); 
                Logger.Close();
            };

            this.Text = "Tie Label & Button";
            Logger.Open("c:\\temp\\tie.log");

            script = new Script();
            script.DS.AddObject("form", this);
            
            string code = @"
               foreach(control in form.Controls)
               {
                  control.Click += function(sender, e)
                  {
                      var i = HOST(sender.Name).Substring(6,1);                             //sender.Name=='button0'
                      var newForm = 'new Tie.FormTest.LabelButtonForm'+i+'()';        //formName == 'new Tie.FormTest.LabelButtonForm0()'
                      *newForm;                                                       //evaluate an expression string
                  };
               }
            "; 
            script.Execute(code);
        }

    }
}
