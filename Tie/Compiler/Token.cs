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
        /// string constant
        /// </summary>
        stringcon,

        /// <summary>
        /// symbol like: +,-,++,>=
        /// </summary>
        symbol,

        /// <summary>
        /// reserved keywords in c/c++
        /// </summary>
        keyword
    }

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

        internal token(string tok, tokty ty)
        {
            this.ty = ty;
            this.tok = tok;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", ty, tok);
        }
    }



// a Token
    enum SYMBOL 
    { 
	intcon,floatcon,boolcon,stringcon,identsy,		// constance number 31,3.14,'c',"STRING"
	nullsy,truesy,falsesy,

	PLUS,MINUS,STAR,DIV,MOD,						// + - * / %
	ANDAND,OROR,								// !.	&&,	||
	AND,OR,XOR,								// bit logic

	RELOP,EQUOP,ASSIGNOP,INCOP,SHIFTOP,STRUCTOP,UNOP,

	LP,RP,LB,RB,LC,RC,						// (	)	[	]	{	}
	COMMA, SEMI, QUEST,COLON, 

	VAR,FUNC,CLASS, METHOD,		// =	::
	EQUAL, VOID,

	IF,ELSE,SIZEOF,
	SWITCH,CASE,DEFAULT,					// switch
	DO,WHILE,FOR,FOREACH,					// for
	BREAK,CONTINUE,GOTO,					// 

	RETURN,

    TRY,CATCH,THROW,FINALLY,

	NEW,THIS,BASE,NAMESPACE, DIRECTIVE, DELIMITER,
	WITH,IN,IS,AS, DEBUG,		

	GOESTO, //  =>

//---------------------------------------------------------------	
	PUBLIC,  PRIVATE, PROTECTED,STATIC,

	localsy, staticsy,friendsy,
	atsy,constsy,typesy,charcon,deletesy,
    NOP     //最后一个token
	
 }

    enum SYMBOL2
	{
	    EQL,NEQ,
	    GTR,GEQ,LSS,LEQ,
        ADR, VLU,                    // &var, *adr
	
	    ePLUS,eMINUS,eSTAR,eDIV,eMOD,		// +=
	    eSHR,eSHL,
	    eAND,eOR,eXOR,

	    NOT,BNOT,NEG,
	
        PPLUS,MMINUS,									// ++, --
	    SHL,SHR,										// <<,	>>
	    DOT,ARROW
    }								// .	->

    class Sym
    {
        public double fnum;				// real number from insymbol 
        public int inum;				// integer from insymbol 
        public string id;

        public int len;			        // string length 
        public string stab;		        // string table


        public Sym()
        {
            
        }
    };


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
