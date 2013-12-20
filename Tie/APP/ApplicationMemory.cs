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
            memory = new Memory();
        }


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
                return (T)(object)this;
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

                storage.Add(variable, val.ToJson(""));
            }

            SaveIntoDevice(storage);
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
