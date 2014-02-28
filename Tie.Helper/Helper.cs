using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Tie.Helper
{
    public static class Helper
    {
        public static void Start()
        {
            Script.FunctionChain.Add(PrimitiveType.functions);
            Valization.Register();
        }


        public static void DefineEnumAsInteger()
        {
            Valizer.Register<Enum>(
                   host =>
                   {
                       return new VAL(Convert.ToInt32(host));
                   },
                   (host, type, val) =>
                   {
                       try
                       {
                           return (Enum)Enum.ToObject(typeof(Enum), val.Intcon);
                       }
                       catch (Exception)
                       {
                           return (Enum)Activator.CreateInstance(type);
                       }
                   }
               );
        }

        public static void DefineEnumAsString()
        {

            Valizer.Register<Enum>(
              host =>
              {
                  return new VAL("\"" + host.ToString() + "\"");
              },
              (host, type, val) =>
              {
                  string s = val.Str;
                  string[] L = s.Split(new char[] { ',' });
                  try
                  {
                      if (L.Length == 1)
                      {
                          return (Enum)Enum.Parse(type, s);
                      }
                      else
                      {
                          int E = 0;
                          for (int i = 0; i < L.Length; i++)
                          {
                              E += Convert.ToInt32(Enum.Parse(type, L[i].Trim()));
                          }
                          return (Enum)Enum.ToObject(typeof(Enum), E);
                      }
                  }
                  catch (Exception)
                  {
                      //return default value if str is not Enum type any more
                      return (Enum)Activator.CreateInstance(type);
                  }
              }
            );
        }


    
    }
}
