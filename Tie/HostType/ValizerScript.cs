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
using System.Reflection;

namespace Tie
{
    abstract class Valization
    {
        protected abstract VAL Valize(object host);
        protected abstract object Devalize(VAL val);


        protected Valization()
        { 
        }
        
        public VAL Valize2(object host)
        {
            VAL val = Valize(host);

            if (val.ty == VALTYPE.stringcon)
                val.ty = VALTYPE.scriptcon;         //scriptcon 不输出"", 不象字符串

            val.Class = host.GetType().FullName;

            return val;
        }
        
        public object Devalize2(VAL val)
        {
            return Devalize(val);
        }
 
    }

    class ValizationDelegate<T> : Valization
    {
        private Valizer<T> valizer;
        private Devalizer<T> devalizer;

        public ValizationDelegate(Valizer<T> valizer, Devalizer<T> devalizer)
        {
            this.valizer = valizer;
            this.devalizer = devalizer;
        }

        protected override VAL Valize(object host)
        {
            VAL val = valizer((T)host);
            return val;
        }

        protected override object Devalize(VAL val)
        {
            if(devalizer != null)
                return devalizer(val);

            return null;
        }

    }

    class ValizationScript : Valization
    {
        private string valizer;
        private string devalizer;

        public ValizationScript(string valizer, string devalizer)
        {
            this.valizer = valizer;
            this.devalizer = devalizer;
        }

        protected override VAL Valize(object host)
        {
            VAL val = Script.Run(host, valizer, new Memory());
            return val;

        }


        protected override object Devalize(VAL val)
        {
            if (devalizer == null)
                return null;

            VAL x = Script.Run(val, devalizer, new Memory());
            return x.HostValue;
        }

    }


    class ValizationInterface<T> : Valization
    {

        private IValizer<T> valizer;

        public ValizationInterface(IValizer<T> valizer)
        {
            this.valizer = valizer;
        }

        protected override VAL Valize(object host)
        {
            VAL val = valizer.Valizer((T)host);
            return val;

        }


        protected override object Devalize(VAL val)
        {
            return valizer.Devalizer(val);
        }
    }


    class ValizationProperty : Valization
    {
        private string[] valizer;
        private object devalizer;


        public ValizationProperty(string[] valizer)
        {
            this.valizer = valizer;
        }

        protected override VAL Valize(object host)
        {
            VAL val;
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


            if (val.ty == VALTYPE.stringcon)
                val.ty = VALTYPE.scriptcon;         //scriptcon 不输出"", 不象字符串

            val.Class = host.GetType().FullName;

            return val;

        }


        protected override object Devalize(VAL val)
        {
            return null;
        }
    }


    /**
     * 
     * 用来支持已经存在的class的Valization
     * 譬如对System.Windows.Forms.TextBox的Valization
     * 
     * 定义用来产生class实例的script
     * 
     * 
     * */

    static class ValizeRegistry
    {

        static Dictionary<Type, Valization> registries = new Dictionary<Type, Valization>();

        public static void Register(Type type, Valization valization)
        {
            if (registries.ContainsKey(type))
                registries.Remove(type);

            registries.Add(type, valization);
        }

        public static bool Registered(Type type)
        {
            return registries.ContainsKey(type);
        }

        //处理注册过Type的customerized的Persistent代码, 用于HostValization.Host2Valor(..)
        public static VAL ToValor(object host)
        {
            if (host == null)
                return null;

            Type type = host.GetType();
            if(registries.ContainsKey(type))
            {
                return registries[type].Valize2(host);
            }
            
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
                    return (new ValizationScript((string)attributes[0].valizer, null)).Valize2(host);
            }

            return ToValor(host);
        }


        //把Val值解析(Devalize)为host, 用于HostValization.Val2Host(..)
        public static object ToHost(VAL val, Type hostType)
        {
             if (registries.ContainsKey(hostType))
             {
                 return registries[hostType].Devalize2(val);
             }
            
            return null;
        }

        


    }

}
