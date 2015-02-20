using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace JackCompiler
{
    abstract class Element
    {
        public string TType { protected set; get; }

        public abstract string Value { get; }

        public abstract Element[] ChildElments { get; }

        public abstract void WriteXml(Action<string> writeLine, bool includeSymbolInfo);

        public abstract void WriteJson(Action<string> writeLine, string prefix, bool includeSymbolInfo);

        public abstract string GetAdditonalAttribute(string name);
    }

    class Terminal : Element
    {
        protected readonly string value;
        public Terminal(string type, string value)
        {
            this.TType = type;
            this.value = value;
        }

        public override string Value
        {
            get { return value; }
        }

        public override Element[] ChildElments
        {
            get { return new Element[0]; }
        }

        public override void WriteXml(Action<string> writeLine, bool includeSymbolInfo)
        {
            writeLine(string.Format("<{0}> {1} </{0}>", TType, Tokenizer.XmlEncode(value)));
        }

        public override void WriteJson(Action<string> writeLine, string prefix, bool includeSymbolInfo)
        {
            writeLine(string.Format(@"{2}{{""Token"": ""{0}"", ""Value"": {1}}}", TType, Json.Encode(value), prefix));
        }

        public override string GetAdditonalAttribute(string name)
        {
            throw new Exception("Terminal elements don't support additional attributes");
        }
    }

    class Identifier : Terminal
    {
        public Identifier(string value)
            : base("identifier", value)
        { }

        public string EType { get; set; }
        public bool BeingDefined { get; set; }
        public int Index { get; set; }
        public string Kind { get; set; }

        public override void WriteXml(Action<string> writeLine, bool includeSymbolInfo)
        {
            if (includeSymbolInfo)
            {
                var line = string.Format(
                    "<{0} kind=\"{4}\" type=\"{2}\" index=\"{5}\"> {1} </{0}>",
                    TType, Tokenizer.XmlEncode(value),
                    EType, BeingDefined, Kind, Index);
                writeLine(line);
            }
            else
            {
                base.WriteXml(writeLine, includeSymbolInfo);
            }
        }

        public override void WriteJson(Action<string> writeLine, string prefix, bool includeSymbolInfo)
        {
            if (includeSymbolInfo)
            {
                var line = string.Format(
                   @"{6}{{""Token"": ""{0}"", ""Value"": ""{1}"", ""Kind"": ""{4}"", ""Type"": ""{2}"", ""Index"": ""{5}""}}",
                    TType, Tokenizer.XmlEncode(value),
                    EType, BeingDefined, Kind, Index, prefix);
                writeLine(line);
            }
            else
            {
                base.WriteJson(writeLine, null, includeSymbolInfo);
            }
        }
    }

    class NonTerminal : Element
    {
        public NonTerminal(string type)
        {
            this.TType = type;
        }

        public override string Value
        {
            get { throw new Exception("Cannot get 'Value' for a non-terminal element."); }
        }

        public void AddElement(Element element)
        {
            elements.Add(element);
        }

        List<Element> elements = new List<Element>();
        public void AddElement(Token token)
        {
            var terminal = token.Type != "identifier" ?
                new Terminal(token.Type, token.Value) : new Identifier(token.Value);
            elements.Add(terminal);
        }

        public override Element[] ChildElments
        {
            get { return elements.ToArray(); }
        }

        Dictionary<string, string> additionalAttributes = new Dictionary<string, string>();
        public void SetAdditionalAttribute(string name, string value)
        {
            additionalAttributes[name] = value;
        }

        public void SetAdditionalAttribute(string name, int value)
        {
            SetAdditionalAttribute(name, value.ToString());
        }

        public override string GetAdditonalAttribute(string name){
            return additionalAttributes[name];
        }

        public override void WriteXml(Action<string> writeLine, bool includeSymbolInfo)
        {
            Action<string> writeIndent = line => writeLine("  " + line);

            string additionalAttributeText = string.Concat(
                additionalAttributes.Select(kvp => " " + kvp.Key + "=" + kvp.Value));

            writeLine("<" + TType + additionalAttributeText + ">");
            foreach (var element in elements)
                element.WriteXml(writeIndent, includeSymbolInfo);
            writeLine("</" + TType + ">");
        }

        public override void WriteJson(Action<string> writeLine, string prefix, bool includeSymbolInfo)
        {
            Action<string> writeIndent = line => writeLine("   " + line);

            string additionalAttributeText = string.Concat(
                additionalAttributes.Select(kvp => @", """ + kvp.Key + @""": """ + kvp.Value + @""""));

            writeLine(string.Format(@"{2}{{""Token"": ""{0}""{1}, ""Children"": [", TType, additionalAttributeText, prefix));
            string childprefix = " ";
            foreach (var element in elements)
            {
                element.WriteJson(writeIndent, childprefix, includeSymbolInfo);
                childprefix = ",";
            }
            writeLine("]}");
        }

    }
}
