using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Tie;

namespace UnitTest
{
    static class HostTypeHelper
    {
        public static void Start()
        {
            Script.FunctionChain.Add(functions);  
            RegisterValizableClass();
        }



        private static VAL functions(string func, VAL parameters, Memory DS)
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


        private static void RegisterValizableClass()
        {

            HostType.Register(typeof(System.Drawing.Size), delegate(object host)
            {
                Size size = (Size)host;
                return new VAL(string.Format("new System.Drawing.Size({0},{1})", size.Width, size.Height));
            });


            
            HostType.Register(typeof(System.Drawing.Point), delegate(object host)
            {
                Point point = (Point)host;
                return new VAL(string.Format("new System.Drawing.Point({0},{1})", point.X, point.Y));
            });


#if VERSION1
            //1: System.Drawing.Color.Red
            //2: System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))))
            HostType.Register(typeof(Color),
                @"this.GetType().FullName + '.' 
                        + (
                        (this.Name.substring(0,1)!='0' && this.Name.substring(0,1)!='f') 
                        ?this.Name
                        :format('FromArgb({0},{1},{2})',this.R,this.G, this.B)
                        )");

#else
            HostType.Register(typeof(Color), delegate(object host)
            {
                Color color = (Color)host;

                if (color.Name.Substring(0, 1) != "0" && color.Name.Substring(0, 1) != "f")
                    return new VAL(host.GetType().FullName + '.' + color.Name);

                return new VAL(string.Format("System.Drawing.Color.FromArgb({0},{1},{2})", color.R, color.G, color.B));
            });
#endif






            //new System.Drawing.Font("MS Gothic", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)))
            HostType.Register(typeof(System.Drawing.FontStyle));
            HostType.Register(typeof(System.Drawing.GraphicsUnit));

#if VERSION1
            HostType.Register(typeof(Font), @"
                $style= 'System.Drawing.FontStyle.Regular';
                if((this.Style & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
                    $style += '| System.Drawing.FontStyle.Bold';
                if((this.Style & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
                    $style += '| System.Drawing.FontStyle.Italic';
                
                return format('new {0}(""{1}"",(float){2},{3},System.Drawing.GraphicsUnit.{4},(byte)0)', this.GetType().FullName, this.Name, this.Size, $style, this.Unit);

            ");
#else

            HostType.Register(typeof(Font), @"
                format('new {0}(""{1}"",(float){2},{3},{4},(byte)0)', 
                    this.GetType().FullName, this.Name, this.Size, this.Style.valize(), this.Unit.valize())
            ");
#endif



  

#if VERSION2            
      


            HostType.Register(typeof(System.Windows.Forms.TextBox), delegate(object host)
            {
                System.Windows.Forms.TextBox control = (System.Windows.Forms.TextBox)host;

                string p = string.Format("Text:'{1}', ReadOnly:{2}", control.Text, control.ReadOnly);
                return string.Format("(new {0}()).classof({1})", host.GetType().FullName, "{"+p+"}" );
            });
#endif


            HostType.Register(typeof(Rectangle), delegate(object host)
            {
                Rectangle rect = (Rectangle)host;
                VAL val = VAL.Boxing(new int[] { rect.X, rect.Y, rect.Width, rect.Height });
                return val;
            },
            delegate(VAL val)
            {
                return new Rectangle(val[0].Intcon, val[1].Intcon, val[2].Intcon, val[3].Intcon);
            }
            );

            HostType.Register(typeof(Guid), delegate(object host)
            {
                Guid guid = (Guid)host;
                byte[] bytes = guid.ToByteArray();
                return new VAL("\""+HostType.ByteArrayToHexString(bytes)+"\"");     //because this is a string, need quotation marks ""
            },
            delegate(VAL val)
            {
                byte[] bytes = HostType.HexStringToByteArray(val.Str); 
                return new Guid(bytes);
            }
            );

        }

    }
}
