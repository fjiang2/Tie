using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Tie;

namespace Tie.UnitTest.NET4
{


    class Car
    {
        public string PetName = string.Empty;
        public string Color = string.Empty;
        public int Speed;
        public string Make = string.Empty;
    }


    public static class MyExtensions
    {
        public static IEnumerable<R> Transform<T, R> ( this IEnumerable<T> input,  Func<T, R> op)
        {
            foreach (var item in input)
            {
                yield return op(item);
            }
        }
    }

    class ExtendMethodTest
    {
        public string Times<T>(T X, T Y) where T: IEnumerable 
        {
            string s = "";
            foreach (var x in X)
                foreach (var y in Y)
                {
                    s += x.ToString()+y.ToString()+";";
                }

            return s;
        }

        public static void main()
        {

            List<Car> myCars = new List<Car>() {
                new Car{ PetName = "Henry", Color = "Silver", Speed = 100, Make = "BMW"},
                new Car{ PetName = "Daisy", Color = "Tan", Speed = 90, Make = "BMW"},
                new Car{ PetName = "Mary", Color = "Black", Speed = 55, Make = "VW"},
                new Car{ PetName = "Clunker", Color = "Rust", Speed = 5, Make = "Yugo"},
                new Car{ PetName = "Melvin", Color = "White", Speed = 43, Make = "Ford"}
           };


            string[] currentVideoGames = 
                {
                    "Morrowind", 
                    "BioShock",
                    "Half Life 2", 
                    "The Darkness",
                    "Daxter", 
                    "System Shock 2"
                };



            var subset = currentVideoGames
                .Where(game => game.Length > 6)
                .OrderBy(game => game)
                .Select(game => game);

            string s1 = "";
            foreach (var game in subset)
            {
                Console.WriteLine("Item: {0}", game);
                s1 += game;
            }

            Logger.Close();
            Logger.Open("c:\\temp\\tie.log");
            Memory DS = new Memory();
            string code;




            ExtendMethodTest test = new ExtendMethodTest();
            string t1 = test.Times<int[]>(new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 });
            DS.RemoveAll();
            DS.Add("test", VAL.NewHostType(test));
            code = @"
            //t1 = test.Times<int[]>(new int[] {1,2,3},new int[] {4,5,6});    
            t1 = test.Times<int[]>({1,2,3},{4,5,6});    

            ";

            Script.Execute(code, DS);
            Debug.Assert(DS["t1"].Str == t1);



            HostType.Register(typeof(Enumerable));
            //HostType.Register(typeof(Queryable)); 

            
            code = @"
            subset2 = currentVideoGames
                .Where( game => HOST(game).Length > 6)
                .OrderBy(game => { return game;} )
                .Select(game => VAL({game, 100}));
            
            s2={};
            foreach(var game in subset2)
                s2 += game;

            s3 ='';
            foreach(var game in subset2)
               foreach(var item in game)
                  s3 += item;

            ";

            DS.RemoveAll();
            DS.Add("currentVideoGames", VAL.Boxing(currentVideoGames));
            Script.Execute(code, DS);


            var subset2 = (IEnumerable<object>)DS["subset2"].Value;
            VAL s2 = VAL.Array();
            foreach (var game in subset2)
            {
                s2 += (VAL)game;
            }

            Debug.Assert(DS["s2"].ToString() == s2.ToString());
            Debug.Assert(DS["s3"].Str == "BioShock100Half Life 2100Morrowind100System Shock 2100The Darkness100");

            VAL x = DS["subset2"][0];

           // Debug.Assert(s1==s2);

            //code = "1+3";
            //DS.Clear();
            //VAL ret1 = Coding.Execute("", code, DS, null);

            //code = "return 1+3;";
            //DS.Clear();
            //VAL ret2 = Coding.Execute("", code, DS, null);

            //code = "a=12; b=30; c=40;";
            //DS.Clear();
            //VAL ret3 = Coding.Execute("", code, DS, null);

            //IQueryable<string> a;
        }
    }
}
