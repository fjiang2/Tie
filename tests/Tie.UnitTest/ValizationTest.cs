using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace UnitTest
{
    class ValizationTest
    {
        private static void ValizationRegister()
        {
            Valizer.Register<Color>(
                   delegate (Color color)
                   {
                       return new VAL(string.Format("\"#{0:X2}{1:X2}{2:X2}\"", color.R, color.G, color.B));
                   },
                   delegate (VAL val)
                   {
                       string hexString = (string)val;
                       int red = Convert.ToByte(hexString.Substring(1, 2), 16);
                       int green = Convert.ToByte(hexString.Substring(3, 2), 16);
                       int blue = Convert.ToByte(hexString.Substring(5, 2), 16);

                       return Color.FromArgb(red, green, blue);
                   }
              );

            Valizer.Register<Control>(new string[] { "ForeColor", "BackColor", "Text" });
            Valizer.Register<ContainerControl>(
                 delegate (ContainerControl form)
                 {
                     VAL val = new VAL();
                     val["BackColor"] = Valizer.Valize(form.BackColor);
                     foreach (Control control in form.Controls)
                     {
                         val.AddMember(control.Name, Valizer.Valize(control));
                     }
                     return val;
                 },
                delegate (ContainerControl form, Type type, VAL val)
                {
                    if (val["BackColor"].Defined)
                        form.BackColor = Valizer.Devalize<Color>(val["BackColor"]);

                    foreach (Control control in form.Controls)
                    {
                        if (val[control.Name].Defined)
                        {
                            Valizer.Devalize(val[control.Name], control);
                        }
                    }

                    return form;
                }
              );

        }

        public static void main()
        {
            ValizationRegister();

            Form form = new Form();
            form.Name = "form1";

            Button button1 = new Button();
            button1.Name = "B1";
            button1.Text = "OK";

            Button button2 = new Button();
            button2.Name = "B2";
            button2.Text = "Cancel";

            form.Controls.Add(button1);
            form.Controls.Add(button2);

            VAL val = Valizer.Valize(form);
            string json = val.ToJson("");
            Debug.Assert(button1.ForeColor.R == Color.Black.R && button1.ForeColor.G == Color.Black.G && button1.ForeColor.B == Color.Black.B);

            button1.ForeColor = Color.Red;
            button2.Text = "Close";

            val = Script.Evaluate(json);
            Valizer.Devalize(val, form);
            Debug.Assert(button1.ForeColor.R == Color.Black.R && button1.ForeColor.G == Color.Black.G && button1.ForeColor.B == Color.Black.B);
            Debug.Assert(button2.Text == "Cancel");


            string text1 = "ABCDEDF12345678";
            StreamWriter writer = new StreamWriter("c:\\temp\\tietest1.txt");
            writer.Write(text1);
            writer.Close();

            using (FileStream fileStream = new FileStream("c:\\temp\\tietest1.txt", FileMode.Open))
            {
                //Valize
                VAL x1 = Valizer.Valize(fileStream);

                //Save to JSON device
                string json1 = x1.ToJson();

                //Load from JSON device
                VAL x2 = Script.Evaluate(json1);

                //Devalize
                MemoryStream memoryStream = new MemoryStream();
                Valizer.Devalize(x2, memoryStream);

                //verify
                memoryStream.Position = 0;
                StreamReader reader = new StreamReader(memoryStream);
                string text2 = reader.ReadToEnd();
                Debug.Assert(text1 == text2);
            }

            var args = Script.Evaluate("new { UserId = 999+20, Name=\"Jane\" + 3}");
            Debug.Assert(args.ToString() == "{{\"UserId\",1019},{\"Name\",\"Jane3\"}}");

            Script.FunctionChain.Add(
                (string func, VAL parameters, Memory DS)
                =>
                {
                    if (func == "Area111")
                    {
                        string name1 = parameters[1].GetName();
                        string name2 = parameters[2].GetName();
                        Debug.Assert(name1 == "userId" && name2 == "Name");
                        return new VAL();
                    }
                    else
                        return null;
                }
            );
            var code1 = "XXX.Area111(userId = 10 +20, Name='Jane')";
            var x = Script.Evaluate(code1, new Memory());

            Dictionary<Type, bool> dict = new Dictionary<Type, bool>();
            dict.Add(typeof(int), true);
            dict.Add(typeof(string), false);

            VAL vDict = Valizer.Valize(dict);
            string j1 = vDict.ToJson();

            dict.Clear();

            dict = Valizer.Devalize<Dictionary<Type, bool>>(Script.Evaluate(j1));

            VAL jsonarray = Script.Evaluate("{{Id:1,Name:'A'},{Id:2,Name:'B'}}");

            TestContract_class[] tc = Valizer.Devalize<TestContract_class[]>(jsonarray);
            Debug.Assert(tc.Length == 2 && tc[0].Id == 1 && tc[1].Name == "B");

            TestContract_struct[] ts = Valizer.Devalize<TestContract_struct[]>(jsonarray);
            Debug.Assert(ts.Length == 2 && ts[0].Id == 1 && ts[1].Name == "B");

            jsonarray = Script.Evaluate("{Age:1,Text:'A', Tc:{Id:1,Name:'B',Addr:{'X1','X2'}}}");
            TestContract_complex tc1 = Valizer.Devalize<TestContract_complex>(jsonarray);
            Debug.Assert(tc1.Tc.Id == 1 && tc1.Tc.Name == "B" && tc1.Tc.Addr[1] == "X2");

            TestContract_complex2 tc2 = Valizer.Devalize<TestContract_complex2>(jsonarray);
            Debug.Assert(tc2.Tc.Id == 1 && tc2.Tc.Name == "B" && tc2.Tc.Addr[1] == "X2");
        }

        class TestContract_class
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string[] Addr { get; set; }
        }

        struct TestContract_struct
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string[] Addr { get; set; }
        }

        class TestContract_complex
        {
            public int Age { get; set; }
            public string Text { get; set; }

            public TestContract_class Tc { get; set; }

        }

        class TestContract_complex2
        {
            public int Age { get; set; }
            public string Text { get; set; }

            public TestContract_struct Tc { get; set; }

        }
    }
}
