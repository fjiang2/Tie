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
using System.IO;
using System.Reflection;

namespace Tie
{

    enum Operator
    {
        op_UnaryNegation,       // -exp
        op_UnaryPlus,           // +exp
        op_Addition,            // exp1 + exp2
        op_Subtraction,         // exp1 - exp2
        op_Multiply,            // exp1 * exp2
        op_Division,            // exp1 / exp2
        op_Modulus,             // exp1 % exp2

        op_Equality,            // exp1 == exp2
        op_Inequality,          // exp1 != exp2
        op_GreaterThan,         // exp1 > exp2
        op_GreaterThanOrEqual,  // exp1 >= exp2
        op_LessThan,            // exp1 <  exp2
        op_LessThanOrEqual,     // exp1 <= exp2

        op_LogicalNot,        // !exp
        //op_LogicalAnd,        // exp1 && exp2
        //op_LogicalOr,         // exp1 || exp2

        op_OnesComplement,    // ~exp
        op_BitwiseAnd,        // exp1 & exp2
        op_BitwiseOr,         // exp1 | exp2
        op_ExclusiveOr,       // exp1 ^ exp2

        op_LeftShift,         // exp1 << exp2
        op_RightShift,        // exp1 >> exp2
        //op_SignedRightShift,
        //op_UnsignedRightShift,

        //op_Assign,                        // v = exp;
        //op_AdditionAssignment,            // v += exp;
        //op_SubtractionAssignment,         // v -= exp;
        //op_MultiplicationAssignment,      // v *= exp;
        //op_DivisionAssignment,            // v /= exp;
        //op_ModulusAssignment,             // v %= exp;

        //op_BitwiseAndAssignment,          // v &= exp;
        //op_BitwiseOrAssignment,           // v |= exp;
        //op_ExclusiveOrAssignment,         // v ^= exp;

        //op_LeftShiftAssignment,           // v <<= exp;
        //op_RightShiftAssignment,          // v >>= exp;
        //op_UnsignedRightShiftAssignment,

        //op_Decrement,                     // exp--, --exp
        //op_Increment,                     // exp++, ++exp

        //op_True,                          // true
        //op_False,                         // false

        //op_Implicit,
        //op_explicit,

        //op_Comma,
        //op_MemberSelection,
        //op_PointerToMemberSelection,
        //op_AddressOf,

        //op_PointerDereference,

        op_Explicit,
        op_Implicit
    };


    class HostFunction
    {
        private object host;
        private Type hostType;

        private string func;

        private object[] args1;     //输入的待处理的参数
        private Type[] argTypes1;   //args1的Types

   
        public HostFunction(object host, string func, VALL parameters)
        {
            this.host = host;
            this.func = func;

            if (host is Type)
                hostType = (Type)host;        //静态函数 static function
            else
                hostType = host.GetType();

            args1 = parameters.ObjectArray;
            int count = args1.Length;

            argTypes1 = new Type[count];
            for (int i = 0; i < count; i++)
                argTypes1[i] = parameters[i].Type;
       
        }

        public override string ToString()
        {
            string s = "";
            foreach (Type ty in argTypes1)
            {
                if (s != "")
                    s += ",";
                s += ty.Name;
            }

            return string.Format("{0}.{1}({2})", hostType.FullName, func, s);
        }

        #region RunFunction

        public VAL RunFunction()
        {
            return RunFunction(null);
        }

        //处理overloading methods
        public VAL RunFunction(MethodInfo[] methods)
        {
            VAL ret = RunFunctionSilently(methods);
            if((object)ret == null)
                throw new HostTypeException("Method {0} in .NET is not defined.", this.ToString());
            
            return ret;
        }

