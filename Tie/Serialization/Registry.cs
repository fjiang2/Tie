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

namespace Tie.Serialization
{
   
    /**
     * 
     * 用来支持已经存在的class的Valization
     * 譬如对System.Windows.Forms.TextBox的Valization
     * 
     * 定义用来产生class实例的script
     * 
     * 
     * */


    static class Registry
    {

        static Dictionary<Type, BaseSerialization> registries = new Dictionary<Type, BaseSerialization>();
        static Dictionary<Type, Tuple<MethodInfo, object, object[]>> genericRegistries = new Dictionary<Type, Tuple<MethodInfo, object, object[]>>();

        public static void Register(Type type, BaseSerialization valization)
        {
            if (registries.ContainsKey(type))
                registries.Remove(type);

            registries.Add(type, valization);
        }

        public static void Register(Type type, MethodInfo genericMethod, object host, object[] args)
        {
            if (genericRegistries.ContainsKey(type))
                genericRegistries.Remove(type);

            genericRegistries.Add(type, Tuple.Create(genericMethod, host, args));
        }

        public static void Unregister(Type type)
        {
            if (type.IsGenericType)
            {
                Type ty = type.GetGenericTypeDefinition();
                if (genericRegistries.ContainsKey(ty))
                    genericRegistries.Remove(ty);

                List<Type> list = new List<Type>();
                foreach (Type key in registries.Keys)
                {
                    if (key.IsGenericType && key.GetGenericTypeDefinition() == ty)
                        list.Add(key);
                }

                foreach (Type key in list)
                    registries.Remove(key);
            }
            else
            {
                if (registries.ContainsKey(type))
                    registries.Remove(type);
            }
        }

        public static bool Registered(Type type)
        {
            if (type.IsGenericType)
                return genericRegistries.ContainsKey(type.GetGenericTypeDefinition());
            else
                return registries.ContainsKey(type);
        }

        private static void GenericRegister(Type type)
        {
            if (!type.IsGenericType)
                return;

            Type ty = type.GetGenericTypeDefinition();
            if (!genericRegistries.ContainsKey(ty))
                return;

            MethodInfo geneticMethod = genericRegistries[ty].Item1;
            MethodInfo method = geneticMethod.MakeGenericMethod(type.GetGenericArguments());

            method.Invoke(genericRegistries[ty].Item2, genericRegistries[ty].Item3);
        }

        //处理注册过Type的customerized的Persistent代码, 用于HostValization.Host2Valor(..)
        public static VAL Serialize(object host)
        {
            if (host == null)
                return null;

            Type type = host.GetType();

            if (!registries.ContainsKey(type))
                GenericRegister(type);

            if (registries.ContainsKey(type))
            {
                return registries[type].Valize(host);
            }

            return null;
        }

        //用于设置[Valizable]属性地方的script处理
        public static VAL Serialize(MemberInfo memberInfo, object host)
        {
            if (host == null)
                return null;

            //处理customerized的Valizable代码
            ValizableAttribute[] attributes = (ValizableAttribute[])memberInfo.GetCustomAttributes(typeof(ValizableAttribute), true);
            if (attributes.Length != 0)
            {
                if (attributes[0].valizer != null)      //Field或者Property定义了[Valizable]属性,并且定义了customerized
                    return (new ScriptSerialization((string)attributes[0].valizer, null)).Valize(host);
            }

            return Serialize(host);
        }


        //把Val值解析(Devalize)为host, 用于HostValization.Val2Host(..)
        public static object Deserialize(VAL val, Type hostType)
        {
            if (!registries.ContainsKey(hostType))
                GenericRegister(hostType);

             if (registries.ContainsKey(hostType))
             {
                 return registries[hostType].Devalize(val);
             }
            
            return null;
        }

        


    }

}
