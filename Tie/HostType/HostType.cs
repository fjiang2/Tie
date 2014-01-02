﻿//--------------------------------------------------------------------------------------------------//
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Tie
{

    /// <summary>
    /// Represent .NET object Type
    /// </summary>
    public partial class HostType
    {

        #region Register Type Functions

        //---------------------------------------------------------------------------------------

        /// <summary>
        /// Register .NET type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Register(Type type)
        {
            return Register(type, false);
        }

        public static bool Register<T>()
        {
            return Register(typeof(T), false);
        }
   
        /// <summary>
        /// Register multiple .NET types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static bool Register(Type[] types)
        {
            return Register(types, false);
        }


        /// <summary>
        /// Register all types of assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool Register(Assembly assembly)
        {
            return Register(assembly, false);
        }

        /// <summary>
        /// Register .NET type with brief/short name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="briefName"></param>
        /// <returns></returns>
        public static bool Register(Type type, bool briefName)
        {
            return Register(new Type[] { type }, briefName);
        }

        /// <summary>
        /// Register multiple .NET types with brief/short name
        /// </summary>
        /// <param name="types"></param>
        /// <param name="briefName"></param>
        /// <returns></returns>
        public static bool Register(Type[] types, bool briefName)
        {
            string code = "";
            for(int i=0; i< types.Length; i++)
            {
                Type type = types[i];
                 Computer.DS1.Add("$" + i, VAL.NewHostType(type));

                code += string.Format("{0}=${1};", type.FullName,i);

                if (briefName)
                    code += string.Format("{0}=${1};", type.Name, i);


                /*
                 * 为了支持extend methods, 把函数名称直接插入到Computer.DS1中去, 允许直接使用静态函数名,而不要class名字
                 * 不同class可能有相同的method name, 需要合并, 例如:
                 *   IEnumerable, IQueryable 都有Where方法
                 * 
                 * */
                if (type.IsClass && type.IsAbstract && type.IsSealed)   
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);   //静态函数
                    foreach (MethodInfo methodInfo in methods)
                    {
                        VAL method;
                        string name = methodInfo.Name;

                        //如果有overloading的函数,以数组方式保存全部的methodInfo
                        if (Computer.DS1.ContainsKey(name))
                        {
                            List<MethodInfo> L = new List<MethodInfo>();
                            object m = Computer.DS1[name].value;
                            if (m is MethodInfo)
                                L.Add((MethodInfo)m);
                            else if (m is MethodInfo[])
                                L.AddRange((MethodInfo[])m);
                            else                                   
                            {
                                //可能其他的变量名,碰巧和extend methods的名字是一样的
                                RuntimeException.Warning("{0}.{1}(...) conflict with variable/function {2} during extend method registering", type.FullName, name, Computer.DS1[name]);        
                                continue;   
                            }

                            if (L.IndexOf(methodInfo) == -1)
                            {
                                L.Add(methodInfo);

                                Computer.DS1.Remove(name);
                                method = VAL.NewHostType(L.ToArray());
                            }
                            else
                                continue;
                        }
                        else
                            method = VAL.NewHostType(methodInfo);

                        Computer.DS1.Add(name, method);
                        method.temp = new HostOffset(typeof(object), name);      //因为这里是static方法,所以host随便设为object, 参见HostTypeFunction(VAL proc, VALL parameters), 
                    }
                }
            }

            Script.Execute(code, Computer.DS1);
            
            for (int i = 0; i < types.Length; i++)
                Computer.DS1.Remove("$" + i);

            return true;
        }


        /*
         * 这个函数主要是用于Generic class的Register, 因为Generic class的Name和FullName包含有'字符,
         *  如: FullName = "System.Collections.Generic.Dictionary'2"
         *  
         * 用法例子:
         *      HostType.Register("DictionaryStringInt32",typeof(Dictionary<string,int>));
         *      或者
         *      HostType.Register("Dictionary.StringInt32",typeof(Dictionary<string,int>)); 
         * 
         * 
         */
        
        /// <summary>
        /// Using alias directive for a generic class. 
        /// ex:
        ///     C#: using UsingAlias = NameSpace2.MyClass<int>;
        ///    Tie: Register("UsingAlias", typeof(NameSpace2.MyClass<int>);
        /// </summary>
        /// <param name="typeName">type name in script</param>
        /// <param name="type">generic type</param>
        /// <returns></returns>
        public static bool Register(string typeName, Type type)
        {
            Computer.DS1.Add("$1", VAL.NewHostType(type));
            Script.Execute(typeName + "=$1;", Computer.DS1);
            Computer.DS1.Remove("$1");
            return true;
        }


        /// <summary>
        /// Register all types of assembly with brief/short name
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="briefName"></param>
        /// <returns></returns>
        public static bool Register(Assembly assembly, bool briefName)
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                try
                {
                    if (!type.IsNestedPublic)
                        HostType.Register(type, briefName);
                }
                catch (Exception)
                {
                    Logger.WriteLine(string.Format("{0} cannot be registed.", type.FullName));
                    return false;
                }
            }

            return true;
        }

        internal static bool IsRegistered(Type type)
        {
            VAL val = Script.Evaluate(type.FullName, Computer.DS1);
            return val.Defined;
        }

        #endregion

        
        #region Add Reference

        private static List<Assembly> references = new List<Assembly>();
        //Dictionary<alias, import>
        private static Dictionary<string, string> aliases = new Dictionary<string, string>(); 
        //List<import>
        private static List<string> imports = new List<string>();

        /**
         * 
         *   AddReference("Tie2");
         *   AddReference(Assembly.Load("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
         *   AddReference(Assembly.Load("System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
         *   
         *   
         * */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddReference(Assembly assembly)
        {
            if (references.IndexOf(assembly) >= 0)
                return;

            references.Add(assembly);
        }

        /// <summary>
        /// using System.Data; 
        ///     is equivalent to AddImport("System.Data");
        /// </summary>
        /// <param name="import"></param>
        public static void AddImport(string import)
        {
            Assembly assembly = InvalidNamespace(import);
            if (assembly == null)
                throw new TieException("invalid namespace:{0}", import);

            if (imports.IndexOf(import) >= 0)
                return;

            imports.Add(import);
        }

        /// <summary>
        ///      C#: using SysData = System.Data;
        ///     Tie: AddImport("System.Data", "SysData");
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="import"></param>
        public static void AddImport(string alias, string import)
        {
            AddImport(import);

            if (aliases.ContainsKey(alias))
                aliases.Remove(alias);

            aliases.Add(alias, import);
        }

        private static Assembly InvalidNamespace(string ns)
        {
            foreach (Assembly assembly in references)
            {
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (type.Namespace.Equals(ns))
                        return assembly;
                }
            }

            return null;
        }


        private static Type GetReferenceType(string fullTypeName)
        {
            foreach (Assembly assembly in references)
            {
                Type type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }

            return null;
        }

    
        #endregion


        #region GetClassType() + NewInstance()
        
        /// <summary>
        /// new instance of class
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="constructorargs">constructor arguments</param>
        /// <returns></returns>
        public static object NewInstance(string className, object[] constructorargs)
        {
            Type type = GetType(className);
            if (type != null)
                   return Activator.CreateInstance(type, constructorargs);
            else
                return null;
        }

        /// <summary>
        /// Return .NET type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            string fullTypeName;
            
            if (typeName.IndexOf('.') >= 0 )
            {
                string[] names = typeName.Split(new char[] { '.' });

                if (aliases.ContainsKey(names[0]))
                {
                    names[0] = aliases[names[0]];
                    fullTypeName = string.Join(".", names);

                    Type type = GetFullType(fullTypeName);
                    if (type != null)
                        return type;
                }
            }

            foreach (string import in imports)
            {
                fullTypeName = import + "." + typeName;

                Type type = GetFullType(fullTypeName);
                if (type != null)
                    return type;
            }

            return GetFullType(typeName);
        }

        /// <summary>
        /// Return .NET type
        /// </summary>
        /// <param name="fullTypeName">type name</param>
        /// <returns></returns>
        private static Type GetFullType(string fullTypeName)
        {
            Type type = null;

            //删除空格
            fullTypeName = fullTypeName.Replace(" ","");
            int isArray = 0;    //数组类型的维数

            //支持多维数组
            while (fullTypeName.EndsWith("[]"))
            {
                isArray++;
                fullTypeName = fullTypeName.Substring(0, fullTypeName.Length - 2); 
            }

            //1.搜索System空间,加速返回基本类型
            type = typeof(object).Assembly.GetType(fullTypeName);
            if (type != null)
                goto L1;

            //2.搜索referecne空间
            type = GetReferenceType(fullTypeName);
            if (type != null)
                 goto L1;

#if !SILVERLIGHT
            //3.在当前的domain中的Assembly中搜索,然后CreateInstance
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(fullTypeName);
                if (type != null)
                    goto L1;

            }
