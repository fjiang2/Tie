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
using System.Collections;
using System.Text;
using System.Reflection;
using Tie.Valization;

namespace Tie
{

    /*
     *  把Host对象Valize可以序列化的值,可以用Coding.Evaluate(..)来Devalize对象
     *  
     * 
     * Concept:
     *   1.Valization
     *   2.Valizable
     *   3.Valize       Host --> Valor
     *   3.Devalize     Valor --> Host
     *   4.Valizer      delegate,用来对已经存在的class定义Valize to Valor的方法
     *   5.Devalizer
     *   6.Valor        用作devalize的VAL值
     *   7.ValizableAttribute
     *   8.NonValizedAttribute
     *   
     * 
     *  .NET class FullName 作为VAL List的Class值
     * 
     * 
     *  paths:
     *                Host2Valor()              Val2Val()           Val2Host()
     *        Host   ------------->     Valor   -------->    VAL    ---------->    Host
     *
     * 
     *                               Host2Val() 
     *        Host -------------------------------------->   VAL       
     * 
     *                                                   Valor2Host()
     *                                  Valor   ------------------------------>    Host         
     * 
     * 
     * 
     * */

 
    class HostValization
    {

        public static object NewInstance(VAL val, object[] args)
        {
            object instance = HostType.NewInstance(val.Class, args);
            Val2HostOffset(val, instance);
            return instance;
        }

    

        //Deserialize
        public static object Val2Host(VAL val, Type type)
        {
            if (val.ty == VALTYPE.nullcon)
                return null;

            if (val.ty == VALTYPE.voidcon)
                throw new TieException("cannot deserialize undefined VAL");

            if (type == typeof(VAL))
                return val;

            object host = val.HostValue;
            if (host.GetType() == type)
                return host;

            object temp = Registry.Deserialize(val, type);
            if (temp != null && (temp.GetType() == type || HostCoding.HasInterface(temp.GetType(), type)))
                return temp;
            else
            {
                if (type.IsArray)
                {
                    if (val.ty != VALTYPE.listcon)
                        return null;

                    host = Array.CreateInstance(type.GetElementType(), val.Size);
                    int i = 0;
                    foreach (object element in (Array)host)
                    {
                        HostOperation.HostTypeAssign(host, new int[] { i }, val[i].HostValue, true);
                        i++;
                    }
                }
                else
                {
                    try
                    {
                        host = Activator.CreateInstance(type, new object[] { });
                    }
                    catch (Exception)
                    {
                        throw new TieException("cannot create instance on type: {0}", type.FullName);
                    }
                }

                return Val2HostOffset(val, host);
            }

        }

        public static object Val2Host(VAL val, object obj)
        {
             return Val2HostOffset(val, obj);
        }

