using System;
using System.Collections.Generic;
using System.Text;
using Tie;


namespace UnitTest
{
    class XMLTest
    {
        public static void main()
        {
            string code = @"
                  book.title = 'My Life';
                  book.color = 'red';
                  book.price = 3.19;
                  Author.FirstName='First';
                  Author.LastName='Last';
                  Author.Address.Street='200 Big Way';
                  Author.Address.City ='New York';
                  book.Author = Author;
                  book.weight=20;  
                  book.page=200;
                ";

            Script script = new Script();
            script.DS = new Memory();
            script.VolatileExecute(code);
            VAL val = script.DS["book"];
            string xml1 = val.ToXml("Book");
            string json1 = val.ToJson("Book");

            Console.WriteLine(xml1);
            Console.WriteLine(json1);

            string code1 = @"
            menu.id='file';
            menu.value='File';
            menuitem[0].value='New';
            menuitem[0].onclick='CreateNewDoc()';
            menuitem[1].value='Open';
            menuitem[1].onclick='OpenDoc()';
            menuitem[2].value='Close';
            menuitem[2].onclick='CloseDoc()';
            menu.popup.menuitem = menuitem;
            ";

            script = new Script();
            script.DS = new Memory();
            script.VolatileExecute(code1);
            val = script.DS["menu"];
            
            string xml2 = val.ToXml("menu");
            string json2 = val.ToJson("menu");
            json2 = val.ToJson(null);

            Console.WriteLine(xml2);
            Console.WriteLine(json2);

        }
    }
}
