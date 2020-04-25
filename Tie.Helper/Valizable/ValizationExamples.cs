using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

#if NETFRAMEWORK
namespace Tie.Helper
{
    public class ValizationExamples
    {
        public static void RegisterDemo()
        {
            Valizer.Register<Size>(delegate(Size size)
            {
                return new VAL(string.Format("new {0}({1},{2})", typeof(Size).FullName, size.Width, size.Height));
            }
             );



            Valizer.Register<Point>(delegate(Point point)
            {
                return new VAL(string.Format("new {0}({1},{2})", typeof(Point).FullName, point.X, point.Y));
            }
            );


#if !VERSION1
            //1: System.Drawing.Color.Red
            //2: System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))))
            Valizer.Register<Color>(
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

            Valizer.Register<Font>(@"
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


            Valizer.Register<Rectangle>(
                delegate(Rectangle rect)
                {
                    VAL val = VAL.Boxing(new int[] { rect.X, rect.Y, rect.Width, rect.Height });
                    return val;
                },
                delegate(VAL val)
                {
                    return new Rectangle(val[0].Intcon, val[1].Intcon, val[2].Intcon, val[3].Intcon);
                }
            );

        }
    }
}

#endif