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
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Tie.Helper
{
    public static class Extension
    {
        public static VAL ToVAL(this object obj)
        {
            Contract.Ensures(obj != null);

            Type type = obj.GetType();

            VAL val = VAL.Array();

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                val.Add(propertyInfo.Name, propertyInfo.GetValue(obj));
            }

            return val;
        }
    }

}
