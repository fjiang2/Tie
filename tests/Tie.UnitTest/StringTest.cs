using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Tie;

namespace UnitTest
{
    class StringTest
    {
        public static void main()
        {
            string code = "\"ABC\"";
            VAL x = Script.Evaluate(code);
            Debug.Assert(x.Str == "ABC");

            try
            {
                code = "\"AB\\C\"";
                x = Script.Evaluate(code);
            }
            catch (TieException ex)
            {
                Debug.Assert(ex.ErrorCode == 1063);
            }
        }
    }
}
