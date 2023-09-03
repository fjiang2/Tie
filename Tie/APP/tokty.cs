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
    /// Token Type used on Tokenizer
    /// </summary>
    public enum tokty
    { 
        /// <summary>
        /// number is int, double, float,...
        /// </summary>
        number,

        /// <summary>
        /// like variable name
        /// </summary>
        identsy,

        /// <summary>
        /// string constant with double quotes
        /// </summary>
        stringcon,

        /// <summary>
        /// string with single quotes
        /// </summary>
        charcon,

        /// <summary>
        /// symbol like: +,-,++,>=
        /// </summary>
        symbol,

        /// <summary>
        /// reserved keywords in c/c++
        /// </summary>
        keyword
    }
    
}
