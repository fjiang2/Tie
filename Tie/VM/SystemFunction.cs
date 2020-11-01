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
using System.IO;
using System.Reflection;

namespace Tie
{


    /// <summary>
    /// 
    /// </summary>
    class SystemFunction
    {

        public static VAL Function(string func, VAL parameters, Memory DS, Position position)
        {
            VALL L = (VALL)parameters.value;
            VAL R0;

            int size = L.Size;
            VAL L0 = size > 0 ? L[0] : null;
            VAL L1 = size > 1 ? L[1] : null;

            switch (func)
            {

                /*
                 *  register(Type type)
                 *  register(Assembly assemby)
                 * */
                case "register":
                    if (size == 1)
                    {
                        if (L0.ty == VALTYPE.hostcon)
                        {
                            object host = L0.HostValue;
                            if (host is Type)
                                return new VAL(HostType.Register((Type)host));
                            if (host is Type[])
                                return new VAL(HostType.Register((Type[])host));
                        }
                    }
                    break;

                /*
                 * add reference
                 * */
                case "addreference":
                    if (size == 1 && L0.ty == VALTYPE.hostcon)
                    {
                        object host = L0.HostValue;
                        if (host is Assembly)
                        {
                            HostType.AddReference((Assembly)host);
                            return VAL.NewHostType(host);
                        }
                    }
                    break;

                /*
                 * using System.Data => import(System.Data)
                 * using SD = System.Data => import("SD", System.Data)
                 * */
                case "import":
                    if (size == 1)
                    {
                        string ns = null;
                        if (L0.ty == VALTYPE.stringcon)
                            ns = L0.Str;
                        else if (L0.name != null)   //because System.Data may exist before addimport(System.Data)
                            ns = L0.name;

                        if (ns != null)
                        {
                            HostType.Import(ns);
                            return new VAL();
                        }

                    }
                    else if (size == 2 && L0.ty == VALTYPE.stringcon)
                    {
                        string ns = null;
                        if (L1.ty == VALTYPE.stringcon)
                            ns = L1.Str;
                        else if (L1.name != null)
                            ns = L1.name;

                        if (ns != null)
                        {
                            HostType.Import(L0.Str, ns);
                            return new VAL();
                        }
                    }
                    break;

                //return VAL type
                case "type":
                    if (size == 1)
                        return new VAL((int)L0.ty);
                    break;

                /*
                 * 
                 * 等价于.NET的.GetType()函数, 用于非HostType类型的变量
                 * **/
                case "GetType":
                    if (size == 1)
                    {
                        if (L0.value == null)
                            return new VAL();
                        else
                            return VAL.NewHostType(L0.value.GetType());
                    }
                    break;



                /*
                 * 
                 * 0. 修改函数名typeof,必须同时修改VAL.encode中的中listcon编码
                 * 
                 * 1. 给listcon类型的值设置Class类型, 譬如:L={1,2,3}; L.typeof("SET");设置L是一个集合. L.typeof() 返回list的Class类型
                 * 
                 * 2. 也可以返回host的type, 等价于.NET的typeof(..) 如: typeof(System.Math)
                 * 
                 * 3. 等价于.NET的.GetType()函数, 用于HostType类型的变量
                 * 
                 * 4. 
                 * 
                 *      
                 * 6. typeof(className) 返回 Type, 例如: TIE: typeof("System.Data.Table") --> C#: typeof(System.Data.Table)
                 * 
                 * */
                case "typeof":
                    if (size == 2)
                    {
                        if (L0.ty == VALTYPE.listcon)                   //使用2个参数, 例如{1,2,3}.typeof(System.Int32[]), 
                        {
                            if (L1.value == null)                       //1.1如果没有注册过,试图search DLL,譬如:Color=typeof(System.Drawing.Color)
                            {
                                Type ty = HostType.GetType(L1.name);
                                if (ty != null)
                                {
                                    L0.Class = ty.FullName;
                                    return L0;
                                }
                                else
                                    return L0;                          //1.1.2 如果没有注册过,那么忽略数组类型
                            }
                            if (L1.ty == VALTYPE.stringcon)             //1.2 例子: {2,4,5}.typeof("System.Int32[]")
                            {
                                L0.Class = L1.Str;
                                return L0;
                            }
                            else if (L1.ty == VALTYPE.hostcon)
                            {
                                L0.Class = L1.HostValue.ToString();     //1.3 例子: {2,4,5}.typeof(System.Int32[]) 或者 {2,3,4}.typeof(int[])
                                return L0;
                            }
                        }
                    }
                    else if (size == 1)
                    {
                        if (L0.value == null)       //如果没有注册过,试图search DLL,譬如:Color=typeof(System.Drawing.Color)
                        {
                            Type ty = HostType.GetType(L0.name);
                            if (ty != null)
                                return VAL.NewHostType(ty);
                            else
                                return new VAL();
                        }
                        else if (L0.ty == VALTYPE.listcon)
                        {
                            if (L0.Class == null)
                                return VAL.NewVoidType();         //空表示没有定义list的类型
                            return new VAL(L0.Class);
                        }
                        else if (L0.ty == VALTYPE.hostcon)
                        {
                            if (L0.value is Type)
                                return L0;                               //2.等价于.NET的typeof(..) 如: typeof(System.Math)
                            else
                                return VAL.NewHostType(L0.value.GetType());  // 3.等价于.NET的.GetType()函数, 用于HostType类型的变量
                        }
                        else if (L0.ty == VALTYPE.stringcon)
                        {
                            return VAL.NewHostType(HostType.GetType(L0.Str));    //6. typeof("System.Int32[]")
                        }
                    }
                    break;

                /*
                 * 1.   Host.classof();    返回host object的可以persistent的VAL对象
                 * 2.   Host.classof({prop1:val1, prop2:val2,....});  属性Map 用来初始化host object
                 * 3.   Host.classof("valor");   字符串序列valor,json,xml用来初始化host object
                 * */
                case "classof":
                    if (size == 1)
                    {
                        if (L0.ty == VALTYPE.hostcon)
                            return HostValization.Host2Val(L0.value);
                    }
                    else if (size == 2 && L0.ty == VALTYPE.hostcon)
                    {
                        if (L1.ty == VALTYPE.listcon)
                        {
                            HostValization.Val2Host(L1, L0.value);
                            return L0;
                        }
                        else if (L0.value != null)
                        {
                            HostValization.Val2Host(L1, L0.value, L0.value.GetType());
                            return L0;
                        }
                        return L0;
                    }
                    break;


                /*
                * 
                * return name of value
                * 
                * */
                case "nameof":
                    if (size == 1)
                    {
                        if (L0.name == null)
                            return new VAL();
                        else
                            return new VAL(L0.name);
                    }
                    break;

                /*
                 *  1.返回Persistent字符串
                 * 
                 * * * */
                case "valize":
                    if (size == 1)
                    {
                        return VAL.NewScriptType(L0.Valor);
                    }
                    break;

                /*
                 * A.isnull(B), 如果A为null,就返回B, 否则还是返回A值
                 * 
                 * */
                case "isnull":
                    if (size == 2)
                    {
                        if (L0.ty == VALTYPE.nullcon)
                            return L1;
                        else
                            return L0;
                    }
                    break;

                case "VAL":
                    if (size == 1)
                    {
                        R0 = VAL.Clone(L0);
                        R0.Class = "VAL";              //force to CAST VAL, don't do HostValue unboxing
                        return R0;
                    }
                    break;

                /*
                 * Translate string into VAR type
                 * */
                case "VAR":
                    if (size == 1 && L0.value is string)
                    {
                        R0 = VAL.NewHostType(new VAR(L0.Str));
                        return R0;
                    }
                    break;

                case "HOST":                       //cast to hostcon
                    if (size == 1)
                    {
                        R0 = VAL.Clone(L0);
                        R0.ty = VALTYPE.hostcon;
                        return R0;
                    }
                    break;

                /***
                 *  强制转换类型,支持数组
                 *  
                 * 1. A={}.cast(typeof("System.String[]"));
                 * 
                 * 2. A = {}.cast("System.String[]") 等价于 C#的 string[] A = new string[0];
                 * 
                 */
                case "ctype":
                    if (size == 2)
                    {
                        if (L1.value is Type)
                        {
                            return VAL.cast(VAL.Clone(L0), (Type)L1.value);
                        }
                        else if (L[1].value is string)
                        {
                            Type ty = HostType.GetType(L1.Str);
                            if (ty != null)
                                return VAL.cast(VAL.Clone(L0), ty);
                        }
                    }
                    break;



                case "DateTime":
                    if (size == 6)
                        return VAL.NewHostType(new DateTime(L0.Intcon, L1.Intcon, L[2].Intcon, L[3].Intcon, L[4].Intcon, L[5].Intcon));
                    else if (size == 3)
                        return VAL.NewHostType(new DateTime(L0.Intcon, L1.Intcon, L[2].Intcon));
                    break;

                //STRING
                case "format":
                    if (size >= 1 && L0.ty == VALTYPE.stringcon)
                        return format(L);
                    break;



                #region LIST 操作函数

                case "size":
                    if (size == 1)
                        return new VAL(L0.Size);
                    break;

                case "array":           //array(2,3,4)
                    int[] A = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        if (L[1].ty != VALTYPE.intcon)
                            return null;

                        A[i] = L[i].Intcon;
                    }
                    return VAL.Array(A);

                case "slice":
                    return Slice(L);


                /*栈操作 push(list, any) pop(list)
                 * 
                 * 1.追加到最后, L.push(element)
                 * 2.插入L.insert(pos,element)
                 * 
                 * 3. pop最后一个element = L.pop();
                 * 4. L.remove(pos)
                 * */
                case "append":
                case "push":    //传址
                    if (size == 2 && L0.ty == VALTYPE.listcon)
                    {
                        R0 = L1;
                        L0.List.Add(VAL.Clone(R0));
                        return L0;
                    }
                    break;
                case "pop":
                    if (size == 1 && L0.ty == VALTYPE.listcon)     //pop最后一个element
                    {
                        int index = L0.List.Size - 1;
                        R0 = L0.List[index];
                        L0.List.Remove(index);
                        return R0;
                    }
                    else if (size == 2 && L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.intcon)  //pop指定位置的的element
                    {
                        int index = L1.Intcon;
                        R0 = L0.List[index];
                        L0.List.Remove(index);
                        return R0;
                    }
                    break;


                //插入元素到指定位置 L.insert(int,e)
                case "insert":
                    if (size == 3 && L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.intcon)
                    {
                        L0.List.Insert(L1.Intcon, VAL.Clone(L[2]));
                        return L0;
                    }
                    break;
                //从指定位置中删除元素 L.remove(int)
                case "remove":
                    if (size == 2 && L0.ty == VALTYPE.listcon && L1.ty == VALTYPE.intcon)
                    {
                        L0.List.Remove(L1.Intcon);
                        return L0;
                    }
                    break;
                #endregion


                //DEBUG
                case "echo":
                    return new VAL(L);
                case "write":
                    return WriteLine(L);
                case "loginfo":
                    return LogInfo(L);



                #region internal functions used by parser

                //内部使用,强制类型转换cast
                //修改这个函数名,必须修改JExpression.s_exp16() 和 s_exp24()
                case Constant.FUNC_CAST_TYPE_VALUE:        //用于JExpression.s_exp16()的cast, (type)value 例如: a = (string[]}null;
                    if (size == 2)
                        return cast(L1, L0);
                    break;

                case Constant.FUNC_CAST_VALUE_TYPE:        //用于JExpression.s_exp24()的cast, value as type 例如: a = null as string;
                    if (size == 2)
                        return cast(L0, L1);
                    break;

                //用来实现.net中的is操作符
                case Constant.FUNC_IS_TYPE:
                    if (size == 2)
                    {
                        Type type = SystemFunction.GetValDefinitionType(L1);
                        if (type != null)
                        {
                            if (L0.value == null)
                                return new VAL(false);
                            else
                                return new VAL(type.IsAssignableFrom(L0.value.GetType()));
                        }
                        else
                            throw new RuntimeException(position, "{0} is not type or not registered.", L1.value);
                    }
                    break;

                //内部使用,产生数组类型 int[], 或者 int[,,]
                //修改这个函数名,必须修改JExpression.s_varnext()
                case Constant.FUNC_MAKE_ARRAY_TYPE:
                    if (size == 1 || size == 2)
                    {
                        Type ty = SystemFunction.GetValDefinitionType(L0);
                        if (ty != null)
                        {
                            if (size == 1)
                                return VAL.Boxing1(ty.MakeArrayType());
                            else if (L1.value is int)
                                return VAL.Boxing1(ty.MakeArrayType(L1.Intcon));
                        }
                        else
                            throw new RuntimeException(position, "declare array failed, {0} is not type.", L0.value);
                    }
                    break;


                //内部使用 $function(moduleName,addr) or $function(moduleName,functionName), 修改这里,必须同时修改VAL.encode中的funccon/classcon编码
                case Constant.FUNC_FUNCTION:
                    if (L[1].ty == VALTYPE.intcon)
                        return new VAL(Operand.Func(L[1].Intcon, L[0].Str));
                    else
                        return new VAL(Operand.Func(L[1].Str, L[0].Str));

                case Constant.FUNC_CLASS:
                    return new VAL(Operand.Clss(L[1].Intcon, L[0].Str));


                #endregion


                #region propertyof, methodof, fieldof

                /*
                 * 用于property name相同,但是返回类型不同的情况下,get/set property
                 * 
                 *   value = get(host, returnType, propertyName);
                 *   set(host, returnType, propertyName, value);
                 *   
                 * 可以读取private/protected property
                 *   value = get(host, propertyName);
                 *   set(host, propertyName, value);
                 * 
                 * 例如:
                 * VAL ret = listBox.propertyof(typeof(string),"SelectedValue");
                 * listBox.propertyof(typeof(string), "SelectedValue", 100);
                 * 
                 * */
                case "propertyof":
                    if (size >= 2 && size <= 4)
                    {
                        object host = L0.value;
                        if (host == null)
                            break;

                        if (L0.ty == VALTYPE.hostcon && L1.ty == VALTYPE.stringcon) //简单property匹配,没有二义性
                        {
                            if (size == 2 || size == 3)
                                return HostFunction.propertyof(size == 2, null, (string)L1.value, host, size == 2 ? null : L[2].HostValue);
                        }
                        else if (L0.ty == VALTYPE.hostcon                       //匹配返回值的property
                            && L1.ty == VALTYPE.hostcon && L1.value is Type
                            && L[2].ty == VALTYPE.stringcon)
                        {
                            if (size == 3 || size == 4)
                                return HostFunction.propertyof(size == 3, (Type)L1.value, (string)L[2].value, host, size == 3 ? null : L[3].HostValue);
                        }
                    }
                    break;


                /*
                 * 读写private/protected变量
                 *      instance.fieldof("fieldName");
                 *      instance.fieldof("fieldName", value);
                 * 
                 * */
                case "fieldof":
                    if (size == 2 || size == 3)
                    {
                        object host = L0.value;
                        if (host == null)
                            break;

                        if (L0.ty == VALTYPE.hostcon && L1.ty == VALTYPE.stringcon)
                        {
                            Type ty = GenericType.GetHostType(host);
                            FieldInfo fieldInfo = ty.GetField((string)L1.value, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if (fieldInfo == null)
                                throw new RuntimeException(position, string.Format("Invalid field name: {0}.{1}", ty.FullName, L1.value));

                            if (size == 2)
                                return VAL.Boxing1(fieldInfo.GetValue(host));
                            else
                            {
                                fieldInfo.SetValue(host, L[2].HostValue);
                                return VAL.NewVoidType();
                            }
                        }
                    }
                    break;




                /***
                 * 调用private/protected/public 方法
                 * 
                 * 例如:
                 *   var methodInfo = instance.methodof(typeof(returnType), methodName, {typeof(parameter0), ...});
                 *   
                 * */
                case "methodof":
                    if (size == 4)
                    {
                        object host = L0.value;
                        if (host == null)
                            break;

                        VAL L2 = L[2];
                        object args = L[3].HostValue;

                        if (L0.ty == VALTYPE.hostcon
                            && L1.ty == VALTYPE.hostcon && L1.value is Type
                            && L2.ty == VALTYPE.stringcon
                            && args is Type[])
                        {
                            MethodInfo methodInfo = HostFunction.methodof(host, (Type)L1.value, (string)L2.value, (Type[])args);
                            if (methodInfo != null)
                            {
                                VAL method = VAL.Boxing1(methodInfo);
                                //用methodInfo,而不是methodInfo.Name用来支持overloading,是因为这个方法可以唯一的赋值给TIE变量,不存在函数overloading的问题
                                //这里的methodInfo(没有什么用处是技术性)是强迫TIE直接使用methodInfo, 参见HostTypeFunction(VAL proc, VALL parameters)
                                method.temp = new HostOffset(host, methodInfo);
                                return method;
                            }
                            else
                                throw new RuntimeException(position, "method {0} is not existed", L2.value);
                        }
                    }
                    break;

                    #endregion

            }

            return null;    //返回null表示继续在FunctionChain中传递
        }



