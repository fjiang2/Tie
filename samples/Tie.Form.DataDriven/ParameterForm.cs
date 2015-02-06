using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tie;
using System.Reflection;


namespace DataDrivenWinForm
{
    public partial class ParameterForm : Form
    {
        Memory memory = null;
        VAL controlDefinitions = null;
        VAL parameters = null;

        public VAL ResultParameters;

        Script TieScript = new Script();

        public ParameterForm(Memory memory, string script)
        {
            InitializeComponent();

            Script.FunctionChain.Add("error", delegate(VAL parameters, Memory DS)
            {
                int size = parameters.Size;
                VAL L0 = (size > 0) ? parameters[0] : null;
                VAL L1 = (size > 1) ? parameters[1] : null;

                Control control = (Control)L0.HostValue;

                if (size == 2)
                {
                    control.BackColor = Color.Red;
                    toolTip1.SetToolTip(control, L1.Str);
                    errorProvider1.SetError(control, L1.Str);
                    return new VAL();
                }
                else if(size == 1)
                {
                    toolTip1.SetToolTip(control, "");
                    errorProvider1.SetError(control, "");
                    return new VAL();
                }


                return null;
            });


            TieScript.DS = memory;
            VAL setting = TieScript.ResidentEvaluate(script);

            this.memory = memory;
            this.controlDefinitions = setting["Controls"];
            this.parameters = setting["Parameters"];

            VAL title = setting["Title"];
            if (title.Defined)
                this.Text = title.Str;

            VAL size1 = setting["Size"];
            if (size1.Defined)
                this.Size = (Size)size1.HostValue;
        }

        private void ParameterForm_Load(object sender, EventArgs e)
        {
            LoadControls(this.Controls, this.controlDefinitions);

        }

        private void LoadControls(Control.ControlCollection Controls, VAL controlDefinitions)
        {
            //create window controls
            foreach(VAL definition in controlDefinitions)
            {
                string clss = definition["Class"].Str;
                Control control = (Control)HostType.NewInstance(clss, new object[] { });

                HostType.SetObjectProperties(control, definition);
                Controls.Add(control);

                if (control.Name != "")
                    this.memory.Add(control.Name, VAL.NewHostType(control));

                VAL sub = definition["Controls"];
                if(sub.Defined)
                {
                    LoadControls(control.Controls, sub);
                }
            }
        }

        private void ParameterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //remove controls from this.memory
            foreach (Control control in this.Controls)
            {
                if (control.Name != "")
                {
                    memory.Remove(control.Name);
                }
            }

            TieScript.RemoveModule(); 
        }

        private void toolStripButtonSubmit_Click(object sender, EventArgs e)
        {
            VAL v = new VAL();

            for (int i = 0; i < parameters.Size; i++)
            {
                VAL parameter = parameters[i];
                string var = parameter[0].Str;
                v[var] = Script.Evaluate(parameter[1].Str, memory);

                memory.Add(var, v[var]);
            }

            ResultParameters = v;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void toolStripButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            ResultParameters = null;
            this.Close();
        }


        public static SqlCommand SetSqlCommandParameters(SqlCommand cmd, VAL parameters)
        {

            for (int i = 0; i < parameters.Size; i++)
            {
                string parameterName = parameters[i][0].Str;
                object value = parameters[i][1].HostValue;
                cmd.Parameters.AddWithValue("@" + parameterName, value);
            }

            return cmd;
        }


        public static string SetSqlStringParameters(string SQL, VAL parameters)
        {
            if (parameters.IsNull)
                return SQL;

            for (int i = 0; i < parameters.Size; i++)
            {
                string parameterName = parameters[i][0].Str;
                object value = parameters[i][1].HostValue;
                if (value == null)
                    continue;

                if (value is DateTime || value is string)
                    SQL = SQL.Replace("@" + parameterName, "'" + value.ToString() + "'");
                else
                    SQL = SQL.Replace("@" + parameterName, value.ToString());

            }

            return SQL;
        }

      

    }
}