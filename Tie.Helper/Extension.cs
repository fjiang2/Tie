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
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Tie.Helper
{
    public static class Extension
    {


        /// <summary>
        /// item which is not typeof(T) is ignored
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this VAL val)
        {
            List<T> list = new List<T>();
            
            foreach (var obj in val)
            {
                if (typeof(T) == typeof(VAL))
                    list.Add((T)(object)obj);
                else if(obj.HostValue is T)
                    list.Add((T)obj.HostValue);
            }

            return list;
        }

        /// <summary>
        /// item which type is not T1, T2 is ignored
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static IDictionary<T1, T2> ToDictionary<T1, T2>(this VAL val)
        {
            Dictionary<T1, T2> dict = new Dictionary<T1, T2>();

            foreach (VAL L in val)
            {
                if (!L.IsList)
                    continue;

                if (L.Count >= 2)
                {
                    object x1 = typeof(T1) == typeof(VAL) ? L[0] : L[0].HostValue;
                    object x2 = typeof(T2) == typeof(VAL) ? L[1] : L[1].HostValue;

                    if (x1 is T1 && x2 is T2)
                        dict.Add((T1)x1, (T2)x2);
                }
            }

            return dict;
        }

        /// <summary>
        /// return val when val is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T IsNull<T>(this VAL val, T defaultValue)
        {
            if (val.IsNull)
                return defaultValue;

            if (val.HostValue is T)
                return (T)val.HostValue;

             return default(T);
        }

        /// <summary>
        /// return val when val is undefined
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Undefined<T>(this VAL val, T defaultValue)
        {
            if (val.Undefined)
                return defaultValue;

            if (val.HostValue is T)
                return (T)val.HostValue;

            return default(T);
        }

        public static T ToObject<T>(this VAL val, T defaultValue)
        {
            if (typeof(T) == typeof(VAL))
                return (T)(object)val;

            if (val.HostValue is T)
                return (T)val.HostValue;

            return defaultValue;
        }


        /// <summary>
        /// Convert source into VAL, it is deep conversion except typeof(string)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static VAL ToVAL(this IEnumerable source)
        {
            VAL L = VAL.Array();
            
            foreach (var obj in source)
            {
                if (obj is IEnumerable && !(obj is string))
                    L.Add(ToVAL((IEnumerable)obj));
                else
                    L.Add(VAL.Boxing(obj));
            }

            return L;
        }


        /// <summary>
        /// Convert Dictionary to associative array
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static VAL ToVAL<T1,T2>(this IDictionary<T1,T2> source)
        {
            VAL L = VAL.Array();

            foreach (var kvp in source)
            {
                VAL L1 = VAL.Array();
                L1.Add(VAL.Boxing(kvp.Key));
                L1.Add(VAL.Boxing(kvp.Value));
                
                L.Add(L1);
            }

            return L;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static VAL ToVAL(this object obj)
        {
            Contract.Ensures(obj != null);

            Type type = obj.GetType();

            VAL val = VAL.Array();

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                val.AddMember(propertyInfo.Name, propertyInfo.GetValue(obj));
            }

            return val;
        }


        /// <summary>
        /// compress varible names, some varibles may belong to one
        /// </summary>
        /// <param name="names"></param>
        /// <returns>varible name list</returns>
        public static IEnumerable<VAR> CompressKeyNames(IEnumerable<VAR> names)
        {
            StringBuilder code = new StringBuilder("{");
            foreach (VAR name in names)
                code.Append((string)name).Append("=0;");
            code.Append("}");

            Memory DS = new Memory();
            Script.Execute(code.ToString(), DS);

            List<VAR> compressedNames = new List<VAR>();
            foreach (VAR name in DS.Names)
            {
                compressedNames.Add(name);
            }

            return compressedNames;
        }


    }

}
