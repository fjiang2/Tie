//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Tie;
using Tie.Helper;
//using Microsoft.Maps.MapControl.WPF;

namespace UnitTest
{
    class Program
    {
        /*
        static void Main1(string[] args)
        {

            Size size = new Size(100, 200);
            VAL x = VAL.Boxing(size);
            string sx1 = x.Valor;
            
            HostType.Register(typeof(AltitudeReference));

            Location p905 = new Location(29.620931484730015, -95.629716997105);
            VAL val = VAL.Boxing(p905);
            string s = val.ToString();
            string s1 = val.Valor;
            string json = val.ToJson();

            VAL v2 = Script.Evaluate(s1);
            object obj = v2.HostValue;


            Memory memory = new Memory();
            string keyName = "XXX";
            Script.Execute(string.Format("{0}={1};", keyName, VAL.Boxing(p905).Valor), memory);
            VAL v = Script.Evaluate(keyName, memory);
        }

         * */


        static void Main(string[] args)
        {
            DataSetTest.main();

            HostType.Register(new Type[]
            {
                typeof(DateTime), 
                typeof(string),
                typeof(System.Reflection.Assembly),
                typeof(Tie.HostType)
            }, true);

            HostType.AddReference(typeof(Program).Assembly);
            HostType.AddReference(typeof(System.Drawing.Color).Assembly);
      

            Helper.Start();
            ValizationExamples.RegisterDemo();

            MemoryTest.main();
            OperatorTest.main();
            LambdaExpressionTest.main();
            GenericTest.main();

            DelegateTest.main();
            MultidimensionalArrayTest.main();

            PropertyTest.main();
            FunctionTest.main(); 
            CastTest.main();
            HostPersistentTest.main();
            HostPersistentTest.main2();
 
            BasicTest.main();
            StatementTest.main();
            ClassTest.main();
            VariableOperatorTest.main();
#if HOME
            HostObjectTest.main();
            EventTest.eventTest();
#endif
            HostObjectSearialization.main();
            VALTest.main();
            Test.main();
            XMLTest.main();
            MathTest.main();
            JSONTest.main();
            SubclassTest.main();
            ExtendMethodTest.main();
            new FaultToleranceTest();
            TryCatchTest.main();
            ParamsTest.main();
            UserDefinedFunctionTest.main();
            ApplicationMemoryTest.main();
            ValizationTest.main();
        }
    }
}
