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
using System.Reflection.Emit;

namespace Tie
{
    class DynamicDelegate
    {
        private VAL func = null;    //如果变量名字改变了,请同时修改函数InstanceDelegate(,)中的引用字符串

        private DynamicDelegate(VAL func)
        {
            this.func = func;
        }


        //这个函数必须为public, 如果函数名称改变了,必须同时修改函数InstanceDelegate(,)中对这个函数的引用
        public static object CallFunc(VAL funccon, object[] arguments)
        {
            if (funccon.ty == VALTYPE.funccon)
            {
                ContextInstance temp = (ContextInstance)funccon.temp;
                Context context = temp.context;
                VAL instance = temp.instance;
                VAL ret = CPU.ExternalUserFuncCall(funccon, instance, VAL.Boxing(arguments), context);
                return ret.HostValue;
            }

            throw new HostTypeException("VAL {0} is not funccon type.", funccon);
        }

        //返回函数参数的个数
        public static int FuncArgc(VAL func)
        {
            string moduleName = func.Class;
            Module module = Library.GetModule(moduleName);
            if (module == null)
                return -1;;

            return module.CS[func.Address].operand.Addr -1;
        }



        public static Delegate InstanceDelegate(Type dType, VAL func)
        {
            DynamicDelegate instance = new DynamicDelegate(func);
            FieldInfo funcconField = typeof(DynamicDelegate).GetField("func", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo methodAdapter = typeof(DynamicDelegate).GetMethod("CallFunc", BindingFlags.Public | BindingFlags.Static);
            return InstanceDelegate(dType, instance, funcconField, methodAdapter);
        }




        /**
         * 把函数public static object xxxx(object target, object[] arguments);
         * 转为任意类型的delegate
         * 
         * 
         * 参考:
         *  http://msdn.microsoft.com/en-us/library/z43fsh67.aspx
         * 
         *  下面产生的code相当于:
         * public int FuncconInvoke(string arg0, int arg1, ...)
         * {
         *   object[] args = new object[] { arg0, arg1, ...};
         *   object ret = CallFunc(target.funcField, args);
         *   return (int)ret;
         * }
         * 
         * 把DynamicMethod中用到的变量放到functionField中
         * 真正的DynamicMethod的实现函数是
         *      public static object methodAdapter(target.funcField, object[] args)
         *      
         * 
         * */
        public static Delegate InstanceDelegate(Type dType, object target, FieldInfo funcField, MethodInfo methodAdapter)
        {

            //获取delegate的原型, 因为delegate是继承class MultiDelegate, Invoke函数的signature和delegate的是一致的
            MethodInfo dMethod = dType.GetMethod("Invoke");
            ParameterInfo[] dParemeters = dMethod.GetParameters();


            //通常DynamicMethod只能生成static函数,但是
            //为了让DynamicMethod支持target类型, 第一个参数必须是这个target的class类型
            //第一个参数 Must be of the same type as the first parameter of the dynamic method
            //所以多了一个参数
            int len = dParemeters.Length;
            Type[] dParameterTypes = new Type[len + 1];
            dParameterTypes[0] = target.GetType();
            for (int i = 0; i < len; i++)
                dParameterTypes[i+1] = dParemeters[i].ParameterType;


            DynamicMethod dynamicMethod = new DynamicMethod(
                Constant.FUNC_CON_INSTANCE_INVOKE,
                dMethod.ReturnType,
                dParameterTypes,
                target.GetType());  //把DynamicMethod关联到target的class

            ILGenerator il = dynamicMethod.GetILGenerator(256);

            //定义局部变量 
            il.DeclareLocal(typeof(object[]));      //object[] L0;
            il.DeclareLocal(typeof(object));        //object ret; 
            il.DeclareLocal(dMethod.ReturnType);    //返回值变量

            //产生var L0 = new object[len] 数组
            il.Emit(OpCodes.Ldc_I4, len);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, 0);

            ////把delegate中的参数传给数组 L0 = new object[]{arg0, arg1, arg2, ...};
            for (int i = 0; i < len; i++)
            {
                il.Emit(OpCodes.Ldloc, 0);    //LOAD L0
                il.Emit(OpCodes.Ldc_I4, i);   //LOAD i
                il.Emit(OpCodes.Ldarg, i+1);  //LOAD arg[i+1]
                if (dParameterTypes[i].IsValueType)
                    il.Emit(OpCodes.Box, dParameterTypes[i]);
                il.Emit(OpCodes.Stelem_Ref);
            }

            //准备CallFunc(target.funcField, object[])的参数
            il.Emit(OpCodes.Ldarg, 0);              //LOAD target
            il.Emit(OpCodes.Ldfld, funcField);      //LOAD field
            il.Emit(OpCodes.Ldloc, 0);              //LOAD L0
            //CALL函数
            il.EmitCall(OpCodes.Call, methodAdapter, null);   //CallFunc(VAL, L0);
            //把函数的返回值存入ret变量中
            il.Emit(OpCodes.Stloc, 1);
            il.Emit(OpCodes.Ldloc, 1);

            //Unbox返回值, 如果是ValueType
            if (dMethod.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, dMethod.ReturnType);
                il.Emit(OpCodes.Stloc, 2);
                il.Emit(OpCodes.Ldloc, 2);
            }

