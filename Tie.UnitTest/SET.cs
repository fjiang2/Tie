////--------------------------------------------------------------------------------------------------//
////                                                                                                  //
////        Tie                                                                                       //
////                                                                                                  //
////          Copyright(c) Datum Connect Inc.                                                         //
////                                                                                                  //
//// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
//// copy of the license can be found in the License.html file at the root of this distribution. If   //
//// you cannot locate the  Datum Connect Software License, please send an email to                   //
//// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
//// by the terms of the Datum Connect Software License.                                              //
////                                                                                                  //
//// You must not remove this notice, or any other, from this software.                               //
////                                                                                                  //
////                                                                                                  //
////--------------------------------------------------------------------------------------------------//
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Tie
//{
//    class SET
//    {
//        public static bool Membership(VAL a, VAL A)
//        {
//            if (A.ty != VALTYPE.listcon)
//                return false;

//            for (int i = 0; i < A.Size; i++)
//                if (a == A[i])
//                    return true;

//            return false;

//        }

//        public static int Cardinality(VAL A)
//        {
//            if (A.ty == VALTYPE.listcon)
//                return A.Size;

//            return -1;
//        }

//        public static VAL EmptySet()
//        {
//            return VAL.Array();
//        }

//        // A is subset of B, A<=B
//        public static bool Subset(VAL A, VAL B)
//        {
//            if (A.ty != VALTYPE.listcon || B.ty != VALTYPE.listcon)
//                return false;

//            for (int i = 0; i < A.Size; i++)
//                if (!Membership(A[i], B))
//                    return false;

//            return true;
//        }


//        //A+B
//        public static VAL Union(VAL A, VAL B)
//        {
//            if (A.ty != VALTYPE.listcon || B.ty != VALTYPE.listcon)
//                return new VAL();

//            VAL C = VAL.Array();
//            for (int i = 0; i < B.Size; i++)
//                if (!Membership(B[i], A))
//                    C.Add(B[i]);

//            return C;
//        }


//        //A*B
//        public static VAL Intersection(VAL A, VAL B)
//        {

//            if (A.ty != VALTYPE.listcon || B.ty != VALTYPE.listcon)
//                return new VAL();

//            VALL L = new VALL();
//            for (int i = 0; i < A.Size; i++)
//                if (Membership(A[i], B))
//                    L.Add(A[i]);

//            return new VAL(L);
//        }

//        //A-B
//        public static VAL Complement(VAL A, VAL B)
//        {

//            if (A.ty != VALTYPE.listcon || B.ty != VALTYPE.listcon)
//                return new VAL();

//            VALL L = new VALL();
//            for (int i = 0; i < A.Size; i++)
//                if (!Membership(A[i], B))
//                    L.Add(A[i]);

//            return new VAL(L);
//        }

//        public static bool Equal(VAL A, VAL B)
//        {
//            return Subset(A, B) && Subset(B, A);
//        }

//        public static bool IsEmpty(VAL A)
//        {
//            if (A.ty != VALTYPE.listcon)
//                return false;

//            return A.Size == 0;
//        }
//    }
//}
