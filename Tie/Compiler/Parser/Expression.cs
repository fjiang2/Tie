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

    class Expression
    {
        protected CodeGeneration gen;
        protected SymbolTable vtab;
        protected JLex lex;
        protected Error error;
        
        public Module module;
        
        public const string BASE_INSTANCE = "$base";


        public SymbolTable GetVTab()
        {
            return vtab;
        }

        public Expression(string sourceCode, CodeSource format, Module module)
        {
            error = module.Error;

            if (format == CodeSource.FILE)
                lex = new FileLex(sourceCode, error);
            else
                lex = new StringLex(sourceCode, error);

            this.module = module;

            gen = new CodeGeneration(module);
            vtab = new SymbolTable(Constant.MAX_SYMBOL_TABLE_SIZE, error);
        }

        public Module Module { get { return module; } }

        
        #region s_expr(), s_expr1(), s_exp()
        
        protected bool s_expr()
        {
            for (; ; )
            {
                if (!s_exp())
                    return false;

                if (lex.sy == SYMBOL.COMMA)
                    lex.InSymbol();
                else
                    return true;
            }
        }

        protected bool s_expr1()
        {
            for (; ; )
            {
                if (!s_exp1())
                    return false;

                if (lex.sy == SYMBOL.COMMA)
                    lex.InSymbol();
                else
                    return true;
            }
        }
        protected void TryRemoveRegisterTop()
        {
            if (gen.IV[gen.IP - 1].cmd == INSTYPE.RMT)
                return;

            if (gen.IV[gen.IP - 1].cmd == INSTYPE.STO)
            {
                gen.IV[gen.IP - 1].cmd = INSTYPE.STO1;
            }
            else if (gen.IV[gen.IP - 1].cmd == INSTYPE.INC || gen.IV[gen.IP - 1].cmd == INSTYPE.DEC)             //i++ or i--;
            {
                gen.emit(INSTYPE.RMT);
            }
            else if (
                    (gen.IP >= 2 && gen.IV[gen.IP - 2] != null && gen.IV[gen.IP - 2].cmd == INSTYPE.CALL)         //e.g: foo(12);
                 || (gen.IP >= 3 && gen.IV[gen.IP - 3] != null && gen.IV[gen.IP - 3].cmd == INSTYPE.CALL && gen.IV[gen.IP - 1].cmd == INSTYPE.ESO) //e.g. math.sin(20.0);
                 )
                gen.emit(INSTYPE.RMT); //remove CPU top on account of assign statement

        }

        protected bool s_exp()
        {
            bool r = s_exp1();
            TryRemoveRegisterTop();
            return r;
        }

        #endregion


        #region s_exp1() .... s_exp13()
        
        protected bool s_exp1() { return s_exp2() && s_exp16(); }
        bool s_exp2() { return s_exp3() && s_exp17(); }
        bool s_exp3() { return s_exp4() && s_exp18(); }
        bool s_exp4() { return s_exp5() && s_exp19(); }
        bool s_exp5() { return s_exp6() && s_exp20(); }
        bool s_exp6() { return s_exp7() && s_exp21(); }
        bool s_exp7() { return s_exp8() && s_exp22(); }
        bool s_exp8() { return s_exp9() && s_exp23(); }
        bool s_exp9() { return s_exp10() && s_exp24(); }
        bool s_exp10() { return s_exp11() && s_exp25(); }
        bool s_exp11() { return s_exp12() && s_exp26(); }
        bool s_exp12() { return s_exp13() && s_exp27(); }
        bool s_exp13() { return s_exp14() && s_exp28(); }
        
        #endregion


        #region s_exp14() .... s_exp28()

        bool s_exp14()
        {
            if (s_exp15()) return true;

            if (lex.sy == SYMBOL.UNOP || lex.sy == SYMBOL.PLUS || lex.sy == SYMBOL.MINUS || lex.sy == SYMBOL.AND || lex.sy == SYMBOL.STAR)		// !exp ~exp -exp, &var, *adr
            {
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                s_exp14();
                switch (Opr)
                {
                    case SYMBOL2.BNOT: gen.emit(INSTYPE.NOT); break;
                    case SYMBOL2.NOT: gen.emit(INSTYPE.NOTNOT); break;
                    case SYMBOL2.NEG: gen.emit(INSTYPE.NEG, lex.sy == SYMBOL.PLUS? 1:-1); break;
                    case SYMBOL2.ADR: gen.emit(INSTYPE.ADR); break;
                    case SYMBOL2.VLU: gen.emit(INSTYPE.VLU); break;
                    default: return false;
                }
                return true;
            }

            else if (lex.sy == SYMBOL.INCOP)		//++i
            {
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                if (s_var(false))
                {
                    s_incop(Opr);
                    return true;
                }
                else
                    return false;
            }

            else if (s_var(false))
            {
                if (lex.sy == SYMBOL.INCOP)	//i++
                {
                    switch (lex.opr)
                    {
                        case SYMBOL2.PPLUS: gen.emit(INSTYPE.INC); break;
                        case SYMBOL2.MMINUS: gen.emit(INSTYPE.DEC); break;
                    }
                    lex.InSymbol();
                }

                return true;
            }

            return false;
        }




        bool s_exp15()
        {
            switch (lex.sy)
            {
                case SYMBOL.LP:
                    int index = lex.Index();
                    lex.InSymbol();
                    if (lex.sy == SYMBOL.RP)        //Lambda Expression 零个参数 () => x+20
                    {
                        lex.InSymbol();
                        if (lex.sy == SYMBOL.GOESTO)
                        {
                            ((JParser)this).e_lambda(new string[] { }, 0);
                            return true;
                        }
                        else
                            error.OnError(56);
                    }
                    else if (lex.sy == SYMBOL.identsy)  //Lambda Expression 多个参数
                    {
                        string[] args = new string[32];         //保存arguments变量
                        int argc = 0;
                        while (lex.sy == SYMBOL.identsy)
                        {
                            args[argc++] = lex.sym.id;
                            lex.InSymbol();

                            if (lex.sy == SYMBOL.COMMA)
                                lex.InSymbol();
                            else
                                break;
                        }

                        if (lex.sy == SYMBOL.RP)
                            lex.InSymbol();
                        else
                            goto L1;

                        if (lex.sy == SYMBOL.GOESTO)
                        {
                            ((JParser)this).e_lambda(args, argc);
                            return true;
                        }

                    L1:
                        lex.InSymbol(index); //失败,回溯
                    }

                    s_exp1();

                    expect(SYMBOL.RP);
                    if (       //强制类型转换
                           lex.sy == SYMBOL.identsy || lex.sy == SYMBOL.THIS || lex.sy == SYMBOL.BASE
                        || lex.sy == SYMBOL.LP || lex.sy == SYMBOL.LC || lex.sy == SYMBOL.LB
                        || lex.sy == SYMBOL.nullsy || lex.sy == SYMBOL.intcon
                        || lex.sy == SYMBOL.floatcon || lex.sy == SYMBOL.stringcon
                        || lex.sy == SYMBOL.truesy || lex.sy == SYMBOL.falsesy
                        )
                    {
                        s_exp1();
                        s_call(Constant.FUNC_CAST_TYPE_VALUE, 2);
                    }
                    break;

                case SYMBOL.intcon:
                case SYMBOL.floatcon:
                case SYMBOL.stringcon:
                case SYMBOL.nullsy:
                case SYMBOL.VOID:
                case SYMBOL.truesy:
                case SYMBOL.falsesy: gen.emit(INSTYPE.MOV, new Operand(new Numeric(lex.sy, lex.sym))); lex.InSymbol(); break;

                case SYMBOL.LC: lex.InSymbol(); gen.emit(INSTYPE.MARK); s_expr1(); expect(SYMBOL.RC); gen.emit(INSTYPE.END); break;
                case SYMBOL.LB: lex.InSymbol(); gen.emit(INSTYPE.MARK); s_expr1(); expect(SYMBOL.RB); gen.emit(INSTYPE.END); break;

                case SYMBOL.FUNC: ((JParser)this).e_func(OPRTYPE.funccon); break;
                case SYMBOL.CLASS: ((JParser)this).e_func(OPRTYPE.classcon); break;
                case SYMBOL.VAR: ((JParser)this).e_decl(); break;

                case SYMBOL.NEW:
                    lex.InSymbol();
                    if (lex.sy == SYMBOL.LC) //匿名class, 例如:new {Id=100, Name="Jane"}
                    {
                        gen.emit(INSTYPE.MARK);
                        lex.InSymbol();

                        while (lex.sy == SYMBOL.identsy)
                        {
                            string ident = lex.sym.id;
                            gen.emit(INSTYPE.MARK);
                            Operand x = new Operand(new Numeric(ident));
                            gen.emit(INSTYPE.MOV, x);
                            lex.InSymbol();
                            
                            expect(SYMBOL.EQUAL);

                            s_exp1();
                            gen.emit(INSTYPE.END);

                            if (lex.sy == SYMBOL.COMMA)
                                lex.InSymbol();
                        }

                        expect(SYMBOL.RC);
                        gen.emit(INSTYPE.END);
                    }
                    else if (s_decl_instance())  //如果不是new object()
                    {
                        int operand = 1;
                        if (lex.sy == SYMBOL.LC)    //双操作符
                        {
                            lex.InSymbol(); gen.emit(INSTYPE.MARK); s_expr1(); expect(SYMBOL.RC); gen.emit(INSTYPE.END);
                            operand = 2;
                        }

                        gen.emit(INSTYPE.NEW, operand);
                    }
                    break;

                default:
                    return false;
            }
            return true;
        }




        bool s_exp16()
        {
            bool r = true;
            if (lex.sy == SYMBOL.EQUAL)			// A=1
            {
                Operand var = gen.IV[gen.IP - 1].operand;
                lex.InSymbol();
                r = s_exp1();
                gen.emit(INSTYPE.STO);//,var);
            }
            else if (lex.sy == SYMBOL.ASSIGNOP)	// A+=1;
            {
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                repeatvar();
                r = s_exp1();
                s_assignop(Opr);
            }
   
            return r;
        }

        bool s_exp17()	// a>1?b:c;
        {
        LL1:
            switch (lex.sy)
            {
                case SYMBOL.QUEST:
                    {
                        lex.InSymbol();

                        int L1 = gen.emit(INSTYPE.LJZ);		//JZ L2+1
                        s_exp3();
                        int L2 = gen.emit(INSTYPE.LJMP);	//JMP END

                        expect(SYMBOL.COLON);
                        s_exp3();

                        gen.remit(L1, L2 + 1 - L1);
                        gen.remit(L2, gen.IP - L2);			//gen.remit(L2,END);
                    }
                    break;

                /*
                 * 
                 * 用来支持JSON格式的Associative Array输入 { Width:40, Height:200} 等价于 { {"Width",40}, {"Height",200}}
                 * 
                 * 
                 * */
                case SYMBOL.COLON:
                    if (gen.IP > gen.Size())        //表示在编译switch中的case语句,case语句的代码生成在底部
                        return true;

                    /*
                     * 
                     * 
                     * 用于JSON风格的Dictionary. 
                     *   把 ident:exp 转为为 {"ident", exp}
                     *   把 "str":exp 转为为 {"str", exp}
                     *   
                     * 
                     * **/
                    Operand OPR = gen.IV[gen.IP - 1].operand;
                    switch (OPR.ty)
                    {
                        case OPRTYPE.identcon:  //把ident转化为字符串
                            OPR.ty = OPRTYPE.numcon;
                            OPR.value = new Numeric((string)(OPR.value));
                            break;
                        
                        case OPRTYPE.numcon:          
                            if( ((Numeric)(OPR.value)).ty != NUMTYPE.stringcon)
                                error.OnError(SYMBOL.identsy);
                            break;

                        default:
                            error.OnError(SYMBOL.identsy);
                            break;
                    }
                    gen.IP--;
                    gen.emit(INSTYPE.MARK);
                    gen.emit(INSTYPE.MOV, OPR); 
                    lex.InSymbol(); s_exp3(); 
                    gen.emit(INSTYPE.END);
                    break;

                default: return true;
            }
            goto LL1;
        }

        bool s_exp18()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.OROR: lex.InSymbol(); s_exp4(); gen.emit(INSTYPE.OROR); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp19()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.ANDAND: lex.InSymbol(); s_exp5(); gen.emit(INSTYPE.ANDAND); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp20()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.OR: lex.InSymbol(); s_exp6(); gen.emit(INSTYPE.OR); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp21()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.XOR: lex.InSymbol(); s_exp7(); gen.emit(INSTYPE.XOR); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp22()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.AND: lex.InSymbol(); s_exp8(); gen.emit(INSTYPE.AND); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp23()
        {
            while (lex.sy == SYMBOL.EQUOP)
            {
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                s_exp8();
                switch (Opr)
                {
                    case SYMBOL2.EQL: gen.emit(INSTYPE.EQL); break;
                    case SYMBOL2.NEQ: gen.emit(INSTYPE.NEQ); break;
                    default: return false;
                }
            }
            return true;
        }

        bool s_exp24()
        {
            while (lex.sy == SYMBOL.RELOP || lex.sy == SYMBOL.IS || lex.sy == SYMBOL.IN || lex.sy == SYMBOL.AS)
            {
                SYMBOL sy = lex.sy;
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                switch (sy)
                {
                    case SYMBOL.RELOP:
                        s_exp9();
                        switch (Opr)
                        {
                            case SYMBOL2.GTR: gen.emit(INSTYPE.GTR); break;
                            case SYMBOL2.LSS: gen.emit(INSTYPE.LSS); break;
                            case SYMBOL2.LEQ: gen.emit(INSTYPE.LEQ); break;
                            case SYMBOL2.GEQ: gen.emit(INSTYPE.GEQ); break;
                        }
                        break;

                    case SYMBOL.IN:     //if(a in A) ...  翻译成 if (a < A) .. , 意思是a是不是A的一个元素
                        s_exp9();
                        gen.emit(INSTYPE.LSS);
                        break;

                    case SYMBOL.IS:     //exp1 is exp2 翻译成 HostType.IsType(exp1,exp2)
                        s_var(true);    //s_exp9(); //is后面是.net的System.Type, 函数s_var(true)中的true表示支持generic type
                        s_call(Constant.FUNC_IS_TYPE, 2);
                        break;

                    case SYMBOL.AS:     //exp1 as exp2  翻译成 $castvt(exp1, exp2)
                        s_var(true);    //s_exp9();     //参照上面的SYMBOL.IS子句
                        s_call(Constant.FUNC_CAST_VALUE_TYPE, 2);
                        break;
                }
            }
            return true;
        }

  

        bool s_exp25()
        {
            while (lex.sy == SYMBOL.SHIFTOP)
            {
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                s_exp10();

                switch (Opr)
                {
                    case SYMBOL2.SHL: gen.emit(INSTYPE.SHL); break;
                    case SYMBOL2.SHR: gen.emit(INSTYPE.SHR); break;
                    default: return false;
                }
            }

            return true;

        }

        bool s_exp26()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.PLUS: lex.InSymbol(); s_exp12(); gen.emit(INSTYPE.ADD); break;
                case SYMBOL.MINUS: lex.InSymbol(); s_exp12(); gen.emit(INSTYPE.SUB); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp27()
        {
        L1:
            switch (lex.sy)
            {
                case SYMBOL.STAR: lex.InSymbol(); s_exp13(); gen.emit(INSTYPE.MUL); break;
                case SYMBOL.DIV: lex.InSymbol(); s_exp13(); gen.emit(INSTYPE.DIV); break;
                case SYMBOL.MOD: lex.InSymbol(); s_exp13(); gen.emit(INSTYPE.MOD); break;
                default: return true;
            }
            goto L1;
        }

        bool s_exp28()
        {
            return s_varnext(false);
        }
        
        #endregion



        #region s_var(), s_var1(), s_varnext(bool)

        bool s_var(bool typevar)
        {
            if (lex.sy == SYMBOL.identsy || lex.sy == SYMBOL.THIS || lex.sy == SYMBOL.BASE)
            {
                if (lex.sy == SYMBOL.THIS)
                {
                    gen.emit(INSTYPE.THIS, 0);
                    lex.InSymbol();
                }
                else if (lex.sy == SYMBOL.BASE)
                {
                    //int n = 0;
                    //do
                    //{
                    //    n++;
                    //    lex.InSymbol();
                    //    if (lex.sy == SYMBOL.STRUCTOP)
                    //        lex.InSymbol();
                    //    else
                    //        break;
                    //} while (lex.sy == SYMBOL.BASE);
                    //gen.emit(INSTYPE.BSE, n);
                    gen.emit(INSTYPE.BASE, 1);   //支持base.x, 不支持base.base.x
                    lex.InSymbol();
                }
                else
                {
                    string ident = lex.sym.id;

                    lex.InSymbol();
                    if (lex.sy == SYMBOL.GOESTO)
                    {
                        ((JParser)this).e_lambda(new string[] { ident }, 1);
                        return true;
                    }


                    int addr = vtab.VarAddr(ident);
                    if (addr == -1)
                    {
                        Operand x = Operand.Ident(ident);
                        gen.emit(INSTYPE.MOV, x);//LOAD
                    }
                    else   //local var
                    {
                        gen.emit(INSTYPE.MOV, Operand.REGAddr(SEGREG.BP, addr, ident));
                    }
                }


                s_varnext(typevar);

                return true;
            }

            return false;

        }

        //用来支持s_varnext()中的小括号嵌套
        bool s_var1(bool typevar)
        {

            switch (lex.sy)
            {
                case SYMBOL.identsy:
                    Operand x = Operand.Ident(lex.sym.id);
                    gen.emit(INSTYPE.MOV, x);//LOAD
                    lex.InSymbol();
                    break;

                case SYMBOL.LP:
                    lex.InSymbol();
                    s_var1(typevar);
                    expect(SYMBOL.RP);
                    break;

                default:
                    error.OnError(7);     // C07: ident,var expected,
                    break;
            }

            s_varnext(typevar);
            return true;
        }


        bool s_varnext(bool typevar)
        {
            bool compvar = false;  //compound variable
        L1:
            switch (lex.sy)
            {
                case SYMBOL.LB:
                    s_decl_array();
                    break;

                case SYMBOL.STRUCTOP:
                    compvar = true;
                    SYMBOL2 Opr = lex.opr;
                    lex.InSymbol();
                    if (lex.sy == SYMBOL.LP)
                    {
                        lex.InSymbol();
                        s_var1(typevar);
                        expect(SYMBOL.RP);
                    }
                    else
                    {
                        Operand x = Operand.Ident(lex.sym.id);
                        gen.emit(INSTYPE.MOV, x);//LOAD
                        lex.InSymbol();
                    }
                     
                    switch (Opr)
                    {
                        case SYMBOL2.DOT: gen.emit(INSTYPE.OFS); break;
                        case SYMBOL2.ARROW: gen.emit(INSTYPE.OFS); break;
                    }
                    break;

                case SYMBOL.LP:		//Call    ex.System.Math.sin(30) ==> sin(System.Math,30)
                    s_funcarg(compvar, -1);
                    break;

                case SYMBOL.RELOP:      
                    /***
                     * 支持Genric class如:new System.Collections.Generic.Dictionary<string, int>(...)
                     * 以及Generic method 如: Add<string>("abc");
                     * typevar 
                     *      true: generic class
                     *      false: generic method
                     *      
                     **/
                    if (lex.opr == SYMBOL2.LSS)
                        s_decl_generic(typevar, compvar);

                    return true;

                default:
                    return true;
            }
            goto L1;
        }

        private void s_decl_generic(bool typevar, bool compvar)
        {
            Operand generic;

            if (compvar)
                generic = gen.IV[gen.IP - 2].operand;
            else
                generic = gen.IV[gen.IP - 1].operand;


            if (generic.ty != OPRTYPE.identcon)
            {
                if (typevar)
                    error.OnError(2);              //ident expected
                else
                    return;        //因为到这里没有lex任何新的token,所以不需要完全TRACEBACK
            }

            //移去最后一个MOV ident 和 OFS, 目的是为了extend method做准备
            if (compvar)
                gen.IP -= 2;                //##删除的2条指令
            else
                gen.IP -= 1;                //##删除的1条指令

            int index = lex.Index();
            int IP = gen.IP;

            lex.InSymbol();
            int L0 = gen.emit(INSTYPE.MARK);
            s_var(true);                   //至少得有一个Type
            int count = 1;
            while (lex.sy == SYMBOL.COMMA)   //如果这种情况, Dictionary<,> 那么MARK和END之间为空
            {
                lex.InSymbol();
                s_var(true);
                count++;
            }
            gen.emit(INSTYPE.END);

            if (lex.sy == SYMBOL.RELOP && lex.opr == SYMBOL2.GTR)
                lex.InSymbol();
            else if (lex.sy == SYMBOL.SHIFTOP && lex.opr == SYMBOL2.SHR)    //把>> SHR 替换成2个 > GTR, 用掉一个,还有1个
            {
                lex.Traceback(lex.Index(), new Token(SYMBOL.RELOP, SYMBOL2.GTR));  //插入一个> GTR
            }
            else
            {
                if (typevar)
                    error.OnError(54);     // '>' expected
                else
                {
                    //TRACEBACK:
                    lex.Traceback(index, new Token(SYMBOL.RELOP, SYMBOL2.LSS));
                    gen.IP = IP;
                    //恢复被删除的指令, 见上面的注释: ##删除的2条指令
                    gen.emit(INSTYPE.MOV, generic);
                    if (compvar)
                        gen.emit(INSTYPE.OFS);

                    return;
                }
            }

            //如果typevar=true,那么一定要加`count的
            //如果不是,那么继续判断是不是generic method, which 后面会跟着函数参数(....)
            //用LP来预判后面是不是s_func_arg(..)可能不够充分, 如果是method,那么generic不加`符号
            if (typevar || lex.sy != SYMBOL.LP)
                generic.value = (string)generic.value + '`' + count;

            gen.emit(INSTYPE.GNRC, generic);

            return;
        }

        #endregion



        protected void s_funcarg(bool compvar, int entry)
        {
            Operand call;
            int parameter = 0;
            bool funcptr = false;

            if (entry > 0)       //用于把语句当成表达式, 如: sum = function(a,b) { return a+b;} (20,30);
            {
                call = Operand.Func(entry, this.module.moduleName);
               // call = new VAL(entry);
            }
            else if (gen.IV[gen.IP - 1].cmd == INSTYPE.GNRC)
            {
                call = gen.IV[gen.IP - 1].operand;      //取函数名称
                call = new Operand(); //NewVAL.Func(call.Str, module.moduleName);
            }
            else
            {
                /*
                 *  
                 * 第一种可能: call.ty==stringcon, 字符串
                 * 
                 * 第二种可能: call.ty==addrcon, 把函数名在另外一个函数参数中传递, high-level function
                 * merge = function (func, L, A)
                 * {   
                 *   return A + func(L);  
                 * };
                 *   
                 * 如果 (call.ty == VALTYPE.addrcon), 意味着在调用, 例如上面merge函数内部的语句return A + func(L); 中的函数func(L) 中的参数表 (L), 
                 * 这里的func是一个地址, 参见CPU中的CALL指令, 从堆栈中去入口地址
                 *   .....
                 *    else if (V.ty == VALTYPE.addrcon)
                 *        SysFuncCallAddr(SS[V.Address + BP]);
                 *   ....
                 * 
                 * 
                 * 第三种可能:
                 *  L={0,1,2,3,4,5,6,7,8,9,10};
                 *  A.plus = function(L)        //可以把函数赋值给一个复合变量
                 *     {
                 *        var sum=0; foreach(l in L) sum+=l;
                 *         return sum;
                 *     };
                 * sum6 = L.(A.plus)();   这个时候A.plus就是函数指针
                 * 
                 * 
                 * 
                 * */
                if (compvar)
                {
                    //第一种可能: 复合函数变量
                    if (gen.IV[gen.IP - 2].cmd == INSTYPE.MOV && gen.IV[gen.IP - 1].cmd == INSTYPE.OFS)
                    {
                        call = gen.IV[gen.IP - 2].operand;      //取函数名称
                        gen.IP -= 2;                        //移去[函数名称]指令,在后面CALL指令使用函数名 譬如: MOV plus; OFS
                    }
                    else
                    {
                        //第三种可能
                        gen.IP -= 1;                        //移去指令OFS
                        funcptr = true;
                        call = Operand.REG(SEGREG.NS);       //SEGREG.NS 只是用作标记而已
                        gen.emit(INSTYPE.PUSH);             //这个时候REG.Top是函数指针, 放入SS堆栈, 在函数调用后用SP -1 弹出
                    }
                }
                else
                {  //第一种可能:简单函数变量
                    call = gen.IV[gen.IP - 1].operand;      //取函数名称
                    gen.IP -= 1;
                }

                //第一种可能
                if (!funcptr && call.ty != OPRTYPE.addrcon)
                    call = Operand.Func(call.Str, module.moduleName);
            }

            lex.InSymbol();
            if (lex.sy != SYMBOL.RP)
            {
                for (; ; )
                {
                    if (s_exp1())
                    {
                        parameter++;
                        if (lex.sy == SYMBOL.COMMA)
                            lex.InSymbol();
                        else
                            break;
                    }
                }

                expect(SYMBOL.RP);
            }
            else
                lex.InSymbol();

            for (int i = 0; i < parameter; i++)
                gen.emit(INSTYPE.PUSH);

            /*
             * namespace of arguments has higher priority than function. 
             * e.g. this.form.Controls.Add(this.button1)
             *    this(.button1)is higher than this.form.Controls(.Add)
             * */
            if (compvar)
                gen.emit(INSTYPE.ESI);

            gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.IP));//func return address[BP-1]


            if (entry > 0)
            {
                gen.emit(INSTYPE.CALL, call); 
            }
            else
            {
                if (call.ty == OPRTYPE.addrcon)
                    gen.emit(INSTYPE.CALL, call);       //第二种可能:　address
                else if (funcptr)
                {
                    call.value = -parameter - 1;         //第三种可能: 函数入口地址 function pointer 在SS[SP+call.value]
                    gen.emit(INSTYPE.CALL, call);
                }
                else
                {                                       //第一种可能:
                    int addr = vtab.FuncAddr(call.Str);
                    if (addr != -1)
                    {
                        gen.emit(INSTYPE.CALL, new Operand(addr));      //传统的C语言风格的函数
                    }
                    else
                        gen.emit(INSTYPE.CALL, call);   //string
                }
            }



            gen.emit(INSTYPE.SP, new Operand(-(parameter + 1)));    //CPU中使用这个来计算有多少个函数参数的.
            
            if (compvar)
                gen.emit(INSTYPE.ESO);                          //CPU中把ESO用作有没有arg0标志的,参照CPU.cs

            
            //如果SS[SP-(parameter+1)] 保存有函数入口地址,那么,SS要多POP一个
            if (funcptr)                                        //因为上面的SP, ESO 有标志作用, 所以下面的SP -1 必须放在最后,
                gen.emit(INSTYPE.SP, new Operand(-1));

        }



        /**
      * 返回false: 
      *  格式为 new Circle(...) 
      *  或者 new new System.Windows.Forms.Label()
      *  
      * 返回true:
      *   格式为 new int[]
      *   或者 其他的情况 new T
      * 
      * */
        public bool s_instance()
        {
            s_var(true);

            if (gen.IV[gen.IP - 1].cmd == INSTYPE.SP)     //指令CALL后面,一定是指令SP
                gen.remit(gen.IP - 2, INSTYPE.NEW);       //普通对象 new Circle()    CALL/SP
            else if (gen.IV[gen.IP - 1].cmd == INSTYPE.ESO)
                gen.remit(gen.IP - 3, INSTYPE.NEW);       //带有名字空间的对象 new System.Windows.Forms.Label()   CALL/SP/ESO
            else
                return true;        // new int[][]{1,2,3}, 这个时候 gen.IV[gen.IP - 1].cmd == INSTYPE.NOP

            return false;
        }


      /**
      * 返回false: 
      *  格式为 new Circle(...) 
      *  或者 new new System.Windows.Forms.Label()
      *  
      * 返回true:
      *   格式为 new int[]
      *   或者 其他的情况 new T
      * 
      * */
        public bool s_decl_instance()
        {
            if (lex.sy != SYMBOL.identsy)
                error.OnError(2);              //ident expected

            string ident = lex.sym.id;
            lex.InSymbol();

            Operand x = Operand.Ident(ident);
            gen.emit(INSTYPE.MOV, x);//LOAD

            bool compvar = false;  //name space
            while (lex.sy == SYMBOL.STRUCTOP)
            {
                compvar = true;
                SYMBOL2 Opr = lex.opr;
                lex.InSymbol();
                if (lex.sy == SYMBOL.identsy)
                {
                    x = Operand.Ident(lex.sym.id);
                    gen.emit(INSTYPE.MOV, x);//LOAD
                    lex.InSymbol();
                }
                else
                    expect(SYMBOL.identsy);

                switch (Opr)
                {
                    case SYMBOL2.DOT: gen.emit(INSTYPE.OFS); break;
                    case SYMBOL2.ARROW: gen.emit(INSTYPE.OFS); break;
                }
            }


            /***
             * 支持Genric class如:new System.Collections.Generic.Dictionary<string, int>(...)
             * 以及Generic method 如: Add<string>("abc");
             * typevar 
             *      true: generic class
             *      false: generic method
             *      
             **/
            if (lex.sy == SYMBOL.RELOP && lex.opr == SYMBOL2.LSS)
            {
                s_decl_generic(true, compvar);
            }


            if (lex.sy == SYMBOL.LP)     //Call    ex.System.Math.sin(30) ==> sin(System.Math,30)
            {
                s_funcarg(compvar, -1);
                if (gen.IV[gen.IP - 1].cmd == INSTYPE.SP)     //指令CALL后面,一定是指令SP
                {
                    gen.remit(gen.IP - 2, INSTYPE.NEW);       //普通对象 new Circle()    CALL/SP
                    return false;
                }
                else if (gen.IV[gen.IP - 1].cmd == INSTYPE.ESO)
                {
                    gen.remit(gen.IP - 3, INSTYPE.NEW);       //带有名字空间的对象 new System.Windows.Forms.Label()   CALL/SP/ESO
                    return false;
                }

                return true;
            }

            /***
            * new 数组
            *      new int[2,3]
            *      new List<int>[20]
            **/
            while (lex.sy == SYMBOL.LB)
            {
                s_decl_array();
            }

            return true;
        }

        private void s_decl_array()
        {
            lex.InSymbol();
            if (lex.sy == SYMBOL.RB)
            {
                s_call(Constant.FUNC_MAKE_ARRAY_TYPE, 1);

                //技术性的插入一个空指令NOP, 是因为上面的s_call(....) emit了一个CALL指令,
                //如果new一个数组的话, 如new int[], 跟new Circle(), 最后2句都是CALL/SP指令,分不清楚
                //所以: new int[]的指令是  CALL/SP/NOP
                //参照JExpression.s_instance()
                gen.emit(INSTYPE.NOP);
                lex.InSymbol();
            }
            else if (lex.sy == SYMBOL.COMMA)    //多维数组
            {
                int rank = 1;
                do
                {
                    rank++;
                    lex.InSymbol();
                }
                while (lex.sy == SYMBOL.COMMA);
                expect(SYMBOL.RB);
                gen.emit(INSTYPE.MOV, new Operand(new Numeric(rank)));
                s_call(Constant.FUNC_MAKE_ARRAY_TYPE, 2);
                gen.emit(INSTYPE.NOP);          //技术性的插入一个空指令NOP. 参照上面的注释
            }
            else
            {
                //支持多维数组和多维属性   this[i,j,k];
                int L0 = gen.emit(INSTYPE.NOP);
                s_exp1();                   //至少得有一个下标
                if (lex.sy == SYMBOL.COMMA)
                {
                    gen.remit(L0, INSTYPE.MARK);
                    lex.InSymbol();
                    s_expr1();
                    gen.emit(INSTYPE.END);
                }

                expect(SYMBOL.RB);
                gen.emit(INSTYPE.ARR);
            }
        }



        #region +=, ++, --, #scope

        void repeatvar()	//i+=2  =>  i=i+2
        {
            gen.emit(INSTYPE.RCP);
        }

        bool s_assignop(SYMBOL2 opr)
        {
            switch (opr)
            {
                case SYMBOL2.ePLUS: gen.emit(INSTYPE.ADD); break;
                case SYMBOL2.eMINUS: gen.emit(INSTYPE.SUB); break;
                case SYMBOL2.eSTAR: gen.emit(INSTYPE.MUL); break;
                case SYMBOL2.eDIV: gen.emit(INSTYPE.DIV); break;
                case SYMBOL2.eMOD: gen.emit(INSTYPE.MOD); break;
                case SYMBOL2.eAND: gen.emit(INSTYPE.AND); break;
                case SYMBOL2.eOR: gen.emit(INSTYPE.OR); break;
                case SYMBOL2.eXOR: gen.emit(INSTYPE.XOR); break;
                case SYMBOL2.eSHL: gen.emit(INSTYPE.SHR); break;
                case SYMBOL2.eSHR: gen.emit(INSTYPE.SHL); break;
                default: return false;
            }
            gen.emit(INSTYPE.STO);

            return true;
        }

        bool s_incop(SYMBOL2 opr)
        {
            switch (opr)
            {
                case SYMBOL2.PPLUS:
                    repeatvar();
                    gen.emit(INSTYPE.MOV, new Operand(new Numeric(1)));
                    gen.emit(INSTYPE.ADD);
                    gen.emit(INSTYPE.STO);
                    break;

                case SYMBOL2.MMINUS:
                    repeatvar();
                    gen.emit(INSTYPE.MOV, new Operand(new Numeric(1)));
                    gen.emit(INSTYPE.SUB);
                    gen.emit(INSTYPE.STO);
                    break;
                default: return false;
            }

            return true;
        }


        protected bool s_scope()
        {
            string scope = "";
            Operand S = Operand.Scope("");

            if (lex.sy == SYMBOL.identsy)
            {
                scope = lex.sym.id;
                lex.InSymbol();
                while (lex.sy == SYMBOL.STRUCTOP)
                {
                    SYMBOL2 Opr = lex.opr;
                    switch (Opr)
                    {
                        case SYMBOL2.DOT: scope += "."; break;
                        case SYMBOL2.ARROW: scope += "->"; break;
                    }
                    lex.InSymbol();
                    scope += lex.sym.id;
                    lex.InSymbol();
                }

                S.value = scope;
                
            }


            gen.emit(INSTYPE.DIRC, S);
            return true;

        }
        
        #endregion


        protected void s_call(int call, int argc)
        {
            s_call(Operand.Func(call, module.moduleName), argc); 
        }

        protected void s_call(string func, int argc)
        {
            s_call(Operand.Func(func, module.moduleName), argc);
        }

        private void s_call(Operand func, int argc)
        {
            for (int i = 0; i < argc; i++)
                gen.emit(INSTYPE.PUSH);

            gen.emit(INSTYPE.PUSH, Operand.REG(SEGREG.IP));
            gen.emit(INSTYPE.CALL, func);
            gen.emit(INSTYPE.SP, -(argc + 1));
        }


        protected bool expect(SYMBOL sy)
        {
            if (lex.sy == sy)
            {
                lex.InSymbol();
                return true;
            }
            else
            {
                error.OnError(sy); // add code throw a exception
                return false;
            }
        }

    
    }
}

