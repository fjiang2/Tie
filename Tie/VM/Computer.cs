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
using System.IO;

namespace Tie
{
    /// <summary>
    /// Compiler and Virtual Machine
    /// CPU has 3-level predefined variable,
    ///     0: System Level
    ///     1: User global variable for all users
    ///     2: User temp variable in user's space
    /// </summary>
    static class Computer
    {

        public static readonly Memory DS1 = new Memory();
        public static readonly Memory DS2 = new Memory();

        static Computer()
        {
            register("object", typeof(object));
            register("bool", typeof(bool));

            register("sbyte", typeof(sbyte));
            register("byte", typeof(byte));
            register("short",typeof(short));
            register("ushort", typeof(ushort));
            register("int", typeof(int));
            register("uint", typeof(uint));
            register("long", typeof(long));
            register("ulong",typeof(ulong));

            register("double", typeof(double));
            register("float", typeof(float));
            register("decimal", typeof(decimal));

            register("char", typeof(char));
            register("string", typeof(string));

       

            VALL L = new VALL();
            L.Add("VOID", new VAL((int)VALTYPE.voidcon));
            L.Add("NULL", new VAL((int)VALTYPE.nullcon));
            L.Add("BOOL", new VAL((int)VALTYPE.boolcon));
            L.Add("INT", new VAL((int)VALTYPE.intcon));
            L.Add("DOUBLE", new VAL((int)VALTYPE.doublecon));
            L.Add("STRING", new VAL((int)VALTYPE.stringcon));
            L.Add("LIST", new VAL((int)VALTYPE.listcon));
            L.Add("FUNCTION", new VAL((int)VALTYPE.funccon));
            L.Add("CLASS", new VAL((int)VALTYPE.classcon));
            L.Add("HOST", new VAL((int)VALTYPE.hostcon));
            
            DS1.Add("TYPE", new VAL(L));


            //HostType.Register(new Type[]
            //{
            //    typeof(DateTime), 
            //    typeof(string),
            //    typeof(System.Reflection.Assembly),
            //    typeof(Tie.HostType)
            //}, true);

        }

        private static void register(string ty, Type type)
        {
            DS1.Add(ty, VAL.NewHostType(type));
        }
   
        public static VAL Run(CPU cpu, int breakPoint)
        {
#if DEBUG
            //if (!Extension.DesignMode())
            //    throw new RuntimeException("This version is trial only. Please buy TIE licence on http://www.datconn.com"); 

            return cpu.Run(breakPoint);

#else
            VAL ret;

            try
            {
                ret = cpu.Run(breakPoint);
            }
            catch (Exception e)
            {
                if (e is PositionException)
                    throw e;
                else
                    throw new RuntimeException(cpu.Position, "{0}", e);
            }

            return ret;
#endif

        }

        public static VAL Run(Module module, Context context)
        {
            CPU cpu = new CPU(module, context);
            return Run(cpu, -1);
        }



        public static VAL Run(string scope, string code, CodeType ty, Context context)
        {
            if (code == "")
                return new VAL();

            Module module = new Module();
            Library.AddModule(module);
            if (module.CompileCodeBlock(scope, code, ty, CodeMode.Overwritten))
            {
                return Computer.Run(module, context);
            }

            return new VAL();
        }

    }
}
