using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class DebugTest
    {
        public static void main()
        {
              
            string code6 = @"a=1;
            a=2;
            a=3;
            test.StringArray = {'A','B', 'C'};
            ";
           
            Script script = new Script("unknown", 500);
            Script.DebugHandler h = delegate(int breakpoint, int cursor, string info, Memory DS2)
            {
                VAL a = new VAL();
                a = DS2["a"];
                Console.WriteLine(string.Format("line={0} DEBUG a={1} {2} cur={3}", breakpoint, a, info, cursor)); 
            };

            int line = 1;
            while (line < 10)
                script.DebugContinue(line++, h);
            script.DebugStart(code6);

        
        }
    }
}