        //处理overloading methods
        //return null if method is not defined
        public VAL RunFunctionSilently(MethodInfo[] methods)
        {
            MethodInfo methodInfo = hostType.GetMethod(func, argTypes1);    //利用.NET能找到最合适method的能力,先找一下再说
            if (methodInfo != null)
                return InvokeMethod(methodInfo, host, args1);

            //由于根据函数传入值来确定调用哪个函数是一个不准确定位过程
            //如果没有指定methods的范围,那么需要搜索所有的函数,因为有些特殊情况: 
            //      譬如: 参数值null, 可以指向任何object或者数组
            //            参数值是delegate, generic
            if(methods==null)
                methods = OverloadingMethods(hostType, func);

            Tuple<MethodInfo, object[]> call = ChooseMethod(methods);
            if (call == null)
                return null;

            return InvokeMethod(call.Item1, host, call.Item2);
        }
        
        #endregion


        //从methodInfo[]中找出合适的methodInfo
        //如果找不到,返回null
        private Tuple<MethodInfo, object[]> ChooseMethod(MethodInfo[] methods)
        {
            Tuple<MethodInfo, object[]> call = null;

            if (methods.Length == 0)
                return null;

            if (methods.Length > 1)
            {
                int ambiguous = 0;
                foreach (MethodInfo method in methods)
                {
                    Tuple<MethodInfo, object[]> temp = CheckParameters(method);
                    if (temp != null)
                    {
                        call = temp;
                        ambiguous++;
                    }
                }

                if (ambiguous > 1)  //找一个最佳搭配的, 譬如 Convert.ToDecimal(object), Convert.ToDecimal(int), 如果arguments为int,就匹配后面一个
                {
                    throw new HostTypeException("Ambiguous Methods {0} in .NET are found.", this.ToString());
                }
                else if (ambiguous == 0)
                    return null;
            }
            else
            {
                call = CheckParameters(methods[0]);
                if (call == null)
                    return null;
            }

            return call;
        }

       




        //检查函数参数类型和传入值的类型是不是相容
        private Tuple<MethodInfo,object[]> CheckParameters(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            int len = parameters.Length;
            
            object[] args2 = new object[len];
            Type[] argTypes2 = new Type[len];

            //如果是可变参数
            if (len > 0 && len < args1.Length)
            {
                bool isParams = Attribute.IsDefined(parameters[len - 1], typeof(ParamArrayAttribute));
                if (isParams)
                {
                    for (int i = 0; i < len - 1; i++)
                    {
                        args2[i] = args1[i];
                        argTypes2[i] = argTypes1[i]; 
                    }

                    //组装最后一个参数,为数组
                    Type ty = parameters[len - 1].ParameterType;
                    Array array = (Array)Activator.CreateInstance(ty, new object[] { args1.Length - len + 1 });
                    for (int i = 0; i < args1.Length - len + 1; i++)
                        array.SetValue(args1[len + i - 1], i);

                    args2[len - 1] = array;
                    argTypes2[len - 1] = ty;
                    goto L1;
                }
            }

            //如果参数个数不相等
            if (len != args1.Length)
                return null;
            
            //如果参数个数相等
            for (int i = 0; i < len; i++)
            {
                args2[i] = args1[i];
                argTypes2[i] = argTypes1[i]; 
            }

            L1:
            GenericArguments gas = new GenericArguments();
            for (int i = 0; i < len; i++)
            {
                if (!HostOperation.IsCompatibleType(parameters[i].ParameterType, args2[i], argTypes2[i]))     //初步检查
                {
                    if (args2[i] == null && argTypes2[i] == null)
                        return null;

                    object temp = gas.CheckCompatibleType(parameters[i], args2[i], argTypes2[i]);//深度检查,函数参数
                    if (temp == null)
                        return null;
                    
                    args2[i] = temp;
                }
            }

            if (method.IsGenericMethod && method.IsGenericMethodDefinition)
            {
                Type[] gaty2 = gas.ConstructGenericArguments(method.GetGenericArguments());
                method = method.MakeGenericMethod(gaty2);
            }

            return Tuple.Create(method, args2);
        }




