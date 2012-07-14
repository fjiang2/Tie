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
using Tie;


namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {

            HostType.Register(new Type[]
            {
                typeof(DateTime), 
                typeof(string),
                typeof(System.Reflection.Assembly),
                typeof(Tie.HostType)
            }, true);

            HostType.AddReference("UnitTest", typeof(Program).Assembly);
            //HostType.AddReference("System.Data", typeof(System.Data.DataTable).Assembly);
            //HostType.AddReference("System.Windows.Forms", typeof(System.Windows.Forms.BorderStyle).Assembly);
            HostType.AddReference("System.Drawing", typeof(System.Drawing.Color).Assembly);
      

            HostTypeHelper.Start();

            OperatorTest.main();
            LambdaExpressionTest.main();
            GenericTest.main();

            DelegateTest.main();
            MultidimensionalArrayTest.main();

            PropertyTest.main();
            FunctionTest.main(); 
            ArrayTest.main();
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
        }
    }
}
