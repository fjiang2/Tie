using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using Tie;


namespace Tie.UnitTest.NET4
{
    class Student
    {
        public int Id;
        public string Name;
    }

    class LinqDataSeteTest
    {
        static DataTable GetDataTable(Student[] students)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(Int32));
            table.Columns.Add("Name", typeof(string));
            foreach (Student student in students)
            {
                table.Rows.Add(student.Id, student.Name);
            }
            return (table);
        }


        static void OutputDataTableHeader(DataTable dt, int columnWidth)
        {
            string format = string.Format("{0}0,-{1}{2}", "{", columnWidth, "}");
            // Display the column headings.
            foreach (DataColumn column in dt.Columns)
            {
                Console.Write(format, column.ColumnName);
            }
            Console.WriteLine();
            foreach (DataColumn column in dt.Columns)
            {
                for (int i = 0; i < columnWidth; i++)
                {
                    Console.Write("=");
                }
            }
            Console.WriteLine();
        }

   

        public static void main()
        {

            Student[] students = {
                new Student { Id = 1, Name = "Joe Rattz" },
                new Student { Id = 6, Name = "Ulyses Hutchens" },
                new Student { Id = 19, Name = "Bob Tanko" },
                new Student { Id = 45, Name = "Erin Doutensal" },
                new Student { Id = 1, Name = "Joe Rattz" },
                new Student { Id = 12, Name = "Bob Mapplethorpe" },
                new Student { Id = 17, Name = "Anthony Adams" },
                new Student { Id = 32, Name = "Dignan Stephens" }
                };


            DataTable dt = GetDataTable(students);
            IEnumerable<DataRow> seq1 = dt.AsEnumerable();

            //Console.WriteLine("{0}Before calling Distinct(){0}",
            //System.Environment.NewLine);
            //OutputDataTableHeader(dt, 15);
            //foreach (DataRow dataRow in dt.Rows)
            //{
            //    Console.WriteLine("{0,-15}{1,-15}", dataRow.Field<int>(0), dataRow.Field<string>(1));
            //}
            
            //IEnumerable<DataRow> distinct = dt.AsEnumerable().Distinct(DataRowComparer.Default);
            //Console.WriteLine("{0}After calling Distinct(){0}",
            //System.Environment.NewLine);
            //OutputDataTableHeader(dt, 15);
            //foreach (DataRow dataRow in distinct)
            //{
            //    Console.WriteLine("{0,-15}{1,-15}",  dataRow.Field<int>(0),  dataRow.Field<string>(1));
            //}


            var S = seq1.Where( student => (int)student["Id"] > 10).Select(student => student);
            string names = "";
            foreach (var s in S)
            {
                names += s["Name"] + ";";
            }

            SecurityDataSetDataContext dc = new SecurityDataSetDataContext();
            var users = dc.Users.Where(U => U.User_ID < 50).Select(U => U);

            string u1 = "";
            foreach (var user in users)
            {
                u1 += user.Last_Name + ";";
            }

            HostType.Register(typeof(Queryable)); 

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            string code;

            DS.Add("dc", VAL.NewHostType(dc));
            code = @"
            Users = dc.Users;
            users = Users.Where(U => U.User_ID < 50).Select(U => U);  
            
            u1='';
            foreach (var user in users)
            {
                u1 += user.Last_Name + ';';
            }
            ";

           // Script.Execute(code, DS);
           // Debug.Assert(DS["t1"].Str == t1);
        }
    }
}