            il.Emit(OpCodes.Ret);

            
            //下面可有可无,为了可读性
            for (int i = 0; i < len+1; i++)
                dynamicMethod.DefineParameter(i, ParameterAttributes.In, "arg" + i);

            return dynamicMethod.CreateDelegate(dType, target);
        }


        //比较2个函数的参数类型和返回值类型
        private static bool CompareMethodSignature(MethodInfo method1, MethodInfo method2)
        {
            ParameterInfo[] parameters1 = method1.GetParameters();
            ParameterInfo[] parameters2 = method2.GetParameters();
            if (parameters1.Length != parameters2.Length)
                return false;

            if (method1.ReturnType != method2.ReturnType)
                return false;

            for (int i = 0; i < parameters1.Length; i++)
                if (parameters1[i].ParameterType != parameters2[i].ParameterType)
                    return false;

            return true;
        }

        public static Type[] GetDelegateParameterTypes(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                return null;

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                return null;

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeParameters[i] = parameters[i].ParameterType;
            }
            return typeParameters;
        }

        public static Type GetDelegateReturnType(Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
                return null;

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
                return null;

            return invoke.ReturnType;
        }


        // 把参数值类型为MethodInfo或者funccon的val, 转为Delegate类型的值
        public static object ToDelegate(Type type, object val)
        {
            MethodInfo method1 = type.GetMethod("Invoke");
            if (val is MethodInfo)
            {
                MethodInfo method2 = (MethodInfo)val;
                if (CompareMethodSignature(method1, method2))
                {
                    //把MethodInfo转为Delegate
                    val = Delegate.CreateDelegate(type, null, method2);
                    return val;
                }
            }
            else if (val is MulticastDelegate)
            {
                MethodInfo method2 = ((MulticastDelegate)val).GetType().GetMethod("Invoke");    //这个语句和 ((MulticastDelegate)val).Method; 是有细微区别的,这里不返回delegate的target
                if (CompareMethodSignature(method1, method2))
                    return val;
            }
            else if (val is VAL)
            {
                VAL func = (VAL)val;
                if (func.ty == VALTYPE.funccon) //Tie函数其实都是generic函数
                {
#if DEBUG_TIE_DELEGATE
                    DynamicDelegate.funccon = func;
                    return Delegate.CreateDelegate(type, null, typeof(DynamicDelegate).GetMethod("test102"));
#else
                    int argc = DynamicDelegate.FuncArgc(func);                      //TIE func函数的参数个数
                    Type[] pTypes = DynamicDelegate.GetDelegateParameterTypes(type);//delegate 参数类型
                    if (argc == pTypes.Length)                                      //比较参数个数
                        return DynamicDelegate.InstanceDelegate(type, func);
                    else
                        return null;
#endif
                }
            }

            return null;
        }



#if DEBUG_TIE_DELEGATE
        public static VAL funccon = null;

        public static int test101(int[] A)
        {
            object[] args = new object[1];
            args[0] = A;
            return (int)DynamicDelegate.CallFunc(funccon, args);
        }

        public static bool test102(string A)
        {
            return A.Length > 6;
        }

#endif

    }
}
