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
using System.IO;

namespace Tie
{
    class FileLex : JLex
    {
        private StreamReader fi;        //source file
        private char[] buffer;

        public FileLex(string sourceFileName, Error error)
            : base(error)
        {
            buffer = new char[2];
            try
            {
                fi = File.OpenText(sourceFileName);
            }
            catch (Exception)
            {
                error.OnError(55);
            }
            NextCh();
        }

        public override void Close()
        {
            fi.Close();
        }

        protected override char NextCh()
        {
            if (fi.EndOfStream)
                return ch = (char)0;

            fi.Read(buffer, 0, 1);
            ch = buffer[0];
            base.NextCh();
            return ch;
        }

        protected override void set_index(int index)
        {
        }

        public override int Index()
        {
            return -1;
        }
    }





}
