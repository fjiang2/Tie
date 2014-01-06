using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    interface ITest : IComparable
    {
        int IMethod();
    }

    class Test2 : ITest
    {

        public Test2()
        { }

        public int IMethod()
        {
            return 2;
        }

        public int CompareTo(object obj)
        {
            if (obj is Test2)
                return 0;
            
            else
                return 1;
        }
    
    }
    

    class Test : ITest
    {

        public int IMethod()
        {
            return 1;
        }

        public int CompareTo(object obj)
        {
            if (obj is Test)
                return 0;

            else
                return 1;
        }

        
        public static void main()
        {
            string code = @"
var a=1;
Setting = class(gridViewer)
{
   //known:
   //  dataViewContainer;

   this.gridViewer = gridViewer;
   this.gridControl = gridViewer.ViewControl;
   this.gridView = this.gridControl.MainView;

   this.Initialize =function()
   {
      this.gridView.Columns['WO'].Width=120;
   };


   this.ContextMenu=function()
   {
      var contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();

      var menuItem = new System.Windows.Forms.ToolStripMenuItem('End of Project');
      contextMenuStrip.Items.Add(menuItem);
      menuItem.Click += function(sender, e)
          { 
	     var form = new ViewManager.WinForm.SmartList.ContainerForm(4); 	
             form.PopUp(dataViewContainer);
             form.Inquire(); 
           };


     menuItem = new System.Windows.Forms.ToolStripMenuItem('Tool Report By SO');
     contextMenuStrip.Items.Add(menuItem);
     menuItem.Click += function(sender, e)
          { 
	     var form = new ViewManager.WinForm.SmartList.ContainerForm(176); 	
             mySO = this.gridViewer.SelectedDataRow['WO']; 
             form.ChangeCaption(format('[{0}] {1}', mySO, form.Text));
             form.PopUp(dataViewContainer);
             form.Inquire(); 
           };

      menuItem = new System.Windows.Forms.ToolStripMenuItem('Material Part Report By SO');
      contextMenuStrip.Items.Add(menuItem);
      menuItem.Click += function(sender, e)
          { 
	     var form = new ViewManager.WinForm.SmartList.ContainerForm(171); 	
             mySO = this.gridViewer.SelectedDataRow['WO']; 
             form.ChangeCaption(format('[{0}] {1}', mySO, form.Text));
             form.PopUp(dataViewContainer);
             form.Inquire(); 
           };

      return contextMenuStrip;
   };

};

c=a+1;
";



            string code3 = @"

Setting = class(pivotGridViewer)
{
   this.pivotGridViewer = pivotGridViewer;
   this.pivotGridControl = pivotGridViewer.ViewControl;

   this.Initialize=function()
   {
      this.pivotGridControl.Fields['AComplete'].Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea;
      this.pivotGridControl.Fields['AComplete'].GroupInterval = DevExpress.XtraPivotGrid.PivotGroupInterval.DateWeekOfYear;
      this.pivotGridControl.Fields['AComplete'].Caption = 'Started';
      this.pivotGridControl.Fields['DateKey'].GroupInterval = DevExpress.XtraPivotGrid.PivotGroupInterval.DateWeekOfYear;
      this.pivotGridControl.Fields['DateKey'].Caption = 'Completed';
      this.pivotGridControl.Fields['Budget'].Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
      this.pivotGridControl.Fields['Budget'].CellFormat.FormatString = '0.00';
      this.pivotGridControl.Fields['Budget'].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.pivotGridControl.Fields['Actual'].Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
      this.pivotGridControl.Fields['Actual'].CellFormat.FormatString = '0.00';
      this.pivotGridControl.Fields['Actual'].CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.pivotGridControl.Fields['EN'].Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
      this.pivotGridControl.Fields['EmpName'].Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;

      var pEfficiency = new DevExpress.XtraPivotGrid.PivotGridField();
      pEfficiency.Caption = 'Efficiency';
      pEfficiency.Name = 'pEfficiency';
      //pEfficiency.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
      pEfficiency.AreaIndex = 2;
      pEfficiency.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
      pEfficiency.CellFormat.FormatString = 'p';
      pEfficiency.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      pEfficiency.GrandTotalCellFormat.FormatString = 'p';
      pEfficiency.GrandTotalCellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      pEfficiency.TotalCellFormat.FormatString = 'p';
      pEfficiency.TotalCellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      pEfficiency.TotalValueFormat.FormatString = 'p';
      pEfficiency.TotalValueFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      pEfficiency.UnboundFieldName = 'Efficiency';
      pEfficiency.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
      pEfficiency.ValueFormat.FormatString = 'p';
      pEfficiency.ValueFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
      this.pivotGridControl.Fields.Add(pEfficiency);

      this.pivotGridControl.FieldValueDisplayText +=this.handler1;
      this.pivotGridControl.CustomSummary += this.handler2;

      pEfficiency.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;


    };


    this.handler1 = function(sender,e)
    {
       if(e.Field==null)
          return;

     this.pivotGridViewer.FirstDayOfWeek(e, 'AComplete');
     this.pivotGridViewer.FirstDayOfWeek(e, 'DateKey');
     return;
    };


   this.handler2 = function(sender,e)
   {
       //this.pivotGridViewer.pivotGrid_CustomSummary(sender,e);  return;

       if(e.DataField==null)
          return;

       if (e.DataField.Name != 'pEfficiency')
             return;

       var budget = 0.0;
       var actual = 0.0;

       var ds = e.CreateDrillDownDataSource();
       var i;
       for (i = 0; i < ds.RowCount; i++)
       {
           var row = ds[i];
           budget += row['Budget'];
           actual += row['Actual'];
        }
       if (actual != 0.0)
          e.CustomValue = budget / actual;
       else
          e.CustomValue = 99.0;

   };


};



";
//            code = @"
//            b = 0;
//            A={2,4,6};
//            foreach(a in A)
//               b += a;
//            ";


            //DataTable dt = SQLCommand.FillDataTable("SELECT * FROM Workflows");
            string[] arr = new string[] { "A", "B", "C" };
            string code2 = @"
//            b = {};
//            foreach(dataRow in dt.Rows)
//               b += dataRow['Label'];

            var a=0;
            var b=20;
            c='';
            foreach(a in arr)
               c+=a;
            d = b;
            ";


            string code4 = @"
   a=2; c=2;
    switch(a++)
    { case 1: b=1; break;
      case c++: b=3; break;
   };

    switch(a)
    { case 3: x=1; break;
      case 4: x=3; break;
   };

";

            //test: pass LIST into .NET object
            string code5 = @"a=1;
var parameters={};
parameters['mySO']='000-11-222';
test.Entry = parameters; 
SA[0]='1111'; 
SA[1]='2222'; 
//SA = {'1','2'};   //SA不再是HostType了
test.StringArray = SA;
X = test.StringArray;
test.StringArray = {'A','B', 'C'};
I1 = new UnitTest.Test();
";

            string code6 = @"
            test.DialogResult = null;
            test.StringArray = {'A','B', 'C'};
            ITestArray += test;
            sum = test.IArrayMethod(ITestArray);
L={0,1,2,3,4,5,6};
a=L[-1];
L.push(7);
R=L.pop();
L.insert(-1,'AA');
L.remove(4);
L1={0,1,2,3,4,5,6};

mth = test.IArrayMethod;
if(mth!=null)
    sum = mth(ITestArray);
else
    sum = 200;

if(test.IArrayMethod!=null)
    sum = test.IArrayMethod(ITestArray);
else
    sum = 200;

A={10,11,12,13,14,15};
A3 = A.pop(3);
            ";


            Tie.Logger.Open("C:\\temp\\tie.log");
        
            Test test = new Test();
            ITest[] ITestArray =  new ITest[3];
            ITestArray[0] = new Test2();
            ITestArray[1] = test;
            ITestArray[2] = new Test2();
           
            Script script = new Script("unknown", 500);
            script.DS.AddHostObject("arr", arr);
            script.DS.AddHostObject("test", test);
            script.DS.AddObject("ITestArray", new VAL(ITestArray));
            script.DS.AddHostObject("SA", new string[3]);   //as HostType

            script.Execute(code6);
            //Debug.Assert(script.DS["sum"].Intcon == 6, "Interface Array in HostValue, IArrayMethod");


            VAL v1 = VAL.NewHostType(test);
            VAL v2 = VAL.NewHostType(new Test2());

            bool b1 = v1 == VAL.NewHostType(DateTime.Now);
            bool b2 = v1 == v2;
            bool b3 = v1 >= VAL.NewHostType(null);

            Tie.Logger.Close();

        }


        public int IArrayMethod(ITest[] A)
        { 
            int sum=0;
            foreach (ITest a in A)
            {
                if(a!=null)
                    sum += a.IMethod();
            }
            //throw new ApplicationException("Test throw");
            return sum;
        }


        object entry;
        public object Entry
        {
            set { entry = value; }

        }

        string[] sa = new string[]{"X","Y"};
        public string[] StringArray
        {
            set 
            { 
                sa = value; 
            }
            get 
            { 
                return sa; 
            }
        }
        public bool? dialogResult=true ;
        public bool? DialogResult
        {
            get { return dialogResult; }
            set { dialogResult = value; }
        }
            
    }
}
