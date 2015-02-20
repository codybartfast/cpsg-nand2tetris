using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JackCompiler
{
    // Just to demonstrate how to consume the JSON tree in C#
    class JsonExample
    {
        //Name of the Members
        //  Everything:
        //      Token   # the token type
        //
        //  Terminal:
        //      Value
        //  Identifier:
        //      Type
        //      Kind
        //      Index
        //  SubroutineDec
        //      VarCount
        //  SubroutineDec - constructor
        //      FieldCount
        //
        //  NonTerminal
        //      Children

        public void CodeFile(string filePath)
        {
            string fileText;
            using (var reader = File.OpenText(filePath))
            {
                fileText = reader.ReadToEnd();
            }
            WriteLine();
            CodeFileText(fileText);
        }

        public void CodeFileText(string fileText)
        {
            dynamic tree = JsonConvert.DeserializeObject(fileText);
            var children = tree.Children;
            if (tree.Token != "class")
                throw new Exception("Expected a class");
            CodeClass(tree);
        }

        void CodeClass(dynamic tree)
        {
            var children = tree.Children;
            var className = children[1].Value;
            WriteLine("I'm in CLASS " + className);
            
            foreach (var child in children)
            {
                if (child.Token == "subroutineDec")
                    CodeSubroutineDec(child);
            }
            /*  N.B.
             *  We don't have to do anything with the classVarDecs.
             *  that information was used by the symbol table.
             */
            WriteLine("Ding, ding, end of CLASS ", className);
        }

        void CodeSubroutineDec(dynamic subroutineDec)
        {
            var children = subroutineDec.Children;
            var subroutineType = (string)children[0].Value;
            var type = children[1].Value;
            var name = children[2].Value;
            var localCount = subroutineDec.VarCount;

            string suffix = ".";
            if (subroutineType == "constructor")
            {
                var fieldCount = subroutineDec.FieldCount;
                suffix = ", oh and the class has " + fieldCount + " fields.";
            }
            WriteLine("    ", subroutineType.ToUpper(), " '", name, "' returns a ", type, ", it has ",
                localCount, " local variables", suffix);
            /* N.B.  we don't need to do anything with the parameter list */
        }

        void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        void WriteLine(params object[] parts)
        {
            WriteLine(string.Concat(parts));
        }

    }
}
