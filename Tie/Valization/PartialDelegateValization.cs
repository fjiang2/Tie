using System;

namespace Tie.Valization
{
    /// <summary>
    /// typeof(T) may not be equal to hostType. typeof(T) could be base class of hostType
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class PartialDelegateValization<T> : BaseValization
    {
        private Valizer<T> valizer;
        private PartialDevalizer<T> devalizer;
        

        public PartialDelegateValization(Valizer<T> valizer, PartialDevalizer<T> devalizer)
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
            {
                if(host==null)
                    return devalizer(default(T), hostType, val);
                else
                    return devalizer((T)host, hostType, val);
            }

            return null;
        }

    }
}
