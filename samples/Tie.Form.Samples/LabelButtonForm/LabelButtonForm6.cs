using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;

namespace Tie.FormTest 
{
    class LabelButtonForm6 
    {
        Script script;

        public LabelButtonForm6()
        {

            //HostType.Register(typeof(System.Windows.Forms.Form));
            //HostType.Register(typeof(System.Windows.Forms.Button));
            //HostType.Register(typeof(System.Windows.Forms.Label));
            //HostType.Register(typeof(System.Drawing.Point));
            //HostType.Register(typeof(System.Drawing.Size));

            script = new Script();

            string code = @"
                count = 0;
                form = new System.Windows.Forms.Form();
                button1 = new System.Windows.Forms.Button();
                label1 = new System.Windows.Forms.Label();
            
                button1.Location = new System.Drawing.Point(50, 140);
                button1.Size = new System.Drawing.Size(120, 24);
                button1.Text = 'Cancel';

                label1.Location = new System.Drawing.Point(50, 40);
                label1.AutoSize = true;
                label1.Size = new System.Drawing.Size(120, 24);
                label1.Text = 'Good Morning!';

                form.Controls.Add(label1);
                form.Controls.Add(button1);
                form.Text = 'Tie Script';

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

                form.Show();
             ";
            try
            {
                script.Execute(code);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
    }
}
