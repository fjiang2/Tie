using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Tie;

namespace Tie.Helper
{
    public static class Conversion
    {

        public static VAL ToVAL(this DataRow dataRow)
        {
            VAL val = new VAL();
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                val[dataColumn.ColumnName] = VAL.Boxing(dataRow[dataColumn.ColumnName]);
            }
            return val;

        }


        public static VAL ToVAL(this DataRow dataRow, VAL val)
        {
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                val[dataColumn.ColumnName] = VAL.Boxing(dataRow[dataColumn.ColumnName]);
            }
            return val;

        }

        public static DataRow ToDataRow(this VAL val, DataRow dataRow)
        {
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                if (val[dataColumn.ColumnName].Defined)
                {
                    object v = VAL.UnBoxing(val[dataColumn.ColumnName]);

                    DataColumnAssign(dataRow, dataColumn.ColumnName, v);
                }
            }

            return dataRow;
        }

        public static DataTable ToDataTable(this VAL val)
        {

            DataTable dataTable = new DataTable();

            for (int i = 0; i < val.Size; i++)
            {
                VAL field = val[i];
                VAL key = field[0];
                VAL value = field[1];
                Type ty;
                if (value.Value != null)
                    ty = value.Value.GetType();
                else
                    ty = typeof(string);

                DataColumn dataColumn = new DataColumn(key.Str, ty);
                dataTable.Columns.Add(dataColumn);
            }

            DataRow dataRow = dataTable.NewRow();
            ToDataRow(val, dataRow);
            dataTable.Rows.Add(dataRow);
            return dataTable;
        }



        public static void DataColumnAssign(DataRow dataRow, string columnName, object value)
        {
            if (value == null)
            {
                if (dataRow[columnName] != System.DBNull.Value)  //suppress RowChanged event handler invoke
                    dataRow[columnName] = System.DBNull.Value;
            }
            else
            {
                DataColumn dataColumn = dataRow.Table.Columns[columnName];
                if (dataColumn.DataType == value.GetType())
                {
                    if (!dataRow[columnName].Equals(value))     //suppress RowChanged event handler invoke
                        dataRow[columnName] = value;
                }
            }
        }



    }
}
