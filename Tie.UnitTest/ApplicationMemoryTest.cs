using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tie;
using System.IO;
using System.Diagnostics;
using System.IO.Ports;
using Tie.Helper;
using System.Windows;

namespace UnitTest
{

    interface IUrlConfig
    {
        string Protocol { get; set; }
        string Host { get; set; }
        int Port { get; set; }
    }

    class HttpConfig : IUrlConfig
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

    class FtpConfig : IUrlConfig
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public FtpConfig()
        {
            this.Protocol = "ftp";
            this.Host = "127.0.0.10";
            this.Port = 21;
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
        public IUrlConfig Http { get; set; }
        public CommConfig Comm { get; set; }

        public AppConfig()
        {
            this.Http = new HttpConfig();
            this.Comm = new CommConfig();
        }
    }

    class ApplicationMemoryTest : DbMemory
    {
        const string fileName = "c:\\temp\\DbMemory.txt";


        public ApplicationMemoryTest(Memory memory)
            : base(memory)
        {

            Valizer.Register<Guid>(delegate(Guid guid)
                {
                    byte[] bytes = guid.ToByteArray();
                    return new VAL("\"" + Serialization.ByteArrayToHexString(bytes) + "\"");     //because this is a string, need quotation marks ""
                },
                delegate(VAL val)
                {
                    byte[] bytes = Serialization.HexStringToByteArray(val.Str);
                    return new Guid(bytes);
                }
         );

        }

        public int ValColWidh = 40;
        public int VarColWidh = 16;

        protected override int MaxVariableSpaceLength { get { return VarColWidh; } }
        protected override int MaxValueSpaceLength { get { return ValColWidh; } }

        protected override IEnumerable<KeyValuePair<string, string>> ReadMemory(IEnumerable<string> varibles)
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


        protected override void WriteMemory(IEnumerable<KeyValuePair<string, string>> dict)
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
            Size NotExists = device.GetValue<Size>("NotExists");

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
            device.Save(new VAR[] { "Place" });
            string text1 = device.GetFileText();
            string text2 = 
@"Place.Zip 	 ""60015""
Place.State 	 ""TX""
Place.City 	 ""Stafford""
Place.StreetName 	 ""500 Airport Highway""
";
            Debug.Assert(text1.Equals(text2));

            device.VarColWidh = 16;
            device.ValColWidh = 160;
            device.Save(new VAR[] { "Place" });
            text1 = device.GetFileText();
            text2 = @"Place 	 {""Zip"":""60015"",""State"":""TX"",""City"":""Stafford"",""StreetName"":""500 Airport Highway""}
";
            Debug.Assert(text1.Equals(text2));


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
            device.SetValue("Guid", new Guid("DEC32C1A-550E-4F5C-8B81-DDD395578A77"));
            device.SetValue("Integers", new int[] { 1, 2, 3, 4, 5 });
            device.SetValue("Bytes", new byte[] { 1, 2, 3, 4, 5 });
            device.SetValue("Color", System.Drawing.Color.Red); //Valizer defined in HostTypeHelper.cs
            device.Save();

            DS.Clear();
            device.Load();
            size = device.GetValue<System.Windows.Size>("Size");
            Debug.Assert(size.Width == 10 && size.Height == 20);
            
            Guid guid = device.GetValue<Guid>("Guid");
            Debug.Assert(guid == new Guid("DEC32C1A-550E-4F5C-8B81-DDD395578A77"));

            int[] ints = device.GetValue<int[]>("Integers");
            double[] d = device.GetValue<double[]>("Integers");

            byte[] bytes = device.GetValue<byte[]>("Bytes");
            Debug.Assert(bytes[0]==1 && bytes[1]==2 && bytes[2]==3 && bytes[3]==4);

            System.Drawing.Color color = device.GetValue<System.Drawing.Color>("Color");
            Debug.Assert(color == System.Drawing.Color.Red);

            //device.ValColWidh = 400;
            HostType.Register(typeof(Parity));
            HostType.Register(typeof(StopBits));
            AppConfig appConfig = new AppConfig();
            appConfig.Http.Host = "196.168.0.1";
            device.SetValue("AppConfig", appConfig);
            device.Save();

            DS.Clear();
            device.Load();
            appConfig = device.GetValue<AppConfig>("AppConfig");
            Debug.Assert(appConfig.Http.Port == 80);


            Valizer.Register<IUrlConfig>(
                    host => new VAL(new object[] { host.Host, host.Protocol, host.Port }),
                    val => new HttpConfig { Host = val[0].Str, Protocol = val[1].Str, Port = val[2].Intcon }
               );

            appConfig.Http.Port = 88;
            device.SetValue("Url", appConfig.Http);


            /**
             * 
             * List<T>
             * 
             * */
            List<int> list = new List<int>();
            list.Add(10); list.Add(20); list.Add(30);
            device.SetValue("list", list);

            /**
             * 
             * Dictionary<T1, T2>
             * 
             * */
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("A", 1);
            dict.Add("B", 2);
            device.SetValue("dict", dict);

             /**
             * 
             * Nullable<T>
             * 
             * */
            bool? nbool = true;
            device.SetValue("nullable", nbool);

            /**
            * 
            * Enum
            * 
            * */
            HostType.Register(typeof(System.Reflection.BindingFlags));
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.IgnoreCase;
            device.SetValue("enum", flags);
            device.SetValue("enum2", System.Reflection.BindingFlags.ExactBinding);
            device.SetValue("ienum", (int)flags);

            device.Save();
            DS.Clear();
            device.Load();
            
            IUrlConfig url = device.GetValue<IUrlConfig>("Url");
            Debug.Assert(url.Port == 88);

            list = device.GetValue<List<int>>("list");
            Debug.Assert(list[1] == 20);
            dict = device.GetValue<Dictionary<string, int>>("dict");
            Debug.Assert(dict["B"] == 2);


            nbool = device.GetValue<bool?>("nullable");
            Debug.Assert(nbool == true);

            flags = device.GetValue<System.Reflection.BindingFlags>("enum");
            Debug.Assert(flags == System.Reflection.BindingFlags.IgnoreCase);
            int iflags = device.GetValue<int>("enum");
            Debug.Assert(iflags == (int)System.Reflection.BindingFlags.IgnoreCase);

            flags = device.GetValue<System.Reflection.BindingFlags>("ienum");
            Debug.Assert(flags == System.Reflection.BindingFlags.IgnoreCase);

            Valizer.Unregister(typeof(Dictionary<,>));



            FileMemory fm = new FileMemory("c:\\temp\\FileMemory.txt", DS);
            fm.Save();
            DS.Clear();
            fm.Load();
            
            Logger.Close();



        }


     
    }
}
