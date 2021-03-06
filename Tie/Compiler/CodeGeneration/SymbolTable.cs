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
using System.IO;

namespace Tie
{

    class SymbolTable
    {
        struct Stamp
        {
            public int varLevel;       // local variable nest
            public int varNum;         // local variable offset [BP+n]

            public Stamp(int varLevel, int varNum)
            {
                this.varLevel = varLevel ;
                this.varNum = varNum;
            }



        }

        private Symbol[] symTab;
        private Stack<Stamp> stack;
        private int SP;

        private int funcLevel;				// local function nest
        private Stamp current;


        private Error error;

        public SymbolTable(int size, Error error)
        {
            this.error = error;

            symTab = new Symbol[size];
            stack = new Stack<Stamp>();

            SP = -1;
            funcLevel = -1;

            current = new Stamp(-1, 0);
        }

        public void NewFunction()
        {
            funcLevel++;
            
            stack.Push(current);
            current = new Stamp(-1, 0);
        }

        public void BackFunction()
        {
            funcLevel--;
            current = stack.Pop();
        }

        public void NewLevel()
        {
            current.varLevel++;
        }

        public void BackLevel()
        {
            if (SP != -1)
            {
                Symbol sym = symTab[SP];
                while (sym.funcLevel == funcLevel && sym.varLevel == current.varLevel)
                {
                    if (sym.addr > 0 && !sym.isFunc)
                        current.varNum--;	//consider local variable except parameter

                    SP--;
                    
                    if (SP == -1)
                        break;

                    sym = symTab[SP];
                }
            }

            current.varLevel--;
        }




        public void AddParameter(string id, int addr)		// call function parameter
        {
            Add(id, current.varLevel, addr, false);
        }

        public void AddFunc(string id, int addr)		// ::func(..) 这个函数可能有毛病,是用来支持传统的C语言风格的函数
        {
            Add(id, 0, addr, true);
        }

        //分配count局部变量空间
        public int AddLocal(int count)
        {
            int addr = current.varNum + 1;
            current.varNum += count;
            return addr;
        }

        public int AddLocal(string id)
        {
            int addr = current.varNum + 1;

            if (!Add(id, current.varLevel, addr, false))    //09/29/2010 从 varNum 改变为 varNum+1, 因为函数局部变量从[BP+1]开始,不从[BP+0]开始,防止覆盖BP的值
                current.varNum++;                           // used to local variable

            return addr;
        }

        private bool Add(string id, int lev, int addr, bool isfunc)
        {
            bool dup = true;
            Symbol sym = new Symbol(id, addr, funcLevel, lev, isfunc);

            if (!IsExisted(id))
            {
                if (SP >= Constant.MAX_SYMBOL_TABLE_SIZE)
                    throw error.CompilingException("Symbol Table overflow."); 
                
                symTab[++SP] = sym;

                dup = false;
            }

            return dup;
        }

        private bool IsExisted(string id)
        {

            for (int i = SP; i > -1; i--)
            {
                Symbol sym = symTab[i];

                if (sym.funcLevel == funcLevel)
                {
                    if (sym.varLevel == current.varLevel && id == sym.ident)
                    {
                        sym.duplicated = true;
                        error.OnWarning(2);
                        return true;
                    }
                }
                else
                    return false;
            }

            return false;
        }

        public int FuncAddr(string key)
        {
            Symbol sym = LookAt(key);

            if (sym != null && sym.isFunc)
                return sym.addr;

            return -1;
        }

        public int VarAddr(string key)
        {
            Symbol sym = LookAt(key);

            if (sym != null && !sym.isFunc)
                return sym.addr;

            return -1;
        }


        private Symbol LookAt(string key)
        {
            for (int i = SP; i > -1; i--)
            {
                Symbol sym = symTab[i];
                
                if(sym.funcLevel != funcLevel)
                    return null;

                if(sym.ident == key)            //search at varlevel
                    return sym;
            }
            
            return null;
        }





        public override string ToString()
        {
            StringWriter o = new StringWriter();
            if (SP != -1)
            {
                for (int i = 0; i < SP; i++)
                    o.Write("{0},", symTab[i]);

                o.Write("{0}", symTab[SP]);
            }
            else
                o.Write("[EMPTY]");

            return o.ToString();
        }


    }

}
