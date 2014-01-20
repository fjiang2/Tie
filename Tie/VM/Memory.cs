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
        Dictionary<VAR, VAL> ds = new Dictionary<VAR, VAL>();

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
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public VAL AddHostObject(VAR key, object v)
        {
            return Add(key, VAL.NewHostType(v));
        }

   
        /// <summary>
        /// Add object variable into data segment
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public VAL AddObject(VAR key, object v)
        {
            return Add(key, VAL.Boxing1(v));
        }

     
        /// <summary>
        /// Add VAL variable 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public VAL Add(VAR key, VAL v)
        {
            bool removed = false;
            VAL oldValue = null;

            if (ds.ContainsKey(key))
            {
                oldValue = ds[key];
                removed = ds.Remove(key);
            }

            v.name = key.Ident;
            ds.Add(key, v);

            return oldValue;
        }


      

        #endregion


        /// <summary>
        /// Dictionary of varible
        /// </summary>
        internal Dictionary<VAR, VAL> DS
        {
            get
            {
                return ds;
            }
        }


        internal bool ContainsKey(VAR key)
        {
            return ds.ContainsKey(key);
        }


        /// <summary>
        /// Get value by variable name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VAL this[VAR key]
        {
            get
            {
                if (ds.ContainsKey(key))
                    return ds[key];
               else
                   return VAL.NewVoidType(); 
            }
            set
            {
                Add(key, value);              //ds[key] = value;
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
        /// <param name="names"></param>
        /// <param name="val"></param>
        public void SetValue(string variable, VAL val)
        {
            if (val.Undefined || val.IsNull)
                return;

            string[] names = variable.Split(new char[] { '.' });

            if (names.Length == 0)
                return;

            VAR _var = new VAR(names[0]);
            VAL _val;

            if (this.ContainsKey(_var))
            {
                if (names.Length > 1)
                {
                    _val = this[_var];
                    VAL.Assign(_val, names, 1, val);
                }
                else
                    this[_var] = val;
            }
            else
            {
                if (names.Length > 1)
                {
                    _val = new VAL(new VALL());
                    VAL.Assign(_val, names, 1, val);
                    this.Add(_var, _val);
                }
                else
                    this.Add(_var, val);
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
            
            _val.Remove(names[names.Length - 1]);

        }

 
        #endregion


        /// <summary>
        /// all variables name in memory
        /// </summary>
        public ICollection<VAR> Keys
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
        /// <param name="key">varible name</param>
        /// <returns></returns>
        public bool Remove(VAR key)
        {
            if (ds.ContainsKey(key))
                return ds.Remove(key);

            return false;
        }

        #endregion


        /// <summary>
        /// Clear void or null value
        /// </summary>
        /// <param name="key"></param>
        public void ClearNullorVoid(VAR key)
        {
            if (!ContainsKey(key))
                return;

            VAL dict = this[key];
            
            if (dict.Undefined || dict.IsNull)
                Remove(key);


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



        //压缩KeyNames,合并结构分量类型的变量
        /// <summary>
        /// compress varible names, some varibles may belong to one
        /// </summary>
        /// <param name="keyNames"></param>
        /// <returns>varible name list</returns>
        public static IEnumerable<VAR> CompressKeyNames(IEnumerable<VAR> keyNames)
        {
            StringBuilder code = new StringBuilder("{");
            foreach (VAR key in keyNames)
                code.Append(key.Ident).Append("=0;");
            code.Append("}");

            Memory SS = new Memory();
            Computer.Run("", code.ToString(), CodeType.statements, new Context(SS));

            List<VAR> compactedKeyNames = new List<VAR>();
            foreach (VAR key in SS.DS.Keys)
            {
                compactedKeyNames.Add(key);
            }

            return compactedKeyNames;
        }

        //从DS中抽取keyNames的值
        /// <summary>
        /// Copy some varibles into new varible dictionary
        /// </summary>
        /// <param name="compactedKeyNames"></param>
        /// <returns></returns>
        public Memory CopyBlock(IEnumerable<VAR> compactedKeyNames)
        {

            Memory XS = new Memory();
            foreach (VAR key in compactedKeyNames)
            {
                if (DS.ContainsKey(key))
                {
                    VAL x = DS[key];
                    if (x.ty != VALTYPE.nullcon)
                        XS.Add(key, x);
                }
            }
            return XS;
        }

        /// <summary>
        /// Remove unchanged varibles
        /// </summary>
        /// <param name="referenceMemory">reference varibles</param>
        public void RemoveValueUnchangedBlock(Memory referenceMemory)
        {
            foreach (KeyValuePair<VAR, VAL> kvp in DS)
            {
                if (referenceMemory[kvp.Key] != kvp.Value)
                    DS.Remove(kvp.Key);
            }
            return;
        }

     

        /// <summary>
        /// explicit convert varible dictionary into VAL associative array
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator VAL(Memory x)
        {
            return new VAL(x);
        }

    }
}

