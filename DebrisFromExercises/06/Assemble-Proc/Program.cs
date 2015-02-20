using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Table = System.Collections.Generic.Dictionary<string, int>;

namespace Assemble_Proc
{
    class Program
    {
        static void Main(string[] args)
        {
            int nextInstruction = 0, nextVariableAdress = 16;
            var variables = new HashSet<string>();

            var commands =
                File.ReadAllLines(args[0])
                .Where(line => !Regex.IsMatch(line, @"^\s*(//|$)"))
                .Select(line => line.Trim());

            foreach (var command in commands)
            {
                var match = Regex.Match(command, @"(?<=\()\S+(?=\))");
                if (match.Success) table.Add(match.Value, nextInstruction);
                else nextInstruction++;

                match = Regex.Match(command, @"(?<=@)\D.*");
                if (match.Success) variables.Add(match.Value);
            }

            variables.ExceptWith(table.Keys);
            foreach (var variable in variables) table.Add(variable, nextVariableAdress++);

            using (var stream = File.OpenWrite(args[0].Replace(".asm", ".hack")))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var command in commands)
                {
                    if (command.StartsWith("(")) continue;

                    if (command.StartsWith("@"))
                    {
                        var load = command.Trim('@');
                        var value = Regex.IsMatch(load, @"^\d") ? int.Parse(load) : table[load];
                        writer.WriteLine(Convert.ToString(value, 2).PadLeft(16, '0'));
                        continue;
                    }

                    string comp, dest = "", jmp = "null";
                    if (command.Contains("="))
                    {
                        var parts = command.Split('=');
                        dest = parts[0];
                        comp = parts[1];
                    }
                    else
                    {
                        var parts = command.Split(';');
                        comp = parts[0];
                        jmp = parts[1];
                    }

                    var instruction = "111" +
                        (comp.Contains("M") ? "1" : "0") + computes[comp.Replace("M", "A")] +
                        (dest.Contains("A") ? "1" : "0") +
                            (dest.Contains("D") ? "1" : "0") +
                            (dest.Contains("M") ? "1" : "0") +
                        jumps[jmp];
                    writer.WriteLine(instruction);
                }
            }

        }

        static readonly Table table = new Table
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

        static readonly Dictionary<string, string> computes = new Dictionary<string, string>
        {
            {"0", "101010"},
            {"1", "111111"},
            {"-1", "111010"},
            {"D", "001100"},
            {"A", "110000"},
            {"!D", "001101"},
            {"!A", "110001"},
            {"-D", "001111"},
            {"-A", "110011"},
            {"D+1", "011111"},
            {"A+1", "110111"},
            {"D-1", "001110"},
            {"A-1", "110010"},
            {"D+A", "000010"},
            {"D-A", "010011"},
            {"A-D", "000111"},
            {"D&A", "000000"},
            {"D|A", "010101"}
        };

        static readonly Dictionary<string, string> jumps = new Dictionary<string, string>
        {
            {"null", "000"},
            {"JGT", "001"},
            {"JEQ", "010"},
            {"JGE", "011"},
            {"JLT", "100"},
            {"JNE", "101"},
            {"JLE", "110"},
            {"JMP", "111"},
        };

    }
}
