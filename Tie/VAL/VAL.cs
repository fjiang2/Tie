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
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using System.Reflection;

namespace Tie
{


    /// <summary>
    /// Any data in the Tie is VAL
    /// </summary>
    public sealed class VAL : IValizable, IEnumerable<VAL>, IEnumerable, IComparable, IComparable<VAL>, IEquatable<VAL>
#if !SILVERLIGHT
        ,ISerializable
        ,ICloneable
#endif
    {
        /// <summary>
        /// internal value of VAL
        /// </summary>
        internal object value;
        /// <summary>
        /// type of value
        /// </summary>
        internal VALTYPE ty;

        internal string Class;
        internal string name;
        internal HandlerActionType hty = HandlerActionType.None;    //indicate Add/Remove event

        internal object temp;  //runtime临时存储单元,内容可丢弃, 现用于.NET 对象的属性支持


        #region IValization
        /// <summary>
        /// IValizable function.
        /// </summary>
        /// <returns></returns>
        public VAL GetVAL()
        {
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SetVAL(VAL val)
        {
            Copy(val, this);
        }
        #endregion



        #region ICloneable/IComparable/IComparable<VAL>/IEquatable<VAL>/ISerializable
        /// <summary>
        /// reates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns> A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return VAL.Clone(this);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        ///     an integer that indicates whether the current instance precedes, follows,
        ///     or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="v">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects
        ///     being compared. The return value has these meanings: Value Meaning Less than
        ///     zero This instance is less than obj. Zero This instance is equal to obj.
        ///     Greater than zero This instance is greater than obj.
        ///</returns>
        public int CompareTo(VAL v)
        {
            if (this == v)
                return 0;
            else if (this > v)
                return 1;
            else
                return -1;
        }

        int IComparable.CompareTo(object v)
        {
            if(v is VAL)
                return CompareTo((VAL)v);
            
            throw new InvalidCastException("cannot compare non VAL"); 
        }

        /// <summary>
        ///  Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="o"> An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(VAL o)
        {
            return this == o;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Create instance from SerializationInfo
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public VAL(SerializationInfo info, StreamingContext ctxt)
        {
            ty = (VALTYPE)info.GetValue("ty", typeof(VALTYPE));
            Class = (string)info.GetValue("Class", typeof(string));
            value = info.GetValue("value", typeof(object));
        }

       
        /// <summary>
        ///  Serialization function.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ty", ty);
            info.AddValue("Class", Class);
            info.AddValue("value", value);
        }
#endif

        #endregion

        #region VAL Contructor
        
        /// <summary>
        /// create instance with value = null
        /// </summary>
        public VAL()
        {
            ty = VALTYPE.nullcon;
            value = null;
        }
 
        /// <summary>
        /// create instance with value = boolean
        /// </summary>
        /// <param name="b"></param>
        public VAL(bool b)
        {
            ty = VALTYPE.boolcon;
            value = b;
        }

        /// <summary>
        /// create instance with value = integer
        /// </summary>
        /// <param name="i"></param>
        public VAL(int i)
        {
            ty = VALTYPE.intcon;
            value = i;
        }

        /// <summary>
        /// create instance with value = double
        /// </summary>
        /// <param name="d"></param>
        public VAL(double d)
        {
            ty = VALTYPE.doublecon;
            value = d;
        }

        /// <summary>
        /// create instance with value = decimal
        /// </summary>
        /// <param name="d"></param>
        public VAL(decimal d)
        {
            ty = VALTYPE.decimalcon;
            value = d;
        }

        /// <summary>
        /// create instance with value = string
        /// </summary>
        /// <param name="str"></param>
        public VAL(string str)
        {
            ty = VALTYPE.stringcon;
            value = str;
        }


        //list
        internal VAL(VALL list)
        {
            ty = VALTYPE.listcon;
            value = list;
        }

        internal VAL(Operand opr)
        {
            switch (opr.ty)
            { 
      
                case OPRTYPE.numcon:
                    Numeric c = (Numeric)(opr.value);
                    switch (c.ty)
                    { 
                        case NUMTYPE.boolcon:
                            ty = VALTYPE.boolcon;
                            break;

                        case NUMTYPE.doublecon:
                            ty = VALTYPE.doublecon;
                            break;

                        case NUMTYPE.intcon:
                            ty = VALTYPE.intcon;
                            break;

                        case NUMTYPE.stringcon:
                            ty = VALTYPE.stringcon;
                            break;

                        case NUMTYPE.nullcon:
                            ty = VALTYPE.nullcon;
                            break;

                        case NUMTYPE.voidcon:
                            ty = VALTYPE.voidcon;
                            break;
                    }
                    value = c.value;
                    break;

                case OPRTYPE.classcon:
                    ty = VALTYPE.classcon;
                    value = opr.value;
                    break;
                case OPRTYPE.funccon:
                    ty = VALTYPE.funccon;
                    value = opr.value;
                    break;

                case OPRTYPE.addrcon:
                    ty = VALTYPE.addrcon;
                    value = opr.value;
                    break;

                case OPRTYPE.identcon:
                    ty = VALTYPE.identcon;
                    value = opr.value;
                    break;

                //case OperandType.intcon:
                //case OperandType.none:
                //case OperandType.regcon:
                default :
                    throw new TieException("{0} not supported in VAL", opr.ty);

            }


            name = opr.name;
            Class = opr.mod;
        }


        /// <summary>
        /// create instance with value = .NET object
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static VAL NewHostType(object host)
        {
            VAL v = new VAL();
            v.ty = VALTYPE.hostcon;

            v.value = host;

            if (host != null)
                v.Class = v.value.GetType().UnderlyingSystemType.FullName;
            else
                v.Class = "";

            return v;
        }


        internal static VAL NewVoidType()
        {
            VAL v = new VAL();
            v.ty = VALTYPE.voidcon;
            return v;
        }


        internal static VAL NewScriptType(string script)
        {
            VAL v = new VAL();
            v.ty = VALTYPE.scriptcon;
            v.value = script;
            return v;
        }
        
        internal static VAL NewEnumType(Enum field)
        {
            VAL v = new VAL();
            v.ty = VALTYPE.enumcon;
            v.value = field;
            return v;
        }

        /// <summary>
        /// create instance with value = varible dictionary
        /// </summary>
        /// <param name="memory"></param>
        public VAL(Memory memory)
        {
            VAL v = Memory.Assemble(memory);
            this.ty = v.ty;
            this.Class = v.Class;
            this.value = v.value;
        }

        /// <summary>
        /// create instance with value = Jagged Array
        /// </summary>
        /// <param name="A"></param>
        public VAL(Array A)
        {
            VALL L = new VALL();
            L.ty = A.GetType();
            foreach (object obj in A)
                L.Add(VAL.Boxing(obj));

            this.ty = VALTYPE.listcon;
            this.Class = L.ty.FullName;
            this.value = L;
        }

        internal VAL(VAL val)
        {
            Copy(val, this);
        }

        #endregion

        /// <summary>
        /// constant NULL 
        /// </summary>
        public static readonly VAL NULL = new VAL();

        /// <summary>
        /// constant VOID
        /// </summary>
        public static readonly VAL VOID = new VAL() { ty = VALTYPE.voidcon, value = null };
       
        /// <summary>
        /// get this value's type
        /// </summary>
        public VALTYPE VALTYPE
        {
            get { return ty; }
        }

        /// <summary>
        /// update this object
        /// </summary>
        /// <param name="ty"></param>
        /// <param name="value"></param>
        public void UpdateObject(VALTYPE ty, object value)
        {
            this.ty = ty;
            this.value = value;
        }


        /// <summary>
        /// internal value of VAL
        /// </summary>
        public object Value
        {
            get { return value; }
        }

        internal static VAL Clone(VAL from)
        {
            VAL to = new VAL();
            Copy(from, to);
            return to;
        }

        private static void Copy(VAL from, VAL to)
        {
            to.ty = from.ty;
            if (from.ty != VALTYPE.listcon)
                to.value = from.value;
            else
            {
                to.value = new Tie.VALL(from.List);
            }
            to.Class = from.Class;
            to.name = from.name;
            to.hty = from.hty;
            to.temp = from.temp;
        }



        #region Boxing/UnBox
        
        /// <summary>
        /// Box any object into VAL, support multiple dimension array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static VAL Boxing(object v)
        {
            if (v is Array)
            {
                VAL L = VAL.Array();
                Array A = (Array)v;
                foreach (object obj in A)
                    L.List.Add(VAL.Boxing(obj));

                int[] lengths = new int[A.Rank];
                for (int i = 0; i < lengths.Length; i++)
                    lengths[i] = A.GetLength(i);

                L = multiDimensionalArray(L, lengths, A.Rank - 1);
                L.List.ty = A.GetType();
                L.Class = L.List.ty.FullName; 
                return L;
            }
            return Boxing1(v);
        }

     

        
        /// <summary>
        /// Box any object into VAL without support array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal static VAL Boxing1(object v)
        {
            if (v == null)
                return new VAL();
            else if (v == System.DBNull.Value)
            {
                VAL x = new VAL();
                x.Type = typeof(DBNull);
                return x;
            }

            else if (v is string)
                return new VAL((string)v);
            else if (v is bool)
                return new VAL((bool)v);
            else if (v is int)
                return new VAL((int)v);
            else if (v is double)
                return new VAL((double)v);
            else if (v is decimal)
                return new VAL((decimal)v);

            else if (v is byte)
                return new VAL(Convert.ToInt32(v));
            else if (v is sbyte)
                return new VAL(Convert.ToInt32(v));
            else if (v is short)
                return new VAL(Convert.ToInt32(v));
            else if (v is ushort)
                return new VAL(Convert.ToInt32(v));
            else if (v is uint)
                return new VAL(Convert.ToDecimal(v));
            else if (v is long)
                return new VAL(Convert.ToDecimal(v));
            else if (v is ulong)
                return new VAL(Convert.ToDecimal(v));

            else if (v is float)
                return new VAL(Convert.ToDouble(v));
            else if (v is char)
                return new VAL(((char)v).ToString());
            

            else if (v is VAL)
                return (VAL)v;
            else if (v is VALL)
                return new VAL((VALL)v);
            else if (v is Memory)
                return new VAL((Memory)v);

            
            return VAL.NewHostType(v);
        }

        //支持多维数组, int[,,] A = new int[,,] { {{1,2}, {3,4}, {5,6}}, {{7,8}, {9,10}, {11,12}} };  
        //把1维数组A转为2维数组L1
        private static VAL multiDimensionalArray(VAL A, int[] lengths, int d)
        {
            if (d == 0)
                return A;

            VAL L1 = VAL.Array();
            int len = lengths[d];
            
            VAL L2 = VAL.Array();
            for (int i = 0; i < A.Size; i++)
            {
                L2.Add(A[i]);
                if ((i + 1) % len == 0)
                {
                    L1.Add(L2);
                    L2 = VAL.Array();
                }
            }

            return multiDimensionalArray(L1, lengths, d - 1);
        }

        /// <summary>
        /// Unbox VAL to .NET host object
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static object UnBoxing(VAL val)
        { 
            return val.HostValue;
        }

      

   
        #endregion


        #region VAL Operator v=-exp; v=exp1+exp2; v=exp1-exp2;

        /// <summary>
        /// not operator, support operator overloading in host object
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool operator !(VAL v)
        {
            switch (v.ty)
            {
                case VALTYPE.boolcon:
                    return !(bool)(v.value);

                case VALTYPE.hostcon:
                    VAL v1 = HostFunction.OperatorOverloading(Operator.op_LogicalNot, v);
                    if(v1.ty == VALTYPE.boolcon)
                        return (bool)v1.value;
                    
                    throw new InvalidOperationException("overloading operator !(.) must return bool value."); 

            }

            return false;
        }

        /// <summary>
        /// negative operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <returns>return reversed list if v1 is list</returns>
        public static VAL operator -(VAL v1)
        {
            VAL v = VAL.Clone(v1);

            switch (v.ty)
            {
                case VALTYPE.intcon:
                    v.value = -(int)(v.value);
                    return v;

                case VALTYPE.doublecon:
                    v.value = -(double)(v.value);
                    return v;

                case VALTYPE.decimalcon:
                    v.value = -(decimal)(v.value);
                    return v;

                case VALTYPE.listcon:
                    v.List.Reverse();
                    return v;

                case VALTYPE.stringcon:
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_UnaryNegation, v);

            }

            return v;
        }

        /// <summary>
        ///  positive operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static VAL operator +(VAL v1)
        {
            VAL v = VAL.Clone(v1);

            switch (v.ty)
            {
                case VALTYPE.intcon:
                case VALTYPE.doublecon:
                case VALTYPE.decimalcon:
                    return v;

                case VALTYPE.listcon:
                    return v;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_UnaryPlus, v);

            }

            return v;
        }

