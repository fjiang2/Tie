using System;
using System.Collections.Generic;
using System.Text;

namespace Tie
{
    public partial class HostType
    {
        Type type;

        public HostType(Type type)
        {
            this.type = type;
        }

        public HostType(object obj)
        {
            if (obj is Type)
                this.type = (Type)obj;
            else
                this.type = obj.GetType();
        }

        public Type Type 
        { 
            get { return this.type; } 
        }

        public string[] Namespace
        {
            get
            {
                return type.Namespace.Split(new char[] { '.' });
            }
        }

        public string FullName
        {
            get
            {
                return type.Namespace + "." + type.Name;
            }
        }

        public string TypeName
        {
            get
            {
                if (type.IsGenericType)
                {
                    StringBuilder builder = new StringBuilder();

                    string name = type.GetGenericTypeDefinition().FullName;
                    int index = name.IndexOf('`');

                    string x = name.Substring(0, index);
                    int count = int.Parse(name.Substring(index + 1));

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

        public override string ToString()
        {
            return TypeName;
        }
    }
}
