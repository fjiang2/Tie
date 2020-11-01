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
using System.IO;

namespace Tie
{


    class Token
    {

        public SYMBOL sy;
        public Sym sym;
        public SYMBOL2 opr;


        public Token()
        {
            sym = new Sym();

        }

        public Token(SYMBOL sy, SYMBOL2 opr)
            : this()
        {
            this.sy = sy;
            this.opr = opr;
        }

        private String encode(OutputType ot, out tokty ty)
        {
            bool quotationMark = (ot & OutputType.QuotationMark) == OutputType.QuotationMark;
            bool nullMark = (ot & OutputType.NullMark) == OutputType.NullMark;
            bool persistent = (ot & OutputType.Valization) == OutputType.Valization;
            bool hostMark = (ot & OutputType.Host) == OutputType.Host;

            ty = tokty.symbol;

            //search keyword
            for (int i = 0; i < Constant.NKW; i++)
            {
                if (sy == JLex.Key[i].ksy)
                {
                    ty = tokty.keyword;
                    return JLex.Key[i].key;
                }
            }

            StringWriter o = new StringWriter();
            switch (sy)
            {
                case SYMBOL.intcon:
                    ty = tokty.number;
                    o.Write(sym.inum); 
                    break;
                
                case SYMBOL.floatcon:
                    ty = tokty.number;
                    o.Write(sym.fnum);
                    if (Math.Ceiling(sym.fnum) == sym.fnum)
                            o.Write(".0");
                    break;

                case SYMBOL.stringcon:
                    ty = tokty.stringcon;
                    if (quotationMark)
                        o.Write("\"{0}\"", sym.stab); 
                    else
                        o.Write("{0}", sym.stab); 
                    break;

                case SYMBOL.identsy:
                    ty = tokty.identsy;
                    if (persistent)
                        o.Write("${0}", sym.id);
                    else
                        o.Write("{0}", sym.id);
                    break;

                //---------------------------------------------------------------------
                case SYMBOL.PLUS: o.Write('+'); break;
                case SYMBOL.MINUS: o.Write('-'); break;
                case SYMBOL.STAR: o.Write('*'); break;
                case SYMBOL.DIV: o.Write('/'); break;
                case SYMBOL.MOD: o.Write('%'); break;

                case SYMBOL.INCOP: switch (opr)
                    {
                        case SYMBOL2.PPLUS: o.Write("++"); break;
                        case SYMBOL2.MMINUS: o.Write("--"); break;
                    }
                    break;

                case SYMBOL.ASSIGNOP: switch (opr)
                    {
                        case SYMBOL2.ePLUS: o.Write("+="); break;
                        case SYMBOL2.eMINUS: o.Write("-="); break;
                        case SYMBOL2.eSTAR: o.Write("*="); break;
                        case SYMBOL2.eDIV: o.Write("/="); break;
                        case SYMBOL2.eMOD: o.Write("%="); break;
                        case SYMBOL2.eAND: o.Write("&="); break;
                        case SYMBOL2.eOR: o.Write("|="); break;
                        case SYMBOL2.eXOR: o.Write("^="); break;
                        case SYMBOL2.eSHL: o.Write("<<="); break;
                        case SYMBOL2.eSHR: o.Write(">>="); break;
                    }
                    break;
                case SYMBOL.EQUOP: switch (opr)
                    {
                        case SYMBOL2.EQL: o.Write("=="); break;
                        case SYMBOL2.NEQ: o.Write("!="); break;
                    }
                    break;
                case SYMBOL.RELOP: switch (opr)
                    {
                        case SYMBOL2.GTR: o.Write(">"); break;
                        case SYMBOL2.GEQ: o.Write(">="); break;
                        case SYMBOL2.LSS: o.Write("<"); break;
                        case SYMBOL2.LEQ: o.Write("<="); break;
                    }
                    break;

                case SYMBOL.SHIFTOP: switch (opr)
                    {
                        case SYMBOL2.SHL: o.Write("<<"); break;
                        case SYMBOL2.SHR: o.Write(">>"); break;
                    }
                    break;

                case SYMBOL.EQUAL: o.Write('='); break;

                case SYMBOL.LP: o.Write('('); break;
                case SYMBOL.RP: o.Write(')'); break;
                case SYMBOL.LB: o.Write('['); break;
                case SYMBOL.RB: o.Write(']'); break;
                case SYMBOL.LC: o.Write('{'); break;
                case SYMBOL.RC: o.Write('}'); break;

                case SYMBOL.ANDAND: o.Write("&&"); break;
                case SYMBOL.OROR: o.Write("||"); break;
                case SYMBOL.AND: o.Write("&"); break;
                case SYMBOL.OR: o.Write("|"); break;
                case SYMBOL.XOR: o.Write("^"); break;

                case SYMBOL.UNOP: switch (opr)
                    {
                        case SYMBOL2.BNOT: o.Write('~'); break;
                        case SYMBOL2.NOT: o.Write("!"); break;
                        case SYMBOL2.NEG: o.Write("-"); break;
                    }
                    break;

                case SYMBOL.STRUCTOP: switch (opr)
                    {
                        case SYMBOL2.DOT: o.Write('.'); break;
                        case SYMBOL2.ARROW: o.Write("->"); break;
                    }
                    break;

                case SYMBOL.QUEST: o.Write('?'); break;
                case SYMBOL.COLON: o.Write(':'); break;
                case SYMBOL.COMMA: o.Write(','); break;
                case SYMBOL.SEMI: o.WriteLine(';'); break;

                case SYMBOL.DELIMITER: o.Write('\\'); break;

                default: o.Write("undefined symbol:{0} {1}", sy, sym.id); break;
            }

            return o.ToString();
        }

        public token ToToken()
        {
            tokty ty;
            string tok = encode(OutputType.Nothing, out ty);
            return new token(tok, ty);

        }

        public override String ToString()
        {
            tokty ty;
            return encode(OutputType.QuotationMark | OutputType.Parentheses | OutputType.NullMark, out ty);
        }

    }



    
}
