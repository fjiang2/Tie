# Project Description
## C# statement/expression interpreter
* Tie is a dynamic language(script) which accepts C# statement/expression syntax and JavaScript like function and class syntax.
* Support script debug, breakpoint, log.
* Middleware, high performance, compact size around 150KB(tie.dll)

* Integrate into .Net. Execute C# statements and expressions, call any classes/methods of .net DLL
* Build Tie script event handler, delegate, function and class as well, which can be invoked by .NET.
* Tokenize C/C++/C# code like string
* Support WinForm, WPF, ASP.NET Core
* Support .NET 4.5, 5.0, 6.0 and .NET Standard 2.0
* Support Generic class, Extension method, Lambda expression, serialize any objects, high level programming
* Support JSON serialization
* Support 3rd party software such as DevExpress, and Telerik

Example 0:
```javascript
   1:  Memory ds = new Memory();   
   2:  ds.AddObject("a", 10);   
   3:  int a = (int)Script.Evaluate("a+1", ds);   
   4:  // a --> 11;   
   5:  int b = (int)Script.Evaluate("12+1");   
   6:  // b --> 13;   
```   
Example 1:
```javascript
   1:  Memory ds = new Memory();   
   2:  ds.AddObject("a", 10);   
   3:  Script.Execute("if(a>1) {a=a+1; b=true; }", ds);   
   4:  // (int)ds["a"] --> 11;   
   5:  // (bool)ds["b"] --> true;
```   
Example 2:
```javascript
   1: string code = "function(a, dt) { return a + dt.Year;}";
   2: var args = new object[] { 1, DateTime.Now };
   3: int b = (int)Script.InvokeFunction(new Memory(), new VAL(), code, args, null);
```   
 Example 3:
```javascript
   1: Memory ds = new Memory();
   2: HostType.Register(typeof(System.Drawing.Color));
   3: string code = @"label1 = new System.Windows.Forms.Label();
   4:                 label1.Text = \"Hello World\";
   5:                 label1.ForeColor = System.Drawing.Color.Red;";
   6: Script.Execute(code, ds);
   7: Label label1 = (Label)ds["label1"].Value;                 
   // label1.Text --> "Hello World";   
```  
 Example 4:
```javascript
   1: string path = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools 
      /all 
      /has:true";
   2: IEnumerable<token> L = Script.Tokenize(path);
/* L =
line col ty tok   
1	3	identsy	C
1	4	symbol	:
1	5	symbol	\
1	12	identsy	Program
1	18	identsy	Files
1	20	symbol	(
1	23	identsy	x86
1	24	symbol	)
1	25	symbol	\
1	34	identsy	Microsoft
1	41	identsy	Visual
1	48	identsy	Studio
1	53	number	12.0
1	54	symbol	\
1	61	identsy	Common7
1	62	symbol	\
1	67	identsy	Tools
2	5	symbol	/
2	8	identsy	all
3	5	symbol	/
3	8	identsy	has
3	9	symbol	:
3	12	keyword	true
*/
```  

(The 1st version of Tie was developed by C++ and released in 1999, and it has been migrated into C# version in 2007. Many .net specific features were added into Tie C# from 2009 to 2022, Tie has been used in commercial software include aviation, healthcare, education and etc.)