using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assemble
{
    public abstract class Command
    {
        protected readonly string commandText;
        public Command(string commandText)
        {
            this.commandText = commandText;
        }

        public virtual int InstructionCount { get { return 1; } }

        public abstract string[] ReferencedSymbols { get; }

        public abstract IEnumerable<Instrucction> GetInstructions(Dictionary<string, int> symbolTable);

        public override string ToString()
        {
            return commandText;
        }

        public static Command Build(string commandText)
        {
            if (commandText.StartsWith("@"))
            {
                return new ACommand(commandText);
            }
            else if (Regex.IsMatch(commandText, @"^\(.*\)$"))
            {
                return new LCommand(commandText);
            }
            else
            {
                return new CCommand(commandText);
            }
        }
    }



    class ACommand : Command
    {
        public ACommand(string commandText)
            : base(commandText)
        {
            Parse();
        }

        int value = -1;
        void Parse()
        {
            var load = commandText.TrimStart('@');
            if (Regex.IsMatch(load, @"^\d"))
            {
                value = int.Parse(load);
                referencedSymbols = new string[0];
            }
            else
            {
                referencedSymbols = new string[] { load };
            }
        }

        string[] referencedSymbols;
        public override string[] ReferencedSymbols
        {
            get { return referencedSymbols; }
        }

        public override IEnumerable<Instrucction> GetInstructions(Dictionary<string, int> symbolTable)
        {
            value = value >= 0 ? value : symbolTable[referencedSymbols[0]];
            yield return new Instrucction(this,
                Convert.ToString(value, 2).PadLeft(16, '0'));
        }

    }

    class LCommand : Command
    {
        public LCommand(string commandText)
            : base(commandText) { Parse(); }

        void Parse()
        {
            var label = commandText.Trim('(', ')');
            referencedSymbols = new string[] { label };
        }

        public override int InstructionCount { get { return 0; } }


        string[] referencedSymbols;
        public override string[] ReferencedSymbols
        {
            get { return referencedSymbols; }
        }

        public override IEnumerable<Instrucction> GetInstructions(Dictionary<string, int> symbolTable)
        {
            yield break;
        }

    }


    class CCommand : Command
    {
        public CCommand(string commandText)
            : base(commandText) { }

        public override string[] ReferencedSymbols
        {
            get { return new string[0]; }
        }

        public override IEnumerable<Instrucction> GetInstructions(Dictionary<string, int> symbolTable)
        {
            var prefix = "111";
            string comp;
            string dest;
            string jump;
            if (commandText.Contains('='))
            {
                var parts = commandText.Split('=');
                comp = GetComp(parts[1]);
                dest = GetDest(parts[0]);
                jump = "000";
            }
            else if (commandText.Contains(';'))
            {
                var parts = commandText.Split(';');
                comp = GetComp(parts[0]);
                dest = "000";
                jump = GetJump(parts[1]);
            }
            else
            {
                throw new Exception("Didn't expect this: " + commandText);
            }
            yield return new Instrucction(this,
                    prefix + comp + dest + jump);
        }

        private string GetComp(string commandText)
        {
            return
                (commandText.Contains("M") ? "1" : "0") + commands[commandText.Replace("M", "A")];
        }

        private string GetDest(string destText)
        {
            return
                (destText.Contains("A") ? "1" : "0")
                + (destText.Contains("D") ? "1" : "0")
                + (destText.Contains("M") ? "1" : "0");
        }

        private string GetJump(string jumpCondition)
        {
            return jumps[jumpCondition];
        }

        static readonly Dictionary<string, string> commands = new Dictionary<string, string>
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
