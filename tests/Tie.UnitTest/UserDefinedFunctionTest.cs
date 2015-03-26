using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class UserDefinedFunctionTest
    {
        public static void main()
        {
            Script script = new Script("unknown", 500);
         

            Script.FunctionChain.Add("sum", delegate(VAL args, Memory DS)
            {
                int sum = 0;
                for (int i = 0; i < args.Size; i++)
                {
                    sum += args[i].Intcon;
                }

                return new VAL(sum);
            });

            VAL S = Script.Evaluate("sum(1,2,3,4,5)");

            System.Diagnostics.Debug.Assert(S.Intcon == 15, ".NET implemented function test");
        }
    }
}