        /// <summary>
        /// + operator, support operator overloading in host object
        ///     add eventhandler
        ///     concatenate 2 lists
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator +(VAL v1, VAL v2)
        {

            if (v1.ty == VALTYPE.hostcon)
            {
                //Falut-Tolerance Design
                if (v2.IsAssociativeArray())
                {
                    HostValization.Val2Host(v2, v1.value);
                    return v1;
                }

                if (v1.value is EventInfo)
                {
                    if (v2.ty == VALTYPE.funccon)       //event handler
                    {
                        HostEvent hostEvent = new HostEvent(v1.value as EventInfo, v2);
                        return hostEvent.AddDelegateEventHandler();
                    }
                    else
                        throw new HostTypeException("{0} is not event handler of {1}", v2, v1.value);
                }

            }


            VAL v = VAL.Clone(v1);

            if (v1.ty == VALTYPE.listcon)
            {
                if (v2.ty == VALTYPE.listcon)
                    v.value = v1.List + v2.List;
            }

            else if (v2.ty == VALTYPE.listcon)
            {
                VAL t = VAL.Clone(v2);
                t.List.Insert(v);
                return t;
            }

            else if (v1.ty == VALTYPE.stringcon || v2.ty == VALTYPE.stringcon)
            {
                v.ty = VALTYPE.stringcon;
                v.value = v1.ToSimpleString() + v2.ToSimpleString();
            }
            else if (v1.ty == VALTYPE.hostcon)
            {
                return HostFunction.OperatorOverloading(Operator.op_Addition, v1, v2);
            }
            else
                switch (v1.ty)
                {
                    case VALTYPE.nullcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon:
                            case VALTYPE.doublecon:
                            case VALTYPE.decimalcon:
                                v.ty = v2.ty;
                                v.value = v2.value; 
                                break;

                        }
                        break;


                    case VALTYPE.intcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (int)(v1.value) + (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) + (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) + (decimal)(v2.value); break;
                        }
                        break;

                    case VALTYPE.doublecon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (double)(v1.value) + (int)(v2.value); break;
                            case VALTYPE.doublecon: v.value = (double)(v1.value) + (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (double)(v1.value) + (double)((decimal)(v2.value)); break;
                        }
                        break;


