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

namespace Tie
{
    /***************
     * 
     *  让Symbol table支持function/class 定义是一个表达式 
     *  
     *  2010.10.2
     * 
     ***/

    class Symbol		// description of variable and function
    {

        public readonly string ident;
        public readonly int addr;

        public readonly int funcLevel;
        public readonly int varLevel;
        public readonly bool isFunc;
        public bool duplicated;

        public Symbol(string ident, int addr, int funcLevel, int varLevel, bool isFunc)
        {
            this.ident = ident;
            this.addr = addr;

            this.funcLevel = funcLevel;
            this.varLevel = varLevel;

            this.isFunc = isFunc;
            this.duplicated = false;
        }


        public override String ToString()
        {
            return string.Format("{0}:{1}/{2}{3}[{4}]", 
                ident, 
                funcLevel, 
                varLevel,
                duplicated ? "+" : "", 
                addr);

        }


    }

}
