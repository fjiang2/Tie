using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Tie
{
    class HostReferences : List<Assembly>
    {
        public HostReferences()
        { 
        }

    }

    class HostImport : Dictionary<string, Type>
    {
        private List<Assembly> list = new List<Assembly>();
        private string ns;

        public HostImport(string ns, HostReferences references)
        {
            this.ns = ns;
            foreach (Assembly assembly in references)
            {
                if(AddReference(assembly))
                    list.Add(assembly);
            }
        }

        public bool AddReference(Assembly reference)
        {
            bool found = false;

            foreach (Type type in reference.GetExportedTypes())
            {
                if (type.Namespace.Equals(ns))
                {
                    found = true;
                    if (!this.ContainsKey(type.Name))
                        this.Add(type.Name, type);
                }
            }

            return found;
        }

        public void RemoveReference(Assembly reference)
        {
            List<string> list = new List<string>();
            foreach(KeyValuePair<string, Type> kvp in this)
            {
                if (kvp.Value.Assembly == reference)
                    list.Add(kvp.Key);
            }

            foreach (string name in list)
                this.Remove(name);
        }

        public Assembly[] Assemblies
        {
            get
            {
                return list.ToArray();
            }
        }

        public string NameSpace
        {
            get { return this.ns; }
        }
    }

  
}