#endif
            //4.根据class名字来推断assemblyName,然后返回
            type = GetDefaultAssemblyType(fullTypeName);
            if (type != null)
                goto L1;

            return null;


            L1:
            while (isArray > 0)
            {
                isArray--;
                type = type.MakeArrayType();    //产生多维数组
            }

            return type;
        }


   
        //根据class name来判断assembly的名字
        private static Type GetDefaultAssemblyType(string className)
        {
            string[] nameSpace = className.Split(new char[] { '.' });
            int n = nameSpace.Length - 1;

            while (n > 0)
            {
                string ns = "";
                for (int i = 0; i < n - 1; i++)
                    ns += nameSpace[i] + ".";

                ns += nameSpace[n - 1];

                try
                {
                    Assembly assembly = Assembly.Load(ns);
                    Type type = assembly.GetType(className);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                }

                n--;
            }

            return null;
        }

          
        #endregion



        #region Property Extraction


        
        /// <summary>
        /// New instance by persistent data
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object NewInstance(VAL valor, object[] args)
        {
            return HostValization.NewInstance(valor, args);
        }


#if OBSOLETE
        /*支持VAL对.NET对象的属性的取值
         * 
         * 
         * 
         * 
         * */
        public static VAL GetObjectProperties(object host, VAL properties)
        {
            VAL result = new VAL(new VALL());
            if (properties.IsAssociativeArray())
            {
                for (int i = 0; i < properties.Size; i++)
                {
                    VAL property = properties[i];
                    VAL offset = property[0];
                    VAL val = property[1];

                    VAL child;
                    try
                    {
                        child = HostOperation.HostTypeOffset(host, offset.value, OffsetType.STRUCT);
                    }
                    catch (HostTypeMemberNotFoundException)
                    {
                        child = null;
                    }

                    if (!child.Defined)
                        continue;  //child = val;             //如果想保持原有不存在属性的值,使用注解的语句去掉,去掉continue
                    else if (child.ty == VALTYPE.hostcon)
                    {
                        if (child.value != null)
                        {
                             if(!child.value.GetType().IsValueType)
                                child = GetObjectProperties(child.value, val);
                        }
                        else
                            continue; //child = val;          //如果想保持原有不存在属性的值,使用注解的语句去掉,去掉continue
                    }

                    VALL L = new VALL();
                    L.Add(offset);
                    L.Add(child);
                    result.List.Add(new VAL(L));
                }
            }
            else
            {
                VAL array = properties;
                for (int i = 0; i < array.Size; i++)
                {
                    VAL item = array[i];
                    VAL sibling = HostOperation.HostTypeOffset(host, i, OffsetType.ARRAY);

                    if (!sibling.Defined)
                        result[i] = item;
                    else if (sibling.ty == VALTYPE.hostcon)
                    {
                        if (sibling.value != null)
                            result[i] = GetObjectProperties(sibling.value, item);
                        else
                            result[i] = item;
                    }
                }

            }

            return result;
        }

