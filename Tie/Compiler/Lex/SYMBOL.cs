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



    
}
