using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Valization
{
    class PropertyValization : BaseValization
    {
        private string[] members;

        public PropertyValization(string[] members)
        {
            this.members = members;
        }

        protected override VAL valize(object host)
        {
            VAL val;
            string script = "";
            for (int i = 0; i < members.Length; i++)
            {
                if (i != 0)
                    script += ",";
                script += string.Format("{0} : this.{0}", members[i]);
            }

            script = "{" + script + "}";

            val = Script.Run(host, script, new Memory());

            return val;

        }


        protected override object devalize(object host, VAL val)
        {
            Type type = host.GetType();

            for (int i = 0; i < members.Length; i++)
            {
                string propertyName = members[i];
                VAL x = val[propertyName];

                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo == null)
                    continue;

                object propertyValue = null;
                if (propertyInfo.CanRead)
                {
                    propertyValue = propertyInfo.GetValue(host, null);
                }

                if (propertyInfo.CanWrite)
                {
                    object obj = HostValization.Val2Host(x, propertyValue, propertyInfo.PropertyType);

                    propertyInfo.SetValue(host, obj, null);
                }
            }

            return host;
        }
    }
}
