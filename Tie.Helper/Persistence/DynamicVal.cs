using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Tie.Helper
{
    public class DynamicVal : DynamicObject
    {

        private readonly VAL val;

        public DynamicVal(VAL val)
        {
            this.val = val;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (val.Undefined)
            {
                val.UpdateObject(VALTYPE.nullcon, null);
                VAL a = new VAL();
                a.UpdateObject(VALTYPE.voidcon, null);
                val[binder.Name] = a;
                result = new DynamicVal(a);
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
                result = new DynamicVal(element);
            }

            return true;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (val.Undefined)
                val.UpdateObject(VALTYPE.nullcon, null);

            if (value is DynamicVal)
            {
                val[binder.Name] = ((DynamicVal)value).val;
            }
            else
            {
                val[binder.Name] = VAL.NewHostType(value);
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DynamicVal))
                return false;

            DynamicVal v1 = obj as DynamicVal;

            if (v1.val.Undefined && this.val.Undefined)
                return true;

            return v1.val.Equals(this.val);
        }

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }

        public static bool operator ==(DynamicVal val1, DynamicVal val2)
        {
            return val1.Equals(val2);
        }

        public static bool operator !=(DynamicVal val1, DynamicVal val2)
        {
            return !(val1 == val2);
        }

        public override string ToString()
        {
            return this.val.ToString();
        }
    }
}
