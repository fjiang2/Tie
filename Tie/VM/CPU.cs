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
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;


namespace Tie
{


    class CPU 
    {
        Context context;

        //memory
        Instruction[]           CS;	    //Code segment
        StackSegment<VAL>        SS;	    //Stack segment
        StackSegment<VAL>        ES;	    //一个对象的method被调用时,保存这个对象的实例,也就是通常的Associative Array
        StackSegment<int>        EX;        //用于Exception的catch地址

        //registers
        Register REG;
        int BP;
        int SI;
        int IP;

        VAL R0, R1;

        //internal use
        private static VAL Mark = new VAL("$MaRk#_!`@=+|_x.?:%-;'*#&><");		//	 used in List or Array ({Mark) (}End)

        private string scope;
        private string moduleName;
        private Position position;

        public CPU(Module module, Context context)
        {
            this.context = context;

            this.scope = "";
            this.moduleName = module.moduleName;

            position = new Position(module.moduleName, null); //moduleName可能会被#module directive改变
            


            SS = new StackSegment<VAL>(Constant.MAX_STACK);
            for (int i = 0; i < SS.Size; i++) 
                SS[i] = new VAL();

            ES = new StackSegment<VAL>(Constant.MAX_EXTRA);
            for (int i = 0; i < ES.Size; i++)
                ES[i] = new VAL();

            EX = new StackSegment<int>(Constant.MAX_EXCEPTION);
            REG = new Register(SS);

            IP = module.IP1;
            BP = 0;
            SI = 0;

            CS = module.CS;

        }

        public Position Position
        {
            get
            {
                return this.position;
            }
        }


