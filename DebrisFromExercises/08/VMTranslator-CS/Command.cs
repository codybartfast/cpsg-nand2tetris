using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VMTranslator_CS
{

    class Context
    {
        public Context(string filePath)
        {
            FileId = Regex.Replace(filePath, @"^.*\\|\w+$", "");
        }

        public string FileId { get; private set; }

        int labeSuffix;
        public string GetLabelSuffix()
        {
            return (labeSuffix++).ToString();
        }

        public string GetQualifiedFunctionName(string functionName)
        {
            //return FileId + "." + functionName;
            return functionName;
        }

        string currentFunction;
        public void StartFunction(string functionName)
        {
            this.currentFunction = GetQualifiedFunctionName(functionName);
        }

        public string GetAssemblyLabelName(string vmLabelName)
        {
            return currentFunction == null ?
                vmLabelName :
                currentFunction + "$" + vmLabelName;
        }

        //public void FunctionEnd()
        //{
        //    if (currentFunction == null)
        //        throw new Exception("Was asked end function when there was no current function");
        //    currentFunction = null;
        //}

    }

    abstract class Command
    {
        public static Command Build(Context context, string commandLine)
        {
            var args = Regex.Split(commandLine, @"\s+");
            var argCount = args.Length;
            var commandName = args[0];
            switch (argCount)
            {
                case 1:
                    switch (commandName)
                    {
                        case "add":
                            return new ArithmeticCommand(context, commandName);
                        case "eq":
                            return new ArithmeticCommand(context, commandName);
                        case "lt":
                            return new ArithmeticCommand(context, commandName);
                        case "gt":
                            return new ArithmeticCommand(context, commandName);
                        case "sub":
                            return new ArithmeticCommand(context, commandName);
                        case "and":
                            return new ArithmeticCommand(context, commandName);
                        case "or":
                            return new ArithmeticCommand(context, commandName);
                        case "not":
                            return new ArithmeticCommand(context, commandName);
                        case "neg":
                            return new ArithmeticCommand(context, commandName);
                        case "return":
                            return new ReturnCommand(context);
                        default:
                            throw new Exception(string.Format("Got 1 Arguments but first was unrecognised: {0}", context, commandName));
                    }
                case 2:
                    switch (commandName)
                    {
                        case "label":
                            return new LabelCommand(context, args[1]);
                        case "goto":
                            return new GotoCommand(context, args[1]);
                        case "if-goto":
                            return new IfGotoCommand(context, args[1]);
                        default:
                            throw new Exception(string.Format("Got 2 Arguments but first was unrecognised: {0}", commandName));
                    }
                case 3:
                    switch (commandName)
                    {
                        case "pop":
                            return new PopCommand(context, args[1], args[2]);
                        case "push":
                            return new PushCommand(context, args[1], args[2]);
                        case "function":
                            return new FunctionCommand(context, args[1], args[2]);
                        case "call":
                            return new CallCommand(context, args[1], args[2]);
                        default:
                            throw new Exception(string.Format("Got 3 Arguments but first was unrecognised: {0}", commandName));
                    }
                default:
                    throw new Exception(string.Format("Unexpected number of command arguments: {0} from command: {1}", argCount, commandLine));
            }
        }

        protected readonly Context context;
        public Command(Context context)
        {
            this.context = context;
        }

        public IEnumerable<string> GetAssemblyInstructions()
        {
            AddInstructions();
            return instructions;
        }
        protected abstract void AddInstructions();

        List<string> instructions = new List<string>();
        protected void Add(params string[] assemblyInstuctions)
        {
            foreach (var instruction in assemblyInstuctions)
                instructions.Add(instruction);
        }

        protected Dictionary<string, string> segmentRegisters = new Dictionary<string, string>
        {
            {"local", "LCL"},
            {"argument", "ARG"},
            {"this", "THIS"},
            {"that", "THAT"},
            {"temp", "5"},
            {"pointer", "3"}

        };

        protected void Pop()
        {
            Add(
                "@SP",
                "M=M-1",
                "A=M"
            );
        }

        protected void SetD()
        {
            Add("D=M");

        }

        protected void PushD()
        {
            Add(
                "@SP",
                "A=M",
                "M=D",
                "@SP",
                "M=M+1"
            );
        }

        protected void CalculateAddress(string segment, string index)
        {
            string register;
            if (!segmentRegisters.TryGetValue(segment, out register))
                throw new Exception("Don't recognise segment: " + segment);

            var dontDereference = new[] { "temp", "pointer" }.Contains(segment);

            Add(
                "@" + register,
                dontDereference ? "D=A" : "D=M",
                "@" + index,
                "D=D+A"
            );
        }

        protected void StoreR13()
        {
            Add(
                "@13",
                "M=D"
            );
        }

        protected void LoadAddressInR13()
        {
            Add(
                "@R13",
                "A=M"
            );
        }

    }

    class PushCommand : Command
    {
        readonly string segment;
        readonly string index;

        public PushCommand(Context context, string segment, string index)
            : base(context)
        {
            this.segment = segment;
            this.index = index;
        }

        protected override void AddInstructions()
        {

            if (segment == "constant")
            {
                Add(
                    "@" + int.Parse(index),
                    "D=A",
                    "@SP",
                    "A=M",
                    "M=D",
                    "@SP",
                    "M=M+1"
                );
                return;
            }

            if (segment == "static")
            {
                Add(
                    "@" + context.FileId + index,
                    "D=M"
                );
                PushD();
                return;
            }

            CalculateAddress(segment, index);
            Add("A=D");
            Add("D=M");
            PushD();

        }

    }

    class PopCommand : Command
    {
        readonly string segment;
        readonly string index;

        public PopCommand(Context context, string segment, string index)
            : base(context)
        {
            this.segment = segment;
            this.index = index;
        }


        protected override void AddInstructions()
        {
            if (segment == "static")
            {
                Pop();
                SetD();
                Add(
                    "@" + context.FileId + index,
                    "M=D"
                );
                return;
            }

            CalculateAddress(segment, index);
            StoreR13();
            Pop();
            SetD();
            LoadAddressInR13();
            Add("M=D");
        }

    }

    class ArithmeticCommand : Command
    {
        readonly string operation;
        public ArithmeticCommand(Context context, string operation)
            : base(context)
        {
            this.operation = operation;
        }


        protected override void AddInstructions()
        {
            switch (operation)
            {
                case "add":
                    Pop();
                    SetD();
                    Pop();
                    Add("D=D+M");
                    PushD();
                    break;
                case "sub":
                    SubWithoutPush();
                    PushD();
                    break;
                case "or":
                    Pop();
                    SetD();
                    Pop();
                    Add("D=D|M");
                    PushD();
                    break;
                case "and":
                    Pop();
                    SetD();
                    Pop();
                    Add("D=D&M");
                    PushD();
                    break;
                case "not":
                    Pop();
                    SetD();
                    Add("D=!D");
                    PushD();
                    break;
                case "neg":
                    Pop();
                    Add("D=-D");
                    PushD();
                    break;
                case "eq":
                    Compare("JEQ", context.GetLabelSuffix);
                    break;
                case "lt":
                    Compare("JLT", context.GetLabelSuffix);
                    break;
                case "gt":
                    Compare("JGT", context.GetLabelSuffix);
                    break;
                default:
                    throw new Exception("Don't know how get assembly for arithmetic operation: " + operation);
            }
        }

        void Compare(string comparison, Func<string> getLabelSuffix)
        {
            var labelSuffix = getLabelSuffix();
            SubWithoutPush();
            Add("@true." + labelSuffix);
            Add("D;" + comparison);
            Add("D=0");
            Add("@continue." + labelSuffix);
            Add("0;JMP");
            Add("(true." + labelSuffix + ")");
            Add("D=-1");
            Add("(continue." + labelSuffix + ")");
            PushD();
        }


        void SubWithoutPush()
        {
            Pop();
            SetD();
            Pop();
            Add("D=M-D");
        }

    }

    class LabelCommand : Command
    {
        readonly string label;
        public LabelCommand(Context context, string label)
            : base(context)
        {
            this.label = context.GetAssemblyLabelName(label);
        }
        protected override void AddInstructions()
        {
            Add("(" + label + ")");
        }
    }

    class GotoCommand : Command
    {
        readonly string label;

        public GotoCommand(Context context, string label)
            : base(context)
        {
            this.label = label;
        }

        protected override void AddInstructions()
        {
            Add("@" + context.GetAssemblyLabelName(label));
            Add("0;JMP");
        }
    }

    class IfGotoCommand : Command
    {
        readonly string label;

        public IfGotoCommand(Context context, string label)
            : base(context)
        {
            this.label = label;
        }

        protected override void AddInstructions()
        {
            Pop();
            SetD();
            Add("@" +  context.GetAssemblyLabelName(label));
            Add("D;JNE");
        }
    }

    class FunctionCommand : Command
    {
        readonly string name;
        readonly int localVariableCount;

        public FunctionCommand(Context context, string name, string localVariableCount)
            : base(context)
        {
            this.name = name;
            this.localVariableCount = int.Parse(localVariableCount);
        }

        protected override void AddInstructions()
        {
            context.StartFunction(name);
            Add("(" + name + ")");
            Add("@0");
            Add("D=A");
            for (int i = 0; i < localVariableCount; i++)
                PushD();
        }
    }

    class ReturnCommand : Command
    {
        public ReturnCommand(Context context)
            : base(context)
        { }

        protected override void AddInstructions()
        {
            // put return address in R14
            Add("@LCL");
            Add("D=M");
            Add("@5");
            Add("A=D-A");
            Add("D=M");
            Add("@R14");
            Add("M=D");

            // put callers SP in R13
            Add("@ARG");
            SetD();
            Add("@R13");
            Add("M=D+1");

            // Put return value at top of caller's stack
            Pop();
            SetD();
            Add("@ARG");
            Add("A=M");
            Add("M=D");

            // Move SP to end of caller's pointers
            Add("@LCL");
            Add("D=M");
            Add("@SP");
            Add("M=D");

            // restore caller's pointers
            PopAndSave("@THAT");
            PopAndSave("@THIS");
            PopAndSave("@ARG");
            PopAndSave("@LCL");

            //// put return address in R14
            //PopAndSave("@R14");

            // Restore Caller's SP
            Add("@R13");
            SetD();
            Add("@SP");
            Add("M=D");

            Add("@R14");
            Add("A=M");
            Add("0;JMP");

            //context.FunctionEnd();
        }

        void PopAndSave(string saveTo)
        {
            Pop();
            SetD();
            Add(saveTo);
            Add("M=D");
        }
    }

    class CallCommand : Command
    {
        readonly string name;
        readonly string argCount;
        public CallCommand(Context context, string name, string argCount)
            : base(context)
        {
            this.name = name;
            this.argCount = argCount;
        }

        protected override void AddInstructions()
        {
            // push return address
            var returnLabel = "return." + context.GetLabelSuffix();
            Add("@" + returnLabel);
            Add("D=A");
            PushD();

            // push local pointers
            Add("@LCL"); SetD(); PushD();
            Add("@ARG"); SetD(); PushD();
            Add("@THIS"); SetD(); PushD();
            Add("@THAT"); SetD(); PushD();

            // set function's ARG
            Add("@SP");
            Add("D=M");
            Add("@5");
            Add("D=D-A");
            Add("@" + argCount);
            Add("D=D-A");
            Add("@ARG");
            Add("M=D");

            // LCL = SP
            Add("@SP");
            Add("D=M");
            Add("@LCL");
            Add("M=D");

            // goto function
            var functionLabel = context.GetQualifiedFunctionName(name);
            Add("@" + functionLabel);
            Add("0;JMP");

            // return address
            Add("(" + returnLabel + ")");

        }
    }
}
