using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;

namespace Tie.FormTest 
{
    class LabelButtonForm7 
    {
        TieScript script;

        public LabelButtonForm7()
        {
            HostType.Register(typeof(System.Windows.Forms.Form));
            HostType.Register(typeof(System.Windows.Forms.Button));
            HostType.Register(typeof(System.Windows.Forms.Label));
            HostType.Register(typeof(System.Drawing.Point));
            HostType.Register(typeof(System.Drawing.Size));

            script = new TieScript();
            string code = @"
              LabelButtonForm = class(caption)
              {
                this.count = 0;
                this.form = new System.Windows.Forms.Form();
                this.button1 = new System.Windows.Forms.Button();
                this.label1 = new System.Windows.Forms.Label();
                this.form.Text = caption;
                    
                this.buttonClick = function(sender,e)
                {
                    sender.Text='OK';
                    this.label1.Text = 'Good bye '+ this.count++;
                };    

                this.labelClick = function(sender,e)
                {
                    sender.Text='Hello World';
                    this.button1.Text = 'Cancel ' + this.count++;
                };    

                this.InitializeComponent = function() 
                {
                    this.button1.Location = new System.Drawing.Point(50, 140);
                    this.button1.Size = new System.Drawing.Size(120, 24);
                    this.button1.Text = 'Cancel';
                    this.button1.Click += this.buttonClick;

                    this.label1.Location = new System.Drawing.Point(50, 40);
                    this.label1.AutoSize = true;
                    this.label1.Size = new System.Drawing.Size(120, 24);
                    this.label1.Text = 'Good Morning!';
                    this.label1.Click += this.labelClick;

                    this.form.Controls.Add(this.button1); 
                    this.form.Controls.Add(this.label1);
                    

                };

            

                this.InitializeComponent();
                this.form.Show();

              };
 
             
            //start up   
             new LabelButtonForm('Tie class');
             ";

             script.Execute(code);

        }
    }
}
