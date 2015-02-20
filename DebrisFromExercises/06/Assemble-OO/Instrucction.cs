using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assemble
{
    public class Instrucction
    {
        public Instrucction(Command command, string binary)
        {
            this.Command = command;
            this.Binary = binary;
        }

        public Command Command { get; private set; }
        public string Binary { get; private set; }

        public override string ToString()
        {
            return Binary + " " + Command;
        }
    }
}
