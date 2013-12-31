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
    public sealed class VAR : IComparable, IComparable<string>, IEquatable<VAR>
    {
        private string ident;

        public VAR(string ident)
        {
            this.ident = ident;

            //if (!ValidIdent(id))
            //    throw new TieException("Invalid ident: {0}", id);
        }

        internal string Ident
        {
            get { return this.ident; }
        }


        public override int GetHashCode()
        {
            return ident.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ident.Equals(((VAR)obj).ident);
        }

        public bool Equals(VAR obj)
        {
            return ident.Equals(obj.ident);
        }

        public static bool operator ==(VAR id1, VAR id2)
        {
            return id1.ident.Equals(id2.ident);
        }

        public static bool operator !=(VAR id1, VAR id2)
        {
            return !(id1 == id2);
        }

        public static explicit operator string(VAR ident)
        {
            return ident.ident;
        }

        public static implicit operator VAR(string str)
        {
            return new VAR(str);
        }

        public int CompareTo(object obj)
        {
            return this.ident.CompareTo(obj);
        }

        public int CompareTo(string other)
        {
            return this.ident.CompareTo(other);
        }


        public override string ToString()
        {
            return this.ident;
        }


        internal static bool ValidIdent(string id)
        {
            int i = 0;
            char ch = id[i++];

            if (!char.IsLetter(ch) && ch != '_')
                return false;

            while (i < id.Length)
            {
                ch = id[i++];

                if (ch != '_' && !char.IsLetterOrDigit(ch))
                    return false;
            }

            return true;
        }


        internal static string ToIdent(string s)
        {
            s = s.Trim();
            StringBuilder sb = new StringBuilder();

            if (!char.IsLetter(s[0]) && s[0] != '_')
                sb.Append("_");

            for (int i = 0; i < s.Length; i++)
                if (char.IsLetterOrDigit(s[i]) || s[i] == '_')
                    sb.Append(s[i]);
                else
                    sb.Append('_');

            return sb.ToString();
        }
    }
}