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
    enum OffsetType
    {
        ANY= 1,
        STRUCT=2,
        ARRAY=4
    }

    class HostOffset
    {
        public readonly object host;
        public readonly object offset;

        public HostOffset(object host, object offset)
        {
            this.host = host;
            this.offset = offset;
        }
    }


    class HostOperation
    {

        public static VAL Assign(VAL R0, VAL R1)
        {
            bool r = false;
            if (R0.ty != VALTYPE.funccon)
                r = HostOperation.HostTypeAssign(R0, R1);

            if (!r)
            {
                R0.ty = R1.ty;
                R0.Class = R1.Class;
                R0.hty = R1.hty;
                R0.value = R1.value;
                //R0.name = R1.name;    //变量的name不能传递

                if (R1.ty == VALTYPE.funccon
                    || (R1.ty == VALTYPE.hostcon && (R1.value is MethodInfo || R1.value is MethodInfo[])) //TIE函数或者C#方法
                    ) 
                    R0.temp = R1.temp;      //instance of CPU 
            }
            return R0;
        }



        #region HostType Assign
        //HostType赋值语句
        //HostTypeAssign(VAL DST, VAL SRC)
        public static bool HostTypeAssign(VAL R0, VAL R1)
        {
            /***
             * 
             * CASE 2:  
             *      textbox1.Text ="Hello";
             * 
             * */
            if (R0.temp != null && R0.temp is HostOffset)
            {
                HostOffset hosts = (HostOffset)R0.temp;
                object host = hosts.host;
                object offset = hosts.offset;

                //因为一个变量R0被赋值为MethodInfo以后, R0.temp就会有值,如果R0又再次赋值为另外一个MethodInfo的话,那么就是简单赋值
                //所以这里要返回false, 用于SystemFunction中的methodof(...), 以及Script.SyncInstance(..)
                //offset 在这里作为标志用,是技术性的
                if (offset is MethodInfo)   
                    return false;

                return HostTypeAssign(host, offset, R1.HostValue, R1.hty == HandlerActionType.Add);  //参照class HostEvent, R1.SEG用作Add/Remove Handler的标志
            }

         
            
            return false;
        }

        public static bool HostTypeAssign(object host, object offset, object val, bool addHandler)
        {
            Type type = host.GetType();




            if (offset is string)
            {
                PropertyInfo propertyInfo = type.GetProperty((string)offset);
                if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        if (IsCompatibleType(propertyInfo.PropertyType, val, null))
                        {
                            propertyInfo.SetValue(host, val, null);
                            return true;
                        }
                        else
                            throw new HostTypeValueNotMatchedException(string.Format("{0} is not matched to property {1}.{2}", val, type.FullName, offset));
                    }
                    else
                        throw new HostTypeException("property {0}.{1} is read only.", type.FullName, offset);
                }
            }

            if (host.GetType().IsArray)
            {
                Array array = (Array)host;
                if (offset is int)
                {
                    //如果超过数组的大小
                    if ((int)offset >= array.Length)
                        throw new HostTypeException("Array {0}[{1}] index is out of range[0..{2}].", host, offset, array.Length);

                    array.SetValue(val, (int)offset);
                    return true;
                }
                else if (offset is int[])
                {
                    array.SetValue(val, (int[])offset);
                    return true;
                }
                else
                    throw new HostTypeException("Array {0}.[{1}] subscript must be integer.", host.GetType().FullName, offset);
            }

            //处理this[,,..]属性
            {
                Type offsetType = (offset != null) ? offsetType = offset.GetType() : typeof(object);
                Type valType = (val != null)? val.GetType() : typeof(object);

                Type[] types;
                object[] objectArray;

                //多维属性
                if (offset is Array)
                {
                    Array array = (Array)offset;
                    types = new Type[array.Length + 1];
                    objectArray = new object[array.Length + 1];
                    int i = 0;
                    foreach (object obj in array)
                    {
                        types[i] = obj.GetType();
                        objectArray[i] = obj;
                        i++;
                    }
                    types[i] = valType;
                    objectArray[i] = val;
                }
                else  //简单属性
                {
                    types = new Type[] { offsetType, valType };
                    objectArray = new object[] { offset, val };
                }

                //用于其他的Collection, 例如Dictionary<string.int>, this[int] {get;} this[string] {get;}
                MethodInfo methodInfo = type.GetMethod("set_Item", types);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(host, objectArray);
                    return true;
                }
            }

            FieldInfo fieldInfo = type.GetField((string)offset);
            if (fieldInfo != null)
            {
                if (IsCompatibleType(fieldInfo.FieldType, val, null))
                {
                    fieldInfo.SetValue(host, val);
                    return true;
                }
                else
                    throw new HostTypeValueNotMatchedException(string.Format("{0} is not matched to field {1}.{2}", val, type.FullName, offset));

            }

            EventInfo eventInfo = type.GetEvent((string)offset);
            if (eventInfo != null)
            {
                if (val is Delegate)
                {
                    if (addHandler)
                        eventInfo.AddEventHandler(host, val as Delegate);
                    else
                        eventInfo.RemoveEventHandler(host, val as Delegate);
                    return true;
                }
                else
                    throw new HostTypeException("{0} is not delegate of {1}.{2}.", val, type.FullName, offset);
            }

            //BAD Performance
            //throw new HostTypeMemberNotFoundException(string.Format("Property/Attribute [{0}] is not supported in class {1}", offset, type.FullName));
            return false;
        }

        #endregion


        





        #region HostType Offset
        /**
         * 取出.NET object的属性
         * 如:
         *      this[string]
         *      this[int]
         *      this.property
         * 
         * */
      
        public static VAL HostTypeOffset(VAL R0, VAL R1, OffsetType offsetType)
        {
            if (R0.ty != VALTYPE.hostcon)
                return VAL.NewVoidType();

            
            object host = R0.value;

            object offset = R1.HostValue;
          
            
            object obj = null;
            Type type = null;

            if (host is Type)
            {
                type = (Type)host;

                if (!(offset is string))
                    throw new HostTypeException("{0} offset {1} must be ident type.", type.FullName, offset);

                if (type.IsEnum)            //处理enum类型
                {
                    FieldInfo fieldInfo = type.GetField((string)offset);
                    if (fieldInfo != null && fieldInfo.IsStatic)
                        return VAL.Boxing1(fieldInfo.GetValue(host));
                    else
                        throw new HostTypeException("enum {0} offset {1} is not enum type.", type.FullName, offset);
                }

                if (offsetType == OffsetType.STRUCT || offsetType == OffsetType.ANY)
                {
                    //处理struct中的静态属性System.Drawing.Color.Red, host=System.Drawing.Color; offset=Red
                    return HostTypeOffsetMemberInfo(type, host, offset);
                }
            }

            type = host.GetType();

            
            if (offsetType == OffsetType.ANY || offsetType == OffsetType.ARRAY)
            {
                //数组 abstract Array: IList, ICollection, IEumerable
                //interface IList : ICollection, IEumerable
                //interface ICollection : IEumerable
                if (type.IsArray)
                {
                    Array array = (Array)host;
                    if (offset is int)
                    {
                        if ((int)offset >= array.Length)
                            throw new HostTypeException("Array {0}[{1}] index is out of range[0..{2}].", host, offset, array.Length);
                        return HostTypeOffsetBoxing(array.GetValue((int)offset), host, offset);
                    }
                    else if (offset is int[])
                    {
                        return HostTypeOffsetBoxing(array.GetValue((int[])offset), host, offset);
                    }
                    else
                        throw new HostTypeException("Array {0}.[{1}] subscript must be integer.", host.GetType().FullName, offset);
                }

                //数组其实也是IEnumerable的一种形式,所以上面的type.IsArray那些语句其实是可以省略的.保留着,可能性能会好一点
                if (host is IEnumerable && offset is int)
                {
                    IEnumerable collection = (IEnumerable)host;
                    int index = (int)offset;

                    IEnumerator enumerator = collection.GetEnumerator();

                    //Enumerator很健忘,所以每次得从开始移动到指定的index位置, IEnumerable在.NET是不支持下标为int的操作的, 
                    //TIE通过下面的语句达到支持, TIE的foreach语句需要这个支持
                    bool end = false;
                    int count = 0;
                    for (int i = 0; i < index + 1; i++)
                        if (!enumerator.MoveNext())
                        {
                            end = true;
                            count = i;
                            break;
                        }

                    if (end && index >= count)
                        throw new HostTypeException("IEnumerable {0}[{1}] index is out of range[{2}].", host, offset, count);

                    if (!end)
                        return HostTypeOffsetBoxing(enumerator.Current, host, offset);
                }

                
                {   //处理this[,,...]属性
                    Type[] types;
                    object[] objectArray;

                    //多维属性, 例如: this[int i,string j, int k] { get; set;}
                    if (R1.ty == VALTYPE.listcon)
                    {
                        types = new Type[R1.Size];
                        for (int i = 0; i < types.Length; i++)
                        {
                            types[i] = R1[i].Type;
                        }

                        objectArray = R1.ObjectArray;
                    }
                    else
                    {
                        types = new Type[] { R1.Type };
                        objectArray = new object[] { offset };
                    }
                    //集合Collection, 譬如用于Dictionary<string, int>, 或者 this[string] {get;}, this[int] {get;}
                    MethodInfo methodInfo = type.GetMethod("get_Item", types);
                    if (methodInfo != null)
                    {
                        try
                        {
                            obj = methodInfo.Invoke(host, objectArray);
                            if (obj != null)
                                return HostTypeOffsetBoxing(obj, host, offset);
                        }
                        catch (Exception e)
                        {
                            if (offsetType == OffsetType.ARRAY)
                                throw e;
                        }
                    }
                }


                if (offsetType == OffsetType.ARRAY)
                    return VAL.NewVoidType();
            }

            if (offset is string)
               return HostTypeOffsetMemberInfo(type,host, offset);
            else
               return HostTypeOffsetBoxing(null, host, offset);

        }

        private static VAL HostTypeOffsetMemberInfo(Type type, object host, object offset)
        {
            object obj = null;

            //Type 本身的property是不能用GetProperty方式得到的,所以一一列出
            if (offset.Equals("FullName"))
                return new VAL(type.FullName);


            FieldInfo fieldInfo = type.GetField((string)offset);
            if (fieldInfo != null)
            {
                obj = fieldInfo.GetValue(host);
                return HostTypeOffsetBoxing(obj, host, offset);
            }

            PropertyInfo propertyInfo = type.GetProperty((string)offset);
            if (propertyInfo != null)
            {
                if (propertyInfo.CanRead)
                    obj = propertyInfo.GetValue(host, null);

                return HostTypeOffsetBoxing(obj, host, offset);
            }

            MethodInfo[] methods = HostFunction.OverloadingMethods(type, (string)offset);
            if (methods.Length > 0)
            {
                if (methods.Length == 1)
                    return HostTypeOffsetBoxing(methods[0], host, offset);

                //overloading函数
                return HostTypeOffsetBoxing(methods, host, offset);
            }

            EventInfo eventInfo = type.GetEvent((string)offset);
            if (eventInfo != null)
                return HostTypeOffsetBoxing(eventInfo, host, offset);

            return VAL.NewVoidType();
        }

        private static VAL HostTypeOffsetBoxing(object value, object host, object offset)
        {
            VAL v = VAL.Boxing1(value);
            v.temp = new HostOffset(host, offset);
            return v;
        }


      
        #endregion





        #region HostType Function/Method

        //proc 是HostType函数指针, 如果不是HostType函数,就返回null
        public static VAL HostTypeFunction(VAL proc, VALL parameters)
        {
            VAL ret = VAL.NewVoidType();
            if (proc.ty == VALTYPE.hostcon && (proc.value is MethodInfo || proc.value is MethodInfo[]))
            {
                HostOffset temp = (HostOffset)proc.temp;
                object host = temp.host;
                object offset = temp.offset;  //函数名字

                if (offset is MethodInfo)
                    ret = VAL.Boxing1(((MethodInfo)proc.value).Invoke(host, parameters.ObjectArray));    //用于Script.SyncInstance, 可以处理private的method
                else
                {
                    HostFunction hFunc = new HostFunction(host, (string)offset, parameters);

                    //overloading函数
                    if (proc.value is MethodInfo[])
                        ret = hFunc.RunFunction((MethodInfo[])proc.value);     //处理overloading methods
                    else
                    {
                        //普通函数
                        MethodInfo method = (MethodInfo)proc.value;
                        if (method.IsGenericMethod)
                            ret = hFunc.RunFunction(new MethodInfo[] { method });    //如果是generic method,就在本范围内搜索
                        else
                            ret = hFunc.RunFunction();     //这里只能处理public的method
                    }
                }
            }
            else if(proc.value is Delegate)         //不用假定delegate是静态函数, 因为我们用.net的机制, 把d.Target传入delegate,以便使用到的外部变量
            {
                Delegate d = (Delegate)proc.value;
                MethodInfo method = d.Method;
                object[] arguments = parameters.ObjectArray;
                return HostFunction.InvokeMethod(method, d.Target, arguments);
            }
            return ret;
        }


      
        #endregion



        #region Compatible Type 

      
        
        //检查val是不是和Type是相容的
        public static bool IsCompatibleType(Type type, object val, Type valType)
        {

            if (val == null)
            {
                //有强制类型转换的
                if (valType != null)
                    return type.IsAssignableFrom(valType);

                if (!type.IsValueType)
                    return true;
                else
                {
                    if (Nullable.GetUnderlyingType(type) == null)
                        throw new HostTypeValueNotMatchedException(string.Format("Value type property {0} cannot be assigned by null", type.FullName));
                    else
                        return true;
                }
            }
            else
            {
                valType = val.GetType();

                if (type.IsAssignableFrom(valType))
                    return true;

                else if (type.IsEnum && val is int)
                    return true;

                else if (type.IsGenericType && Nullable.GetUnderlyingType(type) == valType)
                    return true;

                else 
                    return false;
            }

        }



        #endregion


        
        #region HostType Compare
        
        public static int HostCompareTo(Operator opr, VAL v1, VAL v2)
        {
            if (v1.ty != VALTYPE.hostcon || v2.ty != VALTYPE.hostcon)
                throw new HostTypeException("cannot compare different type value {0} and {1}.", v1, v2);

            object x1 = v1.value;
            object x2 = v2.value;

            //如果都为null
            if (x1 == null && x2 == null)
                return 0;

            //如果其中一个为null
            if (x1 != null && x2 == null)
            {
                if (x1.GetType().IsValueType)     //并且另外一个为value type
                    throw new HostTypeException("cannot compare value type {0} to null.", x1);
                else
                    return 1;                           // 任何的非null的object > null
            }
            else if (x1 == null && x2 != null)
            {
                if (x2.GetType().IsValueType)     //并且另外一个为value type
                    throw new HostTypeException("cannot compare value type {0} to null.", x2);
                else
                    return -1;                          // 任何的非null的object > null
            }


            if (x1 is Type && x2 is Type)
            {
                if ((Type)x1 == (Type)x2)
                    return 0;
                else
                    return -1;
            }
            else if (!(x1 is Type) && (x2 is Type) || (x1 is Type) && !(x2 is Type))
            {
                throw new HostTypeException("cannot compare type to non-type: {0} and {1}.", x1, x2);
            }

            //下面的code是二个都不为null
            Type type1 = x1.GetType();
            Type type2 = x2.GetType();
            
            //如果都为ValueType, 目前只支持比较相等的值
            if (type1.IsValueType && type2.IsValueType)
            {
                if (System.ValueType.Equals(x1, x2))
                    return 0;
            }


            Type[] I = type1.GetInterfaces();
            if (I.Length != 0)
            {
                foreach (Type i in I)
                {
                    if (i == typeof(IComparable))
                        return (x1 as IComparable).CompareTo(x2);
                }
            }

            I = type2.GetInterfaces();
            if (I.Length != 0)
            {
                foreach (Type i in I)
                {
                    if (i == typeof(IComparable))
                        return (x2 as IComparable).CompareTo(x1);
                }
            }


            //operator overloading >, >=, <, <=, !=, ==
            VAL comp = HostFunction.OperatorOverloading(opr, v1, v2, true);
            if ((object)comp != null)
            {
                switch (opr)
                {
                    case Operator.op_LessThan:
                        return comp.Boolcon? - 1: 10;
                    case Operator.op_Equality:
                        return comp.Boolcon ? 0 : 10;
                    case Operator.op_GreaterThan:
                        return comp.Boolcon ? 1 : -10;
                }
            }


            //如果是相同基类继承下去的对象,目前只支持比较相等与否
            Type type = HostCoding.CommonBaseClass(new object[] { x1, x2});
            if (type != null)
            {
                if (x1 == x2)
                     return 0;
            }


            throw new HostTypeException("cannot compare value {0} and {1} without implement IComparable.", x1, x2);

        }

        #endregion


        #region HostType Enum

        public static VAL EnumOperation(VAL R0, VAL R1, object value)
        {
            Type type0 = R0.value.GetType();
            Type type1 = R1.value.GetType();

            /*
             *  前面的enum操作数具有高优先级, 最后的结果类型以前面为准, 
             *  譬如:  System.Windows.Forms.FontStyle.Bold |　System.Drawing.GraphicsUnit.Point
             * 
             *  最后输出类型为: System.Windows.Forms.FontStyle
             * 
             **/
            
            Type type = null;
            if (type1.IsEnum)       
                type = type1;
            else if (type0.IsEnum)
                type = type0;

            if (type != null)
                return VAL.Boxing1(Enum.ToObject(type, value));  //如果是enum的bit flags
            else
                return VAL.Boxing1(value);                       //普通的int整数

        }


        //返回用 ｜ 分隔的enum字符串
        public static string EnumBitFlags(object host)
        {
            Type type = host.GetType();

            if(Enum.IsDefined(type,host))
                return string.Format("{0}.{1}", type.FullName, host);

            string s = "";

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (!fieldInfo.IsLiteral)
                    continue;

                int offset = (int)fieldInfo.GetValue(type);
                if (offset != 0 && ((int)host & offset) == offset)
                {
                    if (s != "")
                        s += "|";
                    s += string.Format("{0}.{1}", type.FullName, Enum.ToObject(type, offset).ToString());
                }
            }

            return s;

        }
        
        #endregion

    }
}
