using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.IO;

namespace UnitTest
{
    class ApplicationMemoryTest : ApplicationMemory
    {

        public ApplicationMemoryTest(Memory memory)
            :base(memory)
        { 
        }

        protected override int MaxVarLength { get { return 20; } }
        protected override int MaxValLength { get { return 60; } }

        protected override Dictionary<string, string> LoadFromDevice()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists("c:\\temp\\TestFile.txt"))
            {
                using (StreamReader sr = new StreamReader("c:\\temp\\TestFile.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        String line = sr.ReadLine();
                        string[] x = line.Split(new char[] { '\t' });

                        dict.Add(x[0].Trim(), x[1].Trim());
                    }
                }
            }

            return dict;
        }


        protected override void SaveIntoDevice(Dictionary<string, string> dict)
        {
            using (StreamWriter sw = new StreamWriter("c:\\temp\\TestFile.txt"))
            {
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    sw.WriteLine(string.Format("{0} \t {1}", kvp.Key, kvp.Value));
                }
            }
        }


        public static void main()
        {
            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();

            ApplicationMemoryTest test = new ApplicationMemoryTest(DS);
            test.Load();

            string code = @"
               I0 = {1,2,3}; 
               I1 = new int[]{1,2,3};
               I2 = new string[]; 
               int64 = long[];
               I3 = new int64 {10,20,30};
               I4 = new object[] {'A', 1, 3.0};
               A = new int[][] { {1,2}, {3,4}, {5,6}};
            ";

            DS.Clear();
            Script.Execute(code, DS);

            test.Save();

            //System.Diagnostics.Debug.Assert((string)DS["I0"].Valor == "{1,2,3}");
            //System.Diagnostics.Debug.Assert((string)DS["I1"].Valor == "{1,2,3}.typeof(System.Int32[])");
            //System.Diagnostics.Debug.Assert((string)DS["I2"].Valor == "{}.typeof(System.String[])");


            //VAL x = Script.Evaluate("{1,2,3}.typeof(int[])");
            Logger.Close();
        }
    }
}
