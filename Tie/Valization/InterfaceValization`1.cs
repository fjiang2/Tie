using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Valization
{

    class InterfaceValization<T> : BaseValization
    {

        private IValizer<T> valizer;

        public InterfaceValization(IValizer<T> valizer)
        {
            this.valizer = valizer;
        }

        protected override VAL valize(object host)
        {
            VAL val;
            if (host == null)
                val = valizer.Valizer(default(T));
            else
                val = valizer.Valizer((T)host);

            return val;

        }


        protected override object devalize(object host, VAL val)
        {
            if (host == null)
                return valizer.Devalizer(default(T), val);
            else 
                return valizer.Devalizer((T)host, val);
        }
    }
}
