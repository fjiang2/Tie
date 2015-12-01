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
using System.Collections.Generic;
using System.Text;

namespace Tie
{
    /// <summary>
    /// Variable dictionary
    /// </summary>
    public sealed class Memory 
    {
        private Dictionary<VAR, VAL> ds = new Dictionary<VAR, VAL>();

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public Memory()
        {
        }

        /// <summary>
        /// Initializes a new instance by associative array
        /// </summary>
        /// <param name="dict"></param>
        public Memory(VAL dict)
        { 
            for (int i = 0; i < dict.Size; i++)
            {
                Add(new VAR(dict[i][0].Str), dict[i][1]);
            }
        }

        #region Add to Memory

        /// <summary>
        /// Add host object variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public VAL AddHostObject(VAR name, object value)
        {
            return Add(name, VAL.NewHostType(value));
        }

   
        /// <summary>
        /// Add object variable into data segment
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public VAL AddObject(VAR name, object value)
        {
            return Add(name, VAL.Boxing1(value));
        }

     
        /// <summary>
        /// Add VAL variable 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public VAL Add(VAR name, VAL val)
        {
            bool removed = false;
            VAL oldValue = null;

            if (ds.ContainsKey(name))
            {
                oldValue = ds[name];
                removed = ds.Remove(name);
            }

            val.name = name.Ident;
            ds.Add(name, val);

            return oldValue;
        }


      

        #endregion


        /// <summary>
        /// Dictionary of varible
        /// </summary>
        internal IDictionary<VAR, VAL> DS
        {
            get
            {
                return ds;
            }
        }


        internal bool Exists(VAR name)
        {
            return ds.ContainsKey(name);
        }


        /// <summary>
        /// Get value by variable name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public VAL this[VAR name]
        {
            get
            {
                if (ds.ContainsKey(name))
                    return ds[name];
               else
                   return VAL.NewVoidType(); 
            }
            set
            {
                Add(name, value);              //ds[name] = value;
            }
        }

        #region GetValue/SetValue

        /// <summary>
        /// return value from memory.
        ///   e.g. GetValue("Place.City.Zip");
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public VAL GetValue(string variable)
        {
            string[] names = variable.Split(new char[] { '.' });
            
            if (names.Length == 0)
                return VAL.NewVoidType();
            
            VAR _var = new VAR(names[0]);

            VAL _val = this[_var];

            if (names.Length == 1)
                return _val;

            int i=1;
            while(i < names.Length)
            {
                _val = _val[names[i]];
                if (_val.Undefined)
                    return _val;

                i++;
            }
            
            return _val;
        }

        /// <summary>
        /// Add a value to element
        ///     e.g. 
        ///     Tie: Place.City.Zip = 20341;
        ///      C#: SetValue("Place.City.Zip", new VAL(20341));
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="val"></param>
        public void SetValue(string variable, VAL val)
        {
            if (val.Undefined || val.IsNull)
                return;

            string[] names = variable.Split(new char[] { '.' });

            if (names.Length == 0)
                return;

            VAR _var = new VAR(names[0]);

            if (names.Length == 1)
            {
                if (this.Exists(_var))
                    this[_var] = val;
                else
                    this.Add(_var, val);
            }
            else
            {
                VAL _val;
                if (this.Exists(_var))
                {
                    _val = this[_var];
                    VAL.Assign(_val, names, 1, val);
                }
                else
                {
                    _val = new VAL(new VALL());
                    VAL.Assign(_val, names, 1, val);
                    this.Add(_var, _val);
                }
            }
        }

        /// <summary>
        /// Remove element
        ///     e.g.
        ///     RemoveValue("Place.City");
        /// </summary>
        /// <param name="variable"></param>
        public void RemoveValue(string variable)
        {
            string[] names = variable.Split(new char[] { '.' });

            if (names.Length == 0)
                return;

            VAR _var = new VAR(names[0]);

            VAL _val = this[_var];

            if (names.Length == 1)
            {
                this.Remove(_var);
                return;
            }

            int i = 1;
            while (i < names.Length -1)
            {
                _val = _val[names[i]];
                
                if (_val.Undefined)
                    return;

                i++;
            }

            _val.RemoveMember(names[names.Length - 1]);

        }

 
        #endregion


        /// <summary>
        /// all variables name in memory
        /// </summary>
        public IEnumerable<VAR> Names
        {
            get
            {
                return ds.Keys;
            }
        }


        #region Remove/RemoveAll

        /// <summary>
        /// Clear varible dictionary
        /// </summary>
        public void RemoveAll()
        {
            ds.Clear();
        }


        /// <summary>
        /// Remove a variable
        /// </summary>
        /// <param name="name">varible name</param>
        /// <returns></returns>
        public bool Remove(VAR name)
        {
            if (ds.ContainsKey(name))
                return ds.Remove(name);

            return false;
        }

        #endregion


        /// <summary>
        /// Clear void or null value
        /// </summary>
        /// <param name="name"></param>
        public void ClearNullorVoid(VAR name)
        {
            if (!Exists(name))
                return;

            VAL dict = this[name];
            
            if (dict.Undefined || dict.IsNull)
                Remove(name);
            else
                dict.ClearNullorVoid();
        }

      


        
        /// <summary>
        ///   Converts the value of this instance to a System.String.
        /// </summary>
        /// <returns> A string whose value is the same as this instance.</returns>
        public override string ToString()
        {
            StringBuilder code = new StringBuilder();
            foreach (KeyValuePair<VAR, VAL> kvp in DS)
            {
                if (kvp.Value.ty != VALTYPE.nullcon)
                    code.Append(string.Format("{0}={1};", kvp.Key, kvp.Value));
            }
            return code.ToString();
        }


        internal static VAL Assemble(Memory memory)
        {
            VALL L = new VALL();
            foreach (KeyValuePair<VAR, VAL> pair in memory.ds)
            {
                L.Add(pair.Key.Ident, pair.Value);
            }
            return new VAL(L);

        }




        //从DS中抽取keyNames的值
        /// <summary>
        /// Copy some varibles into new varible dictionary
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public Memory CopyBlock(IEnumerable<VAR> names)
        {

            Memory XS = new Memory();
            foreach (VAR name in names)
            {
                if (DS.ContainsKey(name))
                {
                    VAL x = DS[name];
                    if (x.ty != VALTYPE.nullcon)
                        XS.Add(name, x);
                }
            }
            return XS;
        }

        /// <summary>
        /// Remove unchanged varibles
        /// </summary>
        /// <param name="referenceMemory">reference varibles</param>
        public void RemoveUnchangedBlock(Memory referenceMemory)
        {
            foreach (KeyValuePair<VAR, VAL> kvp in DS)
            {
                if (referenceMemory[kvp.Key] != kvp.Value)
                    DS.Remove(kvp.Key);
            }
            return;
        }


        /// <summary>
        /// return default value when property is undefined, otherwise return this value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValue<T>(string variable, T defaultValue = default(T))
        {
            VAL val = GetValue(variable);
            if (val.Defined)
            {
                if (typeof(T) == typeof(VAL))
                    return (T)(object)val;
                if (val.HostValue is T)
                    return (T)val.HostValue;
                else if (typeof(T).IsEnum && val.HostValue is int)
                    return (T)val.HostValue;
            }

            return defaultValue;
        }


        /// <summary>
        /// explicit convert varible dictionary into VAL associative array
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static explicit operator VAL(Memory memory)
        {
            return new VAL(memory);
        }

    }
}

