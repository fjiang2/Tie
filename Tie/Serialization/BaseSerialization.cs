using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Serialization
{
    abstract class BaseSerialization
    {
        protected abstract VAL valize(object host);
        protected abstract object devalize(VAL val);


        protected BaseSerialization()
        {
        }

        public VAL Valize(object host)
        {
            VAL val = valize(host);

            if (val.ty == VALTYPE.stringcon)
            {
                val.ty = VALTYPE.scriptcon;         //scriptcon 不输出"", 不象字符串
            }

            val.Class = host.GetType().FullName;

            return val;
        }

        public object Devalize(VAL val)
        {
            return devalize(val);
        }

    }
}
