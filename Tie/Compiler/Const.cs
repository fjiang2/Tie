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

namespace Tie
{
    class Const
    {
        public const int NKW = 39;                        // no. of key words 
        public const int ALNG = 64;                        // no. of significant chars in identifiers 

        public const int EMAX = 322;                       // max exponent of real numbers 
        public const int EMIN = -292;                      // min exponent 

        public const int KMAX = 15;                        // max no. of significant digits 
        public const int NMAX = int.MaxValue;              // 2^32-1 

        public const byte MAX_CODEBLOCK_NUM = 16;           // max CODE Block#


        public const string VOLATILE_MODULE_NAME = "volatile"; //temp module
        public const string DEFAULT_MODULE_NAME = "unknown"; // default module in Library

        public const string FUNC_MAKE_ARRAY_TYPE = "$makearraytype";
        public const string FUNC_FUNCTION = "$function";
        public const string FUNC_CLASS = "$class";
        public const string FUNC_CAST_VALUE_TYPE = "$castvt";
        public const string FUNC_CAST_TYPE_VALUE = "$casttv";
        public const string FUNC_IS_TYPE = "$istype";

        public const string FUNC_CON_INSTANCE_INVOKE = "FuncconInstanceInvoke";
        public const string SCOPE = "scope";
        public const string THIS = "$THIS";

    }
}
