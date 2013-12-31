using System;
using System.Collections.Generic;
using System.Text;

namespace Tie
{
    class HostGenericType
    {
        Type type;

        public HostGenericType(Type type)
        {
            this.type = type;
        }

      

        public static string FullName(Type type)
        {
            if (type.IsGenericType)
            {
                StringBuilder builder = new StringBuilder();

                string name = type.GetGenericTypeDefinition().FullName;
                int index = name.IndexOf('`');
                
                string x = name.Substring(0, index);
                int count = int.Parse(name.Substring(index+1));

                builder.Append(x);
                builder.Append("<");

                Type[] types = type.GetGenericArguments();
                if (types.Length != 0)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        builder.Append(types[i].FullName);
                        if (i < types.Length - 1)
                            builder.Append(",");
                    }
                }
                else
                {
                    for (int i = 0; i < count - 1; i++)
                        builder.Append(",");
                }
                builder.Append(">");
                return builder.ToString();
            }
            else
                return type.FullName;
        }
    }
}
