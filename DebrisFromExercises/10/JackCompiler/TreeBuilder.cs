using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JackCompiler
{
    class TreeBuilder
    {
        public static Element Build(IEnumerable<Token> tokens)
        {
            return new TreeBuilder(tokens).Build();
        }

        readonly Queue<Token> tokens;
        readonly SymbolTable symbolTable = new SymbolTable();

        public TreeBuilder(IEnumerable<Token> tokens)
        {
            this.tokens = new Queue<Token>(tokens);
        }

        Element Build()
        {
            return BuildClass();
        }

        Token Next
        {
            get { return tokens.Peek(); }
        }

        Token PopKeyword(params string[] values)
        {
            return Pop("keyword", values);
        }
        Token PopIdentifier(params string[] values)
        {
            return Pop("identifier", values);
        }

        Token PopSymbol(params string[] values)
        {
            return Pop("symbol", values);
        }

        Token Pop(string type = null, string[] values = null)
        {
            var token = tokens.Dequeue();
            if (type != null && token.Type != type)
                throw new Exception(string.Format("Expected token of type '{0}' but got '{1}'", type, token.Type));
            if (values != null && values.Any() && !values.Contains(token.Value))
                throw new Exception(string.Format("Expected token of value '{0}' but got '{1}'", string.Join("|", values), token.Value));
            return token;
        }

        Element BuildClass()
        {
            var el = new NonTerminal("class");
            el.AddElement(PopKeyword("class"));
            el.AddElement(PopIdentifier());
            el.AddElement(PopSymbol("{"));
            while (Regex.IsMatch(Next.Value, "field|static"))
                el.AddElement(BuildClassVarDec());
            while (new[] { "constructor", "function", "method" }.Contains(Next.Value))
                el.AddElement(BuildSubroutineDec());
            el.AddElement(PopSymbol("}"));
            if (tokens.Any())
                throw new Exception("Only expected one Class per file.");
            return el;
        }


        Element BuildClassVarDec()
        {
            var el = new NonTerminal("classVarDec");
            el.AddElement(PopKeyword("static", "field"));
            el.AddElement(Pop());
            el.AddElement(PopIdentifier());
            while (Next.Value == ",")
            {
                el.AddElement(PopSymbol(","));
                el.AddElement(PopIdentifier());
            }
            el.AddElement(PopSymbol(";"));
            return el;
        }


        Element BuildSubroutineDec()
        {
            var el = new NonTerminal("subroutineDec");
            el.AddElement(PopKeyword("constructor", "function", "method"));
            el.AddElement(Pop());
            el.AddElement(PopIdentifier());
            el.AddElement(PopSymbol("("));
            el.AddElement(BuildParameterList());
            el.AddElement(PopSymbol(")"));
            el.AddElement(BuildSubroutineBody());
            return el;
        }

        Element BuildParameterList()
        {
            var el = new NonTerminal("parameterList");
            bool firstParam = true;
            while (Next.Value != ")")
            {
                if (firstParam)
                    firstParam = false;
                else
                    el.AddElement(PopSymbol(","));
                el.AddElement(Pop());
                el.AddElement(PopIdentifier());
            }
            return el;
        }

        Element BuildSubroutineBody()
        {
            var el = new NonTerminal("subroutineBody");
            el.AddElement(PopSymbol("{"));
            while (Next.Value == "var")
                el.AddElement(BuildVarDec());
            el.AddElement(BuildStatements());
            el.AddElement(PopSymbol("}"));
            return el;
        }


        Element BuildVarDec()
        {
            var el = new NonTerminal("varDec");
            el.AddElement(PopKeyword("var"));
            el.AddElement(Pop());
            el.AddElement(PopIdentifier());
            while (Next.Value == ",")
            {
                el.AddElement(PopSymbol(","));
                el.AddElement(PopIdentifier());
            }
            el.AddElement(PopSymbol(";"));

            return el;
        }

        Element BuildStatements()
        {
            var el = new NonTerminal("statements");
            while (Next.Value != "}")
                el.AddElement(BuildStatement());
            return el;
        }

        Element BuildStatement()
        {
            switch (Next.Value)
            {
                case "let":
                    return BuildLetStatement();
                case "if":
                    return BuildIfStatement();
                case "while":
                    return BuildWhileStatement();
                case "do":
                    return BuildDoStatement();
                case "return":
                    return BuildReturnStatement();
                default:
                    throw new Exception("Expected let, if, while, do, return. Got: " + Next.Value);
            }
        }

        Element BuildLetStatement()
        {
            var el = new NonTerminal("letStatement");
            el.AddElement(PopKeyword("let"));
            el.AddElement(PopIdentifier());
            if (Next.Value == "[")
            {
                el.AddElement(PopSymbol("["));
                el.AddElement(BuildExpression());
                el.AddElement(PopSymbol("]"));
            }
            el.AddElement(PopSymbol("="));
            el.AddElement(BuildExpression());
            el.AddElement(PopSymbol(";"));

            return el;
        }

        Element BuildExpression()
        {
            var el = new NonTerminal("expression");
            el.AddElement(BuildTerm());
            while (@"+-*/&|<>=".Contains(Next.Value))
            {
                el.AddElement(PopSymbol());
                el.AddElement(BuildTerm());
            }
            return el;
        }

        Element BuildTerm()
        {
            var el = new NonTerminal("term");
            switch (Next.Type)
            {
                case "integerConstant":
                    el.AddElement(Pop());
                    break;
                case "stringConstant":
                    el.AddElement(Pop());
                    break;
                case "keyword":
                    el.AddElement(PopKeyword("true", "false", "null", "this"));
                    break;
                case "identifier":
                    el.AddElement(PopIdentifier());
                    if (Next.Value == "[")
                    {
                        el.AddElement(PopSymbol("["));
                        el.AddElement(BuildExpression());
                        el.AddElement(PopSymbol("]"));
                    }
                    if (Next.Value == ".")
                    {
                        el.AddElement(PopSymbol("."));
                        el.AddElement(PopIdentifier());
                    }
                    if (Next.Value == "(")
                    {
                        el.AddElement(PopSymbol("("));
                        el.AddElement(BuildExpressionList());
                        el.AddElement(PopSymbol(")"));
                    }
                    break;
                case "symbol":
                    if (Next.Value == "(")
                    {
                        el.AddElement(PopSymbol("("));
                        el.AddElement(BuildExpression());
                        el.AddElement(PopSymbol(")"));
                    }
                    else
                    {
                        el.AddElement(PopSymbol("-", "~"));
                        el.AddElement(BuildTerm());
                    }
                    break;
                default:
                    throw new Exception("Unexpected token type in term: " + Next.Type);
            }
            return el;
        }

        Element BuildExpressionList()
        {
            var el = new NonTerminal("expressionList");
            if (Next.Value != ")")
            {
                el.AddElement(BuildExpression());
                while (Next.Value == ",")
                {
                    el.AddElement(PopSymbol(","));
                    el.AddElement(BuildExpression());
                }
            }
            return el;
        }


        Element BuildIfStatement()
        {
            var el = new NonTerminal("ifStatement");
            el.AddElement(PopKeyword("if"));
            el.AddElement(PopSymbol("("));
            el.AddElement(BuildExpression());
            el.AddElement(PopSymbol(")"));
            AddStatements(el);
            if (Next.Value == "else")
            {
                el.AddElement(PopKeyword("else"));
                AddStatements(el);
            }
            return el;
        }

        void AddStatements(NonTerminal el)
        {
            el.AddElement(PopSymbol("{"));
            el.AddElement(BuildStatements());
            el.AddElement(PopSymbol("}"));
        }

        Element BuildWhileStatement()
        {
            var el = new NonTerminal("whileStatement");
            el.AddElement(PopKeyword("while"));
            el.AddElement(PopSymbol("("));
            el.AddElement(BuildExpression());
            el.AddElement(PopSymbol(")"));
            AddStatements(el);
            return el;
        }

        Element BuildDoStatement()
        {
            var el = new NonTerminal("doStatement");
            el.AddElement(PopKeyword("do"));
            el.AddElement(PopIdentifier());
            if (Next.Value != "(")
            {
                el.AddElement(PopSymbol("."));
                el.AddElement(PopIdentifier());
            }
            el.AddElement(PopSymbol("("));
            el.AddElement(BuildExpressionList());
            el.AddElement(PopSymbol(")"));
            el.AddElement(PopSymbol(";"));
            return el;
        }

        Element BuildReturnStatement()
        {
            var el = new NonTerminal("returnStatement");
            el.AddElement(PopKeyword("return"));
            if (Next.Value != ";")
                el.AddElement(BuildExpression());
            el.AddElement(PopSymbol(";"));
            return el;
        }


    }
}
