﻿//--------------------------------------------------------------------------------------------------//
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
    ///  Allows an object to control its own valization and devalization.
    /// </summary>
    public interface IValizable
    {
        /// <summary>
        /// Populates data
        ///     needed to valize the target object.
        /// </summary>
        /// <returns></returns>
        VAL GetVAL();
        
        /// <summary>
        /// update target object with valized value
        /// </summary>
        /// <param name="val"></param>
        void SetVAL(VAL val);
    }


}