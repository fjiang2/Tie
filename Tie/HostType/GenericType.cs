using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Tie
{
    class GenericType
    {
        Type type;

        public GenericType(Type type)
        {
            this.type = type;
        }

        public GenericType(object obj)
        {
            if (obj is Type)
                this.type = (Type)obj;
            else
                this.type = obj.GetType();
        }


        public Type Type
        {
            get { return this.type; }
        }

        public string[] Namespace
        {
            get
            {
                return type.Namespace.Split(new char[] { '.' });
            }
        }

        public string FullName
        {
            get
            {
                return type.Namespace + "." + type.Name;
            }
        }

        public string TypeName
        {
            get
            {
                if (type.IsGenericType)
                {
                    StringBuilder builder = new StringBuilder();

                    string name = type.GetGenericTypeDefinition().FullName;
                    int index = name.IndexOf('`');

                    string x = name.Substring(0, index);
                    int count = int.Parse(name.Substring(index + 1));

                    builder.Append(x);
                    builder.Append("<");

                    Type[] types = type.GetGenericArguments();
                    if (types.Length != 0)
                    {
                        for (int i = 0; i < types.Length; i++)
                        {
                            builder.Append(types[i].FullName);
                            if (i < types.Length - 1)
                                builder.Append(",");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count - 1; i++)
                            builder.Append(",");
                    }
                    builder.Append(">");
                    return builder.ToString();
                }
                else
                    return type.FullName;
            }
        }

        public override string ToString()
        {
            return TypeName;
        }


        //已经host值,来判断是static or not
        public static Type GetHostType(object host)
        {
            if (host is Type)
                return (Type)host;
            else
                return host.GetType();
        }



        public static bool HasContructor(Type clss, Type[] arguments)
        {
            ConstructorInfo[] constructors = clss.GetConstructors();
            foreach (ConstructorInfo constructorInfo in constructors)
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();
                if (parameters.Length == arguments.Length)
                {
                    int count = 0;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType == arguments[i])
                            count++;
                    }

                    if (count == arguments.Length)
                        return true;
                }
            }

            return false;
        }

        public static bool HasInterface(Type clss, Type interfce)
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
        public static bool IsCompatibleType(Type hostType, Type targetType)
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
            if (HasInterface(hostType, targetType))
                return true;

            //enum
            if (hostType.IsEnum && targetType == typeof(int) || targetType.IsEnum && hostType == typeof(int))
                return true;

            if (hostType.IsArray && targetType.IsArray)
            {
                return IsCompatibleType(hostType.GetElementType(), targetType.GetElementType());
            }

            return false;
        }
    }
}
