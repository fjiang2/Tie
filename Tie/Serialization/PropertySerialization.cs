using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Serialization
{
    class PropertySerialization : BaseSerialization
    {
        private string[] valizer;
       // private object devalizer;


        public PropertySerialization(string[] valizer)
        {
            this.valizer = valizer;
        }

        protected override VAL valize(object host)
        {
            VAL val;
            string[] members = (string[])this.valizer;
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


        protected override object devalize(VAL val)
        {
             return null;
        }
    }
}
