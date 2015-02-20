using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JackCompiler
{
    class FileManager
    {
        readonly List<FileSet> sets;

        public FileManager(string inputPath)
        {
            sets = new List<FileSet>();

            if (inputPath.EndsWith(".jack", StringComparison.OrdinalIgnoreCase))
            {
                var file = new FileInfo(inputPath);
                if (!file.Exists)
                    throw new Exception("Input file doesn't exist: " + inputPath);
                sets.Add(GetSet(file));
            }
            else
            {
                var dir = new DirectoryInfo(inputPath);
                if (!dir.Exists)
                    throw new Exception("Input directory doesn't exist: " + inputPath);
                var files = dir.GetFiles("*.jack");
                sets.AddRange(files.Select(f => GetSet(f)));
            }
        }

        public IEnumerable<FileSet> FileSets
        {
            get { return sets; }
        }

        FileSet GetSet(FileInfo info)
        {
            var set = new FileSet();

            var name = info.Name;
            var nameBase = Regex.Replace(name, @"\.jack$", "");
            var directory = info.Directory.FullName;
            var expectedDirectory = Path.Combine(directory, "Expected");

            set.SourceFile = info;
            set.XmlFile = new FileInfo(Path.Combine(directory, nameBase) + ".xml");
            set.ExpectedXmlFile = new FileInfo(Path.Combine(expectedDirectory, nameBase) + ".xml");
            set.TXmlFile = new FileInfo(Path.Combine(directory, nameBase) + "T.xml");
            set.WsiXmlFile = new FileInfo(Path.Combine(directory, nameBase) + "WSI.xml");
            set.JsonFile = new FileInfo(Path.Combine(directory, nameBase) + ".json");
            set.ExpectedTXmlFile = new FileInfo(Path.Combine(expectedDirectory, nameBase) + "T.xml");

            var vmDir = Path.Combine(directory, "OSPlus");
            var vmFile = Path.Combine(vmDir, nameBase + ".vm");
            set.VMFile = new FileInfo(vmFile);

            return set; 
        }


        public class FileSet 
        {
            public FileInfo SourceFile { get; set; }
            public FileInfo XmlFile { get; set; }
            public FileInfo ExpectedXmlFile { get; set; }
            public FileInfo TXmlFile { get; set; }
            public FileInfo WsiXmlFile { get; set; }
            public FileInfo JsonFile { get; set; }
            public FileInfo ExpectedTXmlFile { get; set; }
            public FileInfo VMFile { get; set; }
        }
    }
}