                    case VALTYPE.decimalcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (decimal)(v1.value) + (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty;  v.value = (double)((decimal)(v1.value)) + (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (decimal)(v1.value) +(decimal)(v2.value); break;
                        }
                        break;

                    default:
                        return new VAL();
                }

            return v;
        }

        /// <summary>
        ///  - operator, support operator overloading in host object
        ///     remove v2 elements from v1 if v1 is list
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator -(VAL v1, VAL v2)
        {

            if (v1.ty == VALTYPE.hostcon && v1.value is EventInfo)
            {
                if (v2.ty == VALTYPE.funccon)       //event handler
                {
                    HostEvent hostEvent = new HostEvent(v1.value as EventInfo, v2);
                    return hostEvent.RemoveDelegateEventHandler();
                }
                else
                    throw new HostTypeException("{0} is not event handler of {1}", v2, v1.value);
            }


            VAL v = VAL.Clone(v1);

            if (v1.ty == VALTYPE.listcon)
            {
                if (v2.ty == VALTYPE.listcon)
                    v.value = v1.List - v2.List;
                else
                    v.List.Remove(v2);
            }

            else if (v2.ty == VALTYPE.listcon)
            {
                v = VAL.Clone(v2);
                v.List.Insert(v1);
            }

            else if (v1.ty == VALTYPE.stringcon || v2.ty == VALTYPE.stringcon)
            {
                v1.ty = VALTYPE.stringcon;
                v1.value = v1.Str + v2.Str;
            }
            else if (v1.ty == VALTYPE.hostcon)
            {
                return HostFunction.OperatorOverloading(Operator.op_Subtraction, v1, v2);
            }
            else
                switch (v1.ty)
                {
                    case VALTYPE.nullcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon:
                                v.ty = v2.ty;
                                v.value = -(int)v2.value; break;
                            case VALTYPE.doublecon:
                                v.ty = v2.ty;
                                v.value = -(double)v2.value; break;
                            case VALTYPE.decimalcon:
                                v.ty = v2.ty;
                                v.value = -(decimal)v2.value; break;

                        }
                        break;

                    case VALTYPE.intcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (int)(v1.value) - (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) - (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) - (decimal)(v2.value); break;
                        }
                        break;

                    case VALTYPE.doublecon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (double)(v1.value) - (int)(v2.value); break;
                            case VALTYPE.doublecon: v.value = (double)(v1.value) - (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (double)(v1.value) - (double)((decimal)(v2.value)); break;
                        }
                        break;

                    case VALTYPE.decimalcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (decimal)(v1.value) - (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty;  v.value = (double)((decimal)(v1.value)) - (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (decimal)(v1.value) - (decimal)(v2.value); break;
                        }
                        break;
                    default:
                        return new VAL();
                }

            return v;
        }


        #endregion


        #region Operator v=exp1*exp2; v=exp1/exp2; v=exp1%exp2;

        /// <summary>
        /// * operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator *(VAL v1, VAL v2)
        {

            VAL v = VAL.Clone(v1);

            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (int)(v1.value) * (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) * (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) * (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (double)(v1.value) * (int)(v2.value); break;
                        case VALTYPE.doublecon: v.value = (double)(v1.value) * (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (double)(v1.value) * (double)((decimal)(v2.value)); break;
                    }
                    break;

                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (decimal)(v1.value) * (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (double)((decimal)(v1.value)) * (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (decimal)(v1.value) * (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.listcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.listcon: v.value = v1.List * v2.List; break;
                    }
                    break;

                case VALTYPE.stringcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: for (int i = 1; i < v2.Intcon; i++) v.Str += v1.Str; break;
                    }
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_Multiply, v1, v2);

                default:
                    return new VAL();
            }

            return v;
        }

        /// <summary>
        /// / operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator /(VAL v1, VAL v2)
        {

            VAL v = VAL.Clone(v1);

            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (int)(v1.value) / (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) / (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) / (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (double)(v1.value) / (int)(v2.value); break;
                        case VALTYPE.doublecon: v.value = (double)(v1.value) / (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (double)(v1.value) / (double)((decimal)(v2.value)); break;
                    }
                    break;

                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (decimal)(v1.value) / (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (double)((decimal)(v1.value)) / (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (decimal)(v1.value) / (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_Division, v1, v2);

                default:
                    return new VAL();
            }

            return v;
        }

        /// <summary>
        /// % operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator %(VAL v1, VAL v2)
        {
            VAL v = VAL.Clone(v1);
            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (int)(v1.value) % (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) % (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) % (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (double)(v1.value) % (int)(v2.value); break;
                        case VALTYPE.doublecon: v.value = (double)(v1.value) % (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (double)(v1.value) % (double)((decimal)(v2.value)); break;
                    }
                    break;

                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (decimal)(v1.value) % (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (double)((decimal)(v1.value)) % (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (decimal)(v1.value) % (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_Modulus, v1, v2);

                default:
                    return new VAL();
            }

            return v;
        }

        #endregion


        #region Operator  ==, >, <, <=, >= !=

        /// <summary>
        /// == operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(VAL v1, VAL v2)
        {
            if (v1.isNumber && v2.isNumber)
                return CompareNumber(v1, v2) == 0;

            if (v1.ty != v2.ty)
                return false;

            switch (v1.ty)
            {
                case VALTYPE.voidcon: return v2.ty == VALTYPE.voidcon;
                case VALTYPE.nullcon: return v2.ty == VALTYPE.nullcon;
                case VALTYPE.boolcon: return (bool)(v1.value) == (bool)(v2.value);
                case VALTYPE.listcon: return (VALL)(v1.value) == (VALL)(v2.value);

//                case VALTYPE.addrcon: return (int)(v1.value) == (int)(v2.value);
                case VALTYPE.stringcon: return (string)(v1.value) == (string)(v2.value);

                case VALTYPE.classcon:
                case VALTYPE.funccon:
                    return (int)(v1.value) == (int)(v2.value)
                        && v1.Class == v2.Class;

                case VALTYPE.hostcon:
                    return HostOperation.HostCompareTo(Operator.op_Equality, v1, v2) == 0;

            }
            return false;
        }

        /// <summary>
        /// > operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >(VAL v1, VAL v2)
        {
            if (v1.isNumber && v2.isNumber)
                return CompareNumber(v1, v2) > 0;
         

            if (v1.ty == v2.ty)
            {
                switch (v1.ty)
                {
                    case VALTYPE.boolcon: return (int)(v1.value) > (int)(v2.value);

                    //case VALTYPE.identcon:
                    case VALTYPE.stringcon:
                        return 0 > string.Compare((string)v1.value, (string)v2.value);
                    case VALTYPE.listcon:
                        return (VALL)(v1.value) > (VALL)(v2.value);
                    case VALTYPE.hostcon:
                        return HostOperation.HostCompareTo(Operator.op_GreaterThan, v1, v2) > 0;
                }
            }
            else if (v1.ty == VALTYPE.listcon)
            {
                if (v1.List.Contains(v2))
                    return true;

            } 

            return false;

        }


        /// <summary>
        /// Less than operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <(VAL v1, VAL v2)
        {
            if (v1.isNumber && v2.isNumber)
                return CompareNumber(v1, v2) < 0;
         
            if (v1.ty == v2.ty)
            {
                switch (v1.ty)
                {
                    case VALTYPE.boolcon: return (int)(v1.value) < (int)(v2.value);

                    //case VALTYPE.identcon:
                    case VALTYPE.stringcon:
                    case VALTYPE.funccon:
                        return 0 < String.Compare((String)v1.value, (String)v2.value);
                    case VALTYPE.listcon:
                        return (VALL)(v1.value) < (VALL)(v2.value);
                    case VALTYPE.hostcon:
                        return HostOperation.HostCompareTo(Operator.op_LessThan, v1, v2) < 0;
      
                }
            }
            else if (v2.ty == VALTYPE.listcon)
            {
                if (v2.List.Contains(v1))
                    return true;

            }
            return false;

        }


        /// <summary>
        /// >= operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >=(VAL v1, VAL v2)
        {
            return v1 == v2 || v1 > v2;
        }

        /// <summary>
        /// less and equal than operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <=(VAL v1, VAL v2)
        {
            return v1 == v2 || v1 < v2;
        }

        /// <summary>
        /// != operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(VAL v1, VAL v2)
        {
            return !(v1 == v2);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(Object o)
        {
            return this == (VAL)o;
        }

        /// <summary>
        /// Returns hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 0;
        }

        private bool isNumber
        {
            get
            {
                return ty == VALTYPE.intcon || ty == VALTYPE.doublecon || ty == VALTYPE.decimalcon;
            }
        }

        private static int CompareNumber(VAL v1, VAL v2)
        {
            object b = v2.value;
            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    {
                        int a = (int)(v1.value);
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: return a.CompareTo(b);
                            case VALTYPE.doublecon: return Convert.ToDouble(a).CompareTo(b);
                            case VALTYPE.decimalcon: return new decimal(a).CompareTo(b);
                        }
                    }
                    break;

                case VALTYPE.doublecon:
                    {
                        double a = (double)(v1.value);
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: return a.CompareTo(Convert.ToDouble(b));
                            case VALTYPE.doublecon: return a.CompareTo(b);
                            case VALTYPE.decimalcon: return new decimal(a).CompareTo(b);
                        }
                    }
                    break;

                case VALTYPE.decimalcon:
                    {
                        decimal a = (decimal)(v1.value);
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: return a.CompareTo(new decimal((int)b));
                            case VALTYPE.doublecon: return a.CompareTo(new decimal((double)b));
                            case VALTYPE.decimalcon: return a.CompareTo(b);
                        }
                    }
                    break;
            }

            throw new NotSupportedException();
        }

        #endregion

        #region Operator v = ~exp, v=exp1&exp2, v=exp1|exp2, v=exp1^exp2

        /// <summary>
        /// ~ operator
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static VAL operator ~(VAL v1)
        {
            
            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_OnesComplement, v1, null, true);

            if (v1.value is byte)
                return VAL.Boxing1(~(byte)v1.value);
            if (v1.value is sbyte)
                return VAL.Boxing1(~(sbyte)v1.value);
            if (v1.value is short)
                return VAL.Boxing1(~(short)v1.value);
            if (v1.value is ushort)
                return VAL.Boxing1(~(ushort)v1.value);
            if (v1.value is int)
                return VAL.Boxing1(~(int)v1.value);
            if (v1.value is uint)
                return VAL.Boxing1(~(uint)v1.value);
            if (v1.value is long)
                return VAL.Boxing1(~(long)v1.value);
            if (v1.value is ulong)
                return VAL.Boxing1(~(ulong)v1.value);

            throw new TieException("invalid operation from ~{0}", v1);
        }

        /// <summary>
        /// bitwise or operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator |(VAL v1, VAL v2)
        {
            if (v1.isEnum && v2.isEnum)
                return HostOperation.EnumOperation(v1, v2, (int)v1.value | (int)v2.value);
            
            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_BitwiseOr, v1, v2, true);

            if (v1.value.GetType() == v2.value.GetType())
            {
                if (v1.value is byte)
                    return VAL.Boxing1((byte)v1.value | (byte)v2.value);
                if (v1.value is sbyte)
                    return VAL.Boxing1((sbyte)v1.value | (sbyte)v2.value);
                if (v1.value is short)
                    return VAL.Boxing1((short)v1.value | (short)v2.value);
                if (v1.value is ushort)
                    return VAL.Boxing1((ushort)v1.value | (ushort)v2.value);
                if (v1.value is uint)
                    return VAL.Boxing1((uint)v1.value | (uint)v2.value);
                if (v1.value is long)
                    return VAL.Boxing1((long)v1.value | (long)v2.value);
                if (v1.value is ulong)
                    return VAL.Boxing1((ulong)v1.value | (ulong)v2.value);
            }
            throw new TieException("invalid operation from {0} | {1}", v1, v2);

        }

        /// <summary>
        /// bitwise and operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator &(VAL v1, VAL v2)
        {
            if (v1.isEnum && v2.isEnum)
                return HostOperation.EnumOperation(v1, v2, (int)v1.value & (int)v2.value);

            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_BitwiseAnd, v1, v2, true);

            if (v1.value.GetType() == v2.value.GetType())
            {
                if (v1.value is byte)
                    return VAL.Boxing1((byte)v1.value & (byte)v2.value);
                if (v1.value is sbyte)
                    return VAL.Boxing1((sbyte)v1.value & (sbyte)v2.value);
                if (v1.value is short)
                    return VAL.Boxing1((short)v1.value & (short)v2.value);
                if (v1.value is ushort)
                    return VAL.Boxing1((ushort)v1.value & (ushort)v2.value);
                if (v1.value is uint)
                    return VAL.Boxing1((uint)v1.value & (uint)v2.value);
                if (v1.value is long)
                    return VAL.Boxing1((long)v1.value & (long)v2.value);
                if (v1.value is ulong)
                    return VAL.Boxing1((ulong)v1.value & (ulong)v2.value);
            }

            throw new TieException("invalid operation from {0} & {1}", v1, v2);
            
        }

        /// <summary>
        /// bitwise xor operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator ^(VAL v1, VAL v2)
        {
            if (v1.isEnum && v2.isEnum)
                return HostOperation.EnumOperation(v1, v2, (int)v1.value ^ (int)v2.value);
        
            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_ExclusiveOr, v1, v2, true);

            if (v1.value.GetType() == v2.value.GetType())
            {
                if (v1.value is byte)
                    return VAL.Boxing1((byte)v1.value ^ (byte)v2.value);
                if (v1.value is sbyte)
                    return VAL.Boxing1((sbyte)v1.value ^ (sbyte)v2.value);
                if (v1.value is short)
                    return VAL.Boxing1((short)v1.value ^ (short)v2.value);
                if (v1.value is ushort)
                    return VAL.Boxing1((ushort)v1.value ^ (ushort)v2.value);
                if (v1.value is uint)
                    return VAL.Boxing1((uint)v1.value ^ (uint)v2.value);
                if (v1.value is long)
                    return VAL.Boxing1((long)v1.value ^ (long)v2.value);
                if (v1.value is ulong)
                    return VAL.Boxing1((ulong)v1.value ^ (ulong)v2.value);
            }

            throw new TieException("invalid operation from {0} ^ {1}", v1, v2);
        }

        private bool isBitwiseValue
        {
            get
            {
                return ty == VALTYPE.intcon || value is uint || value is long || value is ulong;
            }
        }

        private bool isEnum
        {
            get
            {
                return value is int || Type.IsEnum;
            }
        }

        #endregion



        #region v = exp1 << int, v=exp1>> int

        /// <summary>
        /// bitwise left shift operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator <<(VAL v1, int v2)
        {
            if (v1.value is byte)
                return VAL.Boxing1((byte)v1.value << v2);
            if (v1.value is sbyte)
                return VAL.Boxing1((sbyte)v1.value << v2);
            if (v1.value is short)
                return VAL.Boxing1((short)v1.value << v2);
            if (v1.value is ushort)
                return VAL.Boxing1((ushort)v1.value << v2);
            if (v1.value is int)
                return VAL.Boxing1((int)v1.value << v2);
            if (v1.value is uint)
                return VAL.Boxing1((uint)v1.value << v2);
            if (v1.value is long)
                return VAL.Boxing1((long)v1.value << v2);
            if (v1.value is ulong)
                return VAL.Boxing1((ulong)v1.value << v2);

            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_LeftShift, v1, new VAL(v2), true);

            throw new TieException("invalid operation from {0} << {1}", v1, v2);
        }

        /// <summary>
        /// bitwise right shift operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator >>(VAL v1, int v2)
        {
            if (v1.value is byte)
                return VAL.Boxing1((byte)v1.value >> v2);
            if (v1.value is sbyte)
                return VAL.Boxing1((sbyte)v1.value >> v2);
            if (v1.value is short)
                return VAL.Boxing1((short)v1.value >> v2);
            if (v1.value is ushort)
                return VAL.Boxing1((ushort)v1.value >> v2);
            if (v1.value is int)
                return VAL.Boxing1((int)v1.value >> v2);
            if (v1.value is uint)
                return VAL.Boxing1((uint)v1.value >> v2);
            if (v1.value is long)
                return VAL.Boxing1((long)v1.value >> v2);
            if (v1.value is ulong)
                return VAL.Boxing1((ulong)v1.value >> v2);


            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_RightShift, v1, new VAL(v2), true);

            throw new TieException("invalid operation from {0} >> {1}", v1, v2);
        }
        #endregion


        #region Operator []
        //for C# programming use
        /// <summary>
        /// returns i-th element of list
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public VAL this[int pos]
        {
            get
            {
                VAL arr = new VAL(pos);
                return getter(arr, false, OffsetType.ANY);
            }
            set
            {
                VAL arr = new VAL(pos);
                setter(arr, value, OffsetType.ARRAY);
            }
        }

        /// <summary>
        /// returns value by key in associative array
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VAL this[string key]
        {
            get
            {
                VAL arr = new VAL(key);
                return getter(arr, false, OffsetType.ANY);
            }
            set
            {
                VAL arr = new VAL(key);
                setter(arr, value, OffsetType.STRUCT);
            }
        }

        /// <summary>
        /// returns offset of value, either property or array of host object
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public VAL this[VAL arr]
        {
            get
            {
                return getter(arr, true, OffsetType.ANY);
            }
            set
            {
                setter(arr, value, OffsetType.ANY);
            }
        }


        internal VAL getter(VAL arr, bool created, OffsetType offsetType)
        {
            if (ty == VALTYPE.hostcon)
            {
                return HostOperation.HostTypeOffset(this, arr, offsetType);
            }

            if (ty == VALTYPE.stringcon && arr.VALTYPE == VALTYPE.intcon)
            {
                int pos = arr.Intcon;
                if (pos >= 0 && pos < this.Str.Length)
                    return new VAL(new String(this.Str[arr.Intcon], 1));
                else
                    return VAL.NewVoidType();
            }
            else if (ty != VALTYPE.listcon)
            {
                return VAL.NewVoidType();
            }

    
            return ((VALL)value).getter(arr, created);
        }

        private void setter(VAL arr, VAL value, OffsetType offsetType)
        {
            switch (arr.ty)
            {
                case VALTYPE.stringcon:
                    string key = arr.Str;
                    if (ty == VALTYPE.nullcon)
                    {
                        ty = VALTYPE.listcon;
                        this.value = new VALL();
                    }

                    if (ty == VALTYPE.hostcon)
                    {
                        this.temp = new HostOffset( this, key );
                        HostOperation.HostTypeAssign(this, value);
                        return;
                    }

                    if (ty != VALTYPE.listcon)
                    {
                        //ty = VALTYPE.listcon;
                        //this.value = new VALL();
                        return;
                    }

                    ((VALL)this.value)[key] = value;

                    return;

                case  VALTYPE.intcon :
                    int pos = arr.Intcon;

                    if (ty == VALTYPE.nullcon)
                    {
                        ty = VALTYPE.listcon;
                        this.value = new VALL();
                    }

                    if (ty != VALTYPE.listcon)
                        return;

                    ((VALL)this.value)[pos] = value;

                    return;

                case VALTYPE.listcon:
                    return;
            }
            
            return;
      }
    


        #endregion VAL Operator

     
        #region VAL properties

        /// <summary>
        /// returns string value
        /// </summary>
        public string Str
        {
            get
            {
                return (string)value;
            }
            set
            {
                ty = VALTYPE.stringcon;
                this.value = value;
            }
        }

        internal int Address
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = value;
            }

        }

        /// <summary>
        /// returns integet value
        /// </summary>
        public int Intcon
        {
            get
            {
                return (int)value;
            }
            set
            {
                ty = VALTYPE.intcon;
                this.value = value;
            }
        }


        /// <summary>
        /// returns double value
        /// </summary>
        public double Doublecon
        {
            get
            {
                return (double)value;
            }
            set
            {
                ty = VALTYPE.doublecon;
                this.value = value;
            }
        }

        /// <summary>
        /// returns decimal value
        /// </summary>
        public decimal Decimalcon
        {
            get
            {
                return (decimal)value;
            }
            set
            {
                ty = VALTYPE.decimalcon;
                this.value = value;
            }
        }

        /// <summary>
        /// returns boolean value
        /// </summary>
        public bool Boolcon
        {
            get
            {
                return (bool)value;
            }
            set
            {
                ty = VALTYPE.boolcon;
                this.value = value;
            }
        }

   
        internal VALL List
        {
            get
            {
                return (VALL)value;
            }

        }

        /// <summary>
        /// returns number of elements of list, if not list, returns -1
        /// </summary>
        public int Size
        {
            get
            {
                if (ty == VALTYPE.listcon)
                    return ((VALL)value).Size;
                else
                    return -1;
            }

        }

    
        /// <summary>
        /// value is defined
        /// </summary>
        public bool Defined        { get { return ty != VALTYPE.voidcon; }}

        /// <summary>
        /// value is not defined
        /// </summary>
        public bool Undefined      { get { return ty == VALTYPE.voidcon; }}

        /// <summary>
        /// is null?
        /// </summary>
        public bool IsNull         { get { return ty == VALTYPE.nullcon; }}

        /// <summary>
        /// is boolean?
        /// </summary>
        public bool IsBool         { get { return ty == VALTYPE.boolcon; }}

        /// <summary>
        /// is host type object?
        /// </summary>
        public bool IsHostType     { get { return ty == VALTYPE.hostcon; }}

        /// <summary>
        /// is integer?
        /// </summary>
        public bool IsInt          { get { return ty == VALTYPE.intcon;  }}

        /// <summary>
        /// is list value?
        /// </summary>
        public bool IsList         { get { return ty == VALTYPE.listcon; }}

        /// <summary>
        /// is double?
        /// </summary>
        public bool IsDouble       { get { return ty == VALTYPE.doublecon; }}

        /// <summary>
        /// is decimal?
        /// </summary>
        public bool IsDecimal      { get { return ty == VALTYPE.decimalcon; }}

        /// <summary>
        /// is user defined function?
        /// </summary>
        public bool IsFunction     { get { return ty == VALTYPE.funccon; }}

        /// <summary>
        /// is user defined class?
        /// </summary>
        public bool IsClass        { get { return ty == VALTYPE.classcon;}}

        /// <summary>
        /// is boolean and true?
        /// </summary>
        public bool IsTrue         { get { return ty == VALTYPE.boolcon && this.Boolcon; }}

        /// <summary>
        /// is boolean and false?
        /// </summary>
        public bool IsFalse        { get { return ty == VALTYPE.boolcon && !this.Boolcon; }}
        

 
        #endregion


        #region multiple dimensional array

        /// <summary>
        /// Make multiple dimension array
        /// </summary>
        /// <param name="args">ranks</param>
        /// <returns></returns>
        public static VAL Array(params int[] args)
        {
            if (args.Length == 0)
                return new VAL(new VALL());

            return makeArray(0, args);
        }

        private static VAL makeArray(int pos, int[] args)
        {

            if (pos == args.Length)
                return new VAL();

            int n = 0;
            try
            {
                n = (int)args[pos];
            }
            catch (Exception)
            {
                return makeArray(pos + 1, args);
            }

            VALL L = new VALL();
            for (int i = 0; i < n; i++)
                L.Add(makeArray(pos + 1, args));

            return new VAL(L);
        }

        #endregion

        #region Associative Array

        /// <summary>
        /// is associative array?
        /// </summary>
        /// <returns></returns>
        public bool IsAssociativeArray()
        {
            if (ty != VALTYPE.listcon)
                return false;

            return ((VALL)value).IsAssociativeArray();
        }


        /// <summary>
        /// Add element into list
        /// </summary>
        /// <param name="val"></param>
        public void Add(VAL val)
        {
            if (ty == VALTYPE.nullcon)
            {
                ty = VALTYPE.listcon;
                value = new VALL();
            }
                
            ((VALL)value).Add(val);

        }

        /// <summary>
        /// add a key value member to associative array
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
         /// <returns></returns>
        public VAL AddMember(string name, object obj)
        {
            AddMember(new Member(name, Boxing1(obj)));
            return this;
        }

        /// <summary>
        /// add a key value member to associative array
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal VAL AddMember(Member member)
        {
            if (ty == VALTYPE.nullcon)
            {
                ty = VALTYPE.listcon;
                value = new VALL();
            }

            ((VALL)value).Add(member.Name, member.Value);

            return this;
        }

        /// <summary>
        /// remove key value member from associative array
        /// </summary>
        /// <param name="name"></param>
        public void RemoveMember(string name)
        {
            if (ty != VALTYPE.listcon)
                return;

            ((VALL)value).Remove(name);
        }

        /// <summary>
        /// return associative array
        /// </summary>
        public IEnumerable<Member> Members
        {
            get
            {
                if (!IsAssociativeArray())
                    throw new TieException("associative array requried");

                return ((VALL)value).GetMembers();
            }
        }

        /// <summary>
        /// return default value when property is undefined, otherwise return this value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetMemeber<T>(string name, T defaultValue = default(T))
        {
            VAL val = this[name];
            if (val.Defined)
            {
                if (val.HostValue is T)
                    return (T)val.HostValue;
                else if (typeof(T).IsEnum && val.HostValue is int)
                    return (T)val.HostValue;
            }

            return defaultValue;
        }


        /// <summary>
        /// Remove null/void key-value-pair in associative array
        /// </summary>
        public void ClearNullorVoid()
        {
            if (!this.IsAssociativeArray())
                return;

            ClearNullorVoid(this);
        }

        private void ClearNullorVoid(VAL dict)
        {

            List<string> keys = new List<string>();
            foreach (VAL kvp in dict)
            {
                string key = kvp[0].Str;
                VAL value = kvp[1];

                if (value.IsAssociativeArray())
                {
                    ClearNullorVoid(value);
                }
                else
                {
                    if (value.Undefined || value.IsNull)
                    {
                        keys.Add(key);
                    }
                }
            }
            
            foreach (string key in keys)
                dict.RemoveMember(key);
        }

        /// <summary>
        /// returns object array
        /// </summary>
        public object[] ObjectArray
        {
            get
            {
                return ((VALL)value).ObjectArray;
            }
        }

        #endregion


        #region VAL ToString

       

        /// <summary>
        ///   Converts the value of this instance to a System.String. display all format
        ///   Print null, quotation mark, typeof
        /// </summary>
        /// <returns> A string whose value is the same as this instance.</returns>
        public override String ToString()
        {
            return encode(OutputType.QuotationMark | OutputType.Parentheses | OutputType.NullMark | OutputType.Typeof);
        }

        
        /// <summary>
        /// display to end user
        /// Print no null, no quotation mark, no typeof
        /// </summary>
        /// <returns></returns>
        public String ToSimpleString()
        {
            return encode(OutputType.Nothing);
        }

   
        /// <summary>
        /// returns instance string used for persistence purpose
        /// Print null, quotation mark, typeof and valized value
        /// </summary>
        public String Valor
        {
            get
            {
                return encode(OutputType.QuotationMark | OutputType.Parentheses | OutputType.NullMark | OutputType.Typeof | OutputType.Valization);
            }
        }

        /// <summary>
        /// return string with format
        /// ex:
        ///     L = {1,"a", null};
        ///     L.ToString("PN")=> {1,a,null}
        ///     L.ToString("QPN")     => {1,"a",null}
        ///     L.ToString("QPNT")    => {1,"a",null}.typeof(object[])
        ///     L.ToString("N")       => 1,a,null
        ///     L.ToString("")        => 1,a,
        /// </summary>
        /// <param name="format">Q:Quotation Mark |N: null | T: typeof | P: Parentheses</param>
        /// <returns></returns>
        public String ToString(string format)
        {
            string.Format("");
            OutputType ot = OutputType.Nothing;
            foreach (char f in format)
            {
                switch (f)
                {
                    case 'Q':
                        ot |= OutputType.QuotationMark;
                        break;

                    case 'N':
                        ot |= OutputType.NullMark;
                        break;

                    case 'V':
                        ot |= OutputType.Valization;
                        break;

                    case 'P':
                        ot |= OutputType.Parentheses;
                        break;

                    case 'T':
                        ot |= OutputType.Typeof;
                        break;

                    case 'H':
                        ot |= OutputType.Host;
                        break;
                }
            }

            if ((ot & OutputType.Typeof) == OutputType.Valization)
            {
                ot |= OutputType.Typeof;
            }

            if ((ot & OutputType.Typeof) == OutputType.Typeof || (ot & OutputType.Typeof) == OutputType.Host)
            {
                ot |= OutputType.Parentheses;
                ot |= OutputType.QuotationMark;
                ot |= OutputType.NullMark;
            }
            return encode(ot);
        }

        internal String encode(OutputType ot)
        {
            bool quotationMark = (ot & OutputType.QuotationMark) == OutputType.QuotationMark;
            bool nullMark = (ot & OutputType.NullMark) == OutputType.NullMark;
            bool persistent = (ot & OutputType.Valization) == OutputType.Valization;
            bool hostMark = (ot & OutputType.Host) == OutputType.Host; 

            StringWriter o = new StringWriter();
            string s;

            switch (ty)
            {
                case VALTYPE.voidcon:
                    o.Write("void");
                    break;

                case VALTYPE.nullcon:
                    if (nullMark)
                        o.Write("null");
                    break;

                case VALTYPE.intcon:
                    if (value is int)
                        o.Write(value);
                    else if (persistent || hostMark)
                    {
                        if (value is byte)
                            return string.Format("(byte){0}", value);
                        if (value is short)
                            return string.Format("(short){0}", value);
                        if (value is long)
                            return string.Format("(long){0}", value);
                    }
                    break;

                case VALTYPE.decimalcon:
                case VALTYPE.doublecon:
                    if (persistent)
                    {
                        if (value is decimal)
                            return string.Format("(decimal){0}", value);
                        if (value is float)
                            return string.Format("(float){0}", value);
                    }
                    else if (hostMark)
                    {
                        if (value is decimal)
                            return string.Format("{0}M", value);
                        if (value is float)
                            return string.Format("{0}F", value);
                    } 
                    
                    o.Write(value);
                    if (value is double)
                    {
                        if (Math.Ceiling((double)value) == (double)value)
                            o.Write(".0");
                    }
                    
                    break;

                case VALTYPE.boolcon: o.Write("{0}", (bool)value ? "true" : "false"); break;


                case VALTYPE.listcon:
                    s = ((VALL)value).encode(ot);
                    if (Class == null)
                        o.Write(s);
                    else if ((ot & OutputType.Typeof) == OutputType.Typeof)
                        o.Write("{0}{1}", s, encodetypeof());
                    else if (hostMark)
                        o.Write("new {1}{0}", s, Class);
                    else
                        o.Write(s);

                    break;

                case VALTYPE.hostcon:  // .NET object
                    o.Write(HostCoding.Encode(this.value, persistent));
                    break;

                case VALTYPE.enumcon:
                    o.Write(HostOperation.EnumBitFlags(this.value, ot));
                    break;

                case VALTYPE.stringcon:
                case VALTYPE.scriptcon:      //唯一作用是不输出quotation mark ""
                    if (value is char)
                    {
                        o.Write("'{0}'", value);
                        break;
                    }
                    if (ty == VALTYPE.stringcon && quotationMark)
                        o.Write("\"");

                    s = (string)value;
                    for (int i = 0; i < s.Length; i++)
                    {
                        switch (s[i])
                        {
                            case '"':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\\"" : "\"");
                                break;

                            case '\\':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\\\" : "\\");
                                break;

                            case '\n':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\n" : "\n");
                                break;

                            case '\r':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\r" : "\r");
                                break;

                            case '\t':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\t" : "\t");
                                break;

                            default:
                                o.Write(s[i]);
                                break;
                        }

                    }

                    if (ty == VALTYPE.stringcon && quotationMark)
                        o.Write("\"");
                    break;

                case VALTYPE.funccon:
                    o.Write("{0}({1},{2})", Constant.FUNC_FUNCTION, new VAL(Class), VAL.Boxing1(value));
                    if (persistent)
                    {
                        //VAL v = Module.encode(Library.GetModule(Class));
                    }
                    break;
                case VALTYPE.classcon:
                    o.Write("{0}({1},{2})", Constant.FUNC_CLASS, new VAL(Class), VAL.Boxing1(value));
                    if (persistent)
                    {
                        //o.Write(Library.GetModule(Class));
                    }
                    break;

                case VALTYPE.identcon:
                    o.Write(value);
                    break;

                default:
                    o.Write("(ty={0} value={1})", ty, value);
                    break;
          
            }
            return o.ToString();
        }

        internal string encodetypeof()
        {
            Type typ = HostType.GetType(Class);
            if (typ != null)
                return string.Format(".typeof({0})", new GenericType(typ).TypeName);
            else
                return string.Format(".typeof(\"{0}\")", Class);
        }

        #endregion


        #region ToJson, ToXml

        //内部类似JSON或者Javascript格式的,支持HostType对象的类型输出
        /// <summary>
        /// convert instance to JSON
        /// </summary>
        /// <returns></returns>
        public string ToExJson()
        {
            return ToJson("", OutputType.Typeof | OutputType.WellFormatted);
        }

        //输出经典的JSON
        /// <summary>
        /// convert instance to JSON
        /// </summary>
        /// <param name="tag">root tag</param>
        /// <returns>compact JSON string</returns>
        public string ToJson(string tag)
        {
            return ToJson(tag, OutputType.QuotationMark);
        }

        /// <summary>
        /// convert instance to JSON
        /// </summary>
        /// <returns>well-formatted JSON string</returns>
        public string ToJson()
        {
            return ToJson("", OutputType.QuotationMark | OutputType.WellFormatted);
        }

        internal string ToJson(string tag, OutputType fmt)
        {
            return Serializer.ToJson(this, tag, fmt);
        }

        /// <summary>
        /// Convert instance to XML
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string ToXml(string tag)
        {
            return Serializer.ToXml(this, tag, OutputType.WellFormatted); 
        }

       

        #endregion


        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates
        /// </summary>
        /// <returns></returns>
        public IEnumerator<VAL> GetEnumerator()
        {
            if (ty != VALTYPE.listcon)
                throw new TieException(string.Format("VAL {0} is not list.", this));

            return this.List.InternalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (ty != VALTYPE.listcon)
                throw new TieException(string.Format("VAL {0} is not list.", this));

            return this.List.InternalList.GetEnumerator();
        }

     
        #endregion


        #region property HostValue, Type
        
        /// <summary>
        /// returns Host value
        /// </summary>
        public object HostValue
        {
            get
            {
                if (ty == VALTYPE.nullcon)
                    return null;

                if (ty == VALTYPE.funccon)      //用于把Tie函数作为Host(C#.net)的delegate, 
                    return this;                //参照: HostOperation.IsCompatibleType(Type type, ref object val)

                if (ty != VALTYPE.listcon)
                    return this.value;

                if (Class == "VAL")     //cast to VAL, e.g. function VAL(..), skip HostValue unboxing if val.Class=="VAL"
                    return this;

                if (Class != null)      //如果是List类型,那么可以用Class给list指定类型
                {
                    Type type = HostType.GetType(Class);
                    if (type != null)
                        this.List.ty = type;
                }

                return this.List.HostValue;
            }
        }

        internal Type Type
        {
            get
            {
                if (ty == VALTYPE.nullcon)
                {
                    if (value is Type)
                        return (Type)value;     //如果VAL为nullcon,用value来保存null的Type
                    else
                        return typeof(object);
                }
                else if (ty == VALTYPE.voidcon)
                {
                    return typeof(object);
                }

                if (ty == VALTYPE.listcon && List.ty != null)
                    return List.ty;

                return HostValue.GetType();
            }

            set
            {
                if (ty == VALTYPE.nullcon)          //如果VAL为nullcon, 那么用value保存VAL的Type,在强制类型转换时候可以使用
                    this.value = value;

                else if (ty == VALTYPE.listcon)
                {
                    List.ty = value;
                    this.Class = value.FullName;
                }
            }
        }

        #endregion


        #region CAST
        private static T cast<T>(VAL val)
        {
            VAL result = cast(val, typeof(T));
            return (T)result.value;
        }

        internal static VAL cast(VAL val, Type type)
        {

            object value = val.value;
            if (val.ty == VALTYPE.nullcon)
            {
                /*
                 * 对于null的类型转换
                 *   譬如C#中 
                 *   S =(string[])null;  
                 *   A = (int)null;
                 *   
                 *  那么类型type保存在val.value中,
                 *     参照: HostOperation.HostTypeFunction(object host, string func, VALL parameters)
                 *     在查找.NET method时候会使用(Type)val.value 作为参数类型的.
                 *  
                 ***/
                val.Type = type;
                val.Class = type.UnderlyingSystemType.FullName;
                return val;
            }
          
            else if (val.ty == VALTYPE.listcon && type.IsArray)
            {
                //数组
                val.List.ty = type;
                val.Class = type.FullName;
                return val;
            }
            else if (val.ty == VALTYPE.hostcon)
            {
                //操作符重载 explicit operator 
                string func = Operator.op_Explicit.ToString();
                MethodInfo method = HostFunction.methodof(value, type, func, new Type[] { value.GetType() });
                if (method != null)
                {
                    VALL L = new VALL();
                    L.Add(val);

                    HostFunction hFunc = new HostFunction(value, func, L);
                    return hFunc.RunFunction(new MethodInfo[] { method });
                }
            }

            if (type == typeof(object))
                return val;

            //如果不支持Convert
            if (!(value is IConvertible))
                throw new TieException("cannot cast {0} to Type [{1}]", value, type.FullName);

            //布尔类型
            if (type == typeof(bool))
            {
                val.ty = VALTYPE.boolcon;
                val.value = Convert.ToBoolean(value);
            }


            //整数 8 位
            else if (type == typeof(byte))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToByte(value);
            }
            else if (type == typeof(sbyte))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToSByte(value);
            }
            //整数 16 位
            else if (type == typeof(short))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToInt16(value);
            }
            else if (type == typeof(ushort))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToUInt16(value);
            }
            //整数32位
            else if (type == typeof(int))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToInt32(value);
            }
            else if (type == typeof(uint))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToUInt32(value);
            }
            //整数64位
            else if (type == typeof(long))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToInt64(value);
            }
            else if (type == typeof(ulong))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToUInt64(value);
            }


            //浮点数
            else if (type == typeof(float))
            {
                val.ty = VALTYPE.doublecon;
                val.value = Convert.ToSingle(value);
            }
            else if (type == typeof(double))
            {
                val.ty = VALTYPE.doublecon;
                val.value = Convert.ToDouble(value);
            }
            else if (type == typeof(decimal))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToDecimal(value);
            }


            //字符串
            else if (type == typeof(char))
            {
                val.ty = VALTYPE.stringcon;
                val.value = Convert.ToChar(value);
            }
            else if (type == typeof(string))
            {
                val.ty = VALTYPE.stringcon;
                val.value = Convert.ToString(value);
            }


            //日期类型
            else if (type == typeof(DateTime))
                val.value = Convert.ToDateTime(value);
       

