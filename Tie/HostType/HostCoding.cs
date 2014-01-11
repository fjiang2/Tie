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
#if !SILVERLIGHT
//using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace Tie
{
    class HostCoding
    {
        #region 扩展Decode/Encode 支持.NET Object  in Computer.DS

        /*
         * 
         * .net object format/protocol:  
         * 
         *      (1) new className(args,...) 
         *          如 new System.Windows.Form.TextBox()
         *             new Tie.VAL(20)
         *             
         * 
         *      (2) new SerializedObject(string className, string value.ToString(), string SerializedValue);       
         *          请看Encode()生成的格式
         *      
         * 例如:
         *      前提:
         *          System.Windows.Form.Label的值是Type, 已经用Register函数登记在数据字典Computer.DS中.
         *     
         *      求:
         *          new System.Windows.Form.Label();   转化为  new Label(System.Window.Form);
         * 
         * */
        public static VAL Decode(string className, VAL args, VAL scope, Context context)
        {
            VAL clss = new VAL();
            /*
                * 如果是注册过的class, 那么它的namespace是定义在Computer.DS中
                * 譬如 
                *      A= new NS.CLSS();
                *      scope[className] --> clss;
                *  
                * 如果没有定义namespace,那么直接到Computer.DS中去取值
                * 譬如
                *      A= CLSS();
                *      Computer.DS[className] --> clss
                *   
                * */
            clss = scope[className];
            if (clss.Undefined)
                clss = context.GetVAL(className, true);         //如果没有找到val.Class, context.GetVAL(...)返回new VAL()

            //返回注册过的class名字
            if (clss.Defined && clss.value is Type)
            {
                Type type = (Type)clss.value;
                object instance = Activator.CreateInstance(type, ConstructorArguments(args));
                return VAL.Boxing1(instance); //返回实例
            }

            //throw new RuntimeException(string.Format("class [{0}]has not registered yet.", scope.name + "." + val.Class)); 
            //如果没有注册过
            if (scope.name != null)
            {
                object instance = HostType.NewInstance(scope.name + "." + className, ConstructorArguments(args));

                if (instance != null)
                {
                    //把HostType类型注册到CPU.DS2中去
                    VAL hostType = VAL.NewHostType(instance.GetType());
                    scope[className] = hostType;

                    return VAL.Boxing1(instance);  //返回实例
                }
            }
            
          
            if (clss.IsNull)
                throw new HostTypeException("class {0} is not defined.", className);


            if (clss.value != null)
            {
                Type type;
                if (clss.value is Type)
                    type = (Type)clss.value;
                else
                    type = clss.value.GetType();

                object instance = Activator.CreateInstance(type, ConstructorArguments(args));
                return VAL.NewHostType(instance);
            }


            return args;
        }

        public static object[] ConstructorArguments(VAL args)
        {
            if (args.Size > 0)
                return args.List.ObjectArray;
            else
                return null;
        }



        public static string Encode(object host, bool persistent)
        {
            VAL val = HostValization.Host2Val(host);
            if (persistent)
                return val.Valor;


            if (host is MethodInfo)
            {
                //TODO: not finished yet
                MethodInfo methodInfo = (MethodInfo)host;
                if (methodInfo.IsStatic)
                    return string.Format("{0} {1}.{2}()", methodInfo.ReturnType.Name, methodInfo.ReflectedType.FullName, methodInfo.Name);
                else
                    return string.Format("{0} {1}()", methodInfo.ReturnType.Name, methodInfo.Name); 
            }

            Type type = GenericType.GetHostType(host);

            //default contructor      
            if (GenericType.HasContructor(type, new Type[] { }))
                return string.Format("new {0}()", type.FullName);   //有缺省的constructor
            else
                return val.Valor;

        }


        #endregion







     


        
        
        #region 发现HostType数组元素的共同Type( interface[] /base class), 生成.NET需要的数组Type

        /**
        * 如果是.NET普通的数组,那么返回的是特定数组类型,而不是object[]类型, 譬如string[], int[],
        * 因为.NET 不接受这样的cast:
        *     string[] stringArray = (string[])objectArray;
        * 
        * */
        public static object ToHostArray(object[] values)
        {

            if (values.Length == 0)
                return values;

            //如果有相同的base class
            Type type = CommonBaseClass(values);
            if (type == null)
            {
                
                //如果有相同的interface
                Type[] I = CommonInterface(values);
                if (I.Length == 0)
                    return values;

                /*
                 *  选取第一个interface来初始化数组,这个也是有问题的.
                 *  因为有可能是可以用第二个interface 的
                 * 
                 * */
                type = I[0];
            }


            Type arrayType = type.MakeArrayType();
            Array array = (Array)Activator.CreateInstance(arrayType, new object[] { values.Length });

            for(int i=0; i< values.Length; i++)
            {
                array.SetValue(values[i], i);
            }

            return array;

        }



        /**
         * 
         * 假如list中的所有元素都是相同类型的,或者同一个class继承下来的
         * 找出数组相同的base class
         * 
         * */
        public static Type CommonBaseClass(object[] values)
        {
            Type type = null;
            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                type = obj.GetType();
                break;
            }

            if (type == null)
                return null;

            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                Type t = obj.GetType();

                if (t == type || t.IsSubclassOf(type))
                    continue;
                else if (type.IsSubclassOf(t))
                    type = t;
                else
                {
                    //共同的base class没有找到,就去找共同的interface
                    return null;
                }
            }

            return type;

        }



        /**
        * 
        * 假如list中的所有元素都是从实现了同一个interface
        * 
        * 找出实现了相同的interace的Type
        * 
        * 
        * */
        public static Type[] CommonInterface(object[] values)
        {

            Type[] I = new Type[0];
            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                I = obj.GetType().GetInterfaces();
                if (I.Length != 0)
                     break;
                
            }

            if (I.Length == 0)
                return I;

            foreach (object obj in values)
            {
                if (obj == null)
                    continue;

                Type t = obj.GetType();
                I = CommonInterface(I, t.GetInterfaces());

                if (I.Length == 0)
                    return I;
            }

            return I;
        
        }


        /**
         * 
         * 找出相同的interaces
         * 
         * */
        private static Type[] CommonInterface(Type[] I1, Type[] I2)
        {
            List<Type> I = new List<Type>();
            foreach(Type i1 in I1)
            {
                foreach(Type i2 in I2)
                {
                    if(i1==i2)
                        I.Add(i1);
                }
            }
            
            return I.ToArray() ;
        }

    
        #endregion


      

     
    }
}
