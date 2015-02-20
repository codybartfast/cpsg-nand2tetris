using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VMTranslator_CS
{
    class Parser
    {
        readonly string[] commandLines;
        public Parser(string path)
        {
            Console.WriteLine("Reading file: " + path);
            var lines = File.ReadAllLines(path);
            Console.WriteLine("Read {0:n0} lines.", lines.Length);
            commandLines = lines
                .Select(line => Regex.Replace(line, @"(\s*//.*$|^\s*|\s*$)", ""))
                .Where(line => ! string.IsNullOrWhiteSpace(line))
                .ToArray();
            Console.WriteLine("Got {0:n0} commands.", commandLines.Length);
        }

        public IEnumerable<Command> GetCommands(Context context)
        {
            return commandLines
                .Select(commandLine => Command.Build(context, commandLine));
        }

    }
}
