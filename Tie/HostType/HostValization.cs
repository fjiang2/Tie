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
     * */


    class HostValization
    {

        public static object NewInstance(VAL val, object[] args)
        {
            object instance = HostType.NewInstance(val.Class, args);
            Val2HostOffset(val, instance);
            return instance;
        }


        #region Val -> Host
        //Deserialize
        public static object Val2Host(VAL val, object host, Type type)
        {
            if (val.ty == VALTYPE.nullcon)
                return null;

            if (val.ty == VALTYPE.voidcon)
                throw new TieException("cannot deserialize undefined VAL");

            if (type == typeof(VAL))
                return val;

            object hostValue = val.HostValue;
            if (GenericType.IsCompatibleType(hostValue.GetType(), type))
                return hostValue;

            else if (host is IValizable)
            {
                ((IValizable)host).SetVAL(val);
                return host;
            }
            //如果是匿名class
            else if (CheckIfAnonymousType(type))
            {
                PropertyInfo[] properties = type.GetProperties();
                object[] args = new object[properties.Length];
                int i = 0;

                foreach (PropertyInfo propertyInfo in properties)
                {
                    VAL p = val[propertyInfo.Name];

                    //处理匿名内嵌class
                    if (CheckIfAnonymousType(propertyInfo.PropertyType))
                    {
                        args[i] = Val2Host(p, null, propertyInfo.PropertyType);
                    }
                    else
                    {
                        args[i] = p.Defined ? p.HostValue : null;
                    }

                    i++;
                }

                try
                {
                    hostValue = Activator.CreateInstance(type, args);
                    return hostValue;
                }
                catch (Exception)
                {
                    throw new TieException("cannot create instance on anonymous type: {0}", type.FullName);
                }
            }


            object temp = ValizationMgr.Devalize(host, type, val);
            if (temp != null && GenericType.IsCompatibleType(temp.GetType(), type))
                return temp;
            else
            {
                if (type.IsArray)
                {
                    if (val.ty != VALTYPE.listcon)
                        return null;

                    if (host != null)
                        hostValue = host;
                    else
                        hostValue = Array.CreateInstance(type.GetElementType(), val.Size);

                    int i = 0;
                    foreach (object element in (Array)hostValue)
                    {
                        if (element != null)
                        {
                            HostOperation.HostTypeAssign(hostValue, new int[] { i }, val[i].HostValue, true);
                        }
                        else
                        {
                            //object array
                            object x = Val2Host(val[i], null, type.GetElementType());
                            (hostValue as Array).SetValue(x, i);
                        }
                        i++;
                    }
                }
                else
                {
                    try
                    {
                        if (host != null)
                            hostValue = host;
                        else
                            hostValue = Activator.CreateInstance(type, new object[] { });
                    }
                    catch (Exception)
                    {
                        throw new TieException("cannot create instance on type: {0}", type.FullName);
                    }
                }

                return Val2HostOffset(val, hostValue);
            }

        }

        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                return false;

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static object Val2Host(VAL val, object obj)
        {
            return Val2HostOffset(val, obj);
        }

        //Devalize
        private static object Val2HostOffset(VAL val, object host)
        {
#if GET_FIELDS
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
                            SetValue(host, fieldInfo.GetValue(host), fieldInfo.FieldType, fieldInfo.Name, p);
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (Exception)       //防止一些稀奇古怪的val offset
                { 
                }
            }
#endif
            Type type = host.GetType();
            PropertyInfo[] properties = type.GetProperties();
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
                                SetValue(host, propertyInfo.GetValue(host, null), propertyInfo.PropertyType, propertyInfo.Name, p);
                        }
                        else   //CanRead
                        {
                            object prop = propertyInfo.GetValue(host, null);
                            if (prop is IList)
                            {
                                IList array = (IList)prop;
                                int count = array.Count;
                                object phost = p.HostValue;
                                if (phost is IEnumerable && !(phost is string))       //不管p是HostType还是VALTYPE.listcon,他们的HostValue都是数组
                                {
                                    //Host的Collection值
                                    IEnumerable collection = (IEnumerable)phost;
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

            EventInfo[] events = type.GetEvents();
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


        private static void SetValue(object host, object member, Type type, string offset, VAL val)
        {
            object temp = ValizationMgr.Devalize(member, type, val);
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

        #endregion


        #region Host -> Val
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
        private static VAL Host2Val(object host, VAL val)
        {
            if (host == null)
            {
                val = new VAL();
                return val;
            }

            Type type = host.GetType();

            if (host is string || type.IsPrimitive
                //|| host is char 
                //|| host is byte
                //|| host is int || host is short || host is long 
                //|| host is bool 
                //|| host is double || host is float || host is decimal
                )
            {
                val = VAL.Boxing1(host);
            }
            else if (host is VAL)
            {
                val = (VAL)host;
            }
            else if (host is IValizable)
            {
                val = ((IValizable)host).GetVAL();
            }
            else if (host is Type)
            {
                val = VAL.NewScriptType(string.Format("typeof({0})", new GenericType(host).TypeName));
            }
            else if (host is System.DBNull)
            {
                val = new VAL();
            }
            else if (ValizationMgr.IsRegistered(type))
            {
                VAL temp = ValizationMgr.Valize(host);
                temp.Class = type.FullName;
                return temp;
            }
            else if (type.IsEnum)            //处理enum常量
            {
                val = VAL.NewEnumType((Enum)host);
            }
            else if (host is IEnumerable && !(host is string))
            {
                val = VAL.Array();
                foreach (object a in (IEnumerable)host)
                {
                    val.Add(Host2Val(a, new VAL()));
                }
            }
            else
            {
                HostOffset2Val(host, val);
            }

            val.Type = type;
            val.Class = type.FullName;
            return val;
        }

        private static void HostOffset2Val(object host, VAL val)
        {

#if GET_FIELDS
            FieldInfo[] fields = host.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                //Field缺省不转换为VAL 除非设置ValizableAttribute属性
                Attribute[] A = (Attribute[])fieldInfo.GetCustomAttributes(typeof(ValizableAttribute), true);
                if (A.Length == 0)
                    continue;

                //处理customerized的Persistent代码
                object fieldValue = fieldInfo.GetValue(host);
                VAL persistent = ValizationMgr.Valize(fieldInfo, fieldValue);

                if ((object)persistent == null)
                {
                    persistent = VAL.Boxing(fieldValue);
                    if (!fieldInfo.FieldType.IsValueType && persistent.IsHostType)
                    {
                        persistent = Host2Val(fieldValue, new VAL());
                    }
                }

                val[fieldInfo.Name] = persistent;
            }
#endif
            Type type = host.GetType();
            PropertyInfo[] properties = type.GetProperties();
            bool isAnomymous = CheckIfAnonymousType(type);
            foreach (PropertyInfo propertyInfo in properties)
            {
                //Property缺省情况下转为VAL, 除非设置NonValizedAttribute属性, 只存储简单的属性, 有下标的属性,不考虑
                Attribute[] A = (Attribute[])propertyInfo.GetCustomAttributes(typeof(NonValizedAttribute), true);
                if (A.Length != 0)
                    continue;

                if (!(propertyInfo.CanRead && propertyInfo.CanWrite) && !isAnomymous)
                    continue;

                if (!propertyInfo.CanRead)
                    continue;

                if (IsStatic(propertyInfo))
                    continue;

                //处理customerized的Persistent代码
                object propertyValue = propertyInfo.GetValue(host, null);
                if (propertyValue == null)
                    continue;

                VAL persistent = ValizationMgr.Valize(propertyInfo, propertyValue);

                if ((object)persistent == null)
                {
                    if (propertyValue is IEnumerable && !(propertyValue is string))
                    {
                        IEnumerable collection = (IEnumerable)propertyValue;
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
                            persistent = Host2Val(propertyValue, new VAL());
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
        public static VAL Host2Val(object host)
        {
            VAL val = new VAL();
            if (host == null)
                return val;

            return Host2Val(host, val);
        }

        #endregion


    }
}
