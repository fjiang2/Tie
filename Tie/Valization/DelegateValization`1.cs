using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Valization
{
    class DelegateValization<T> : BaseValization
    {
        private Valizer<T> valizer;
        private PartialDevalizer<T> devalizer1;
        private Devalizer<T> devalizer2;

        public DelegateValization(Valizer<T> valizer, PartialDevalizer<T> devalizer)
        {
            this.valizer = valizer;
            this.devalizer1 = devalizer;
        }

        public DelegateValization(Valizer<T> valizer, Devalizer<T> devalizer)
        {
            this.valizer = valizer;
            this.devalizer2 = devalizer;
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

        protected override object devalize(object host, VAL val)
        {
            if (devalizer1 != null)
            {
                if(host==null)
                    return devalizer1(default(T), val);
                else
                    return devalizer1((T)host, val);
            }

            if (devalizer2 != null)
            {
                return devalizer2(val);
            }

            return null;
        }

    }
}
