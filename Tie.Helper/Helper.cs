using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Tie.Helper
{
    public static class Helper
    {
        public static void Start()
        {
            Script.FunctionChain.Add(PrimitiveType.functions);
            Valization.Register();
        }

        public static void RegisterEnumAs<T>()
        {
            if (typeof(T) == typeof(int))
                Valization.RegisterEnumAsInteger();
            else if (typeof(T) == typeof(string))
                Valization.RegisterEnumAsString();
        }
    
    }
}
