using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Tie;

namespace Tie.Helper
{
    static class Serialization
    {
        public static void Register()
        {

            Serializer.Register<byte[]>(
                 delegate(byte[] bytes)
                 {
                     return new VAL("\"" + HostType.ByteArrayToHexString(bytes) + "\"");     //because this is a string, need quotation marks ""
                 },
                 delegate(VAL val)
                 {
                     byte[] bytes = HostType.HexStringToByteArray(val.Str);
                     return bytes;
                 }
            );

            Serializer.Register<Size>(delegate(Size size)
                {
                    return new VAL(string.Format("new System.Drawing.Size({0},{1})", size.Width, size.Height));
                }
            );



            Serializer.Register<Point>(delegate(Point point)
                {
                    return new VAL(string.Format("new System.Drawing.Point({0},{1})", point.X, point.Y));
                }
            );


#if !VERSION1
            //1: System.Drawing.Color.Red
            //2: System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))))
            Serializer.Register<Color>(
                @"this.GetType().FullName + '.' 
                        + (
                        (this.Name.substring(0,1)!='0' && this.Name.substring(0,1)!='f') 
                        ?this.Name
                        :format('FromArgb({0},{1},{2})',this.R,this.G, this.B)
                        )");

#else
            HostType.Register<Color>(delegate(Color color)
            {
                if (color.Name.Substring(0, 1) != "0" && color.Name.Substring(0, 1) != "f")
                    return new VAL(color.GetType().FullName + '.' + color.Name);

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

            Serializer.Register<Font>(@"
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


            Serializer.Register<Rectangle>(delegate(Rectangle rect)
                {
                    VAL val = VAL.Boxing(new int[] { rect.X, rect.Y, rect.Width, rect.Height });
                    return val;
                },
                delegate(VAL val)
                {
                    return new Rectangle(val[0].Intcon, val[1].Intcon, val[2].Intcon, val[3].Intcon);
                }
            );

            Serializer.Register<Guid>(delegate(Guid guid)
                {
                    byte[] bytes = guid.ToByteArray();
                    return new VAL("\""+HostType.ByteArrayToHexString(bytes)+"\"");     //because this is a string, need quotation marks ""
                },
                delegate(VAL val)
                {
                    byte[] bytes = HostType.HexStringToByteArray(val.Str); 
                    return new Guid(bytes);
                }
            );


            Serializer.Register(typeof(List<>), typeof(Serialization)
                     .GetMethod("RegisterList",
                     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
             ));


            Serializer.Register(typeof(Dictionary<,>), typeof(Serialization)
                      .GetMethod("RegistrDictionary",
                      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
              ));

        }


        private static void RegisterList<T>()
        {
            Serializer.Register<List<T>>(
                   host =>
                   {
                       var val = new VAL();
                       foreach (var item in host)
                       {
                           val.Add(Serializer.Serialize(item));
                       }
                       return val;
                   },
                   val =>
                   {
                       var list = new List<T>();
                       foreach (var item in val)
                       {
                           list.Add(Serializer.Deserialize<T>(item));
                       }
                       return list;
                   }
               );
        }

        private static void RegistrDictionary<T1, T2>()
        {
            Serializer.Register<Dictionary<T1, T2>>(
                   host =>
                   {
                       var val = new VAL();
                       foreach (var kvp in host)
                       {
                           VAL assoc = new VAL();
                           assoc.Add(Serializer.Serialize(kvp.Key));
                           assoc.Add(Serializer.Serialize(kvp.Value));
                           val.Add(assoc);
                       }

                       return val;
                   },
                   val =>
                   {
                       var dict = new Dictionary<T1, T2>();
                       foreach (var assoc in val)
                       {
                           dict.Add(
                               Serializer.Deserialize<T1>(assoc[0]), 
                               Serializer.Deserialize<T2>(assoc[1]));
                       }
                       return dict;
                   }
                   );
        }
    }
}
