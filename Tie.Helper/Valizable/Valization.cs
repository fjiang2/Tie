using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using Tie;

namespace Tie.Helper
{
    static class Valization
    {
        public static void Register()
        {

            Valizer.Register<byte[]>(
                 delegate (byte[] bytes)
                 {
                     return new VAL("\"" + Serialization.ByteArrayToHexString(bytes) + "\"");     //because this is a string, need quotation marks ""
                 },
                 delegate (VAL val)
                 {
                     byte[] bytes = Serialization.HexStringToByteArray(val.Str);
                     return bytes;
                 }
            );


            Valizer.Register<Stream>(
             delegate (Stream stream)
             {
                 byte[] bytes = new byte[stream.Length];
                 stream.Read(bytes, 0, bytes.Length);
                 return Valizer.Valize(bytes);
             },
             delegate (Stream stream, Type type, VAL val)
             {
                 byte[] bytes = Valizer.Devalize<byte[]>(val);
                 stream.Write(bytes, 0, bytes.Length);
                 return stream;
             }
          );


            Valizer.Register<Guid>(
                delegate (Guid guid)
                {
                    byte[] bytes = guid.ToByteArray();
                    return Valizer.Valize(bytes);
                },
                delegate (VAL val)
                {
                    byte[] bytes = Valizer.Devalize<byte[]>(val);
                    return new Guid(bytes);
                }
            );



            Valizer.Register(typeof(List<>), typeof(Valization).GetMethod(nameof(RegisterList), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
            Valizer.Register(typeof(Dictionary<,>), typeof(Valization).GetMethod(nameof(RegistrDictionary), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));

            Valizer.Register<DataSet>(_ds => ToVal(_ds), (host, type, _xml) => ToDataSet(host, type, _xml));

        }


        #region List<T>/Dictionary<T1,T2>

        private static void RegisterList<T>()
        {
            Valizer.Register<List<T>>(
                   host =>
                   {
                       if (host == null)
                           return new VAL();

                       var val = VAL.Array();

                       foreach (var item in host)
                       {
                           val.Add(Valizer.Valize(item));
                       }
                       return val;
                   },
                   val =>
                   {
                       if (val.IsNull)
                           return null;

                       var list = new List<T>();

                       foreach (VAL item in val)
                       {
                           list.Add(Valizer.Devalize<T>(item));
                       }
                       return list;
                   }
               );
        }

        private static void RegistrDictionary<T1, T2>()
        {
            Valizer.Register<Dictionary<T1, T2>>(
                   host =>
                   {
                       if (host == null)
                           return new VAL();

                       var val = VAL.Array();

                       foreach (var kvp in host)
                       {
                           VAL assoc = new VAL();
                           assoc.Add(Valizer.Valize(kvp.Key));
                           assoc.Add(Valizer.Valize(kvp.Value));
                           val.Add(assoc);
                       }

                       return val;
                   },
                   (host, type, val) =>
                   {
                       if (val.IsNull)
                           return null;

                       Dictionary<T1, T2> dict;
                       if (host != null)
                           dict = host;
                       else
                           dict = new Dictionary<T1, T2>();

                       foreach (var assoc in val)
                       {
                           T1 key = Valizer.Devalize<T1>(assoc[0]);
                           T2 value = Valizer.Devalize<T2>(assoc[1]);

                           if (dict.ContainsKey(key))
                           {
                               dict[key] = value;
                           }
                           else
                           {
                               dict.Add(key, value);
                           }
                       }
                       return dict;
                   }
                   );
        }

        #endregion



        #region Enum => int/string

        internal static void RegisterEnumAsInteger()
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

        internal static void RegisterEnumAsString()
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

        #endregion


        #region DataSet 

        private static VAL ToVal(DataSet ds)
        {
            string xml = ToXml(ds);
            string code = new VAL(xml).ToString();
            code = string.Format("new {0}().classof({1})", typeof(DataSet).FullName, code);
            return new VAL(code);
        }

        private static DataSet ToDataSet(DataSet ds, Type type, VAL xml)
        {
            return ToDataSet(ds, (string)xml);
        }

        private static DataSet ToDataSet(DataSet ds, string xml)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(xml);
                writer.Flush();
                stream.Position = 0;

                try
                {
                    ds.ReadXml(stream, XmlReadMode.ReadSchema);
                }
                catch (Exception)
                {
                    throw new Exception(xml);
                }
            }
            return ds;
        }

        private static string ToXml(DataSet ds)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ds.WriteXml(stream, XmlWriteMode.WriteSchema);
                stream.Flush();
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
