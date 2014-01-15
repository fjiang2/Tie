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

namespace Tie
{
    /// <summary>
    /// delegate for valizer
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public delegate VAL Valizer<T>(T host);

    /// <summary>
    /// delegate fro devalizer
    /// </summary>
    /// <param name="host"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public delegate T Devalizer<T>(T host, VAL val);
    

    /// <summary>
    /// interface of valizer and devalizer
    /// </summary>
    public interface IValizer<T>
    {
        /// <summary>
        /// prototype of valizer
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        VAL Valizer(T host);


        /// <summary>
        /// prototype of devalizer
        /// </summary>
        /// <param name="val"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        T Devalizer(T host, VAL val);
    }

}
