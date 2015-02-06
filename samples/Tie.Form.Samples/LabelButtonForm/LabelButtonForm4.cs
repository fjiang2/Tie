using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;

namespace Tie.FormTest 
{
    class LabelButtonForm4 : Form
    {
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.Label label1;
        int count = 0;

        Script script;

        public LabelButtonForm4()
        {

            Logger.Open("c:\\temp\\tie.log");
            this.Text = "Namespace";

            HostType.Register(typeof(System.Windows.Forms.Button));
            HostType.Register(typeof(System.Windows.Forms.Label));
            HostType.Register(typeof(System.Drawing.Point));
            HostType.Register(typeof(System.Drawing.Size));

            script = new Script();
            script.Scope = "LabelButtonForm4";
            script.DS.AddObject(script.Scope, this);
            script.DS.AddObject("count", 0);

        
            string code = @"
                this.button1 = new System.Windows.Forms.Button();
                this.label1 = new System.Windows.Forms.Label();
            
                this.button1.Location = new System.Drawing.Point(50, 140);
                this.button1.Size = new System.Drawing.Size(120, 24);
                this.button1.Text = 'Cancel';

                this.label1.Location = new System.Drawing.Point(50, 40);
                this.label1.AutoSize = true;
                this.label1.Size = new System.Drawing.Size(120, 24);
                this.label1.Text = 'Good Morning!';

                this.Controls.Add(this.label1);
                this.Controls.Add(this.button1);


                this.button1.Click += function(sender,e)
                   {
                     sender.Text='OK';
                     this.label1.Text = 'Good bye '+ count++;
        loginfo('this.label1.Text='+this.label1.Text);
                   };    
 
                this.label1.Click += function(sender,e)
                   {
                        sender.Text='Hello World';
                        this.button1.Text = 'Cancel ' + count++;
                   };    
             ";
            try
            {
                script.Execute(code);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            this.Show();
        }
    }
}
