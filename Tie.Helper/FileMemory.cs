using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Tie.Helper
{
    /// <summary>
    /// used to serialize memory to persistent device, such as database server or text file
    /// varible can be simple varible or composite varible, such as "X.a", "X.a.b"
    /// </summary>
    public class FileMemory : PersistentMemory
    {

        private string fileName;

        /// <summary>
        /// 
        /// </summary>
        public FileMemory(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        public FileMemory(string fileName, Memory memory)
            : base(memory)
        {
            this.fileName = fileName;
        }

       
        /// <summary>
        /// Save variables into persistent device
        /// </summary>
        /// <param name="variables"></param>
        private void Save(IEnumerable<VAR> variables)
        {

            string folder = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string bak = fileName + ".bak";
            if (File.Exists(bak))
                File.Delete(bak);

            if (File.Exists(fileName))
                File.Move(fileName, fileName + ".bak");

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (VAR variable in variables)
                {
                    string ident = (string)variable;

                    if (ident.StartsWith("System") || ident.StartsWith("Microsoft") || ident.StartsWith("Tie"))
                        continue;

                    VAL val = GetVAL(ident);

                    if (val.IsNull || val.Undefined)
                        continue;

                    sw.WriteLine(string.Format("{0} = {1};", variable, val.ToJson()));
                }
            }

        }

        /// <summary>
        /// Save all varibles into persistent device
        /// </summary>
        public override void Save()
        {
            Save(memory.Keys);
        }

        /// <summary>
        /// Load all varibles from persistent device
        /// </summary>
        public override void Load()
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string code = sr.ReadToEnd();
                    Script.Execute(code, base.memory);
                }
            }
        }

    }
}
