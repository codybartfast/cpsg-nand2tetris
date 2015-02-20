using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class CodeWriter
    {
        readonly Element tree;
        readonly FileInfo codeFile;
        StreamWriter writer;

        public CodeWriter(Element tree, FileInfo codeFile)
        {
            this.tree = tree;
            this.codeFile = codeFile;
        }

        public void WriteTree()
        {
            if (!codeFile.Directory.Exists)
            {
                codeFile.Directory.Create();
                var osDir = new DirectoryInfo(@"C:\STUFF\Code\Repos\FSS\Nand2Tetris\11\originals\OS");
                if (osDir.Exists)
                    foreach (var osFile in osDir.GetFiles())
                        osFile.CopyTo(Path.Combine(codeFile.Directory.FullName, osFile.Name));
            }

            using (var stream = codeFile.OpenWrite())
            using (writer = new StreamWriter(stream))
            {
                DoClass(tree);
                writer.Flush();
                stream.Flush();
                stream.SetLength(stream.Position);
            }
        }

        void Write(params object[] parts)
        {
            writer.WriteLine(string.Concat(parts));
        }

        string className;
        void DoClass(Element element)
        {
            if (element.TType != "class")
                throw new Exception("Expected a class.");
            className = element.ChildElments[1].Value;

            foreach (var child in element.ChildElments.Where(ce => ce.TType == "subroutineDec"))
            {
                DoSubroutineDec(child);
            }
        }


        void DoSubroutineDec(Element subroutine)
        {
            var els = subroutine.ChildElments;
            var name = els[2].Value;
            var varCount = subroutine.GetAdditonalAttribute("VarCount");
            Write("function ", className, ".", name, " ", varCount);

            var routineType = els[0].Value;
            if (routineType == "function")
            {
                // nothing special to do.
            }
            else if (routineType == "constructor")
            {
                var fieldCount = subroutine.GetAdditonalAttribute("FieldCount");
                Write("push constant ", fieldCount);
                Write("call Memory.alloc 1");
                Write("pop pointer 0");
            }
            else if (routineType == "method")
            {
                Write("push argument 0");
                Write("pop pointer 0");
            }
            else
            {
                throw new Exception("Don't know how to handle routine of type: " + routineType);
            }
            var bodyElements = subroutine.ChildElments[6].ChildElments;
            var statements = bodyElements[bodyElements.Length - 2];
            DoStatements(statements);
        }

        void DoStatements(Element statements)
        {
            foreach (var statement in statements.ChildElments)
            {
                switch (statement.TType)
                {
                    case "doStatement":
                        DoDo(statement);
                        break;
                    case "letStatement":
                        DoLet(statement);
                        break;
                    case "whileStatement":
                        DoWhile(statement);
                        break;
                    case "ifStatement":
                        DoIf(statement);
                        break;
                    case "returnStatement":
                        DoReturn(statement);
                        break;
                    default:
                        throw new Exception("Unexpected statement type: " + statement.TType);
                }
            }
        }

        void DoDo(Element doStatement)
        {
            var els = doStatement.ChildElments;
            var subroutineCall = els.Skip(1).Take(els.Length - 2).ToArray();
            DoSubroutineCall(subroutineCall);
            Write("pop temp 0");
        }

        void DoSubroutineCall(Element[] subroutineCall)
        {
            var els = subroutineCall;
            var expressionList = els[els.Length - 2];
            var name1 = (Identifier)els[0];
            string nameText;
            var haveDot = els[1].Value == ".";
            bool isMethodCall;
            var kind = name1.Kind;

            if (haveDot && kind == "NONE")
            {
                // function call
                isMethodCall = false;
                nameText = name1.Value + "." + els[2].Value;

            }
            else if (haveDot)
            {
                // method call on another object
                isMethodCall = true;
                var segment = segmentLookup[name1.Kind];
                Write("push ", segment, " ", name1.Index);
                nameText = name1.EType + "." + els[2].Value;
            }
            else
            {
                // method call on 'this'
                isMethodCall = true;
                Write("push pointer 0");
                nameText = className + "." + name1.Value;
            }

            var argCount = DoExpressionList(expressionList);
            if (isMethodCall)
                argCount++;
            Write("call ", nameText, " ", argCount);
        }

        void DoLet(Element letStatement)
        {
            var els = letStatement.ChildElments;
            var name1 = (Identifier)els[1];
            var expression = els[els.Length - 2];
            DoExpression(expression);
            if (els[2].Value == "=")
                Write("pop ", segmentLookup[name1.Kind], " ", name1.Index);
            else
            {
                DoExpression(els[3]);
                Write("push ", segmentLookup[name1.Kind], " ", name1.Index);
                Write("add");
                Write("pop pointer 1");
                Write("pop that 0");
            }
        }

        int DoExpressionList(Element expressionList)
        {
            int argCount = 0;
            foreach (var el in expressionList.ChildElments)
            {
                if (el.TType == "expression")
                {
                    argCount++;
                    DoExpression(el);
                }
            }
            return argCount;
        }


        void DoExpression(Element expression)
        {
            var children = expression.ChildElments;
            DoTerm(children[0]);
            for (int i = 1; i < children.Length; i = i + 2)
            {
                DoTerm(children[i + 1]);
                DoOp(children[i]);
            }
        }

        void DoTerm(Element term)
        {
            var els = term.ChildElments;
            var el0 = els[0];

            switch (el0.TType)
            {
                case "integerConstant":
                    Write("push constant ", el0.Value);
                    break;
                case "identifier":
                    DoIdentifier(term);
                    break;
                case "symbol":
                    DoSymbol(term);
                    break;
                case "keyword":
                    DoKeyword(term);
                    break;
                case "stringConstant":
                    DoStringConstant(term);
                    break;
                default:
                    throw new Exception("Don't know how to handle Term of type: " + el0.TType);
            }
        }

        void DoIdentifier(Element identifier)
        {
            var els = identifier.ChildElments;
            var el0 = els[0];

            var haveArray = els.Length > 1 && els[1].Value == "[";
            var haveDot = els.Length > 1 && els[1].Value == ".";

            var id = (Identifier)el0;
            var kind = id.Kind;

            if (haveDot || kind == "NONE")
            {
                DoSubroutineCall(els);
            }
            else
            {
                var segment = segmentLookup[kind];
                Write("push ", segment, " ", id.Index);
            }

            if (haveArray)
            {
                var arrayIndex = els[2];
                DoExpression(arrayIndex);
                Write("add");
                Write("pop pointer 1");
                Write("push that 0");
            }
        }

        void DoSymbol(Element symbol)
        {
            var els = symbol.ChildElments;
            var el0 = els[0];

            switch (el0.Value)
            {
                case "(":
                    DoExpression(els[1]);
                    break;
                case "-":
                    DoTerm(els[1]);
                    Write("neg");
                    break;
                case "~":
                    DoTerm(els[1]);
                    Write("not");
                    break;
                default:
                    throw new Exception("Didn't expect " + el0.Value);
            }

        }

        void DoKeyword(Element keyword)
        {
            var els = keyword.ChildElments;
            var el0 = els[0];

            switch (el0.Value)
            {
                case "true":
                    Write("push constant 1");
                    Write("neg");
                    break;
                case "false":
                    Write("push constant 0");
                    break;
                case "null":
                    Write("push constant 0");
                    break;
                case "this":
                    Write("push pointer 0");
                    break;
                default:
                    throw new Exception("Don't know how to handle keyword: " + el0.Value);
            }

        }

        void DoStringConstant(Element stringConstant)
        {
            var els = stringConstant.ChildElments;
            var el0 = els[0];

            var text = el0.Value;
            var len = text.Length;
            Write("push constant ", len);
            Write("call String.new 1");
            foreach (var character in text)
            {
                Write("push constant ", (byte)character);
                Write("call String.appendChar 2");
            }
        }

        void DoOp(Element op)
        {
            Write(opLookup[op.Value]);
        }

        int labeSuffix;
        public string GetNewLabelSuffix()
        {
            return (labeSuffix++).ToString();
        }


        void DoWhile(Element whileStatement)
        {
            var els = whileStatement.ChildElments;
            var expression = els[2];
            var statements = els[5];
            var labelSuffix = GetNewLabelSuffix();
            var startLabel = "WHILE_START." + labelSuffix;
            var endLabel = "WHILE_END." + labelSuffix;

            Write("label " + startLabel);
            DoExpression(expression);
            Write("not");
            Write("if-goto " + endLabel);
            DoStatements(statements);
            Write("goto " + startLabel);
            Write("label " + endLabel);
        }

        void DoIf(Element ifStatement)
        {
            var els = ifStatement.ChildElments;
            var expression = els[2];
            var statements = els[5];
            var elseStatements = els.Length > 7 ?
                els[9] : null;
            var labelSuffix = GetNewLabelSuffix();
            var bodyLabel = "IF_BODY." + labelSuffix;
            var endLabel = "IF_END." + labelSuffix;

            DoExpression(expression);
            Write("if-goto ", bodyLabel);
            if (elseStatements != null)
                DoStatements(elseStatements);
            Write("goto ", endLabel);
            Write("label ", bodyLabel);
            DoStatements(statements);
            Write("label ", endLabel);
        }

        void DoReturn(Element returnStatement)
        {
            var els = returnStatement.ChildElments;
            if (els.Length > 2)
            {
                DoExpression(els[1]);
            }
            else
            {
                Write("push constant 0");
            }
            Write("return");
        }

        Dictionary<string, string> segmentLookup = new Dictionary<string, string>
        {
            {"VAR", "local"},
            {"ARG", "argument"},
            {"FIELD", "this"},
            {"STATIC", "static"},
        };

        Dictionary<string, string> opLookup = new Dictionary<string, string>{
            {"+", "add"},
            {"-", "sub"},
            {"*", "call Math.multiply 2"},
            {"/", "call Math.divide 2"},
            {"&", "and"},
            {"|", "or"},
            {"<", "lt"},
            {">", "gt"},
            {"=", "eq"},
        };


    }


}
