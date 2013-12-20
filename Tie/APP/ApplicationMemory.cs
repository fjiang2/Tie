//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tie
{
    /// <summary>
    /// used to serialize memory to persistent device, such as database server or text file
    /// varible can be simple varible or composite varible, such as "X.a", "X.a.b"
    /// </summary>
    public abstract class ApplicationMemory 
    {
        protected Memory memory;

        protected ApplicationMemory()
        {
            this.memory = new Memory();
        }

        protected ApplicationMemory(Memory memory)
        {
            this.memory = memory;
        }

        #region Adjust Variable and Value based on the maximum capacity of persistent device
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


        private void Adjust(Dictionary<string, string> storage, string variable, VAL val)
        {
            if (variable.Length > MaxVarLength)
            {
                string var = AdujstVariableName(variable, MaxVarLength);
                if (var == null)
                    throw new TieException("variable \"{0}\"  is oversize on persistent device", variable);

                val = get(var);
                string json = val.ToJson("", false);
                if (json.Length > MaxValLength)
                    throw new TieException("value of variable \"{0}\" is oversize on persistent device", variable);
                
                storage.Add(var, json);
                return;
            }
            else
            {
                string json = val.ToJson("", false);
                if (json.Length <= MaxValLength)
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
                        throw new TieException("value of variable \"{0}\" is oversize on persistent device", variable);
                    }

                }
            }
        }

        #endregion



        private VAL get(string variable)
        {
            VAL val;

            //simple varible
            if (memory.ContainsKey(variable))
                val = memory.DS[variable];
            else
                val = Script.Evaluate(variable, memory); //composite varible

            return val;
        }

        /// <summary>
        /// check if varible is defined
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public bool ContainsVariable(string variable)
        {
            VAL v = get(variable);
            return v.Defined;
        }

        /// <summary>
        /// assign value to variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="v"></param>
        public void SetValue(string variable, object v)
        {
            if (v is byte[])
            {
                Script.Execute(string.Format("{0}={1};", variable, new VAL(HostType.ByteArrayToHexString((byte[])v))), memory);
                return;
            }
            else
            {
                Script.Execute(string.Format("{0}={1};", variable, VAL.Boxing(v).Valor), memory);
                return;
            }
        }

        /// <summary>
        /// return value, variable="X.a"
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public object GetValue(string variable)
        {
            VAL v = get(variable);

            if (v.IsNull)
                return null;
            else
                return v.value;
        }

        /// <summary>
        /// return value by varible
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <returns></returns>
        public T GetValue<T>(string variable)
        {
            VAL v = get(variable);

            if (v.Undefined || v.IsNull)
            {
                return default(T);
            }
            else if (typeof(T) == typeof(VAL))
            {
                return (T)(object)v;
            }
            else
            {
                if (typeof(T) == typeof(byte[]) && v.value is string)
                {
                    return (T)(object)HostType.HexStringToByteArray((string)v.value);
                }
                else
                {
                    if (v.HostValue.GetType() == typeof(T))
                        return (T)v.HostValue;
                    else
                        return default(T);
                }
            }
        }

 
        /// <summary>
        /// Save variables into persistent device
        /// </summary>
        /// <param name="variables"></param>
        public void Save(IEnumerable<string> variables)
        {
            Dictionary<string, string> storage = new Dictionary<string, string>();

            foreach (string variable in variables)
            {
                if (variable.StartsWith("System") || variable.StartsWith("Microsoft"))
                    continue;

                VAL val = get(variable);

                if (val.IsHostType || val.IsNull || val.Undefined)
                    continue;

                try
                {
                    Adjust(storage, variable, val);
                    //storage.Add(variable, val.ToJson("", false));
                }
                catch (TieException ex)
                {
                   SaveErrorHandler(variable, ex.Message);
                }
            }

            SaveIntoDevice(storage);
            
        }

        protected virtual void SaveErrorHandler(string variable, string message)
        { 
        }

        /// <summary>
        /// Save all varibles into persistent device
        /// </summary>
        public void Save()
        {
            Save(memory.DS.Keys);
        }

        /// <summary>
        /// Load varibles from persistent device
        /// </summary>
        public void Load()
        {
            Dictionary<string, string> storage = LoadFromDevice();
            foreach (KeyValuePair<string, string> kvp in storage)
            {
                Script.Execute(string.Format("{0}={1};", kvp.Key, kvp.Value), memory);
            }
        }

        /// <summary>
        /// Maximum length of variable name string
        /// </summary>
        protected abstract int MaxVarLength { get; }

        /// <summary>
        /// Maximum length of value string
        /// </summary>
        protected abstract int MaxValLength { get; }

        /// <summary>
        /// Load varibles/value pair from persistent device
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<string, string> LoadFromDevice();

        /// <summary>
        /// Save varibles/value pair into persistent device
        /// </summary>
        /// <param name="storage"></param>
        protected abstract void SaveIntoDevice(Dictionary<string, string> storage);


        public override string ToString()
        {
            return memory.ToString();
        }
    }
}
