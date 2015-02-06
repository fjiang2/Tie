using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;

namespace Tie.FormTest 
{
    class LabelButtonForm2 : Form
    {
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        int count = 0;

        Script script;

        public LabelButtonForm2()
        {
            button1 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            
            Controls.Add(this.label1);
            Controls.Add(this.button1);

            this.Text = "Properties";

            HostType.Register(typeof(System.Drawing.Point));
            HostType.Register(typeof(System.Drawing.Size));

            script = new Script();
            string code = @"
                button1.Location = new System.Drawing.Point(50, 140);
                button1.Size = new System.Drawing.Size(120, 24);
                button1.Text = 'Cancel';

                label1.Location = new System.Drawing.Point(50, 40);
                label1.AutoSize = true;
                label1.Size = new System.Drawing.Size(120, 24);
                label1.Text = 'Good Morning!';

                button1.Click += function(sender,e)
                   {
                     sender.Text='OK';
                     label1.Text = 'Good bye '+ count++;
                   };    
 
                label1.Click += function(sender,e)
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
