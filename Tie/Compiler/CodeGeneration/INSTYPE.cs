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
    enum INSTYPE
    {
        NEG, ADD, SUB, MUL, DIV, MOD,
        INC, DEC,

        EQL, NEQ, LSS, LEQ, GTR, GEQ,

        NOTNOT, ANDAND, OROR, NOT, AND, OR, XOR,
        EACH,           // foreach(a in A)

        SHR, SHL,       // >> , <<

        JMP, JNZ, JZ, LJMP, LJZ,
        CAS,            // case of switch 

        PUSH, POP, SP,  // SS 

        RMT,
        RCP,            // remove CPU top register, register copy
        RPOP,           // REGO = REG.Pop()
        //RPSH,         // RPSH = REG.Push()

        ESI, ESO,       // EX PUSH/POP

        MOV, STO, STO1, // LOAD


        CALL, RET,      // call function
        MARK, END,      // List, Parameter,
        OFS, ARR,       // struct. array,

        HALT, NOP,
        THIS, BASE, NS, // this, base class, namespace, module
        ADR, VLU,       // &var 返回变量的地址, *VL, 返回地址的值 

        PROC, ENDP,	    // function
        DIRC,           // directive
        DDT,	        // debug
        GNRC,           // generic

        //class	
        NEW,
        CLSS,   // class
        PBLC,   // public
        PRVT,   // private
        PRTC,   // protected
        ENDC,	// end of class


        THRW   // throw

    };



}
