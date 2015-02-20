using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JackCompiler
{
    class Tokenizer
    {
        static string pattern = @"  
            [][{}().,;+*/&|<>=~-]    # Symbols
            | ""[^""]*""             # String Literal
            | \w+                    # the rest
        ";
        static Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        public static IEnumerable<string> GetValues(string source)
        {
            return GetQuotedValues(source).Select(t => t.Trim('"'));
        }
        

        public static IEnumerable<Token> GetTokens(string source)
        {
            var quotedTokens =  GetQuotedValues(source).ToArray();
            var tokens  = quotedTokens.Select(token => 
                new Token
                {
                    Type = GetTokenType(token),
                    Value = token.Trim('"'),
                }).ToArray() ;
            return tokens;
        }

        static IEnumerable<string> GetQuotedValues(string source)
        {
            for (var match = regex.Match(source); match.Success; match = match.NextMatch())
            {
                yield return match.Value;
            }
        }

        public static string GetTokenType(string token)
        {
            if (Regex.IsMatch(token, @"^[][{}().,;+*/&|<>=~-]$"))
                return "symbol";
            if (Regex.IsMatch(token, @"^\d"))
                return "integerConstant";
            else if (token.StartsWith("\""))
                return "stringConstant";
            else if (Regex.IsMatch(token, @"^(class|constructor|function|method|field|static|var|int|char|boolean|void|true|false|null|this|let|do|if|else|while|return)$"))
                return "keyword";
            else
                return "identifier";
        }

        public static string XmlEncode(string text)
        {
            return Regex.Replace(text, @"[<>&""]", c =>
            {
                switch (c.Value)
                {
                    case "<":
                        return "&lt;";
                    case ">":
                        return "&gt;";
                    case "&":
                        return "&amp;";
                    case "\"":
                        return "&qt;";
                    default:
                        throw new Exception("Unexpected Character: " + c);
                }
            });
        }

     }

}
