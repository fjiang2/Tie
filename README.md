# Project Description
C# statement/expression interpreter
Tie is dynamic language(script) which accepts C# statement/expression syntax and JavaScript like function and class syntax. Include lexical analysis, parser and virtual machine(VM)
Script, such method or event handler,  is a simple .Net string which can be saved into SQL Server
Support script debug, breakpoint, log.
Middleware, high performance, compact size around 150KB(tie.dll)

Integrate into .Net. Execute C# statements and expressions, call any classes/methods of .net DLL
Build Tie script event handler, delegate, function and class as well, which can be invoked by .NET.

Support WinForm, WPF, Silverlight, ASP.NET, JSON
Support .NET 2.0/3.0/3.5/4.0
Support Generic class, Extension method, Lambda expression, serialize any objects, high level programming
Support 3rd party software such as DevExpress, Telerik

Example:
   1:  Memory ds = new Memory();   
   2:  ds.AddObject("a", 10);   
   3:  Script.Execute("if(a>1) {a=a+1; b=true; }", ds);   
   4:  // (int)ds["a"] --> 11;   
   5:  // (bool)ds["b"] --> true;
Example:

   int b = (int)Script.InvokeFunction(new Memory(), new VAL(), "function(a, dt) { return a+dt.Year;}", new object[] { 1, DateTime.Now }, null);
 

(The 1st version of Tie was developed by C++ and released in 1999, it was migrated into C# version in 2007. Many .net specific features were added into Tie C# from 2009 to 2011, Tie has been used in commercial software include aviation, healthcare, education …)