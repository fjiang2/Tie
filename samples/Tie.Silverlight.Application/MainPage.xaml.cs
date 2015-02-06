using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Tie;

namespace SilverlightApplicationDemo
{
    public partial class MainPage : UserControl
    {
        int count = 0;
        Script script;

        public MainPage()
        {
            InitializeComponent();

            script = new Script();
            script.DS.AddObject("form", this);
            script.DS.AddObject("button1", button1);
            script.DS.AddObject("label1", label1);
            script.DS.AddObject("count", 0);

            //Label label = new System.Windows.Controls.Label();
            //label.Margin = new System.Windows.Thickness(10, 40, 0, 0);
            //label.Content = "dynamic label";
            //((Grid)this.Content).Children.Add(label);     
            //script.DS.AddObject("label", label);
            HostType.Register(typeof(Label));
            HostType.Register(typeof(TextBox));
            string code = @"
                var label = new System.Windows.Controls.Label();
                label.Content = 'dynamic label';
                form.Content.Children.Add(label);                            

                var textbox = new System.Windows.Controls.TextBox();
                textbox.Content = 'Please Input...';
                //form.Content.Children.Add(textbox);                            

                button1.Content = 'Cancel';
                label1.Content = 'Good Morning!';
            
                button1.Click += function(sender,e)
                    {
                        sender.Content='OK';
                        label1.Content = 'Good bye '+ count++;
                    };    
             
                label1.MouseLeftButtonDown += function(sender,e)
                    {
                        sender.Content='Hello World';
                        button1.Content = 'Cancel ' + count++;
                    };    
            ";


            script.Execute(code);
        }
    }
}
