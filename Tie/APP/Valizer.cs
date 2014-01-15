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
using Tie.Valization;

namespace Tie
{
    /// <summary>
    /// Convert object o VAL format and convert back
    /// </summary>
    public static class Valizer
    {
        /// <summary>
        /// Register valizer
        /// </summary>
        /// <param name="valizer"></param>
        public static void Register<T>(Valizer<T> valizer)
        {
            Register<T>(valizer, (Devalizer<T>)null);
        }

        /// <summary>
        /// Register valizer and devalizer
        /// </summary>
        /// <param name="valizer"></param>
        /// <param name="devalizer"></param>
        public static void Register<T>(Valizer<T> valizer, PartialDevalizer<T> devalizer)
        {
            ValizationMgr.Register(typeof(T), new PartialDelegateValization<T>(valizer, devalizer));
        }

        /// <summary>
        /// Register valizer and devalizer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valizer"></param>
        /// <param name="devalizer"></param>
        public static void Register<T>(Valizer<T> valizer, Devalizer<T> devalizer)
        {
            ValizationMgr.Register(typeof(T), new DelegateValization<T>(valizer, devalizer));
        }

        /// <summary>
        /// Register Valizer by object interface
        /// </summary>
        /// <param name="valizer"></param>
        public static void Register<T>(IValizer<T> valizer)
        {
            ValizationMgr.Register(typeof(T), new InterfaceValization<T>(valizer));
        }

        /// <summary>
        /// Register valizer script 
        /// </summary>
        /// <param name="valizerScript"></param>
        public static void Register<T>(string valizerScript)
        {
            ValizationMgr.Register(typeof(T), new ScriptValization(valizerScript, null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericMethod">must be static method without arguments</param>
        public static void Register(Type type, MethodInfo genericMethod)
        {
            ValizationMgr.Register(type, genericMethod, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericMethod"></param>
        /// <param name="obj">If a method is static,this argument is ignored</param>
        /// <param name="parameters">An argument list for the invoked method</param>
        public static void Register(Type type, MethodInfo genericMethod, object obj, object[] parameters)
        {
            ValizationMgr.Register(type, genericMethod, obj, parameters);
        }


        /// <summary>
        /// Register valizer by class's members
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valizerMembers"></param>
        public static void Register<T>(string[] valizerMembers)
        {
            ValizationMgr.Register(typeof(T), new PropertyValization(valizerMembers));
        }

        /// <summary>
        /// Unregister valizer
        /// </summary>
        /// <param name="type"></param>
        public static void Unregister(Type type)
        {
            ValizationMgr.Unregister(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static VAL Valize(object obj)
        {
            VAL val = HostValization.Host2Val(obj);
            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static object Devalize(VAL val)
        {
            return val.HostValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static T Devalize<T>(VAL val)
        {
            return (T)HostValization.Val2Host(val, null, typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Devalize(VAL val, Type type)
        {
            return HostValization.Val2Host(val, null, type);
        }

        public static object Devalize(VAL val, object obj)
        {
            if (obj == null)
                return null;

            return HostValization.Val2Host(val, obj, obj.GetType());
        }

    }
}
