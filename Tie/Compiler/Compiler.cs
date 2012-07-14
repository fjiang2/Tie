//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------///****
/*
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

#if !SILVERLIGHT 
using System.Runtime.Serialization.Formatters.Binary;

namespace Tie
{
   
    public sealed class Compiler
    {
        private string sourceFile;
        private string objectFile;
        private string asmFile;
        private string moduleName;

        private Compiler(string sourceFile, string objectFile, string asmFile)
        {
            this.sourceFile = sourceFile;
            this.objectFile = objectFile;
            this.asmFile = asmFile;
            this.moduleName = ModuleName(sourceFile);
        }


        private static string ModulePath(string fileName)
        {
            return Path.GetDirectoryName(fileName) + Path.GetFileNameWithoutExtension(fileName);
        }

        private static string ModuleName(string fileName)
        { 
            return Path.GetFileNameWithoutExtension(fileName);
        }


        private static Module LoadModule(string objectFile)
        {
            Stream stream = File.Open(objectFile, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            Module module = (Module)bformatter.Deserialize(stream);
            stream.Close();

            return module;

        }

        private static void SaveModule(string objectFile, Module module)
        {
            Stream stream = File.Open(objectFile, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, module);
            stream.Close();

        }

        private Module Compile()
        {
            Module module = new Module(moduleName, Constant.MAX_INSTRUCTION_NUM, Constant.MAX_CODEBLOCK_NUM);
            JParser parser = new JParser("", sourceFile, CodeSource.FILE, module);
            parser.Compile(CodeType.statements);
            parser.Close();

            if (asmFile != null)
            {
                StreamWriter fo = new StreamWriter(asmFile);
                fo.WriteLine(parser);
                fo.Close();
            }

            return module;
        }


        public static string Compile(string sourceFile)
        {
            string path = ModulePath(sourceFile);
            string objectFile = path + ".bin";
            string asmFile = path + ".asm";
            return Compile(sourceFile, objectFile, asmFile);  
        }

        public static string Compile(string sourceFile, string objectFile, string asmFile)
        {
            Compiler compiler = new Compiler(sourceFile, objectFile, asmFile);
            Module module = compiler.Compile();
            Compiler.SaveModule(objectFile, module);
            return compiler.objectFile;
        }


        public static VAL Run(string objectFile, Memory DS, IUserDefinedFunction userFunc)
        {
            Module module = Compiler.LoadModule(objectFile);
            return Computer.Run(module, DS, userFunc);
        }

    }


}
#endif

***/