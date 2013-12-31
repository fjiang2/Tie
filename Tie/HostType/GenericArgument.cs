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

#if TIE4
using System.Linq;
using System.Linq.Expressions;
#endif

namespace Tie
{

    class GenericArgument
    {
        private GenericArguments gas;

        private Type parameterType;
        private Type valType;

        public GenericArgument(GenericArguments genericArguments, Type parameterType,  Type valType)
        {
            this.gas = genericArguments;
            this.parameterType = parameterType;
            this.valType = valType;

        }


        /*
        * 
        * 查找合适的interface或者class
        * 譬如: 
        * IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);
        * 
        * string[] A;
        * A.Where(a => a>4);       
        * 
        * A有很多的interface, 
        * 那么在string[]中找到INumerable<string>类型,
        * 
        *   如果找到函数返回true, 
        *   并且: parameterType = INumerable<string> //parameterType中的generic arguments 被替换成真正string type,
        *        generic arguments 符号表也被更新为:
        *              {TSource : typeof(string) }
        * 
        * */
        public object PrepareArgument(object val)
        {
            if (parameterType.IsInterface)                  //查找generic interfaces
            {
                Type[] I = valType.GetInterfaces();
                foreach (Type type in I)
                {
                    if (gas.MatchGenericParameters(parameterType, type))
                        return val;
                }
            }
            else if (parameterType.IsClass)
            {

                if (parameterType.IsSubclassOf(typeof(MulticastDelegate)))  //查找generic delegate, delegate是特殊的class
                {
                    return PrepareDelegate(parameterType, val);
                }
#if TIE4
                //Linq SQL, Expression Tree
                else if (parameterType.IsSubclassOf(typeof(LambdaExpression)))
                {
                    /*
                    Type funcGenericType = parameterType.GetMethod("Compile", new Type[] { }).ReturnType;   //这里是generic delegate, Func<TSource, bool>
                    object d = Convert2Delegate(funcGenericType, val, funcGenericType.GetGenericTypeDefinition(), funcGenericType.GetGenericArguments());
                    Type[] funcGenericParameterTypes = ConstructGenericArguments(funcGenericType.GetGenericArguments());
                    
                    Type exprGenericType = funcGenericType.GetGenericTypeDefinition().MakeGenericType(funcGenericParameterTypes);     //让generic delegate实例化, 变成Expression的generic parameter, 例如Func<Security.Users, bool>
                    Type exprType = parameterType.GetGenericTypeDefinition().MakeGenericType(new Type[] { exprGenericType });
                    
                    //把delegate转换为expression tree, 还不知道如何实现DelegateConverter.ToExpression(d); 
                    object exprTree = null;   
                    return Activator.CreateInstance(exprType, new object[] { exprTree });
                    */

                    throw new NotImplementedException("Linq to SQL not implemented yet in TIE");
                }
#endif
                else if (gas.MatchGenericParameters(parameterType, valType))        //查找class
                    return val;
            }

            return null;

        }

        
        /*
        * 把函数参数值转化为delegate
        * 
        * 如果是delegate类型参数, 考虑传入参数值的3种情况
        * 1. MethodInfo
        * 2. Delegate
        * 3. Tie Function
        * 
        * */
        private object PrepareDelegate(Type parameterType, object val)
        {
            Type gty1 = parameterType.GetGenericTypeDefinition();
            Type[] gpty1 = parameterType.GetGenericArguments();

        
            MethodInfo method1 = parameterType.GetMethod("Invoke");
            if (val is MethodInfo)
            {
                MethodInfo method2 = (MethodInfo)val;
                if (gas.MatchGenericMethod(method1, method2))
                {
                    Type[] gpty2 = gas.ConstructGenericArguments(gpty1);
                    parameterType = gty1.MakeGenericType(gpty2);
                    val = Delegate.CreateDelegate(parameterType, null, method2);
                    return val;
                }
                else
                    return null;
            }
            else if (val is MulticastDelegate)
            {
                MethodInfo method2 = ((MulticastDelegate)val).GetType().GetMethod("Invoke");
                if (gas.MatchGenericMethod(method1, method2))
                    return val;
                else
                    return null;
            }
            else if (val is VAL)
            {
                VAL func = (VAL)val;
                if (func.ty == VALTYPE.funccon)
                {
                    int argc = DynamicDelegate.FuncArgc(func);
                    Type[] pTypes = DynamicDelegate.GetDelegateParameterTypes(parameterType);
                    if (argc == pTypes.Length)          //参数个数相同,
                    {
                        Type[] gpty2 = gas.ConstructGenericArguments(gpty1);    //没有办法匹配参数类型是不是一致,因为TIE函数没有显式的参数类型
                        if (gpty2 == null)
                            throw new HostTypeException("Generic Type is not matched on {0}", parameterType);

                        parameterType = gty1.MakeGenericType(gpty2);
                        return DynamicDelegate.ToDelegate(parameterType, val);
                    }
                    return null;
                }
            }

            return null;
        }

    }
}
