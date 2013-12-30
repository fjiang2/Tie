﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Serialization
{
    class DelegateSerialization<T> : BaseSerialization
    {
        private Valizer<T> valizer;
        private Devalizer<T> devalizer;

        public DelegateSerialization(Valizer<T> valizer, Devalizer<T> devalizer)
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
