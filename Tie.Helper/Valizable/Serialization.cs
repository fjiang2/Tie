using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#if !SILVERLIGHT
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace Tie.Helper
{
    public class Serialization
    {

        #region Hex <---> String

        /// <summary>
        /// Utility function:
        ///     conver string into byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(String hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Utility function:
        ///     convert byte array into string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }


        #endregion

        #region BinaryFormatter Encode/Decode

#if !SILVERLIGHT

        public static object DecodeBinary(string hexString)
        {
            byte[] buffer = Serialization.HexStringToByteArray(hexString);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    return formatter.Deserialize(stream);
                }
                catch (Exception)
                {
                    throw new ApplicationException(".NET object Deserialization failed in Tie. " + hexString);
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }


        public static string EncodeBinary(object value)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, value);
                    StringWriter sw = new StringWriter();
                    for (int i = 0; i < stream.Position; i++)
                        sw.Write("{0:x2}", buffer[i]);

                    return sw.ToString();
                }
                catch (Exception)
                {
                    throw new ApplicationException(".NET object Serialization failed in Tie. " + value.ToString()); ;
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

        }

#endif
        #endregion



        #region SOAPFormatter Encode/Decode

#if !SILVERLIGHT

        public static object DecodeSOAP(string SOAP)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(SOAP);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                SoapFormatter formatter = new SoapFormatter();
                try
                {
                    return formatter.Deserialize(stream);
                }
                catch (Exception)
                {
                    throw new ApplicationException(".NET object Deserialization failed in Tie. " + SOAP);
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }


        public static string EncodeSOAP(object value)
        {
            byte[] buffer = new byte[16 * 1024];

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                SoapFormatter formatter = new SoapFormatter();
                try
                {
                    formatter.Serialize(stream, value);
                    return Encoding.UTF8.GetString(buffer, 0, (int)stream.Position);
                }
                catch (Exception)
                {
                    throw new ApplicationException(".NET object Serialization failed in Tie. " + value.ToString());
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

        }



#endif

        #endregion

    }
}
