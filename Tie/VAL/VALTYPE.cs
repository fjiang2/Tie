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
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using System.Reflection;

namespace Tie
{
    /// <summary>
    /// represent VAL type
    /// </summary>
    public enum VALTYPE
    {
        /// <summary>
        /// value: void 
        /// </summary>
        voidcon = 0,

        /// <summary>
        /// value: null
        /// </summary>
        nullcon = 1,

        /// <summary>
        /// boolean: true or false
        /// </summary>
        boolcon = 2,

        /// <summary>
        /// value: integer, short, byte
        /// </summary>
        intcon = 3,

        /// <summary>
        /// value: double, float, single
        /// </summary>
        doublecon = 4,

        /// <summary>
        /// value: string, support UNICODE
        /// </summary>
        stringcon = 5,

        /// <summary>
        /// value: decimal, long
        /// </summary>
        decimalcon = 6,

        /// <summary>
        /// value: list, associative array
        /// </summary>
        listcon = 7,

        /// <summary>
        /// value: function
        /// </summary>
        funccon = 8,

        /// <summary>
        /// value: class
        /// </summary>
        classcon = 9,


        /// <summary>
        /// value: .net object
        /// </summary>
        hostcon = 20,

        /// <summary>
        /// value: script code
        /// </summary>
        scriptcon = 21,

        /// <summary>
        /// value for enum
        /// </summary>
        enumcon = 22,

        /// <summary>
        /// value: address in the memory
        /// </summary>
        addrcon = 30,

        /// <summary>
        /// value: offset of structure
        /// </summary>
        identcon = 31,

    }
    

}