#if !SILVERLIGHT
            else
                val.value = Convert.ChangeType(value, type);
#endif

            return val;
        }

        #endregion


        #region 基本类型 implicit/explict bool/string/int/double/decimal, (object[])/(Memory)

#if IMPLICIT
        /*
         * 用法:
         * 
         *  VAL b1 = true;
         *  等价于: VAL b1 = new VAL(true);
         * 
         **/
        public static implicit operator VAL(bool x)
        {
            return new VAL(x);
        }
        public static implicit operator VAL(string x)
        {
            return new VAL(x);
        }
        public static implicit operator VAL(int x)
        {
            return new VAL(x);
        }

        public static implicit operator VAL(double x)
        {
            return new VAL(x);
        }

        public static implicit operator VAL(decimal x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(Array x)
        {
            return VAL.Boxing(x);
        }
#endif

        /*
         * 用法:
         * 
         *  VAL b1 = new VAL(true);
         *   bool b2 = (bool)b1;
         * 
         **/
        /// <summary>
        /// explicit convert to boolean
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator bool(VAL x)
        {
            //if (x.ty != VALTYPE.boolcon) throw new InvalidCastException();
            //return (bool)x.value;
            return cast<bool>(x);
        }

        /// <summary>
        /// explicit convert to string
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator string(VAL x)
        {
            //if (x.ty != VALTYPE.stringcon) throw new InvalidCastException();
            //return (string)x.value;
            return cast<string>(x);
        }

        /// <summary>
        /// explicit convert to int
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator int(VAL x)
        {
            //if (x.ty != VALTYPE.intcon) throw new InvalidCastException();
            //return (int)x.value;
            return cast<int>(x);
        }

        /// <summary>
        /// explicit convert to double
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator double(VAL x)
        {
            //if (x.ty != VALTYPE.doublecon) throw new InvalidCastException();
            //return (double)x.value;
            return cast<double>(x);
        }

        /// <summary>
        /// explicit convert to decimal
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator decimal(VAL x)
        {
            //if (x.ty != VALTYPE.decimalcon) throw new InvalidCastException();
            //return (decimal)x.value;
            return cast<decimal>(x);
        }

       /// <summary>
        /// explicit convert to object array
       /// </summary>
       /// <param name="x"></param>
       /// <returns></returns>
        public static explicit operator object[](VAL x)
        {
            if (x.ty != VALTYPE.listcon) throw new InvalidCastException();

            return x.ObjectArray;
        }

        /// <summary>
        /// explicit convert to varible dictionary
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator Memory(VAL x)
        {
            return new Memory(x);
        }

        #endregion


   
        #region 扩展类型 implicit/explict  DateTime/DBNull,  byte/sbyte/short/ushort/uint/float/char/long/ulong

    
#if IMPLICIT
        public static implicit operator VAL(char x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(byte x)
        {
            return VAL.Boxing1(x);
        }
        public static implicit operator VAL(sbyte x)
        {
            return VAL.Boxing1(x);
        }


        public static implicit operator VAL(short x)
        {
            return VAL.Boxing1(x);
        }
        public static implicit operator VAL(ushort x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(uint x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(long x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(ulong x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(float x)
        {
            return VAL.Boxing1(x);
        }

     

        public static implicit operator VAL(DateTime x)
        {
            return VAL.NewHostType(x);
        }

        public static implicit operator VAL(DBNull x)
        {
            return VAL.Boxing1(DBNull.Value);
        }
        //-------------------------------------------------------------
#endif

     
        /// <summary>
        /// explicit convert to char
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator char(VAL x)
        {
            return cast<char>(x);
        }

        /// <summary>
        /// explicit convert to byte
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator byte(VAL x)
        {
            return cast<byte>(x);
        }

        /// <summary>
        /// explicit convert to sbyte
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator sbyte(VAL x)
        {
            return cast<sbyte>(x);
        }

        /// <summary>
        /// explicit convert to short
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator short(VAL x)
        {
            return cast<short>(x);
        }

        /// <summary>
        /// explicit convert to ushort
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator ushort(VAL x)
        {
            return cast<ushort>(x);
        }

        /// <summary>
        /// explicit convert to uint
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator uint(VAL x)
        {
            return cast<uint>(x);
        }

        /// <summary>
        /// explicit convert to long
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator long(VAL x)
        {
            return cast<long>(x);
        }

        /// <summary>
        /// explicit convert to ulong
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator ulong(VAL x)
        {
            return cast<ulong>(x);
        }

        /// <summary>
        /// explicit convert to float
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator float(VAL x)
        {
            return cast<float>(x);
        }

        /// <summary>
        /// explicit convert to DateTime
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator DateTime(VAL x)
        {
            return cast<DateTime>(x);
        }

        /// <summary>
        /// explicit convert to System.DBNull
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator DBNull(VAL x)
        {
            if (x.ty == VALTYPE.nullcon || x.value is DBNull)
                return System.DBNull.Value;
            else
                throw new TieException("cannot cast value {0} to System.DBNull", x);
        }

      

        #endregion


        /// <summary>
        /// Assign("A.B.C.D", new VAL(0))
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="val"></param>
        internal void Assign(string ns,  VAL val)
        {
            string[] names = ns.Split(new char[] { '.' });
            Assign(this, names, 0, val);
        }

        internal static void Assign(VAL dict, string[] names, int index, VAL val)
        {
            string name = names[index];

            if (index == names.Length - 1)
            {
                dict[name] = val;
            }
            else
            {
                VAL item = dict[name];
                if (item.Undefined)
                {
                    item = new VAL(new VALL());
                    dict[name] = item;
                }
                else
                    item = dict[name];

                Assign(item, names, index + 1, val);
            }
        }

        /// <summary>
        /// return variable name assigned to this value
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.name;
        }


        
        
      
    }


}