        #region System function implementation

        private static VAL WriteLine(VALL L)
        {
            StringWriter o = new StringWriter();
            for (int i = 0; i < L.Size; i++)
                o.Write(L[i].ToSimpleString());

            Logger.WriteLine(o.ToString());
            return new VAL(L);   //return void
        }

        private static VAL LogInfo(VALL L)
        {
            StringWriter o = new StringWriter();
            o.Write(System.DateTime.Now);
            o.Write(" ");
            for (int i = 0; i < L.Size; i++)
                o.Write(L[i].ToSimpleString());

            Logger.WriteLine(o.ToString());
            return new VAL(L);   //return void
        }




        private static VAL format(VALL L)
        {
            string fmt = L[0].Str;
            object[] args = new object[L.Size - 1];
            for (int i = 1; i < L.Size; i++)
                if (L[i].ty != VALTYPE.listcon)
                    args[i - 1] = L[i].value;  //use C# string.format string control
                else
                    args[i - 1] = L[i].ToSimpleString();

            string s = string.Format(fmt, args);
            return new VAL(s);
        }



        public static VAL Slice(VALL arr)
        {
            VAL V = arr[0];

            int start = 0;
            int stop = -1;
            int step = 1;

            switch (arr.Size)
            {
                case 1:
                    break;
                case 2:
                    start = arr[1].Intcon;
                    break;
                case 3:
                    start = arr[1].Intcon;
                    stop = arr[2].Intcon;
                    break;
                case 4:
                    start = arr[1].Intcon;
                    stop = arr[2].Intcon;
                    step = arr[3].Intcon;
                    break;
            }

            return new VAL(V.List.Slice(start, stop, step));

        }



        #endregion


        #region CAST/Convert

        public static VAL cast(VAL val, VAL type)
        {
            if (type.ty == VALTYPE.voidcon)  //如果cast的类型根本就没有定义过,为了省事就忽略掉,主要原因是为了让C#拷贝过来的代码,不register也能运行
                return val;

            if (type.value is Type)
            {
                return VAL.cast(VAL.Clone(val), (Type)type.value);
            }
            else
                throw new TieException("cast failed, {0} is not type.", type.value);
        }



        #endregion


        //用于编译环境,如果L0已经是注册过的就返回,否则,来推断Type
        private static Type GetValDefinitionType(VAL L0)
        {
            if (L0.value is Type)
                return (Type)L0.value;

            Type ty = HostType.GetType(L0.name);    //如果是没有注册过的Type
            if (ty != null)
                return ty;
            else
                return null;
        }


    }


}

