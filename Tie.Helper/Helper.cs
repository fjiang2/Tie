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


    
    }
}
