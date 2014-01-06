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
            VAL val = valizer((T)host);
            return val;
        }

        protected override object devalize(VAL val)
        {
            if (devalizer != null)
                return devalizer(val);

            return null;
        }

    }
}
