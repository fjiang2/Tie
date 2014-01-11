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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Tie
{

    /// <summary>
    /// Represent .NET object Type
    /// </summary>
    public sealed class HostType
    {
        static HostType()
        {
            AddReference(typeof(object).Assembly);
            
            //Import(typeof(object).Namespace);
            Import("System");

            //Import(typeof(List<>).Namespace);
            Import("System.Collections.Generic");

            //Import(typeof(ASCIIEncoding).Namespace);
            Import("System.Text");
            Import("System.Reflection");
        }

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
            for(int i=0; i< types.Length; i++)
            {
                Type type = types[i];
                Register(type, type.FullName, briefName);


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

            return true;
        }

        private static void Register(Type type, string typeName, bool briefName)
        {
            Memory DS1 = Computer.DS1;
            VAL obj = VAL.NewHostType(type);

            string[] names = typeName.Split(new char[] { '.' });
            string names0 = names[0];
            
            if (DS1.ContainsKey(names0))
            {
                if (names.Length > 1)
                {
                    VAL val = DS1[names0];
                    VAL.Assign(val, names, 1, obj);
                }
                else
                    DS1[names0] = obj;
            }
            else
            {
                if (names.Length > 1)
                {
                    VAL val = new VAL(new VALL());
                    VAL.Assign(val, names, 1, obj);
                    DS1.Add(names0, val);
                }
                else
                    DS1.Add(names0, obj);
                
            }

            if (briefName && !Constant.HOST_TYPE_AUTO_REGISTER)
            {
                if (DS1.ContainsKey(type.Name))
                {
                    DS1[type.Name] = obj;
                }
                else
                    DS1.Add(type.Name, obj);
            }
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
         * 相当于C#:
         *      using DictionaryStringInt32=Dictionary<string,int>;
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
            Register(type, typeName, false);
            //Computer.DS1.Add("$1", VAL.NewHostType(type));
            //Script.Execute(typeName + "=$1;", Computer.DS1);
            //Computer.DS1.Remove("$1");
            return true;
        }


     

  
        #endregion

        
        #region Add Reference

        private static HostReferences references = new HostReferences();
        //Dictionary<alias, namespace>
        private static Dictionary<string, string> aliases = new Dictionary<string, string>();
        //Dictionary<namespace, Assembly[]>
        private static Dictionary<string, HostImport> imports = new Dictionary<string, HostImport>();

        /**
         * 
         *   AddReference(Assembly.Load("System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
         *   AddReference(Assembly.Load("System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
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

            foreach (HostImport import in imports.Values)
            {
                import.AddReference(assembly);
            }
        }

        /// <summary>
        /// Remove added reference. Do nothing if assembly is not added before
        /// </summary>
        /// <param name="assembly"></param>
        public static void RemoveReference(Assembly assembly)
        {
            if (references.IndexOf(assembly) >= 0)
            {
                references.Remove(assembly);
                foreach (HostImport import in imports.Values)
                {
                    import.RemoveReference(assembly);
                }
            }

        }

        /// <summary>
        /// using System.Data; 
        ///     is equivalent to Import("System.Data");
        /// </summary>
        /// <param name="nameSpace"></param>
        public static void Import(string nameSpace)
        {
            Assembly[] assemblies = GetAssemblyByNamespace(nameSpace);
            if (assemblies.Length == 0)
                throw new TieException("invalid namespace:{0}", nameSpace);

            if (imports.ContainsKey(nameSpace))
                return;

            imports.Add(nameSpace, new HostImport(nameSpace, references));
        }

        /// <summary>
        ///      C#: using SysData = System.Data;
        ///     Tie: Import("SysData", "System.Data");
        /// </summary>
        /// <param name="aliasName"></param>
        /// <param name="nameSpace"></param>
        public static void Import(string aliasName, string nameSpace)
        {
            Import(nameSpace);

            if (aliases.ContainsKey(aliasName))
                aliases.Remove(aliasName);

            aliases.Add(aliasName, nameSpace);
        }

        /// <summary>
        /// Remove imported namespace, do nothing if namespace is not added before
        /// </summary>
        /// <param name="nameSpace">either namespace alias or namespace</param>
        public static void RemoveImport(string nameSpace)
        {
            if (aliases.ContainsKey(nameSpace))
            { 
                string import = aliases[nameSpace];
                aliases.Remove(nameSpace);
                nameSpace = import;
            }

            if (imports.ContainsKey(nameSpace))
            {
                imports.Remove(nameSpace);
            }
                
        }

        private static Assembly[] GetAssemblyByNamespace(string ns)
        {
            List<Assembly> list = new List<Assembly>();
            foreach (Assembly assembly in references)
            {
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (type.Namespace.Equals(ns))
                    {
                        list.Add(assembly);
                        break;
                    }
                }
            }

            return list.ToArray();
        }

        internal static Type GetTypeByBriefName(string simpleTypeName)
        {
            foreach(HostImport import in imports.Values)
            { 
                if(import.ContainsKey(simpleTypeName))
                    return import[simpleTypeName];
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

        internal static Type GetType(string ns, string name)
        {
            if (!imports.ContainsKey(ns))
                return null;
         
            string typeName = ns + "." + name;
            Type type = GetType(imports[ns].Assemblies, typeName);
            return type;

        }
      
        /// <summary>
        ///  GetType("Int32[][]")
        ///  GetType("System.DateTime")
        ///  GetType("Dictionary`2")
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            Type type = null;

            //删除空格
            typeName = typeName.Replace(" ", "");
            int isArray = 0;    //数组类型的维数

            //支持多维数组
            while (typeName.EndsWith("[]"))
            {
                isArray++;
                typeName = typeName.Substring(0, typeName.Length - 2);
            }

            type = GetSimpleType(typeName);

            while (isArray > 0)
            {
                isArray--;
                type = type.MakeArrayType();    //产生多维数组
            }

            return type;
        }


        private static Type GetSimpleType(string typeName)
        {
            Type type = null;
            
            string[] names = typeName.Split(new char[] { '.' });
            string ns = string.Join(".", names, 0, names.Length - 1);

            //1: using System.Data;
            //GetType("System.Data.DataTable");
            if (names.Length > 1 && imports.ContainsKey(ns))
            {
                type = GetType(imports[ns].Assemblies, typeName);
                if (type != null)
                    return type;
            }

            //2: using S=System.Data;
            //GetType("S.DataTable");
            if (names.Length > 1 && aliases.Count > 0)
            {
                if (aliases.ContainsKey(names[0]))
                {
                    names[0] = aliases[names[0]];
                    string fullTypeName = string.Join(".", names);

                    type = GetSimpleType(fullTypeName);
                    if (type != null)
                        return type;
                }
            }


            //3: search current domain and references
            //GetType("System.Windows.Controls.Button");
            if (names.Length > 1)
            {
                List<Assembly> list = new List<Assembly>();
#if !SILVERLIGHT
                foreach (Assembly assemby in AppDomain.CurrentDomain.GetAssemblies()) //在当前的domain中的Assembly中搜索
                    list.Add(assemby);
#endif
                foreach (Assembly assemby in references) //搜索referecne空间
                {
                    if (list.IndexOf(assemby) < 0)
                        list.Add(assemby);
                }
                type = GetType(list, typeName);
                if (type != null)
                    return type;
            }

            //4: simple type name
            //using System.Data;
            //GetType("DataTable");
            foreach (KeyValuePair<string, HostImport> kvp in imports)
            {
                string import = kvp.Key;

                if (!typeName.StartsWith(import))
                {
                    string fullTypeName = import + "." + typeName;
                    type = GetType(kvp.Value.Assemblies, fullTypeName);
                    if (type != null)
                        return type;
                }
            }

            //4.根据class名字来推断assemblyName,然后返回
            type = GetDefaultAssemblyType(typeName);
            if (type != null)
                return type;

            return null;
        }

        private static Type GetType(IEnumerable<Assembly> assemblies, string typeName)
        {
            Type type = null;
            foreach (Assembly assembly in assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
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



     
      
    }
}
