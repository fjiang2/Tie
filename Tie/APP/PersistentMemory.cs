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
    /// 
    /// </summary>
    public abstract class PersistentMemory
    {
        /// <summary>
        /// 
        /// </summary>
        protected Memory memory;

        /// <summary>
        /// 
        /// </summary>
        protected PersistentMemory()
            : this(new Memory())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        protected PersistentMemory(Memory memory)
        {
            this.memory = memory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        protected VAL GetVAL(string variable)
        {
            VAL val;
            VAR var = new VAR(variable);

            //simple varible
            if (memory.ContainsKey(var))
                val = memory.DS[var];
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
            VAL v = GetVAL(variable);
            return v.Defined;
        }

        /// <summary>
        /// assign value to variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="v"></param>
        public void SetValue(string variable, object v)
        {
            VAL val = Valizer.Valize(v);
            Script.Execute(string.Format("{0}={1};", variable, val.Valor), memory);
        }

        /// <summary>
        /// return value, variable="X.a"
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public object GetValue(string variable)
        {
            VAL v = GetVAL(variable);

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
            object obj = GetValue(variable, typeof(T));

            if (obj == null)
                return default(T);
            else
                return (T)obj;
        }


        /// <summary>
        /// return value of variable, type must have default constructor
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetValue(string variable, Type type)
        {
            VAL v = GetVAL(variable);


            if (type == typeof(VAL))
            {
                return v;
            }
            else if (v.Undefined || v.IsNull)
            {
                return null;
            }
            else
            {
                return Valizer.Devalize(v, type);
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
            VAL v = GetVAL(variable);
            HostValization.Val2Host(v, host);
            return host;
        }


     
        
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
