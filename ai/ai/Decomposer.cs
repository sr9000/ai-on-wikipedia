using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ai
{
    class Decomposer
    {
        
        protected static string FileNameStopWordsLongList = "long.txt";
        protected static string FileNameStopWordsShortList = "short.txt";
        protected static string FileNameEnglishWordsDictionary = "dictionary.txt";

        protected HashSet<string> StopWords = new HashSet<string>();
        protected HashSet<string> KnownWords = new HashSet<string>();

        public Decomposer(bool useLongStopList = false)
        {
            var stopWordsFile = useLongStopList ? FileNameStopWordsLongList : FileNameStopWordsShortList;

            foreach (var stopWord in File.ReadAllLines("Resources/" + stopWordsFile))
            {
                StopWords.Add(stopWord.ToLower().Trim());
            }
            foreach (var knownWord in File.ReadAllLines("Resources/" + FileNameEnglishWordsDictionary))
            {
                KnownWords.Add(knownWord.ToLower().Trim());
            }
        }
        public List<SGF> Proceed(List<SGF> list)
        {
            return
                list.Select(x =>
                        x.Decompose()
                        .FindAll(mx =>
                            (!StopWords.Contains(mx.GetContent()))
                            && KnownWords.Contains(mx.GetContent())))
                    .Select(l => new SGF().AggregateFrom(l))
                    .ToList();
        }
    }
}
