using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.IO;
using System.Diagnostics;
using System.IO.Ports;

namespace UnitTest
{
    class HttpConfig
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public HttpConfig()
        {
            this.Protocol = "http";
            this.Host = "127.0.0.1";
            this.Port = 80;
        }
    }

    class CommConfig 
    {
        public CommConfig()
        {
            this.PortName = "COM1";
            this.BaudRate = 9600;
            this.DataBits = 8;
            this.Parity = Parity.None;
            this.StopBits = StopBits.One;
        }

        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
    }

    class AppConfig
    {
        public HttpConfig Http { get; set; }
        public CommConfig Comm { get; set; }

        public AppConfig()
        {
            this.Http = new HttpConfig();
            this.Comm = new CommConfig();
        }
    }

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

            ApplicationMemoryTest device = new ApplicationMemoryTest(DS);
            device.Load();

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

            device.VarColWidh = 16;
            device.ValColWidh = 40;
            device.Save(new string[] { "Place" });
            string text1 = device.GetFileText();
            string text2 = 
@"Place.Zip 	 ""60015""
Place.State 	 ""TX""
Place.City 	 ""Stafford""
Place.StreetName 	 ""500 Airport Highway""
";
            Debug.Assert(text1 == text2);

            device.VarColWidh = 16;
            device.ValColWidh = 160;
            device.Save(new string[] { "Place" });
            text1 = device.GetFileText();
            text2 = @"Place 	 {""Zip"" : ""60015"",""State"" : ""TX"",""City"" : ""Stafford"",""StreetName"" : ""500 Airport Highway""}
";
            Debug.Assert(text1 == text2);


            try
            {
                device.Save();
            }
            catch (TieException ex)
            {
                Console.WriteLine(ex.Message);
            }


            DS.Clear();
            System.Windows.Size size = new System.Windows.Size(10, 20);
            device.SetValue("Size", size);
            device.SetValue("Today", DateTime.Today);
            device.Save();

            DS.Clear();
            device.Load();
            size = device.GetValue<System.Windows.Size>("Size");
            Debug.Assert(size.Width == 10 && size.Height == 20);

            AppConfig appConfig = new AppConfig();
            device.SetValue("AppConfig", appConfig);
            device.Save();

            DS.Clear();
            device.Load();
            appConfig = device.GetValue<AppConfig>("AppConfig");
            //System.Diagnostics.Debug.Assert((string)DS["I1"].Valor == "{1,2,3}.typeof(System.Int32[])");
            //System.Diagnostics.Debug.Assert((string)DS["I2"].Valor == "{}.typeof(System.String[])");


            //VAL x = Script.Evaluate("{1,2,3}.typeof(int[])");
            Logger.Close();
        }
    }
}
