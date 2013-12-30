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
    public abstract class PersistentMemory 
    {
        protected Memory memory;

        protected PersistentMemory()
            :this( new Memory())
        {
        }

        protected PersistentMemory(Memory memory)
        {
            this.memory = memory;

         
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


        private void Adjust(Dictionary<string, string> storage, string variable, VAL val)
        {
            if (variable.Length > MaxVariableSpaceLength)
            {
                string var = AdujstVariableName(variable, MaxVariableSpaceLength);
                if (var == null)
                    throw new TieException("variable \"{0}\"  is oversize on persistent device", variable);

                val = get(var);
                string json = ToJson(val);
                if (json.Length > MaxValueSpaceLength)
                    throw new TieException("value of variable \"{0}\" is oversize on persistent device", variable);
                
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

                            if (ident.ValidIdent(v0.Str))
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

        private string ToJson(VAL val)
        {
            return val.ToJson("", ExportFormat.QuestionMark);
            //return val.ToJson("", ExportFormat.QuestionMark | ExportFormat.EncodeTypeof);
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
            VAL val = HostSerialization.Host2Valor(v);
            Script.Execute(string.Format("{0}={1};", variable, val.Valor), memory);
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
                if (v.HostValue.GetType() == typeof(T))
                    return (T)v.HostValue;
                else
                {
                    //used on regular JSON without typeof(list)
                    object host = HostSerialization.Val2Host(v, typeof(T));
                    return (T)host;
                }

            }
        }


        /// <summary>
        /// return value of variable, type must have default constructor
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetValue(string variable, Type type)
        {
            VAL v = get(variable);


            if (v.Undefined || v.IsNull)
            {
                return null;
            }
            else if (type == typeof(VAL))
            {
                return v;
            }
            else
            {
                if (v.HostValue.GetType() == type)
                    return v.HostValue;
                else
                {
                    //used on regular JSON without typeof(list)
                    return HostSerialization.Val2Host(v, type);
                }

            }
            
        }

        /// <summary>
        /// return value of varible, host is instantiated which is used for interface type of object
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public object GetValue(string variable, object host)
        {
            VAL v = get(variable);
            HostSerialization.Val2Host(v, host);
            return host;
        }


        /// <summary>
        /// variable or value is oversize
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="message"></param>
        protected virtual void OversizeHandler(string variable, string message)
        {
        }

        /// <summary>
        /// invalid variable/value pair in persistent device
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="message"></param>
        protected virtual void InvalidVariableHandler(string variable, string message)
        {
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

                if (val.IsNull || val.Undefined)
                    continue;

                try
                {
                    Adjust(storage, variable, val);
                }
                catch (TieException ex)
                {
                   OversizeHandler(variable, ex.Message);
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
            IEnumerable<KeyValuePair<string, string>> storage = ReadMemory(new string[]{});
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
        protected abstract void WriteMemory(IEnumerable<KeyValuePair<string,string>> pairs);


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return memory.ToString();
        }
    }
}
