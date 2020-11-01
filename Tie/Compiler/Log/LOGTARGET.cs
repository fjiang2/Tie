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
    /// Log target device
    /// </summary>
    public enum LOGTARGET
    {
        /// <summary>
        /// null device
        /// </summary>
        NullWriter,

        /// <summary>
        /// default console
        /// </summary>
        Console,

        /// <summary>
        /// memory string writer
        /// </summary>
        StringWriter,

        /// <summary>
        /// file device
        /// </summary>
        File
    }
}
