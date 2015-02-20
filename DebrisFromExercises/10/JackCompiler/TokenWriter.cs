using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class TokenWriter
    {
        public static void ToConsole(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
                Console.WriteLine(token.Type.PadRight(16) + token.Value);
        }

        public static void AsFlatXml(IEnumerable<Token> tokens, FileInfo outFile)
        {
            using (var outStream = outFile.OpenWrite())
            using (var writer = new StreamWriter(outStream))
            {
                writer.WriteLine("<tokens>");
                foreach (var token in tokens)
                {
                    var xmlToken = Tokenizer.XmlEncode(token.Value);

                    writer.WriteLine(string.Format("<{0}> {1} </{0}>",
                        token.Type, xmlToken));
                }
                writer.WriteLine("</tokens>");
                writer.Flush();
                outStream.Flush();
                outStream.SetLength(outStream.Position);
            }
        }

        public static void AsXml(Element tree, FileInfo outFile, bool includeSymbolInfo = false)
        {

            using (var outStream = outFile.OpenWrite())
            using (var writer = new StreamWriter(outStream))
            {
                tree.WriteXml(writer.WriteLine, includeSymbolInfo);
                writer.Flush();
                outStream.Flush();
                outStream.SetLength(outStream.Position);
            }
        }

        public static void AsJson(Element tree, FileInfo outFile, bool includeSymbolInfo = true)
        {

            using (var outStream = outFile.OpenWrite())
            using (var writer = new StreamWriter(outStream))
            {
                tree.WriteJson(writer.WriteLine, null, includeSymbolInfo);
                writer.Flush();
                outStream.Flush();
                outStream.SetLength(outStream.Position);
            }
        }


    }
}