        //执行method
        public static VAL InvokeMethod(MethodInfo methodInfo, object host, object[] arguments)
        {
            try
            {
                object obj = methodInfo.Invoke(host, arguments);

                return VAL.Boxing1(obj);
            }
            catch (Exception e)
            {
                //捕捉把TIE函数作为Host(C#.NET)的delegate的错误
                //可能是参数个数不一致,可能是数据类型不对
                foreach (object arg in arguments)
                {
                    if (arg is Delegate)
                    {
                        Delegate d = (Delegate)arg;
                        if (d.Method.Name == Constant.FUNC_CON_INSTANCE_INVOKE)
                            throw new HostTypeException("Call delegate {0} failed in {1} of {2}", d, methodInfo, host);
                    }
                }

                throw new HostTypeException("Call failed({0}) in {1} of {2}", e, methodInfo, host);
            }
        }

        #region OverloadingMethods(..)/methodof(..)

        //返回所有的相同函数名字的重载函数
        public static MethodInfo[] OverloadingMethods(Type type, string methodName)
        {
            MemberInfo[] members = type.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            //overloading函数
            MethodInfo[] methods = new MethodInfo[members.Length];
            for (int i = 0; i < members.Length; i++)
                methods[i] = (MethodInfo)members[i];

            return methods;
        }



        //查找method,根据函数名,函数返回类型和参数
        public static MethodInfo methodof(object host, Type returnType, string methodName, Type[] arguments)
        {
            Type ty = HostType.GetHostType(host);

            MemberInfo[] members = ty.GetMember(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MemberInfo member in members)
            {
                if (member is MethodInfo)
                {
                    MethodInfo method = (MethodInfo)member;
                    if (method.ReturnType == returnType)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length == arguments.Length)
                        {
                            int i = 0;
                            int count = 0;
                            foreach (ParameterInfo parameterInfo in parameters)
                            {
                                if (parameterInfo.ParameterType == arguments[i])
                                    count++;
                                i++;
                            }

                            if (count == arguments.Length)
                                return method;
                        }
                    }
                }
            }

            return null;
        }

        #endregion


        #region PropertyOf

        public static VAL propertyof(bool isRead, Type returnType, string propertyName, object host, object val)
        {
            PropertyInfo propertyInfo = getproperty(host, returnType, propertyName);

            if (isRead)
            {
                if (propertyInfo.CanRead)
                    return VAL.Boxing1(propertyInfo.GetValue(host, null));
                else
                    return new VAL();
            }
            else if (propertyInfo.CanWrite)
                propertyInfo.SetValue(host, val, null);

            return VAL.NewVoidType();

        }

        private static PropertyInfo getproperty(object host, Type returnType, string propertyName)
        {
            Type ty = HostType.GetHostType(host);

            if (returnType == null)
                return ty.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            PropertyInfo[] properties = ty.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Name == propertyName && propertyInfo.PropertyType == returnType)
                    return propertyInfo;
            }

            throw new HostTypeException("Invalid property name: {0} {1}.{2}", returnType.FullName, ty.FullName, propertyName);
        }




        #endregion
            

        #region static OperatorOverloading

        //operator重载
        public static VAL OperatorOverloading(Operator opr, VAL v1, VAL v2)
        {
            return OperatorOverloading(opr, v1, v2, false);
        }
        
        public static VAL OperatorOverloading(Operator opr, VAL v)
        {
            return OperatorOverloading(opr, v, null, false);
        }

        //slient=true, 如果重载函数不存在,不throw exception
        public static VAL OperatorOverloading(Operator opr, VAL v1, VAL v2, bool silent)
        {
            VALL L = new VALL();
            L.Add(v1);
            
            if((object)v2!=null)
                L.Add(v2);
            
            HostFunction hFunc = new HostFunction(v1.value, opr.ToString(), L);
            if (silent)
                return hFunc.RunFunctionSilently(null);
            else
                return hFunc.RunFunction(null);
        }

        #endregion

    }


}
