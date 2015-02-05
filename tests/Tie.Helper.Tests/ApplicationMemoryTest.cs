using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Tie.Helper.Tests
{
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
    }
}
