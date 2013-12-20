using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.IO;
using System.Diagnostics;

namespace UnitTest
{
    class ApplicationMemoryTest : PersistentMemory
    {
        string fileName = "c:\\temp\\TestFile.txt";
        public ApplicationMemoryTest(Memory memory)
            :base(memory)
        { 
        }

        public int ValColWidh = 40;
        public int VarColWidh = 16;

        protected override int MaxVarLength { get { return VarColWidh; } }
        protected override int MaxValLength { get { return ValColWidh; } }

        protected override Dictionary<string, string> LoadFromDevice()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
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
            string folder = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    sw.WriteLine(string.Format("{0} \t {1}", kvp.Key, kvp.Value));
                }
            }
        }


        public string GetFileText()
        {

            using (StreamReader sr = new StreamReader(fileName))
            {
                return sr.ReadToEnd();
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
               Place.Zip = '60015'; 
               Place.State = 'TX'; 
               Place.City = 'Stafford'; 
               Place.StreetName = '500 Airport Highway'; 
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

            test.VarColWidh = 16;
            test.ValColWidh = 40;
            test.Save(new string[] { "Place" });
            string text1 = test.GetFileText();
            string text2 = 
@"Place.Zip 	 ""60015""
Place.State 	 ""TX""
Place.City 	 ""Stafford""
Place.StreetName 	 ""500 Airport Highway""
";
            Debug.Assert(text1 == text2);

            test.VarColWidh = 16;
            test.ValColWidh = 160;
            test.Save(new string[] { "Place" });
            text1 = test.GetFileText();
            text2 = @"Place 	 {""Zip"" : ""60015"",""State"" : ""TX"",""City"" : ""Stafford"",""StreetName"" : ""500 Airport Highway""}
";
            Debug.Assert(text1 == text2);


            try
            {
                test.Save();
            }
            catch (TieException ex)
            {
                Console.WriteLine(ex.Message);
            }


            
            //System.Diagnostics.Debug.Assert((string)DS["I1"].Valor == "{1,2,3}.typeof(System.Int32[])");
            //System.Diagnostics.Debug.Assert((string)DS["I2"].Valor == "{}.typeof(System.String[])");


            //VAL x = Script.Evaluate("{1,2,3}.typeof(int[])");
            Logger.Close();
        }
    }
}
