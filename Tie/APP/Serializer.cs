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
using System.Reflection;
using Tie.Serialization;

namespace Tie
{
    public class Serializer
    {
        /// <summary>
        /// Register valizer
        /// </summary>
        /// <param name="valizer"></param>
        public static void Register<T>(Valizer<T> valizer)
        {
            Register<T>(valizer, null);
        }

        /// <summary>
        /// Register valizer and devalizer
        /// </summary>
        /// <param name="valizer"></param>
        /// <param name="devalizer"></param>
        public static void Register<T>(Valizer<T> valizer, Devalizer<T> devalizer)
        {
            Registry.Register(typeof(T), new DelegateSerialization<T>(valizer, devalizer));
        }

        /// <summary>
        /// Register Valizer by object interface
        /// </summary>
        /// <param name="valizer"></param>
        public static void Register<T>(IValizer<T> valizer)
        {
            Registry.Register(typeof(T), new InterfaceSerialization<T>(valizer));
        }

        /// <summary>
        /// Register valizer script 
        /// </summary>
        /// <param name="valizerScript"></param>
        public static void Register<T>(string valizerScript)
        {
            Registry.Register(typeof(T), new ScriptSerialization(valizerScript, null));
        }

        public static void Register(Type type, MethodInfo genericMethod)
        {
            Registry.Register(type, genericMethod);
        }

        /// <summary>
        /// Register valizer by class's members
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizerMembers"></param>
        public static void Register<T>(string[] valizerMembers)
        {
            Registry.Register(typeof(T), new PropertySerialization(valizerMembers));
        }

        /// <summary>
        /// Unregister valizer
        /// </summary>
        /// <param name="type"></param>
        public static void Unregister(Type type)
        {
            Registry.Unregister(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static VAL Serialize(object obj)
        {
            VAL val = HostSerialization.Host2Valor(obj);
            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static object Deserialize(VAL val)
        {
            return val.HostValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T Deserialize<T>(VAL val)
        {
            return (T)Deserialize(val, typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(VAL val, Type type)
        {
            return HostSerialization.Val2Host(val, type);
        }

      
        

    }
}
