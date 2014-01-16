using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Helper
{
    /// <summary>
    /// used to serialize memory to persistent device, such as database server or text file
    /// varible can be simple varible or composite varible, such as "X.a", "X.a.b"
    /// </summary>
    public abstract class DbMemory : PersistentMemory
    {
        /// <summary>
        /// 
        /// </summary>
        protected DbMemory()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        protected DbMemory(Memory memory)
            : base(memory)
        {
        }

        #region Adjust Variable and Value based on the maximum capacity of persistent device


        private static string AdujstVariableName(string variable, int maxLength)
        {
            string[] nameSpace = variable.Split(new char[] { '.' });
            int n = nameSpace.Length - 1;

            while (n > 0)
            {
                string ns = "";
                for (int i = 0; i < n - 1; i++)
                    ns += nameSpace[i] + ".";

                ns += nameSpace[n - 1];

                if (ns.Length <= maxLength)
                    return ns;

                n--;
            }

            return null;
        }


        private static bool ValidIdent(string id)
        {
            int i = 0;
            char ch = id[i++];

            if (!char.IsLetter(ch) && ch != '_')
                return false;

            while (i < id.Length)
            {
                ch = id[i++];

                if (ch != '_' && !char.IsLetterOrDigit(ch))
                    return false;
            }

            return true;
        }

        private void Adjust(Dictionary<string, string> storage, string variable, VAL val)
        {
            if (variable.Length > MaxVariableSpaceLength)
            {
                string var = AdujstVariableName(variable, MaxVariableSpaceLength);
                if (var == null)
                    throw new ApplicationException(string.Format("variable \"{0}\"  is oversize on persistent device", variable));

                val = GetVAL(var);
                string json = ToJson(val);
                if (json.Length > MaxValueSpaceLength)
                    throw new ApplicationException(string.Format("value of variable \"{0}\" is oversize on persistent device", variable));

                storage.Add(var, json);
                return;
            }
            else
            {
                string json = ToJson(val);
                if (json.Length <= MaxValueSpaceLength)
                {
                    storage.Add(variable, json);
                    return;
                }
                else
                {
                    if (val.IsAssociativeArray())
                    {
                        foreach (VAL v in val)
                        {
                            VAL v0 = v[0];
                            VAL v1 = v[1];

                            if (ValidIdent(v0.Str))
                                Adjust(storage, string.Format("{0}.{1}", variable, v0.Str), v1);
                            else
                                Adjust(storage, string.Format("{0}[\"{1}\"]", variable, v0.Str), v1);
                        }
                    }
                    else
                    {
                        throw new ApplicationException(string.Format("value of variable \"{0}\" is oversize on persistent device", variable));
                    }

                }
            }
        }

        #endregion




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
                    Adjust(storage, ident, val);
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
        public override void Save()
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
        public override void Load()
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
        /// Maximum length of variable name string
        /// </summary>
        protected abstract int MaxVariableSpaceLength { get; }

        /// <summary>
        /// Maximum length of value string
        /// </summary>
        protected abstract int MaxValueSpaceLength { get; }


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