        //Devalize
        private static object Val2HostOffset(VAL val, object host)
        {
            FieldInfo[] fields = host.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                try
                {
                    VAL p = val[fieldInfo.Name];

                    if (p.Defined)
                    {
                        if (fieldInfo.FieldType == typeof(VAL))
                            fieldInfo.SetValue(host, p);
                        else
                            SetValue(host, fieldInfo.FieldType, fieldInfo.Name, p);
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (Exception)       //防止一些稀奇古怪的val offset
                { 
                }
            }

            PropertyInfo[] properties = host.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    VAL p = val[propertyInfo.Name];
                    if (p.Defined)
                    {
                        if (propertyInfo.CanWrite)
                        {
                            if (propertyInfo.PropertyType == typeof(VAL))
                                propertyInfo.SetValue(host, p, null);
                            else
                                SetValue(host, propertyInfo.PropertyType, propertyInfo.Name, p);
                        }
                        else   //CanRead
                        {
                            object prop = propertyInfo.GetValue(host, null);
                            if (prop is IList)
                            {
                                IList array = (IList)prop;
                                int count = array.Count;
                                object phost = p.HostValue;
                                if (phost is ICollection)       //不管p是HostType还是VALTYPE.listcon,他们的HostValue都是数组
                                {
                                    //Host的Collection值
                                    ICollection collection = (ICollection)phost;
                                    int i = 0;
                                    foreach (object x in collection)
                                    {
                                        if (i < count)
                                            array[i] = x;
                                        else
                                            array.Add(x);           //自动增长数组

                                        i++;
                                    }
                                }


                            }
                            else 
                                Val2HostOffset(p, propertyInfo.GetValue(host, null));
                        }
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            EventInfo[] events = host.GetType().GetEvents();
            foreach (EventInfo eventInfo in events)
            {
                try
                {
                    VAL p = val[eventInfo.Name];
                    if (p.Defined)
                    {
                        if (p.ty == VALTYPE.funccon)       //event handler
                        {
                            HostEvent hostEvent = new HostEvent(eventInfo, p);
                            VAL d = hostEvent.AddDelegateEventHandler();
                            eventInfo.AddEventHandler(host, d.value as Delegate);
                        }
                    }
                }
                catch (ArgumentException)
                { 
                
                }
            }

            return host;
        }


        private static void SetValue(object host, Type type, string offset, VAL val)
        {
            object temp = Registry.Deserialize(val, type);
            if (temp == null)
                temp = val.HostValue;

            if (val.IsAssociativeArray())
            {
                VAL x = HostOperation.HostTypeOffset(VAL.Boxing(host), new VAL(offset), OffsetType.STRUCT);
                Val2HostOffset(val, x.value);
            }
            else
                HostOperation.HostTypeAssign(host, offset, temp, true);
        }

        /**
         * Valize
         * 
         * 
         * 抽取.net class instance的变量和属性值,保存到VAL中,反之亦然
         * 
         * 如果instance支持IValizable interface, 那么直接call GetValData()
         * 否则:
         * public 属性, 缺省情况下被转换为VAL, 除非设置[NonValized]属性
         * public 变量, 缺省情况下不转为VAL, 除非设置[Valizable]属性, 只存储简单的属性, 有下标的属性,不考虑
         * 
         * */
        private static VAL Host2Valor(object host, VAL val)
        {
            if (host == null || host is System.DBNull)
            {
                val = new VAL();
            }
            else if (host is string || host is char 
                || host is byte
                || host is int || host is short || host is long 
                || host is bool 
                || host is double || host is float || host is decimal
                || host is DateTime)
            {
                val = VAL.Boxing1(host);
            }
            else if (host is IValizable)
            {
                val = ((IValizable)host).GetValData();
            }
            else if (host is Type)
            {
                val = VAL.NewScriptType(string.Format("typeof({0})", GenericArgument.FullName((Type)host)));
            }
            else if (host.GetType().IsEnum)            //处理enum常量
            {
                val = VAL.NewScriptType(HostOperation.EnumBitFlags(host));
            }
            else if (Registry.Registered(host.GetType()))
            {
                VAL temp = Registry.Serialize(host);
                temp.Class = host.GetType().FullName;
                return temp;
            }
            else if (host is ICollection)
            {
                val = VAL.Array();
                foreach (object a in (ICollection)host)
                {
                    val.Add(Host2Valor(a, new VAL()));
                }
            }
            else
            {
                HostOffset2Val(host, val);
            }

            val.Type = host.GetType();
            val.Class = host.GetType().FullName;
            return val;
        }

        private static void HostOffset2Val(object host, VAL val)
        {
            FieldInfo[] fields = host.GetType().GetFields(BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields)
            {
                //Field缺省不转换为VAL 除非设置ValizableAttribute属性
                Attribute[] A = (Attribute[])fieldInfo.GetCustomAttributes(typeof(ValizableAttribute), true);
                if (A.Length == 0)
                    continue;

                //处理customerized的Persistent代码
                object fieldValue = fieldInfo.GetValue(host);
                VAL persistent = Registry.Serialize(fieldInfo, fieldValue);

                if ((object)persistent == null)
                {
                    persistent = VAL.Boxing(fieldValue);
                    if (!fieldInfo.FieldType.IsValueType && persistent.IsHostType)
                    {
                        persistent = Host2Valor(fieldValue, new VAL());
                    }
                }

                val[fieldInfo.Name] = persistent;
            }

            PropertyInfo[] properties = host.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                //Property缺省情况下转为VAL, 除非设置NonValizedAttribute属性, 只存储简单的属性, 有下标的属性,不考虑
                Attribute[] A = (Attribute[])propertyInfo.GetCustomAttributes(typeof(NonValizedAttribute), true);
                if (A.Length != 0)
                    continue;

                if (!(propertyInfo.CanRead && propertyInfo.CanWrite))
                    continue;

                //if (!propertyInfo.CanRead)
                //    continue;

                if (IsStatic(propertyInfo))
                    continue;

                //处理customerized的Persistent代码
                object propertyValue = propertyInfo.GetValue(host, null);
                if (propertyValue == null)
                    continue;

                VAL persistent = Registry.Serialize(propertyInfo, propertyValue);

                if ((object)persistent == null)
                {
                    if (propertyValue is ICollection)
                    {
                        ICollection collection = (ICollection)propertyValue;
                        persistent = VAL.Array();
                        foreach (object obj in collection)
                        {
                            persistent.Add(VAL.Boxing(obj));
                        }
                    }
                    else
                    {
                        persistent = VAL.Boxing(propertyValue);
                        if (!propertyInfo.PropertyType.IsValueType && persistent.IsHostType)
                        {
                            persistent = Host2Valor(propertyValue, new VAL());
                        }
                    }
                }

                val[propertyInfo.Name] = persistent;

            }
        }

        public static bool IsStatic(PropertyInfo propertyInfo)
        {
            return ((propertyInfo.CanRead && propertyInfo.GetGetMethod().IsStatic) ||
                (propertyInfo.CanWrite && propertyInfo.GetSetMethod().IsStatic));
        }

        //用来输出Persistent对象的,不能回填到函数Val2Host(VAL, object)中, 回填要使用Host2Val(object)的输出
        public static VAL Host2Valor(object host)
        {
            VAL val = new VAL();

            return Host2Valor(host, val);
        }

        public static VAL Valor2Val(VAL valor)
        {
            return Script.Evaluate(valor.Valor);
        }

        public static VAL Host2Val(object host)
        {
            VAL valor = Host2Valor(host);
            return Valor2Val(valor);
        }

        public static object Valor2Host(VAL valor, object host)
        {
            VAL val = Valor2Val(valor);
            return Val2HostOffset(val, host);
        }
    
    }
}
