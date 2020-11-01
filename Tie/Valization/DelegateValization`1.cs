using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Valization
{
    class DelegateValization<T> : BaseValization
    {
        private Valizer<T> valizer;
        private Devalizer<T> devalizer;

        public DelegateValization(Valizer<T> valizer, Devalizer<T> devalizer)
        {
            this.valizer = valizer;
            this.devalizer = devalizer;
        }

        protected override VAL valize(object host)
        {

            VAL val;
            if (host == null)
                val = valizer(default(T));
            else
                val = valizer((T)host);
            return val;
        }

        protected override object devalize(object host, Type hostType, VAL val)
        {
            if (devalizer != null)
                return devalizer(val);
            else
                return null;
        }

    }
}
