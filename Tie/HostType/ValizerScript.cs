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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Tie
{
    /**
     * 
     * 用来支持已经存在的class的Valization
     * 譬如对System.Windows.Forms.TextBox的Valization
     * 
     * 定义用来产生class实例的script
     * 
     * 
     * */

    class ValizerScript
    {

        #region class implementation

        /**
         * 1. string script
         * script example for typeof(Color) :
         *  "this.GetType().FullName + '.' + (this.Name=='0'?'Black':this.Name)"
         *  --> System.Drawing.Color.Name
         *  
         * 2. delegate Valizer /delegate Devalizer
         *   .NET实现的Persistent Object Script
         *   
         * 3. 支持interface IValizer
         * 
         * 4. Field/Property List, string[] members = new string[]{"Text","Name"};
         * 
         * */

        private object valizer;
        private object devalizer;

        protected ValizerScript(object valizer, object devalizer)
        {
            this.valizer = valizer;
            this.devalizer = devalizer;
        }

        private VAL Valize(object host)
        {
            VAL val;

            if (this.valizer is string)
            {
                val = Script.Run(host, (string)valizer, new Memory());
                goto L1;
            }


            if (this.valizer is Valizer)
            {
                Valizer valizer = (Valizer)this.valizer;
                val = valizer(host);
                goto L1;    
            }

            if (this.valizer is IValizer)
            {
                IValizer I = (IValizer)this.valizer;
                val = I.Valizer(host);
                goto L1;
            
            }

            if (this.valizer is string[])
            {
                string[] members = (string[])this.valizer;
                string script = "";
                for (int i = 0; i < members.Length; i++)
                {
                    if (i != 0)
                        script += ",";
                    script += string.Format("{0} : this.{0}", members[i]);
                }

                script = "{" + script + "}";

                val = Script.Run(host, script, new Memory());
                goto L1;
            }

            return null;
        
            L1:
            if (val.ty == VALTYPE.stringcon)
                val.ty = VALTYPE.scriptcon;         //scriptcon 不输出"", 不象字符串

            val.Class = host.GetType().FullName;

            return val;

        }


        private object Devalize(VAL val)
        {
            if (this.devalizer is string)
            {
                VAL x = Script.Run(val, (string)devalizer, new Memory());
                return x.HostValue;
            }

            if (this.devalizer is Devalizer)
            {
                Devalizer devalizer = (Devalizer)this.devalizer;
                return devalizer(val);
            }

            if (this.valizer is IValizer)
            {
                IValizer I = (IValizer)this.valizer;
                return I.Devalizer(val);
            }
            
            return null;
        }


  
        private static ValizerScript Engine(Type type)
        {
          //  if (!type.IsArray)
            {
                VAL ty = Script.Evaluate(type.FullName);
                if (ty.temp is ValizerScript)                                        //用HostType.Register(Type, Persistent)注册
                    return (ValizerScript)ty.temp;
            }

            return null;
        }

        #endregion


        #region public Tools

        //为已经存在的class注册一个Valizable的对象, 供 HostType.Register使用
        public static VAL Register(Type type, object valizer, object devalizer)
        {
            if (type.IsArray)
                HostType.Register(type.GetElementType(), false);
            else
                HostType.Register(type, false);

            VAL ty = Script.Evaluate(type.FullName);
            ty.temp = new ValizerScript(valizer, devalizer);
            return ty;
        }


        public static bool Registered(Type type)
        {
            ValizerScript engine = ValizerScript.Engine(type);
            return engine != null;
        }

        //处理注册过Type的customerized的Persistent代码, 用于HostValization.Host2Valor(..)
        public static VAL ToValor(object host)
        {
            if (host == null)
                return null;

            ValizerScript engine = ValizerScript.Engine(host.GetType());
            
            if (engine != null)
                return engine.Valize(host);
            
            return null;
        }

        //用于设置[Valizable]属性地方的script处理
        public static VAL ToValor(MemberInfo memberInfo, object host)
        {
            if (host == null)
                return null;

            //处理customerized的Valizable代码
            ValizableAttribute[] attributes = (ValizableAttribute[])memberInfo.GetCustomAttributes(typeof(ValizableAttribute), true);
            if (attributes.Length != 0)
            {
                if (attributes[0].valizer != null)      //Field或者Property定义了[Valizable]属性,并且定义了customerized
                    return (new ValizerScript(attributes[0].valizer, null)).Valize(host);
            }

            return ToValor(host);
        }


        //把Val值解析(Devalize)为host, 用于HostValization.Val2Host(..)
        public static object ToHost(VAL val, Type hostType)
        {
          ValizerScript engine = ValizerScript.Engine(hostType);
            
            if (engine != null)                                   
                 return engine.Devalize(val);
            
            return null;
        }

        #endregion


    }

}