#endif

        /// <summary>
        /// Get object persistent data
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static VAL GetObjectProperties(object host)
        {
            return HostValization.Host2Val(host);
        }


        /*
         * 支持VAL对.NET对象的属性的赋值
         *      支持简单数组
         *      Collection: this[key]=value;
         *      Property: this.key = value;
         * 
         * Bug:
         *    如果: this[key] 和 this.key 2种情形同时存在, 那么都会赋值. 因为在Tie中, 认为this[key] 和 this.key 等价的.
         *    
         * */

        /// <summary>
        /// Set object properties by persistent data
        /// </summary>
        /// <param name="host"></param>
        /// <param name="properties"></param>
        public static void SetObjectProperties(object host, VAL properties)
        {
            HostValization.Val2Host(properties, host);
            return;

#if OBSOLETE
            if (properties.IsAssociativeArray())
            {
                for (int i = 0; i < properties.Size; i++)
                {
                    VAL property = properties[i];
                    object offset = property[0].value;
                    VAL val = property[1];

                    bool result = true;
                    try
                    {
                        result = HostOperation.HostTypeAssign(host, offset, val.value, true);
                    }
                    catch (HostTypeException)
                    {
                        result = false; //函数VAL.HostTypeAssign(...)调用失败,Fault-Tolerance 设计, 忽略那些不合法的属性
                    }

                    if (!result)
                    {

                        VAL child;
                        try
                        {
                            child = HostOperation.HostTypeOffset(host, offset, OffsetType.STRUCT);
                            if(child.Defined)
                                SetObjectProperties(child.value, val);
                        }
                        catch (HostTypeMemberNotFoundException)
                        {
                            child = null; //此属性没有定义在函数VAL.HostTypeOffset,Fault-Tolerance 设计, 忽略那些不合法的属性
                        }
                    }
                }
            }
            else
            {
                VAL array = properties;
                for (int i = 0; i < array.Size; i++)
                {
                    VAL item = array[i];
                    VAL sibling = HostOperation.HostTypeOffset(host, i, OffsetType.ARRAY);
                    if (sibling.Defined)   //此属性有定义
                        SetObjectProperties(sibling.value, item);
                }

            }

            return;

#endif
        }

        #endregion



        #region Hex <---> String

        /// <summary>
        /// Utility function:
        ///     conver string into byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(String hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Utility function:
        ///     convert byte array into string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }


        #endregion

        //已经host值,来判断是static or not
        internal static Type GetHostType(object host)
        {
            if (host is Type)
                return (Type)host;
            else
                return host.GetType();
        }


        internal static bool HasInterface(Type clss, Type interfce)
        {
            if (!interfce.IsInterface)
                return false;

            Type[] I = clss.GetInterfaces();
            foreach (Type i in I)
            {
                if (i == interfce)
                    return true;
            }

            return false;

        }

        /// <summary>
        /// allow to convert like: 
        ///     TargetType y = (HostType)x;
        /// </summary>
        /// <param name="hostType"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        internal static bool IsCompatibleType(Type hostType, Type targetType)
        {
            if (hostType == targetType)
                return true;

            //base class
            if (hostType.IsSubclassOf(targetType))
                return true;

            //Nullable<T>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsCompatibleType(hostType, targetType.GetGenericArguments()[0]);

            //interface
            if (HostType.HasInterface(hostType, targetType))
                return true;

            //enum
            if (hostType.IsEnum && targetType == typeof(int) || targetType.IsEnum && hostType == typeof(int))
                return true;

            return false;
        }
    }
}
