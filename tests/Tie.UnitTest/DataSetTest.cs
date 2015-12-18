using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Tie;
using System.Diagnostics;

namespace UnitTest
{
    class DataSetTest
    {
        private static DataSet CreateInstance()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            ds.Tables.Add(dt);

            dt.Columns.Add(new DataColumn("Id", typeof(int)));
            dt.Columns.Add(new DataColumn("Name", typeof(string)));

            var newRow = dt.NewRow();
            newRow["Id"] = 1;
            newRow["Name"] = "Sugar";
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow["Id"] = 2;
            newRow["Name"] = "Land";
            dt.Rows.Add(newRow);

            ds.AcceptChanges();

            return ds;
        }


        public static void main()
        {
            var ds = CreateInstance();

            Valizer.Register<DataSet>(_ds => ToVal(_ds), (host, type, _xml) => ToDataSet(host, type, _xml));
            Memory DS = new Memory();
            DS.AddHostObject("ds", ds);

            string mem = new VAL(DS).Valor;
            Logger.Open("c:\\temp\\tie.log");
            VAL val = Script.Evaluate(mem, new Memory());
            Memory DS2 = new Memory(val);
            var ds2 = (DataSet)DS2["ds"].Value;

            Debug.Assert(ds2.Tables[0].Rows[1]["Name"].Equals("Land"));
        }

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


    }
}
