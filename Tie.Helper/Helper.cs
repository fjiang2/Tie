using System;
using System.Collections.Generic;
using System.Text;

namespace Tie.Helper
{
    public static class Helper
    {
        public static void Start()
        {
            Script.FunctionChain.Add(PrimitiveType.functions);
            Valization.Register();
        }
    }
}
