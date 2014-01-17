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

namespace Tie.Valization
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


    static class ValizationMgr
    {
       private static Dictionary<Type, BaseValization> registries = new Dictionary<Type, BaseValization>();
       private static Dictionary<Type, Tuple<MethodInfo, object, object[]>> genericRegistries = new Dictionary<Type, Tuple<MethodInfo, object, object[]>>();

        public static void Register(Type type, BaseValization valization)
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

        public static bool IsRegistered(Type type)
        {
            if (type.IsGenericType)
                return genericRegistries.ContainsKey(type.GetGenericTypeDefinition());
            else
            {
                //High Priority
                if (registries.ContainsKey(type))
                    return true;

                //Low Priority
                foreach (Type ty in registries.Keys)
                {
                    if (type.IsSubclassOf(ty))
                        return true;

                    if (GenericType.HasInterface(type, ty))
                        return true; 
                }
                
                return false;
                
            }
        }

        private static BaseValization GetValization(Type type)
        {
            if (!registries.ContainsKey(type))
                InvokeGenericRegistry(type);

            //High Priority
            if (registries.ContainsKey(type))
                return registries[type];

            //Find the best matched base type
            Dictionary<int, Type> ranks = new Dictionary<int, Type>();
            foreach (Type ty in registries.Keys)
            {
                if (type.IsSubclassOf(ty))
                {
                    int rank = 1;
                    Type x = type;
                    while (x.BaseType != ty)
                    {
                        x = x.BaseType;
                        rank++;
                    }

                    ranks.Add(rank, ty);
                }
            }

            if (ranks.Count > 0)
            {
                int min = int.MaxValue;
                foreach (int rank in ranks.Keys)
                {
                    if (rank < min)
                        min = rank;
                }
                return registries[ranks[min]];
            }


            //Find the best interface
            List<Type> list = new List<Type>();
            foreach (Type ty in registries.Keys)
            {
                if (GenericType.HasInterface(type, ty))
                    list.Add(ty);
            }

            if (list.Count == 1)
                return registries[list[0]];
            else if (list.Count > 1)
            {
                string[] interfaces = new string[list.Count];
                for (int i = 0; i < interfaces.Length; i++)
                    interfaces[i] = list[i].FullName;

                throw new TieException("ambiguous valization registry interfaces: {0} for type: {1}", string.Join(",", interfaces), type.FullName);
            }

            return null;
        }


        private static void InvokeGenericRegistry(Type type)
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
        public static VAL Valize(object host)
        {
            if (host == null)
                return null;

            Type type = host.GetType();

            BaseValization valization = GetValization(type);
            if (valization != null)
            {
                return valization.Valize(host);
            }

            return null;
        }

        //用于设置[Valizable]属性地方的script处理
        public static VAL Valize(MemberInfo method, object host)
        {
            if (host == null)
                return null;

            //处理customerized的Valizable代码
            ValizableAttribute[] attributes = (ValizableAttribute[])method.GetCustomAttributes(typeof(ValizableAttribute), true);
            if (attributes.Length != 0)
            {
                object valizer = attributes[0].valizer;

                if (valizer != null)      //Field或者Property定义了[Valizable]属性,并且定义了customerized
                {

                    if (valizer is string)
                    {
                        ScriptValization x = new ScriptValization((string)valizer, null);
                        return x.Valize(host);
                    }
                    else if (valizer is string[])
                    {
                        PropertyValization x = new PropertyValization((string[])valizer);
                        return x.Valize(host);
                    }
                    else
                        throw new TieException("not supported Valizer: {0}", valizer);
                }
            }

            return Valize(host);
        }


        //把Val值解析(Devalize)为host, 用于HostValization.Val2Host(..)
        public static object Devalize(object host, Type hostType, VAL val)
        {
            BaseValization valization = GetValization(hostType);
             if (valization != null)
             {
                 return valization.Devalize(host, hostType, val);
             }
            

             return null;
        }

        


    }

}
