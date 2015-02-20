using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VMTranslator_CS
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Translate(args);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        void Translate(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Ooops!  Expect exactly one argument.");
                return;
            }

            var path = args[0];
            var vmFiles = new List<string>();
            DirectoryInfo outDir;
            if (path.EndsWith(".vm", StringComparison.OrdinalIgnoreCase))
            {
                var file = new FileInfo(path);
                if (file.Exists)
                    vmFiles.Add(file.FullName);
                else
                    Console.WriteLine("Ooops!  File doesn't exist: " + path);
                outDir = file.Directory;
            }
            else
            {
                var dir = new DirectoryInfo(path);
                if (dir.Exists)
                    vmFiles.AddRange(dir.GetFiles("*.vm").Select(d => d.FullName));
                else
                    Console.WriteLine("Ooops!  Directory doens't exist:  " + path);
                outDir = dir;
            }
            var asmFile = Path.Combine(outDir.FullName, outDir.Name + ".asm");

            Console.WriteLine(string.Format("Got {0:n0} file(s).", vmFiles.Count));

            //var asmFile = Regex.Replace(vmFile, @"\.vm$", @".asm");
            using (var stream = File.OpenWrite(asmFile))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var vmFile in vmFiles)
                {
                    var parser = new Parser(vmFile);
                    var context = new Context(vmFile);
                    var commands = parser.GetCommands(context);
                    var assembly = commands
                        .SelectMany(command => command.GetAssemblyInstructions());


                    //assembly = new[] { "@256", "D=A", "@SP", @"M=D" }.Concat(assembly).Concat(new[] { "(end)", "@end", "0;JMP" });

                    //assembly = assembly.Concat(new[] { "(end)", "@end", "0;JMP" });

                    var bootstrap = new List<string>(new[] { "@256", "D=A", "@SP", @"M=D" });
                    bootstrap.AddRange(new CallCommand(new Context(vmFile), "Sys.init", "0").GetAssemblyInstructions());
                    assembly = bootstrap.Concat(assembly);


                    foreach (var a in assembly)
                        Console.WriteLine(a);



                    foreach (var a in assembly)
                    {
                        writer.WriteLine(a);
                    }
                    writer.Flush();
                    stream.Flush();
                    stream.SetLength(stream.Position);
                }
            }

        }

    }
}
