using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.Windows.Forms;

namespace SubclassDemo
{
    class MyForm : System.Windows.Forms.Form
    {
        Script script = new Script();

        public System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.Label label1;
        
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(76, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(60, 95);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(194, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // MyForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Name = "MyForm";
            this.Text = "Main Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    
        public MyForm()
        {
            InitializeComponent();


            string code = @"
MyForm1 = class(text, mainForm) 
    :System.Windows.Forms.Form()
{
   this.Text = text;
   this.button = new System.Windows.Forms.Button();  
   button.Text = 'OK';
   button.Location = new System.Drawing.Point(60, 80);
   Controls.Add(button);
  
   button.Click+=delegate(sender,e)
   {
       mainForm.label1.Text = 'Hi....';
   };

   this.MyProp1 = 'Hello';
};

//form = new MyForm1('my title');
//form.Show();

MyForm2 = class(mainForm) 
    : MyForm1('2 level subclass', mainForm)
{
   
   this.button = new System.Windows.Forms.Button();  
   button.Text = 'Cancel';
   button.Location = new System.Drawing.Point(60, 120);
   button.Click+=delegate(sender,e)
   {
       mainForm.label1.Text = 'bye....';
   };
   this.Controls.Add(button);
   
   this.textBox1 = new System.Windows.Forms.TextBox();
   this.textBox1.Size = new System.Drawing.Size(194, 20);
   this.textBox1.TextChanged+=delegate(sender,e)
   {
       mainForm.textBox1.Text = this.textBox1.Text;
   };
   this.Controls.Add(this.textBox1);
   this.MyProp2 = 20;
};

form = new MyForm2(mainForm);
form.Show();

";
            Memory DS2 = new Memory();
            DS2.AddHostObject("mainForm", this);
            
            script.DS = DS2;
            script.Execute(code);
        
        
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            (script.DS["form"]["textBox1"].HostValue as TextBox).Text = textBox1.Text;
        }

  
    }
}