        /*
         * 
         * 运行到源程序line==breakpoint时候就涨停下来, 以后可以继续运行到下一个breakpoint或者运行到HALT 
         *
         * 
         * breakpoint = -1 相当于没有breakpoint,表示一直运行知道程序结束
         * 
         * */
        public VAL Run(int breakPoint)
        {
        L1:
     
            Instruction I = CS[IP];
            Operand operand = I.operand;

            /*
             * 下面的位置信息用于跟踪Exception发生时,TIE源程序的位置, 便于使用TIE的程序员调试程序
             * 尤其是在Exception没有被TIE系统捕捉到的时候,查看ExceptionPosition的变量能知道那行源程序出问题了.
             * 
             * 因为这个module还可以调用其他的module,那么moduleName和sourceCode有可能被改变,所以要不断的赋值
             * 
             * */
            position.line = I.line;
            position.col = I.col;
            position.cur = I.cur;
            position.block = I.block;

            if (I.line == breakPoint)
                return null;

            switch (I.cmd)
            {
                //----------------------------------------------------------------------------	

                #region 指令: MOV, STO,STO1, RMT. RCP

                case INSTYPE.MOV:
                    if (operand.ty == OPRTYPE.identcon)		    // MOV [v]
                    {
                        if (CS[IP + 1].cmd == INSTYPE.OFS)  // if this is offset of a struct, keep variable name
                            REG.Push(new VAL(operand));
                        else
                            REG.Push(GetVAL(operand.Str, false));
                    }
                    else if (operand.ty == OPRTYPE.addrcon)	// MOV [BP-3]
                    {
                        //VAL opr = Register.BPAddr(BP, operand);

                        VAL opr = new VAL();
                        opr.ty = VALTYPE.addrcon;
                        opr.value = BP + operand.Addr;
                        opr.name = operand.name;

                        REG.Push(opr);
                    }
                    else
                    {
                        VAL x = new VAL(operand);
                        if (operand.ty == OPRTYPE.funccon)
                        {
                            if (ES.SP > -1)
                                x.temp = new ContextInstance(this.context, ES.Top()) ;
                            else
                                x.temp = new ContextInstance(this.context, new VAL());
                        }

                        REG.Push(VAL.Clone(x)); // MOV 3
                    }
                    break;


    

                //----------------------------------------------------------------------------	


                case INSTYPE.STO1: 
                case INSTYPE.STO: 
                    R0 = REG.Pop(); 
                    R1 = REG.pop();
                    if (R1.ty == VALTYPE.addrcon)
                        SS[R1.Address] = R0;
                    else
                        HostOperation.Assign(R1, R0);

                    if (I.cmd == INSTYPE.STO)
                        REG.Push(R1);
                    break;

                case INSTYPE.RMT:
                    if (CS[IP + 1].cmd != INSTYPE.HALT)  //used for expression decoding,keep last value 
                        REG.Pop();
                    break;

                case INSTYPE.RCP:
                    REG.Push(VAL.Clone(REG.Top()));
                    break;
                
                #endregion


               //----------------------------------------------------------------------------	

                #region 指令 PUSH/POP/SP/ESI/ESO
                case INSTYPE.ESI:
                    R0 = REG.Pop();
                    ES.Push(R0);
                    break;
                case INSTYPE.ESO:
                    ES.Pop();
                    break;

                case INSTYPE.PUSH:
                    if (operand != null && operand.ty == OPRTYPE.regcon)
                        switch (operand.SEG)
                        {
                            case SEGREG.BP: SS.Push(new VAL(BP)); break;
                            case SEGREG.SP: SS.Push(new VAL(SS.SP)); break;
                            case SEGREG.SI: SS.Push(new VAL(SI)); break;
                            case SEGREG.IP: SS.Push(new VAL(IP + 2)); break;
                            case SEGREG.EX: EX.Push((int)operand.value); break;
                        }
                    else
                    {
                        R0 = REG.Pop();
                        SS.Push(R0);
                    }
                    break;

                case INSTYPE.POP:
                    if (operand.ty == OPRTYPE.regcon)
                        switch (operand.SEG)
                        {
                            case SEGREG.BP: BP = (SS.Pop()).Address; break;
                            case SEGREG.SI: SI = (SS.Pop()).Address; break;
                            case SEGREG.SP: SS.SP = (SS.Pop()).Address; break;
                            case SEGREG.EX: EX.Pop(); break;
                        }
                    else
                    {
                        R0 = SS.Pop();
                        REG.Push(R0);
                    }
                    break;
                case INSTYPE.SP:
                    SS.SP += operand.Addr;
                    break;

                #endregion


                //----------------------------------------------------------------------------	

                #region 指令: OFS, ARR
                //----------------------------------------------------------------------------	
                // Associated List
                // Mike={{"street", "3620 Street"},{"zip", 20201},{"phone","111-222-333},{"apt",1111}}
                // Mike.street = "3620 Street";
                // Mike.zip = 40802;
                case INSTYPE.OFS:
                    R0 = REG.Pop();
                    R1 = REG.pop();
                    {
                        VAL v = new VAL();
                        switch (R1.ty)
                        {
                            case VALTYPE.hostcon:
                                v = R1.getter(R0, true, OffsetType.STRUCT);
                                goto LOFS;
                            case VALTYPE.addrcon:
                                v = SS[R1.Address];
                                if (v.Undefined || v.IsNull)        //v.IsNull这个条件是因为这个局部变量初始化为null
                                {
                                    v.ty = VALTYPE.listcon;
                                    v.value = new VALL();
                                }
                                break;
                            case VALTYPE.listcon:
                                v = R1;
                                break;
                            default:   // if assoicative arrary is empty or not list
                                R1.ty = VALTYPE.listcon;
                                R1.value = new VALL();
                                v = R1;
                                break;
                        }

                        switch (v.ty)
                        {
                            case VALTYPE.listcon:
                                VALL L = v.List;
                                v = L[R0.Str];

                                // if property is not found in the associative array
                                if (!v.Defined)
                                {
                                    //class, static method or enum without registerting
                                    if(Constant.HOST_TYPE_AUTO_REGISTER)
                                        TryHostType(v);
                                    L[R0.Str] = v;
                                }
                                break;

                            case VALTYPE.hostcon:
                                //VAL.Assign(v, VAL.HostTypeOffset(v.value, R0.value));
                                v = HostOperation.HostTypeOffset(v, R0, OffsetType.STRUCT); 
                                break;
                        }

                    LOFS:
                        if ((object)v == null)
                            v = new VAL();

                        v.name = R1.name + "." + R0.name;      
                        REG.Push(v);
                    }
                    break;

                case INSTYPE.ARR:
                    R0 = REG.Pop();
                    R1 = REG.pop(); 
                    {
                        VAL v = new VAL();
                        switch (R1.ty)
                        {
                            case VALTYPE.addrcon:
                                v = SS[R1.Address];   //indirect addressing
                                if (v.Undefined || v.IsNull)  //v.IsNull这个条件是因为这个局部变量初始化为null
                                {
                                    v.ty = VALTYPE.listcon;
                                    v.value = new VALL();
                                    v = v.getter(R0, true, OffsetType.ARRAY);
                                }
                                else
                                    v = v.getter(R0, true, OffsetType.ARRAY);
                                break;

                            case VALTYPE.listcon:               //push reference
                                v = R1.getter(R0, true, OffsetType.ARRAY);
                                break;
                            case VALTYPE.hostcon:
                                v = R1.getter(R0, true, OffsetType.ARRAY);
                                if (!v.Defined)
                                    throw new RuntimeException(position, "{0} does not have property {1}.", R1, R0); 
                                break;
                            case VALTYPE.stringcon:
                                v = R1.getter(R0, true, OffsetType.ARRAY);
                                break;
                            default:
                                //refer: public VAL this[VAL arr], when R1 == null, dynamically allocate a list
                                R1.ty = VALTYPE.listcon;
                                R1.value = new VALL();
                                v = R1.getter(R0, true, OffsetType.ARRAY);

                                //JError.OnRuntime(0);
                                break;
                        }
                        
                        v.name = R1.name + "[" + R0.ToString() + "]";
                        REG.Push(v);
                    }
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	

                #region 指令: CALL, NEW, ENDP, RET, GNRC
                case  INSTYPE.GNRC:
                    R0 = REG.Pop();     // R0 = new Type[] { string, int }
                    R1 = REG.Pop();     // R1 = System.Collection.Generic
                    {
                        Operand opr = I.operand;
                        VAL R2 = R1[opr.Str];     // I.val.Str == Dictionary`2
                        if (R2.Undefined)             // Type is not registered
                        {
                            object t = HostType.GetType(R1.name + "." + opr.Str);
                            if (t != null)
                                R2 = VAL.NewHostType(t);
                            else
                                throw new RuntimeException(position, "Type {0}.{1} is not registered.", R1.name, opr.Str);
                        }
                        
                        object A = R0.HostValue;
                        if (A is object[] && ((object[])A).Length == 0)     //case: typeof(Dictionary<,>)
                        {
                            if (R2.value is Type)
                                REG.Push(VAL.NewHostType(R2.value));
                            else
                                throw new RuntimeException(position, "{0} is not System.Type.", R1);
                        }
                        else
                        {
                            if (!(A is Type[]))
                                throw new RuntimeException(position, "<{0}> is not System.Type[].", R0.ToSimpleString());

                            if (R2.value is Type)
                            {
                                Type t = (Type)R2.value;
                                REG.Push(VAL.NewHostType(t.MakeGenericType((Type[])A)));
                            }
                            else if (R2.value is MethodInfo)
                            {
                                MethodInfo t = (MethodInfo)R2.value;
                                VAL m = VAL.NewHostType(t.MakeGenericMethod((Type[])A));
                                m.temp = R2.temp;
                                REG.Push(m);
                            }
                            else if (R2.value is MethodInfo[])
                            {
                                MethodInfo[] T = (MethodInfo[])R2.value;
                                for (int i = 0; i < T.Length; i++)
                                    T[i] = T[i].MakeGenericMethod((Type[])A);
                                VAL m = VAL.NewHostType(T);
                                m.temp = R2.temp;
                                REG.Push(m);
                            }
                            else
                                throw new RuntimeException(position, "{0} is not System.Type.", R1);
                        }
                    }
                    
                    break;
                    
                case INSTYPE.CALL:
                    if (operand.ty == OPRTYPE.intcon)                                 //user-defined function使用传统的格式 e.g. function add(a,b) {return a+b;}
                        IP = operand.Addr;
                    else if (operand.ty == OPRTYPE.addrcon)  // high-level programming 
                    {
                        SysFuncCallByAddr(SS[operand.Addr + BP]);
                    }
                    else if (operand.ty == OPRTYPE.regcon)  // 函数指针, 参照JExpression.s_funcarg(bool compvar, int entry)
                    {
                        SysFuncCallByAddr(SS[operand.Addr + SS.SP]);
                    }
                    else if (operand.ty == OPRTYPE.none)   //used for Generic method
                    {
                        if (ES.IsEmpty())           //simple var, generic method还保留在REG.Top中
                            R0 = REG.Pop();
                        else
                            R0 = ES.Top();          //compvar, 因为Reg.Top被ESI指令Push到ES中去了.

                        SysFuncCallByName(R0);
                    }
                    else
                        SysFuncCallByName(new VAL(operand));              
                    goto L1;

                case INSTYPE.NEW:
                    if (operand.ty == OPRTYPE.funccon)
                    {
                        NewInstance(new VAL(operand));     //system predifined class & user-defined class
                    }
                    else if (operand.ty == OPRTYPE.none)   //used for Generic class
                    {
                        if (ES.IsEmpty())           //simple var, generic type还保留在REG.Top中
                            R0 = REG.Pop();
                        else
                            R0 = ES.Top();          //compvar, 因为Reg.Top被ESI指令Push到ES中去了.

                        NewInstance(R0);
                    }
                    else if (operand.ty == OPRTYPE.intcon)
                    {
                        int opr = (int)operand.value;
                        if (opr > 1)       // A = new int[] {1,2,3};  二个操作数
                        {
                            R0 = REG.Pop();
                            R1 = REG.Pop();
                        }
                        else
                        {
                            R1 = REG.Pop();
                        }

                        if (R1.value is Type)
                        {
                            Type ty = (Type)R1.value;

                            if (opr == 1)       // A = new int[];   一个操作数
                            {
                                if (ty.IsArray)
                                    R0 = VAL.Array();
                                else
                                    R0 = new VAL();
                            }

                            if (R0.ty == VALTYPE.listcon)
                            {
                                if (ty.IsArray)
                                    R0.List.ty = ty;
                                else
                                    throw new RuntimeException(position, "new object failed. {0} is not Array Type", R1);
                            }

                            R0.Class = ty.FullName;
                            REG.Push(R0);
                        }
                        else
                            throw new RuntimeException(position, "new object failed. {0} is not System.Type", R1);

                        IP++;

                    }

                    goto L1;

                case INSTYPE.ENDP:  //有些函数可能没有写return,就把ENDP当成是return语句
                    if ((OPRTYPE)operand.Intcon == OPRTYPE.classcon)
                        REG.Push(ES.Top());	        //return this;
                    else
                        REG.Push(VAL.NewVoidType());    //return void;

                    SS.SP = BP;                 //PUSH BP; POP SP;
                    BP = (SS.Pop()).Address;    //POP BP;
                    
                    R0 = SS.Top();              //EXEC RET
                    IP = R0.Address;		    
                    goto L1;
                case INSTYPE.RET:
                    if (!SS.IsEmpty())
                    {
                        R0 = SS.Top();
                        IP = R0.Address;		    //goto V.i
                    }
                    else
                    {
                        //用来支持Coding.Execute返回值, 遇到return语句就返回
                        if (REG.IsEmpty())
                            return VAL.NewVoidType();
                        else
                            return REG.Top();
                    }

                    goto L1;
                
                #endregion

                
                //----------------------------------------------------------------------------	

                #region 指令: +,-,*,/,%,>,<,!=,==,<=,>=, ++, --
                //----------------------------------------------------------------------------	
                case INSTYPE.NEG: 
                    R0 = REG.Pop();
                    if (operand.Intcon == -1)
                        R0 = -R0;   //call VAL.operator -(VAL)
                    else
                        R0 = +R0;   //call VAL.operator +(VAL)
                    REG.Push(R0);  
                    break;
                case INSTYPE.ADD: R0 = REG.Pop(); R1 = REG.Pop(); R1 += R0; REG.Push(R1); break;
                case INSTYPE.SUB: R0 = REG.Pop(); R1 = REG.Pop(); R1 -= R0; REG.Push(R1); break;
                case INSTYPE.MUL: R0 = REG.Pop(); R1 = REG.Pop(); R1 *= R0; REG.Push(R1); break;
                case INSTYPE.DIV: R0 = REG.Pop(); R1 = REG.Pop(); R1 /= R0; REG.Push(R1); break;
                case INSTYPE.MOD: R0 = REG.Pop(); R1 = REG.Pop(); R1 %= R0; REG.Push(R1); break;

                case INSTYPE.GTR: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 > R0)); break;
                case INSTYPE.LSS: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 < R0)); break;
                case INSTYPE.GEQ: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 >= R0)); break;
                case INSTYPE.LEQ: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 <= R0)); break;
                case INSTYPE.EQL: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 == R0)); break;
                case INSTYPE.NEQ: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 != R0)); break;

                case INSTYPE.INC: 
                    R0 = REG.pop();
                    R1 = VAL.Clone(R0); 
                    if (R0.ty == VALTYPE.addrcon)
                        SS[R0.Address] += new VAL(1);
                    else //global varible
                        HostOperation.Assign(R0, R0 + new VAL(1));
                    REG.Push(R1);
                    break;
                case INSTYPE.DEC: 
                    R0 = REG.pop();
                    R1 = VAL.Clone(R0);
                    if (R0.ty == VALTYPE.addrcon)
                        SS[R0.Address] -= new VAL(1);
                    else //global varible
                        HostOperation.Assign(R0, R0 - new VAL(1));
                    REG.Push(R1);
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	

                #region 指令: MARK, END, EACH, CAS, DIRC

                case INSTYPE.MARK: REG.Push(Mark); break;
                case INSTYPE.END:
                    {
                        VAL L = new VAL(new VALL());
                        while (REG.Top() != Mark)
                            L.List.Insert(REG.Pop());
                        REG.Pop();		// pop mark
                        REG.Push(L);
                    } break;
                
                case INSTYPE.EACH:
                    R0 = REG.Pop();         //Collection
                    R1 = REG.pop();         //address of element [BP+addr]
                    REG.Push(ForEach(R0, SS[R1.Intcon], SS[R1.Intcon + 1]));
                    break;
      
                case INSTYPE.CAS:
                    R0 = REG.Pop(); R1 = REG.Top();
                    if (R1 == R0) { REG.Pop(); IP = operand.Addr; goto L1; } //goto V.i
                    break;

                case INSTYPE.DIRC:      //directive command
                    switch (operand.mod)
                    {
                        case Constant.SCOPE:
                            this.scope = (string)operand.value;
                            break;

                    }
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	

                #region 指令: &&, ||, ~, &, |, >>, <<
                //----------------------------------------------------------------------------	
                case INSTYPE.NOTNOT: 
                    R0 = REG.Pop();
                    if (R0.ty == VALTYPE.stringcon)
                        REG.Push(Computer.Run(scope, R0.Str, CodeType.statements, context));
                    else
                        REG.Push(new VAL(!R0));     //call VAL.operator !(VAL)
                    break;

                case INSTYPE.ANDAND: R0 = REG.pop(); R1 = REG.Pop(); 
                    REG.Push(new VAL(R0.ty == VALTYPE.boolcon && R1.ty == VALTYPE.boolcon? R0.Boolcon && R1.Boolcon: false)); 
                    break;
                case INSTYPE.OROR: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(new VAL(R0.ty == VALTYPE.boolcon && R1.ty == VALTYPE.boolcon ? R0.Boolcon || R1.Boolcon : false));
                    break;


                //----------------------------------------------------------------------------	
                case INSTYPE.NOT: R0 = REG.Pop();
                    REG.Push(~R0);  //call VAL.operator ~(VAL)
                    break;
                case INSTYPE.AND: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(R1 & R0);
                    break;
                case INSTYPE.OR: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(R1 | R0 );
                    break;
                case INSTYPE.XOR: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(R1 ^ R0);
                    break;

                //----------------------------------------------------------------------------	
                case INSTYPE.SHL: R0 = REG.pop(); R1 = REG.Pop();
                    if (!(R0.value is int))
                        throw new RuntimeException(position, "the 2nd operand {0} in << operation must be integer", R0);
                    REG.Push(R1 << (int)R0.value);
                    break;
                case INSTYPE.SHR: R0 = REG.pop(); R1 = REG.Pop();
                    if (!(R0.value is int))
                        throw new RuntimeException(position, "the 2nd operand {0} in >> operation must be integer", R0);
                    REG.Push(R1 >> (int)R0.value);
                    break;
                #endregion


                //----------------------------------------------------------------------------	
                
                #region 指令: JUMP, ADR, VLU

                case INSTYPE.JMP: IP = operand.Addr; goto L1;
                case INSTYPE.LJMP: IP += operand.Addr; goto L1;

                case INSTYPE.JNZ:
                    if (REG.Pop() == new VAL(true))
                    { IP = operand.Addr; goto L1; }
                    else break;
                case INSTYPE.JZ:
                    if (REG.Pop() != new VAL(true))
                    { IP = operand.Addr; goto L1; }
                    else break;
                case INSTYPE.LJZ:
                    if (REG.Pop() != new VAL(true))
                    { IP += operand.Addr; goto L1; }
                    else break;

                case INSTYPE.ADR:
                    R0 = REG.Pop();
                    REG.Push(new VAL(R0.name));
                    break;

                case INSTYPE.VLU:
                    R0 = REG.Pop();
                    if (R0.ty == VALTYPE.stringcon)
                        REG.Push(Computer.Run(scope, R0.Str,CodeType.expression, context));
                    else
                        throw new RuntimeException(position, "Invalid address type:" + R0.ToString()); 
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	


                #region  指令: THIS,BASE
                case INSTYPE.THIS:                                       //this.x
                    if (ES.SP > -1)                                     //从当前的对象中查找变量
                    {
                        if (this.scope != "")                       //优先使用namespace定义中的this
                        {
                            VAL NS = GetScopeVAL(this.scope);
                            if (NS != ES.Top())
                                REG.Push(NS);
                        }
                        else
                            REG.Push(ES.Top());
                    }
                    else if (this.scope != "")
                    {
                        if (operand.ty == OPRTYPE.intcon)                     //this.x=100;
                            REG.Push(GetScopeVAL(this.scope));
                        else
                            REG.Push(new VAL(this.scope));         //p=&this.x;
                    }
                    else
                    {
                        //如果没有namespace, 那么就把this操作符当成没有作用,把后面的OFS操作符跳过去
                        if (CS[IP + 2].cmd == INSTYPE.OFS)
                            CS[IP + 2].cmd = INSTYPE.NOP;
                        else
                            throw new RuntimeException(position, "Operator[this] is invalid since namespace is empty.");
                    }
                    break;

                case INSTYPE.BASE:       //base.x
                    //if (ES.SP > -1)                                     //从当前的对象中查找变量
                    //{
                    //    VAL BS = ES.Top()[JExpression.BASE_INSTANCE];
                    //    if((object)BS!=null)
                    //        REG.Push(BS);
                    //    else
                    //        throw new RuntimeException(string.Format("Operator[base] is invalid since class {0} does not have base class.",ES.Top()));
                    //}
                    if (this.scope != "")
                    {
                        string bv = "";
                        int n = 1;
                        if (operand.ty == OPRTYPE.intcon)      //base.base.x=10;
                        {
                            n = operand.Intcon;

                            bv = GetBaseScope(this.scope, n);

                            if (bv != "")
                                REG.Push(GetScopeVAL(bv));
                            else
                            {
                                if (CS[IP + 2].cmd == INSTYPE.OFS)
                                    CS[IP + 2].cmd = INSTYPE.NOP;
                            }
                        }
                        else
                        {
                            n = operand.Addr;              // p = &base.base.x;
                            bv = GetBaseScope(this.scope, n);
                            REG.Push(new VAL(bv));
                        }
                    }
                    else
                        throw new RuntimeException(position, "Operator[base] is invalid since scope is root.");
                    break;

                #endregion


                //----------------------------------------------------------------------------	

                #region 指令: THRW, DDT, HALT

                case INSTYPE.THRW:
                    R0 = REG.Top();
                    if (!(R0.HostValue is Exception))
                        throw new RuntimeException(position, "{0} is not type of System.Exception.", R0.HostValue);

                    if (EX.IsEmpty())  //throw没有被放在try...catch的block里面
                        throw (Exception)R0.HostValue;
                    IP = EX.Pop();  //跳到catch的入口地址

                    if (IP == -1)      //没有定义catch语句,只定义了try{..}finally{..}, 那么直接把exception传递给系统
                        throw (Exception)R0.HostValue;
                    break;

            //调试指令
                case INSTYPE.DDT:
                    Logger.WriteLine(DebugInfo()); 
                    break;
                case INSTYPE.HALT:
                    if (REG.IsEmpty())
                        return VAL.NewVoidType();
                    else
                        return REG.Top();

                #endregion

            }

            IP++;
            goto L1;
        }

        private void TryHostType(VAL v)
        {
            if (R1.name != null && (R1.name == "System" || R1.name.StartsWith("System.")))
            {
                Type type = HostType.GetType(R1.name + "." + R0.name);
                if (type != null)
                {
                    v.ty = VALTYPE.hostcon;
                    v.value = type;
                    v.Class = type.UnderlyingSystemType.FullName;
                }
            }
        }

       

        #region Functions parameters/arguments

        private VALL SysFuncBeginParameter()
        {
            int count = SysFuncBegin();
            VALL L = new VALL();
            for (int i = 0; i < count; i++)
                L.Add(SysFuncParam(i));
            
            return L;
        }

        //return number of arguments passed in
        private int SysFuncBegin()
        {
            SS.Push(new VAL(BP));		// push BP
            BP = SS.SP;				    // mov BP,SP
            int num = -CS[IP + 1].operand.Addr - 1;	//IV[IP+1] == SP -n, total number of parameters
            return num;
        }

        //set returning value
        private bool SysFuncEnd(VAL ret)
        {
            REG.Push(ret);		// return value

            SS.SP = BP;				    // mov SP,BP
            IP = SS[BP - 1].Address;	// mov IP, (return address)
            BP = SS.Pop().Address;		// pop BP
            
            return true;
        }

        //get ith argument
        private VAL SysFuncParam(int i)
        {
            return SS[BP - 2 - i];      // skip PUSH IP, CALL 
        }

        #endregion


        #region 用于CALL指令的SysFuncCall(..)

        /**
         *  函数有3类
         *    1.A类函数, TIE script定义的User Function
         *    2.B类函数, .NET DLL库中的函数
         *    3.C类函数, TIE定义的系统函数,和Function Chain中的函数
         * 
         * 
         * */
        //这里的proc就是TIE Userfunction函数体本身, 或者是
        bool SysFuncCallByAddr(VAL proc)
        {
            VAL instance = new VAL();
            bool arg0 = !ES.IsEmpty() && CS[IP + 2].cmd == INSTYPE.ESO;  //查看有没有函数第一个参数

            if (ES.SP > -1)             //调用Tie定义的有namespace的函数.e.g  system.math.add= function(a,b) {return a+b;}; 
                instance = ES.Top();     //这里可能有bug, 因为当前的ES.Top() 有可能不是这个函数的namespace   


            return SysFuncCall(proc.value, proc, instance, arg0);
        }



        //call本地扩展的.NET function, 这里的name只是函数指针变量名字,不是函数体入口地址
        bool SysFuncCallByName(VAL name)
        {
            if (name.value is int)
                return SysFuncCallByAddr(name);
            else if(name.value is MethodInfo || name.value is MethodInfo[]) //此时,用于generic method
                return SysFuncCall(null, name, new VAL(), false);



            string func = name.Str;
            VAL proc = new VAL();
            VAL instance = new VAL();
            bool arg0 = !ES.IsEmpty() && CS[IP + 2].cmd == INSTYPE.ESO;     //查看有没有函数第一个参数

            //调用Tie定义的有namespace的函数
            if (ES.SP > - 1 )                     //user-defined delegate .e.g  system.math.add= function(a,b) {return a+b;};   
            {
                instance = ES.Top();
                if (instance.ty == VALTYPE.listcon)
                {
                    proc = instance[func];
                    if (proc.ty == VALTYPE.funccon)
                    {
                        UserFuncCall(proc, instance, false);  //这里不可能用Extend Method的, 如对象中的Method调用cirle.Area();
                        return true;
                    }
                }
            }
            
            //调用Tie定义的函数
            proc = GetVAL(func, true);
            
            return SysFuncCall(func, proc, instance, arg0);
        }


        /**
         * 
         *  这里的func可能是int(A类函数入口地址),也可能是string(B或C类函数)
         *  如果是A类函数, func为int, proc是函数入口, func = proc.Intcon;
         *  如果是B类函数, func为string,proc有值, func = proc.Str;
         *  如果是C类函数, func为函数名字, 那么proc为nullcon
         * 
         * */
        private bool SysFuncCall(object func, VAL proc, VAL instance, bool arg0)
        {
            if (proc.ty == VALTYPE.funccon)
            {
                //Tie定义的函数, A类函数
                UserFuncCall(proc, instance, arg0);
                return true;
            }

            VALL L = SysFuncBeginParameter();

            //调用HostType的函数指针函数, B类函数
            if (arg0 && proc.temp is HostOffset)
            {
                //例如 A.plus(20,30) 可以有3种情况, 1. A是class name, 2. A是一个class的instance, 3. extend method
                object host = ((HostOffset)proc.temp).host;
                if (host != instance.value)      //第3种情况下, 函数的host一定不等于ES.Top()中的内容
                {
                    L.Insert(instance);       //extend method
                    arg0 = false;             //只允许insert一次
                }
            }
            VAL ret = HostOperation.HostTypeFunction(proc, L);
            if (ret.Defined)
                return SysFuncEnd(ret);



            //调用TIE用C#实现的函数, C类函数
            if (arg0)
                L.Insert(instance);                         //如果是Activity.Unlock(20)-->转化为Unlock(Activity,20)
            try
            {
                ret = context.InvokeFunction((string)func, new VAL(L), position);
            }
            catch (FunctionNotFoundException e1)
            {
                throw e1;

            }
            catch (Exception e2)
            {
                if (e2 is RuntimeException)
                    throw e2;
                else
                    throw new RuntimeException(position, "Error: function {0}({1}) implementation, {2}", func, L.ToString2(), e2.Message);
            }

            return SysFuncEnd(ret);
        }


        /*
            * 用于CALL指令
            * 
            * call本地或者其他module的Tie function
            * 
            * */
        private void UserFuncCall(VAL func, VAL instance, bool arg0)
        {
            int count1 = -CS[IP + 1].operand.Addr - 1;       //传入参数个数

            if (moduleName == func.Class)                   //在同一个Module
            {
                if (arg0)      //压入第一个参数, Extend Method
                {
                    VAL ip = SS.Pop();
                    SS.Push(instance);
                    SS.Push(ip); 
                    count1++;
                }

                IP = (int)func.value;
                paramsCheck(IP, count1);

            }
            else   //call另外module的function
            {
                VALL L = SysFuncBeginParameter();
                if (arg0) L.Insert(instance);
                VAL ret = InternalUserFuncCall(func, instance, new VAL(L));

                SysFuncEnd(ret);
            }

        }

        //检查参数个数是不是匹配, 并支持unbound参数
        private void paramsCheck(int call, int count1)
        {
            int count2 = CS[call].operand.Intcon -1;             //PROC中定义的参数个数    

            int diff = count1 - count2;
            if (diff == 0)
                return;

            if (diff < -1 || (diff > 0 && count2 == 0))      //传入的参数个数不足
                throw new RuntimeException(position, "Number of function arguments is not matched in module=[{0}] address=[{1}].",
                    moduleName, call);
            else if (diff >= -1)
                paramsArray(count1, count2);  //多余的参数项目压制成数组作为可变参数
            
        }

        //支持函数可变参数的传递,多余的参数自动转化为可变参数array
        private void paramsArray(int argc1, int argc2)
        {
            //当前的SS数据格式为: {..., argn,....arg1, arg0, retAddr} -->TOP

            int diff = argc1 - argc2;
            
            //函数返回地址
            VAL retAddr = SS.Pop();                 

            //保存固定参数
            VALL L1 = new VALL();
            for (int i = 0; i < argc2 - 1; i++)
                L1.Insert(SS.Pop());

            //把多余的参数压制成array,并推入堆栈
            VALL L2 = new VALL();
            for (int i = 0; i <= diff; i++)
                L2.Add(SS.Pop());
            SS.Push(new VAL(L2));

            //复原固定参数
            for (int i = 0; i < argc2 - 1; i++)
                SS.Push(L1[i]);
            
            //插入返回地址
            SS.Push(retAddr);

            //修正arguments堆栈的个数
            CS[retAddr.Intcon].operand.Addr += diff;
        }

        #endregion


        
        #region 独立执行函数调用工具InternalUserFuncCall(..)


        /*
         * 调用执行(运行)本地函数
         * 
         *  arguments 是listcon, e.g.  
         *  
         * 对象中的方法
         *      ret = instance.func(arguments); 
         *      
         * 或者全局函数
         *      ret = instance.func(arguments); 
         * */
        public VAL InternalUserFuncCall(int address, VAL instance, VAL arguments)  
        {
           // if (func.ty != VALTYPE.funccon && func.ty!=VALTYPE.classcon)
           //     return null;
           // int address = (int)func.value;

            int BP1 = BP;
            int IP1 = IP;
            int SP1 = SS.SP;

            
            //推入对象实例, 全局函数的instance为new VAL()
            ES.Push(instance);

            //逆向推入函数参数
            int count1 = arguments.Size;
            for(int i=0; i<count1; i++)
                SS.Push(arguments[count1 - i -1]);

            IP = CS.Length-2;
            SS.Push(new VAL(IP));                                           //函数执行完毕以后返回地址
            CS[IP] = new Instruction(INSTYPE.SP, new Operand(-count1 - 1), Position.UNKNOWN);    //用以清除stack
            CS[IP + 1] = new Instruction(INSTYPE.HALT, Position.UNKNOWN);         //插入HALT指令到CS的最下端,终止程序

            //function entry
            IP = address;                                                   //delegate PROC address
            paramsCheck(IP, count1);

            VAL ret;
#if DEBUG
            ret = Run(-1);                                                  //执行函数, breakpoint = -1 意思没有breakpoint
#else
            try
            {
                ret = Run(-1);                                              
            }
            catch (Exception e)
            {
                if (e is PositionException)
                    throw e;
                else
                    throw new RuntimeException(position, "failed to invoke {0}({1}) {2}", CS[address].operand, arguments.List.ToString2(), e);
            }
#endif
            REG.Pop();                                                      //清除寄存器中的返回值
            ES.Pop();
            BP = BP1;
            IP = IP1;
            SS.SP = SP1;
            return ret;
        }


        /*
         *  调用执行其他module的函数
         * 
         * 
         * */
        public VAL InternalUserFuncCall(VAL func, VAL instance, VAL arguments)
        {
            if (func.Class != moduleName)
                return ExternalUserFuncCall(func, instance, arguments, this.context);   //调用其他模块的
            else
                return InternalUserFuncCall((int)func.value, instance, arguments);  //同一个模块
        }
           
        public static VAL ExternalUserFuncCall(VAL func, VAL instance, VAL arguments, Context context)
        {
            string moduleName = func.Class;
            Module module = Library.GetModule(moduleName);
            if (module == null)
                throw new TieException("Module is not loaded yet. " + moduleName);

            CPU cpu = new CPU(module, context);
            VAL ret = cpu.InternalUserFuncCall((int)func.value, instance, arguments);

            return ret;
        }

        #endregion



        #region New Instance

        /**
         * 用于NEW指令
         * 
         * new instance 产生对象实例
         * 
         * 1. 产生Tie实例   
         * 2. 产生.Net 对象实例, 如: new System.Windows.Form.Label();
         * 3. 产生usercon类型的list
         * 
         * 
         * */
        bool NewInstance(VAL V)         //new user defined class, .net object or a listcon
        {
            VALL L = SysFuncBeginParameter();

            if (V.ty == VALTYPE.funccon)
            {
                string func = V.Str;


                VAL userClass = GetVAL(func, true);     //用户自定义对象 circle = new Circle(1,2,3), 就是在调用Circle = function(x,y,radius
                if (userClass.ty == VALTYPE.classcon)        // 等价于circle.Circle(1,2,3);
                {
                    //VAL instance =  NewVAL.UserType(func);
                    VAL instance = new VAL();   //NewInstance的返回值为listcon类型

                    VAL ret = InternalUserFuncCall(userClass, instance, new VAL(L));   //调用其他模块的
                    return SysFuncEnd(ret);
                }
                else
                {
                    VAL args = new VAL(L);

                    VAL scope = new VAL();
                    if (!ES.IsEmpty())
                        scope = ES.Top();

                    VAL Clss = HostCoding.Decode(func, args, scope, context);       //如果是.NET object,那么需要解码
                    return SysFuncEnd(Clss);
                }
            }
            else if (V.value is Type)       //generic class
            {
                VAL args = new VAL(L);
                object instance = Activator.CreateInstance((Type)V.value, HostCoding.ConstructorArguments(args));
                return SysFuncEnd(VAL.NewHostType(instance));
            }

            throw new RuntimeException(position, "new instance {0} failed", V);
        }
        
        



    
      
        
    #endregion



        #region Data Segment Management

  
        private VAL GetScopeVAL(string scope)
        {
            VAL THIS = new VAL();
            if (scope == "")
                return THIS;

            string[] L = Module.ParseScope(scope);
            THIS = GetVAL(L[0], false);
            
            int i = 1;
            while (i < L.Length)
            {
                string id = L[i];
                if (!THIS[id].Defined) //如果namespace说代表的变量不存在,就自动产生一个
                    THIS[id] = new VAL();
                THIS = THIS[id];

                i++;
            }
            
            THIS.name = scope;
            return THIS;
        }

        /*** old version
        private VAL GetScopeVAL(string scope)
        {
            VAL THIS = new VAL();
            if (scope == "")
                return THIS;

            JLex lex = new JStringLex(scope);
            lex.InSymbol();
            if (lex.sy == SYMBOL.identsy)
            {
                THIS = GetVAL(lex.sym.id, false);
                lex.InSymbol();
                while (lex.sy == SYMBOL.STRUCTOP)
                {
                    SYMBOL2 Opr = lex.opr;
                    if (!lex.InSymbol())     //没有字符可读
                    {
                        JError.OnError(7);
                        break;
                    }
                    if (!THIS[lex.sym.id].Defined) //如果namespace说代表的变量不存在,就自动产生一个
                        THIS[lex.sym.id] = new VAL();
                    THIS = THIS[lex.sym.id];
                    lex.InSymbol();
                }
            }

            THIS.name = scope;
            return THIS;
        }
        **/

        private VAL GetVAL(string ident, bool readOnly)
        {
            if (ES.SP > -1)         //从当前的对象中查找变量
            {
                VAL val = null;
                VAL instance = ES.Top();
                if(instance.ty == VALTYPE.hostcon && CS[IP + 2].cmd == INSTYPE.ESO)
                    val = HostOperation.HostTypeOffset(instance, new VAL(ident), OffsetType.STRUCT);
                else
                    val = instance[ident];

                if (val.Defined)
                    return val;
            }

            return context.GetVAL(ident, readOnly);
        }

        private static string GetBaseScope(string scope, int n)
        {
            
            char[] a = scope.ToCharArray();
            int i = a.Length-1;
            while (i >0)
            {
                if (a[i] == '.')
                    n--;
                
                if(n==0)
                    break;
                
                i--;
            }

            return new string(a,0,i);
        }

        #endregion


        //用于CPU的foreach指令
        private VAL ForEach(VAL collection, VAL element, VAL index)
        {
            int i = index.Intcon;

            if (collection.ty == VALTYPE.listcon)
            {
                if (i >= collection.Size)
                    return new VAL(false);

                HostOperation.Assign(element, collection[i]);
                index.value = ++i;
                return new VAL(true);
            }

            else if (collection.ty == VALTYPE.hostcon)
            {
                if (!(collection.value is IEnumerable))
                    throw new RuntimeException(position, "foreach statement requires {0} to be IEnumerable", collection.value.GetType().FullName);

                IEnumerable enumerable = (IEnumerable)collection.value;
                
                IEnumerator enumerator;
                if (index.temp is IEnumerator)
                    enumerator = (IEnumerator)index.temp;
                else
                {
                    enumerator = enumerable.GetEnumerator();
                    index.temp = enumerator;
                }
                
                if (enumerator.MoveNext())
                {
                    HostOperation.Assign(element, VAL.Boxing1(enumerator.Current));
                    index.value = ++i;      //没有什么用处,就是好玩;
                    return new VAL(true);
                }
                else
                {
                    index.temp = null;
                    return new VAL(false);
                }
            }
            else
                throw new RuntimeException(position, "foreach statement requires [{0}]={1} to be IEnumerable", collection.name, collection.ToString());

        }

        public string DebugInfo()
        {
            StringWriter b = new StringWriter();
            b.WriteLine("mod:" + moduleName);
            b.WriteLine(CS[IP]);
            b.WriteLine(string.Format("IP={0}\tBP={1}", IP, BP));
            b.WriteLine(REG.ToString());
            b.WriteLine("ES: " + ES.ToString());
            b.WriteLine("SS: " + SS.ToString());
            //b.WriteLine("DS1: " + DS1.ToString());
            //b.WriteLine("DS2: " + DS2.ToString());
            
            return b.ToString();
        }


    

    }
}
