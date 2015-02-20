using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Table = System.Collections.Generic.Dictionary<string, JackCompiler.SymbolTable.SymbolInfo>;

namespace JackCompiler
{
    class SymbolTable
    {

        Table classTable = new Table();
        Dictionary<string, Table> subTables = new Dictionary<string, Table>();
        Table subTable;

        public void StartSubroutine(string name)
        {
            if (!subTables.TryGetValue(name, out subTable))
            {
                subTables.Add(name, subTable = new Table());
            }
        }

        public int Define(string name, string type, string kind)
        {
            if (kind == "NONE")
                return 0;
            var index = VarCount(kind);
            var symbol = new SymbolInfo(type, kind, index);
            GetTable(kind).Add(name, symbol);
            return index;
        }


        public int VarCount(string kind)
        {
            if (kind == "NONE")
                return 0;
            if (kind == null)
                throw new Exception();
            return GetTable(kind).Values.Where(s => s.Kind == kind).Count();
        }


        public string TypeOf(string name)
        {
            return GetSymbol(name).Type;
        }


        public int IndexOf(string name)
        {
            return GetSymbol(name).Index;
        }

        public string Kind(string name)
        {
            var symbol = GetSymbol(name);
            return symbol != null ? symbol.Kind : "NONE";
        }

        SymbolInfo GetSymbol(string name)
        {
            SymbolInfo symbol;
            if (subTable != null && subTable.TryGetValue(name, out symbol))
                return symbol;
            if (classTable.TryGetValue(name, out symbol))
                return symbol;
            return null;
        }

        Table GetTable(string kind)
        {
            if (kind == "STATIC" || kind == "FIELD")
                return classTable;
            if (kind == "ARG" || kind == "VAR")
                return subTable;
            else
                return null;

        }


        public class SymbolInfo
        {
            public SymbolInfo(string type, string kind, int index)
            {
                Type = type;
                Index = index;
                Kind = kind;
            }

            public string Type { get; private set; }
            public int Index { get; private set; }

            public string Kind { get; private set; }
        }
    }
}
