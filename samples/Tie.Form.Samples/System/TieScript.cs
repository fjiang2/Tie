using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;

namespace Tie.FormTest
{
    partial class TieEditor : Form
    {
        private TieEditor(string sourceCode, string message, int cur)
        {
            InitializeComponent();

            this.richTextBox1.AcceptsTab = true;

            this.richTextBox1.Text = sourceCode;
            this.statusStrip1.Items[1].Text = message;
            this.richTextBox1.SelectionStart = cur;
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;
            int line = Line(rtb);
            int col = Column(rtb);
            int pos = rtb.SelectionStart;

            this.statusStrip1.Items[0].Text = "Ln " + line + ", Col " + col;
        
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        
        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }


        public static string Show(PositionException e)
        {
            TieEditor tieException = new TieEditor(e.Position.CodePiece, e.Message, e.Position.Cursor);
            
            if (tieException.ShowDialog() == DialogResult.Yes)
                return tieException.richTextBox1.Text;
            else
                return null;
        }


        public static string Show(string sourceCode, string message, int cur)
        {
            TieEditor tieException = new TieEditor(sourceCode, message, cur);

            if (tieException.ShowDialog() == DialogResult.Yes)
                return tieException.richTextBox1.Text;
            else
                return null;
        }

        #region Line/Column/Position

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int GetCaretPos(ref Point lpPoint);

        private static int GetCorrection(RichTextBox e, int index)
        {
            Point pt1 = Point.Empty;
            GetCaretPos(ref pt1);
            Point pt2 = e.GetPositionFromCharIndex(index);

            if (pt1 != pt2)
                return 1;
            else
                return 0;
        }

        public static int Line(RichTextBox e)
        {
            return Line(e, e.SelectionStart);
        }

        public static int Column(RichTextBox e)
        {
            return Column(e, e.SelectionStart);
        }

        public static int Line(RichTextBox e, int index)
        {
            int correction = GetCorrection(e, index);
            return e.GetLineFromCharIndex(index) - correction + 1;
        }

        public static int Column(RichTextBox e, int index1)
        {
            int correction = GetCorrection(e, index1);
            Point p = e.GetPositionFromCharIndex(index1 - correction);

            if (p.X == 1)
                return 1;

            p.X = 0;
            int index2 = e.GetCharIndexFromPosition(p);

            int col = index1 - index2 + 1;

            return col;
        }
        
        #endregion

    }


    public class TieScript   
    {
        Tie.Script script;
        public delegate void ExecuteHandler(string src);
        public delegate VAL EvaluteHandler(string src);

        private string src;

        public TieScript()
        {
            script = new Script(Guid.NewGuid().ToString(), 1024 * 8, true);
        }

        public string SourceCode
        { 
            get { return src; }
        }

        public Memory DS
        {  
            get { return script.DS; }
            set { script.DS = value; }
        }
        
        public void Dispose()
        {
            script.Dispose();
        }

        public void Execute(string src)
        {
            this.src = src;
            Execute(script.Execute);
        }

        public void VolatileExecute(string src)
        {
            this.src = src;
            Execute(script.VolatileExecute);
        }

        public void Evaluate(string src)
        {
            this.src = src;
            Evalute(script.ResidentEvaluate);
        }

        public void VolatileEvaluate(string src)
        {
            this.src = src;
            Evalute(script.VolatileEvaluate);
        }

        private void Execute(ExecuteHandler handler)
        {

            L1:
            try
            {
                handler(src);
            }
            catch (PositionException e1)
            {
                string ret = TieEditor.Show(e1);
                if (ret != null)
                {
                    src = ret;
                    goto L1;
                }

            }
            catch (Exception e2)
            {
                string ret = TieEditor.Show(src, e2.Message, 0);
                if (ret != null)
                {
                    src = ret;
                    goto L1;
                }
            
            }

        }

        private VAL Evalute(EvaluteHandler handler)
        {
        L1:

            try
            {
                return handler(src);
            }
            catch (PositionException e1)
            {
                string ret = TieEditor.Show(e1);
                if (ret != null)
                {
                    src = ret;
                    goto L1;
                }

            }
            catch (Exception e2)
            {
                string ret = TieEditor.Show(src, e2.Message, 0);
                if (ret != null)
                {
                    src = ret;
                    goto L1;
                }

            }

            return new VAL();
        }


    }

}
