using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Tie;

namespace Tie.Helper
{
    static class PrimitiveType
    {
        internal static VAL functions(string func, VAL parameters, Memory DS)
        {
            int size = parameters.Size;
            VAL L0 = size > 0 ? parameters[0] : null;
            VAL L1 = size > 1 ? parameters[1] : null;
            VAL L2 = size > 2 ? parameters[2] : null;

            switch (func)
            {
                //VALUE CAST
                case "char":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.stringcon;
                        L0.value = System.Convert.ToChar(L0.value);
                        return L0;
                    }
                    break;

                case "float":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.doublecon;
                        L0.value = System.Convert.ToSingle(L0.value);
                        return L0;
                    }
                    break;

                case "decimal":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.doublecon;
                        L0.value = System.Convert.ToDecimal(L0.value);
                        return L0;
                    }
                    break;

                case "double":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.doublecon;
                        L0.value = System.Convert.ToDouble(L0.value);
                        return L0;
                    }
                    break;

                case "byte":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.intcon;
                        L0.value = System.Convert.ToByte(L0.value);
                        return L0;
                    }
                    break;

                case "int":
                case "int32":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.intcon;
                        L0.value = System.Convert.ToInt32(L0.value);
                        return L0;
                    }
                    break;

                case "int16":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.intcon;
                        L0.value = System.Convert.ToInt16(L0.value);
                        return L0;
                    }
                    break;

                case "int64":
                    if (size == 1)
                    {
                        L0.ty = VALTYPE.intcon;
                        L0.value = System.Convert.ToInt64(L0.value);
                        return L0;
                    }
                    break;

                case "object":
                    if (size == 1 && L0.ty == VALTYPE.listcon)         //强制变为object[] 数组
                        return VAL.NewHostType(L0.ObjectArray);
                    else
                        break;

                case "lowercase":
                    if (size == 1)
                        return new VAL(L0.ToString2().ToLower());
                    break;
                case "uppercase":
                    if (size == 1)
                        return new VAL(L0.ToString2().ToUpper());
                    break;

                case "substring":
                    if (size == 2 && L0.ty == VALTYPE.stringcon && L1.ty == VALTYPE.intcon)
                        return new VAL(L0.ToString2().Substring(L1.Intcon));
                    if (size == 3 && L0.ty == VALTYPE.stringcon && L1.ty == VALTYPE.intcon && L2.ty == VALTYPE.intcon)
                        return new VAL(L0.ToString2().Substring(L1.Intcon, L2.Intcon));
                    break;

                case "Date":
                    if (size == 3)
                        return VAL.NewHostType(new DateTime(L0.Intcon, L1.Intcon, L2.Intcon));
                    break;
            }
            
            return null;
        }


      
    }
}
