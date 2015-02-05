using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Tie.Helper
{
    public class Val : DynamicObject
    {
        private VAL val;
        public Val(VAL val)
        {
            this.val = val;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (val.Undefined)
            {
                val.ty = VALTYPE.nullcon;
                VAL a = new VAL();
                a.ty = VALTYPE.voidcon;
                val[binder.Name] = a;
                result = new Val(a);
                return true;
            }

            VAL element = val[binder.Name];
            object hostValue = element.HostValue;

            if (hostValue == null || hostValue is string || hostValue.GetType().IsValueType)
            {
                result = hostValue;
                return true;
            }
            else
            {
                result = new Val(element);
            }

            return true;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (val.Undefined)
                val.ty = VALTYPE.nullcon;

            if (value is Val)
            {
                val[binder.Name] = ((Val)value).val;
            }
            else
            {
                val[binder.Name] = VAL.NewHostType(value);
            }

            return true;
        }

    }
}
