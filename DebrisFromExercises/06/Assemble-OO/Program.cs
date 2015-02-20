using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Table = System.Collections.Generic.Dictionary<string, int>;


namespace Assemble
{
    class Program
    {

        static void Main(string[] args)
        {
            new Program().Assemble(args);
        }

        FileInfo sourceFile;
        FileInfo targetFile;
        FileInfo debugFile;

        void Assemble(string[] args)
        {
            SetFileInfo(args);

            var commandLines = ReadCommandLines();

            var commands = commandLines
                .Select(line => Command.Build(line))
                .ToArray();

            var symbolTable = BuildSymbolTable(commands);

            Display("Creating instructins.");
            var instructions = commands
                .Select(command => command.GetInstructions(symbolTable))
                .SelectMany(ins => ins)
                .ToArray();

            WriteInstructions(instructions);

            Display("All Done.");

            Display("Press any key in exit...");
            Console.ReadKey();
        }

        void Display(object message)
        {
            Console.WriteLine(message);
        }

        void SetFileInfo(string[] args)
        {
            if (!args.Any())
            {
                Display("USAGE:  assembe source-file");
                return;
            }

            sourceFile = new FileInfo(args.First());
            if (!sourceFile.Exists)
            {
                Display("Couldn't find file: " + sourceFile.FullName);
                return;
            }
            Display("Got Source File: " + sourceFile.FullName);

            var targetPath = Regex.Replace(sourceFile.FullName, @"(\.asm)$", "") + ".hack";
            targetFile = new FileInfo(targetPath);
            Display("Using Target File: " + targetFile.FullName);

            var debugPath = Regex.Replace(sourceFile.FullName, @"(\.asm)$", "") + ".dbg";
            debugFile = new FileInfo(debugPath);
            Display("Using Debug File: " + debugFile.FullName);
        }

        IEnumerable<string> ReadCommandLines()
        {
            Display("Reading commands from soucce file.");
            using (var reader = sourceFile.OpenText())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = Regex.Match(line, @"(?<=^\s*)(?!//)\S+");
                    if (match.Success)
                        yield return match.Value;
                }
            }
        }

        Table BuildSymbolTable(Command[] commands)
        {
            Display("Building symbol table");
            var table = GetPredefinedTable();

            var nextInstruction = 0;
            var nextVariableAddr = 16;
            var variableLength = 1;

            foreach (var command in commands)
            {
                if (command is LCommand)
                {
                    foreach (var symbol in command.ReferencedSymbols)
                    {
                        table.Add(symbol, nextInstruction);
                    }
                }
                nextInstruction += command.InstructionCount;
            }
            
            foreach (var command in commands)
            {
                if (command is ACommand)
                {
                    foreach (var symbol in command.ReferencedSymbols)
                    {
                        if (!table.ContainsKey(symbol))
                        {
                            table.Add(symbol, nextVariableAddr);
                            nextVariableAddr += variableLength;
                        }
                    }
                }
            }
            return table;
        }

        Table GetPredefinedTable()
        {
            return new Table
            {
                {"SP", 0},
                {"LCL", 1},
                {"ARG", 2},
                {"THIS", 3},
                {"THAT", 4},
                {"SCREEN", 16384},
                {"KBD", 24576},
                {"R0", 0},
                {"R1", 1},
                {"R2", 2},
                {"R3", 3},
                {"R4", 4},
                {"R5", 5},
                {"R6", 6},
                {"R7", 7},
                {"R8", 8},
                {"R9", 9},
                {"R10", 10},
                {"R11", 11},
                {"R12", 12},
                {"R13", 13},
                {"R14", 14},
                {"R15", 15},
            };
        }

        void WriteInstructions(Instrucction[] instructions)
        {
            Display("Writing instructions to target file.");
            using (var targetStream = targetFile.OpenWrite())
            using (var debugStream = debugFile.OpenWrite())
            using (var targetWriter = new StreamWriter(targetStream, Encoding.ASCII))
            using (var debugWriter = new StreamWriter(debugStream, Encoding.ASCII))
            {
                foreach (var instruction in instructions)
                {
                    //Display(instruction);
                    targetWriter.WriteLine(instruction.Binary);
                    debugWriter.WriteLine(instruction);
                }
                targetStream.SetLength(targetStream.Position);
                debugStream.SetLength(debugStream.Position);
            }
        }

    }

}
