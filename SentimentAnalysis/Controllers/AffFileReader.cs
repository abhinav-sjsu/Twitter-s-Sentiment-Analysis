using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace SentimentAnalysis.Controllers
{
    public class AffFileReader
    {
        private static Dictionary<string, int> _wordDegreeMap = new Dictionary<string, int>();

        public static void ReadFile()
        {
            if (!_wordDegreeMap.Any())
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "SentimentAnalysis.Resources.words.txt";
                List<string> lines = new List<string>();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (reader.Peek() >= 0)
                        lines.Add(reader.ReadLine());
                }
                foreach (string line in lines)
                {
                    string[] lineElements = line.Split(new char[] { '\t' });
                    _wordDegreeMap.Add(lineElements[0], Int32.Parse(lineElements[1]));
                }
            }
        }

        public static Int32 GetWordDegree(string word)
        {
            Int32 value = 0;
            _wordDegreeMap.TryGetValue(word, out value);
            return value;
        }
    }
}