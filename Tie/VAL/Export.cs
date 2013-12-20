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
using System.Text;
using System.IO;

namespace Tie
{
    [Flags]
    enum ExportFormat
    {
        QuestionMark = 0x01,        //write question mark
        WellFormatted = 0x02,       //well-formatted, indent and new line
        EncodeTypeof = 0x04         //write typeof on the class
    }

    class Export
    {
        public static string ToXml(VAL val, string tag, ExportFormat fmt)
        {
            return ToXML(val, tag, 0, fmt);
        }


        private static string ToXML(VAL val, string tag, int tab, ExportFormat fmt)
        {

            //bool quotationMark = (fmt & ExportFormat.QuestionMark) == ExportFormat.QuestionMark;
            bool formatted = (fmt & ExportFormat.WellFormatted) == ExportFormat.WellFormatted;
            //bool encodeTypeof = (fmt & ExportFormat.EncodeTypeof) == ExportFormat.EncodeTypeof;

            StringWriter o = new StringWriter();
            if (val.IsAssociativeArray())
            {
                o.Write(Indent(tab, formatted)); 
                o.Write("<" + tag + ">");
                if (formatted) o.WriteLine();
                
                for (int i = 0; i < val.Size; i++)
                {
                    VAL v = val[i];
                    o.Write(ToXML(v[1], v[0].Str, tab + 1, fmt));
                }
                
                o.Write(Indent(tab, formatted)); 
                o.Write("</" + tag + ">");
                if (formatted) o.WriteLine();
            }
            else if (val.ty == VALTYPE.listcon)
            {
                for (int j = 0; j < val.Size; j++)
                {
                    VAL v = val[j];
                    o.Write(ToXML(v, tag, tab + 1, fmt));
                }
            }
            else
            {
                o.Write(Indent(tab, formatted)); o.Write("<" + tag + ">"); 
                o.Write(XmlString(val.ToString2())); 
                o.Write("</" + tag + ">");
                if (formatted) o.WriteLine();
            }
            return o.ToString();

        }





        public static string ToJson(VAL val, string tag,  ExportFormat fmt)
        {
            if(tag==null || tag=="")
                return ToJson(val, "", 0, fmt);
            else
                return "{" + ToJson(val, tag, 0, fmt) + "}";
        }


        private static string ToJson(VAL val, string tag, int tab, ExportFormat fmt)
        {
        
            StringWriter o = new StringWriter();

            bool quotationMark = (fmt & ExportFormat.QuestionMark) == ExportFormat.QuestionMark;
            bool formatted = (fmt & ExportFormat.WellFormatted) == ExportFormat.WellFormatted;
            bool encodeTypeof = (fmt & ExportFormat.EncodeTypeof) == ExportFormat.EncodeTypeof;

            o.Write(Indent(tab, formatted));
            if (tag != "")
            {
                if(quotationMark)
                    o.Write("\"" + tag + "\""); 
                else
                    o.Write(tag);

                if (formatted)
                    o.Write(" : ");
                else
                    o.Write(":");
            }
            
            if (val.IsAssociativeArray())
            {
                o.Write("{"); if (formatted) o.WriteLine();
                for (int i = 0; i < val.Size; i++)
                {
                    VAL v = val[i];
                    o.Write(ToJson(v[1], v[0].Str, tab + 1, fmt));

                    if (i < val.Size - 1)
                         o.Write(",");

                    if (formatted) o.WriteLine();
                }
                o.Write(Indent(tab, formatted)); o.Write("}");
                if (encodeTypeof && val.Class != null)
                    o.Write(val.encodetypeof());
            }
            else if (val.ty == VALTYPE.listcon)
            {
                o.Write("["); if (formatted) o.WriteLine();
                for (int j = 0; j < val.Size; j++)
                {
                    VAL a = val[j];
                    o.Write(ToJson(a, "", tab + 1, fmt));
                    
                    if (j < val.Size - 1)
                        o.Write(",");
                    
                    if (formatted) o.WriteLine();
                }
                o.Write(Indent(tab, formatted)); o.Write("]");
                if (encodeTypeof && val.Class != null)
                    o.Write(val.encodetypeof());
            }
            else if (val.ty == VALTYPE.hostcon)
            {
                val = HostValization.Host2Valor(val.value);
                if (val.ty == VALTYPE.listcon)
                    o.Write(ToJson(val, "", tab, fmt));
                else
                    o.Write(val.Valor);
            }
            else
            {
                o.Write(val.Valor);
            }
            
            return o.ToString();

        }

        private static string Indent(int n, bool formatted)
        {
            if (formatted)
                return Indent(n);
            else
                return "";
        }

        private static string Indent(int n)
        {
            string tab = "";
            for (int k = 0; k < n; k++)
                tab += "  ";

            return tab;
        }


        private static string XmlString(string s)
        {
            StringWriter o = new StringWriter();
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '"':
                        o.Write("&quot;");
                        break;

                    case '\'':
                        o.Write("&apos;");
                        break;

                    case '\\':
                        o.Write("\\\\");
                        break;

                    case ' ':
                        o.Write("&nbsp;");
                        break;

                    case '\t':
                        o.Write("\\t");
                        break;

                    case '&':
                        o.Write("&amp;");
                        break;

                    case '<':
                        o.Write("&lt;");
                        break;

                    case '>':
                        o.Write("&gt;");
                        break;

                    default:
                        o.Write(s[i]);
                        break;
                }

            }
            
            return o.ToString();
        }
    }
}
