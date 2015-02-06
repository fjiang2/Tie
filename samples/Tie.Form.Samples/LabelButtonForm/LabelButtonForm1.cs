using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;

namespace Tie.FormTest 
{
    class LabelButtonForm1 : Form
    {
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        int count = 0;
        Script script = new Script();
        public LabelButtonForm1()
        {
            button1 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            
            button1.Location = new System.Drawing.Point(50, 140);
            button1.Size = new System.Drawing.Size(120, 24);
            button1.Text = "Cancel";

            label1.Location = new System.Drawing.Point(50, 40);
            label1.AutoSize = true;
            label1.Size = new System.Drawing.Size(120, 24);
            label1.Text = "Good Morning!";

            Controls.Add(this.label1);
            Controls.Add(this.button1);

            this.Text = "Event Handler";

            string code = @"
            button1.Click += delegate(sender,e)
                {
                    sender.Text='OK';
                    label1.Text = 'Good bye '+ count++;
                };    
 
            label1.Click += delegate(sender,e)
                {
                    sender.Text='Hello World';
                    button1.Text = 'Cancel ' + count++;
                };    
             ";
             script.Execute(code, this);
            this.Show();
        }
    }
}
