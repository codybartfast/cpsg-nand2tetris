using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Pleae provide soure file or directory.");
            }
            else
            {
                Go(args);
            }
            Console.WriteLine("Done, press any key to finish...");
            Console.ReadKey();
        }

        static void Go(string[] args)
        {
            var inputPath = args[0];
            var fileManager = new FileManager(inputPath);

            foreach (var set in fileManager.FileSets)
            {
                var content = FileReader.GetContent(set.SourceFile);
                var tokens = Tokenizer.GetTokens(content);

                // Write 'flat' XML file.
                TokenWriter.AsFlatXml(tokens, set.TXmlFile);
                Compare(set.TXmlFile, set.ExpectedTXmlFile);

                // Write heirarchical XML file.
                var tree = TreeBuilder.Build(tokens);
                TokenWriter.AsXml(tree, set.XmlFile);
                Compare(set.XmlFile, set.ExpectedXmlFile);

                // Write XML file with extra identifier info
                var sBuilder = new SymbolTableBuilder();
                var table = sBuilder.BuildTable(tree);
                sBuilder.PopulateIdentifiers(tree);
                TokenWriter.AsXml(tree, set.WsiXmlFile, true);

                // Write JSON file with extra identifier info
                TokenWriter.AsJson(tree, set.JsonFile, true);

                // Run JSON through JsonUsage example code
                new JsonExample().CodeFile(set.JsonFile.FullName);

                // Write VM file
                var codeWriter = new CodeWriter(tree, set.VMFile);
                codeWriter.WriteTree();
            }

        }

        static FileInfo textComparer = new FileInfo(@"C:\STUFF\Nand2Tetris\Suite\tools\TextComparer.bat");
        static void Compare(FileInfo actual, FileInfo expected)
        {
            if (! textComparer.Exists || !actual.Exists || !expected.Exists)
                return;
            Console.WriteLine();
            Console.WriteLine("About to compare: " + actual.FullName);
            Console.WriteLine("With:             " + expected.FullName);
            Console.WriteLine("Actual is {0:n0} seconds old.", (DateTime.UtcNow - actual.LastWriteTimeUtc).TotalSeconds);

            var psi = new ProcessStartInfo(textComparer.FullName,
                actual.FullName + " " + expected.FullName);
            psi.UseShellExecute = false;
            Process.Start(psi).WaitForExit();

            Console.WriteLine();
        }
    }
}
