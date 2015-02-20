using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class SymbolTableBuilder
    {
        SymbolTable table;

        public SymbolTableBuilder()
        {
            table = new SymbolTable();
        }

        public SymbolTable BuildTable(Element tree)
        {
            Build(tree);
            return table;
        }

        void Define(Element name, string type, string kind)
        {
            var index = table.Define(name.Value, type, kind);
            var id = (Identifier)name;
            id.BeingDefined = true;
            id.EType = type;
            id.Kind = kind;
            id.Index = index;
        }

        void Build(Element element)
        {
            var childElements = element.ChildElments;
            Func<int, string> childVal = i => childElements[i].Value;

            switch (element.TType)
            {
                case "class":
                    var nameEl = childElements[1];
                    Define(nameEl, nameEl.Value, "NONE");
                    break;
                case "classVarDec":
                    for (int i = 2; i < childElements.Length; i = i + 2)
                    {
                        var kind = childVal(0).ToUpper();
                        Define(childElements[i], childVal(1), kind);
                    }
                    break;
                case "subroutineDec":
                    table.StartSubroutine(childVal(2));
                    Define(childElements[2], childVal(1), "NONE");
                    if (childElements[0].Value == "method")
                        Define(new Identifier("<this>"), "<null>", "ARG");
                    break;
                case "parameterList":
                    for (int i = 0; i < childElements.Length; i = i + 3)
                    {
                        Define(childElements[i + 1], childVal(i), "ARG");
                    }
                    break;
                case "varDec":
                    for (int i = 2; i < childElements.Length; i = i + 2)
                        Define(childElements[i], childVal(1), "VAR");
                    break;
            }
            foreach (var child in childElements)
                Build(child);
        }

        public void PopulateIdentifiers(Element element, Element parent = null)
        {
            if (element.TType == "subroutineDec")
                table.StartSubroutine(element.ChildElments[2].Value);

            if (element.TType == "identifier")
            {
                var id = (Identifier)element;
                if (!id.BeingDefined)
                {
                    var name = id.Value;
                    id.Kind = table.Kind(name);
                    if (id.Kind != "NONE")
                    {
                        id.EType = table.TypeOf(name);
                        id.Index = table.IndexOf(name);
                    }
                }
                else if (id.Kind == "NONE")
                {
                    if (parent.TType == "subroutineDec")
                    {
                        var subroutineDec = (NonTerminal)parent;
                        var subroutineName = subroutineDec.ChildElments[2].Value;
                        var varCount = table.VarCount("VAR");
                        subroutineDec.SetAdditionalAttribute("VarCount", varCount);
                        if (subroutineName == "new")
                        {
                            var fieldCount = table.VarCount("FIELD");
                            subroutineDec.SetAdditionalAttribute("FieldCount", fieldCount);
                        }
                    }
                }
            }
            foreach (var child in element.ChildElments)
                PopulateIdentifiers(child, element);
        }

    }
}
