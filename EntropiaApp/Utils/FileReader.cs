using System.Collections.Generic;
using System.IO;

namespace EntropiaApp.Utils
{
    class FileReader
    {
        public List<string[]> Arrays { get; set; }
        private readonly string fileName = "C:\\Users\\Bartosz\\Desktop\\plik.txt";
        public void ReadFile()
        {
            Arrays = new List<string[]>();
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                Arrays.Add(line.Split(','));
            }
        }
    }
}
