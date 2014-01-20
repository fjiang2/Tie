using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tie.Helper
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
            //VAR var = new VAR(variable);

            ////simple varible
            //VAL val = memory[var];
            //if (!val.Defined)
            //    val = Script.Evaluate(variable, memory); //composite varible

            //return val;

            return memory.GetValue(variable);
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
        /// <param name="obj"></param>
        public void SetValue(string variable, object obj)
        {
            VAL v = Valizer.Valize(obj);
            
            if (v.Undefined || v.IsNull)
                return;

            //v.ty = VALTYPE.listcon;
            //memory.SetValue(variable, v);
            Script.Execute(string.Format("{0}={1};", variable, v.ToJson()), memory);
        }

        /// <summary>
        /// return value, variable="X.a"
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public object GetValue(string variable)
        {
            VAL v = GetVAL(variable);

            if (v.Undefined || v.IsNull)
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
            
            if (v.Undefined || v.IsNull)
            {
                return null;
            }

            Valizer.Devalize(v, host);
            return host;
        }

        /// <summary>
        /// Save memory from persistent device
        /// </summary>
        public abstract void Save();


        /// <summary>
        /// Load memory from persistent device
        /// </summary>
        public abstract void Load();
        
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
