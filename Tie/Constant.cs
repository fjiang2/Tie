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
    /// <summary>
    /// TIE configuration parameters
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Default maximum length of string variable
        /// </summary>
        public static int MAX_STRING_SIZE = 16000;           // size of string-table 

        /// <summary>
        /// Default maximum #columns of source code
        /// </summary>
        public static int MAX_SRC_COL = 512;                 // max SOURCE CODE width

        /// <summary>
        /// Default maximum #lines of source code
        /// </summary>
        public static int MAX_SRC_LINE = 1024;               // max SOURCE CODE width

        /// <summary>
        /// Default maximum code segment size
        /// </summary>
        public static int MAX_INSTRUCTION_NUM = 1024 * 16;   // max CODE segment



        /// <summary>
        /// Maximum symbol table size, used by compiler
        /// </summary>
        public static int MAX_SYMBOL_TABLE_SIZE = 2024;		// max size of symbol table


        //--------------------- VM --------------------------------------------------------------------

        /// <summary>
        /// Maximum stack size in virtual machine, used by local variable, recursive function calls and etc.
        /// </summary>
        public static int MAX_STACK = 1024 * 16;			    // max STACK segment

        /// <summary>
        /// Maximum Extra size in virtual machine, used Tie class
        /// </summary>
        public static int MAX_EXTRA = 1024;        	        // max Extra segment

        /// <summary>
        /// Maximum #nested exceptions in try...catch...
        /// </summary>
        public static byte MAX_EXCEPTION = 16;       	        // max nested Exception

        /// <summary>
        /// Maximum #registers in CPU
        /// </summary>
        public static int MAX_CPU_REG_NUM = 256;		        // max register number of CPU

        /// <summary>
        /// if true, VM searches unregistered type automatically which may reduce performance
        /// </summary>
        public static bool HOST_TYPE_AUTO_REGISTER = false;


    }
}
