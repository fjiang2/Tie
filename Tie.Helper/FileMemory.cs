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
    public abstract class FileMemory : PersistentMemory
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

       
        private string ToJson(VAL val)
        {
            return val.ToJson("");
        }


        /// <summary>
        /// variable or value is oversize
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="message"></param>
        protected virtual void OversizeHandler(string variable, string message)
        {
            Console.WriteLine(string.Format("{0} at variable {1} of class {2}", message, variable, this.GetType().FullName));
        }

        /// <summary>
        /// invalid variable/value pair in persistent device
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="message"></param>
        protected virtual void InvalidVariableHandler(string variable, string message)
        {
            Console.WriteLine(string.Format("{0} at variable {1} of class {2}", message, variable, this.GetType().FullName));
        }


        /// <summary>
        /// Save variables into persistent device
        /// </summary>
        /// <param name="variables"></param>
        public void Save(IEnumerable<VAR> variables)
        {
            Dictionary<string, string> storage = new Dictionary<string, string>();

            foreach (VAR variable in variables)
            {
                string ident = (string)variable;

                if (ident.StartsWith("System") || ident.StartsWith("Microsoft"))
                    continue;

                VAL val = GetVAL(ident);

                if (val.IsNull || val.Undefined)
                    continue;

                try
                {
                   // Adjust(storage, ident, val);
                }
                catch (TieException ex)
                {
                    OversizeHandler(ident, ex.Message);
                }
            }

            WriteMemory(storage);

        }

        /// <summary>
        /// Save all varibles into persistent device
        /// </summary>
        public void Save()
        {
            Save(memory.DS.Keys);
        }

        /// <summary>
        /// Load variable/pair from persistent device
        /// </summary>
        /// <param name="variables"></param>
        public void Load(IEnumerable<string> variables)
        {
            IEnumerable<KeyValuePair<string, string>> storage = ReadMemory(variables);
            Load(storage);
        }

        /// <summary>
        /// Load all varibles from persistent device
        /// </summary>
        public void Load()
        {
            IEnumerable<KeyValuePair<string, string>> storage = ReadMemory(new string[] { });
            Load(storage);
        }

        private void Load(IEnumerable<KeyValuePair<string, string>> storage)
        {
            foreach (KeyValuePair<string, string> kvp in storage)
            {
                try
                {
                    Script.Execute(string.Format("{0}={1};", kvp.Key, kvp.Value), memory);
                }
                catch (TieException ex)
                {
                    InvalidVariableHandler(kvp.Key, ex.Message);
                }
            }
        }

       
        /// <summary>
        /// Read values from persistent device by variables. Read all if variables is empty
        /// caution: Keys are dynamic generated based on length of Key/Value space.
        /// </summary>
        /// <param name="variables">varibles must be in Key FIELD of persistent device</param>
        /// <returns></returns>
        protected abstract IEnumerable<KeyValuePair<string, string>> ReadMemory(IEnumerable<string> variables);

        /// <summary>
        /// Write varibles/value pair into persistent device
        /// </summary>
        /// <param name="pairs"></param>
        protected abstract void WriteMemory(IEnumerable<KeyValuePair<string, string>> pairs);

    }
}
