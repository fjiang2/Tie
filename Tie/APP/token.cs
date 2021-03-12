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
    /// define a token 
    /// </summary>
    public struct token
    {
        /// <summary>
        /// token type
        /// </summary>
        public readonly tokty ty;

        /// <summary>
        /// token itself
        /// </summary>
        public readonly string tok;

        /// <summary>
        /// code line number
        /// </summary>
        public readonly int line;

        internal token(string tok, tokty ty, Position pos)
        {
            this.ty = ty;
            this.tok = tok;
            this.line = pos.line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} : {1}", ty, tok);
        }
    }

}
