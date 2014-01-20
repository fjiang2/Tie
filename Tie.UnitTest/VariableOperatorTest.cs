using System;
using System.Collections.Generic;
using System.Text;
using Tie;

namespace UnitTest
{
    class VariableOperatorTest
    {
        public static void main()
        {
            /**
             * 
             *  地址&, *操作符测试
             * 
             * 
             * */
            Script.CommonMemory.RemoveAll();
            Memory myDS = new Memory();
            string code = @"{
//                        tb.Name = 'tbEdit';
//                            tb.Text= 'Hello World';
//                            tb.WordWrap= true;
//                            tb.Padding.Bottom=111;
//                    
//                         A = tb.Padding.Bottom;  



               instance = new System.Windows.Forms.ListBox();       //初始化没有注册过的对象


                System.Math.x='xxx'; System.Math.y='yyy'; 
                #scope System.Math ; 
                A = &this.x;
 
                         addr = &this;
                         addr2 = &base;
                           val  = *addr;
                         C = &base.Math;

                         tx = {{'x',1},{'y',2},{'z',3}};
                         ty = {12, tx};    
                         B = &tx[1].x;
                         B1 = &tx[1][1];    
                         B2 = &tx[1]['z'];    
                         
                         D = *B;
                         *B1=200;

    
                         ty = {{'x',1},{'y',2},{'z',3}}.y;   
                         E = &ty;   
                         
                         F = &{{'x',1},{'y',2},{'z',3}}.y;      //匿名数组没有变量名字,也就是返回的地址为null 
                        
                        !('AAA=123;');                 
                  }";

            Script.Execute(code, myDS);

            //System.Reflection.Assembly assembly = HostType.LoadAssembly("System.Windows.Forms");
            //Console.ReadKey(); 

        }
    }
}
