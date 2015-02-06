using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Tie.FormTest
{
   
    class SearchItem
    {
        /*
         * 
         * Example of Searching
         *    given:
         *      1.SQL statement = string SQL1
         *      2.Initial Value of paramter  = string Code0
         *      3.Window Form = string Code1
         * 
         * user Interface form
         *     +--------------------------------------------+
         *     |                                            |
         *     |   ID: ______________                       |
         *     |   Date From: ____________                  |
         *     |   Date To: _____________                   |
         *     |                                            |
         *     |               [OK]         [Cancel]        |
         *     |                                            |
         *     |                                            |
         *     +--------------------------------------------+
         * 
         * 
         * 
         * */
        //to retrive data from SQL Server by SELECT Statement with paramters(@ID, @Date1, @Date2)
        public string SQL = @"SELECT * FROM Security..Activity WHERE Name=@ID AND DateEntered BETWEEN @Date1 AND @Date2";

        //Initial values (initialValue.ID, initialValue.Date1, initialValue.Date2)       
        public string InitialValueCode = @"
            initialValue.ID = '00090';
            initialValue.Date1 = HostType(System.DateTime.Now).AddMonths(-3);       //DateTime is basic type of TIE, then we have to cast System.DateTime.Now to HostType to call AddMonths method
            initialValue.Date2 = System.DateTime.Now;
            initialValue;           //return last value;
            ";

        //define WinForm to get 3 values(Result.ID,Result.Date1, Result.Date2)
        public string FormCode = @"
              MyForm = class(initialValue)
              {
                this.form = new System.Windows.Forms.Form();
    
                this.buttonClick = function(sender,e)
                {
                    switch(sender.Text)
                     {
                       case 'OK': //result of criteria form
                            this.Result.ID = this.tbID.Text;
                            this.Result.Date1 = this.dtDate1.Value;
                            this.Result.Date2 = this.dtDate2.Value;
                            break;
                       case 'Cancel': 
                            this.Result =null;
                            break;
                     }
                     this.form.Close();
                };    

                
                this.InitializeComponent = function() 
                {
                    var x=20;
                    var y=40;
                    var i;  
                    var title = {'ID', 'Date From', 'Date To'};
                    
                    for(i=0;i<3;i++) 
                    { 
                       this.label[i] = new System.Windows.Forms.Label();
                       this.label[i].Location = new System.Drawing.Point(x, y+40*i);
                       this.label[i].AutoSize = true;
                       this.label[i].Text = title[i];
                       this.form.Controls.Add(this.label[i]);
                    }
 
    
                    this.tbID = new System.Windows.Forms.TextBox();
                    this.tbID.Location = new System.Drawing.Point(x+100, y+40*0);
                    this.tbID.Text = initialValue.ID;    
                    this.form.Controls.Add(this.tbID); 

                    this.dtDate1 = new System.Windows.Forms.DateTimePicker();
                    this.dtDate1.Location = new System.Drawing.Point(x+100, y+40*1);
                    this.dtDate1.Value = initialValue.Date1;
                    this.form.Controls.Add(this.dtDate1); 

                    this.dtDate2 = new System.Windows.Forms.DateTimePicker();
                    this.dtDate2.Location = new System.Drawing.Point(x+100, y+40*2);
                    this.dtDate2.Value = initialValue.Date2;
                    this.form.Controls.Add(this.dtDate2); 

                    title = {'OK', 'Cancel'};
                    for(i=0;i<2;i++) 
                    {
                       this.button[i] = new System.Windows.Forms.Button();
                       this.button[i].Location = new System.Drawing.Point(50+100*i, 200);
                       this.button[i].Size = new System.Drawing.Size(80, 24);
                       this.button[i].Text = title[i];
                       this.button[i].Click += this.buttonClick;
                       this.button[i].DialogResult = i==0? System.Windows.Forms.DialogResult.OK : System.Windows.Forms.DialogResult.Cancel;  
                       this.form.Controls.Add(this.button[i]); 
                    }  
   
                    this.form.Text = 'Search Criteria Demo';
                    this.form.Size = new System.Drawing.Size(400,300);
                };

             };
             ";

        
        public VAL initialValue = new VAL();
        public VAL Result;
        public DataTable DataTable;

        public SearchItem(int ID)
        { }

        public VAL InitialValue
        {
            get
            {
                if (!initialValue.IsNull)
                    return initialValue;

                if (InitialValueCode != "")
                    initialValue = Script.Execute(InitialValueCode, new Memory());

                return initialValue;
            }
            set
            { 
                initialValue = value; 
            }
        }
    }
}
