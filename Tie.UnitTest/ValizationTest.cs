using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.Windows.Forms;
using System.Drawing;

namespace UnitTest
{
    class ValizationTest
    {
        private static void ValizationRegister()
        {
            Valizer.Register<Color>(
                   delegate(Color color)
                   {
                       return new VAL(string.Format("\"#{0:X2}{1:X2}{2:X2}\"", color.R, color.G, color.B));
                   },
                   delegate(VAL val)
                   {
                       string hexString = (string)val;
                       int red = Convert.ToByte(hexString.Substring(1, 2), 16);
                       int green = Convert.ToByte(hexString.Substring(3, 2), 16);
                       int blue = Convert.ToByte(hexString.Substring(5, 2), 16);

                       return Color.FromArgb(red, green, blue);
                   }
              );

            Valizer.Register<Control>(new string[] { "ForeColor", "BackColor" });
            Valizer.Register<Form>(
                 delegate(Form form)
                 {
                     VAL val = new VAL();
                     foreach (Control control in form.Controls)
                     {
                         val.Add(control.Name, Valizer.Valize(control));
                     }
                     return val;
                 });

        }

        public static void main()
        {
            ValizationRegister();

            Form form = new Form();
            form.Name = "form1";

            Button button1 = new Button();
            button1.Name = "B1";

            Button button2 = new Button();
            button2.Name = "B1";
            form.Controls.Add(button1);
            form.Controls.Add(button2);

            VAL val = Valizer.Valize(form);

        }
    }
}
