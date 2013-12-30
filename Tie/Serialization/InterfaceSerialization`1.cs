using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Serialization
{

    class InterfaceSerialization<T> : BaseSerialization
    {

        private IValizer<T> valizer;

        public InterfaceSerialization(IValizer<T> valizer)
        {
            this.valizer = valizer;
        }

        protected override VAL valize(object host)
        {
            VAL val = valizer.Valizer((T)host);
            return val;

        }


        protected override object devalize(VAL val)
        {
            return valizer.Devalizer(val);
        }
    }
}
