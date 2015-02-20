using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JackCompiler
{
    class FileReader
    {
        public static string GetContent(FileInfo file)
        {
            string text;
            using(var reader = file.OpenText()){
                text = reader.ReadToEnd();
            }
            text = Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline);
            text = Regex.Replace(text, @"//.*", "");
            return text;         }
    }
}
