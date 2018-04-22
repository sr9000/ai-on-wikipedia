using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai
{
    class QueryGenerator
    {
        public static List<SGF> GenQueries(SGF sgf)
        {
            var list = sgf.Decompose();
            var ret = new List<SGF>();
            {//gen words
                foreach (var msgf in list)
                {
                    ret.Add(
                        new SGF().AggregateFrom(
                            new List<MSGF> {msgf}));
                }
            }
            {//gen pairs
                MSGF a1 = null;
                foreach (var msgf in list)
                {
                    if (a1 != null)
                    {
                        ret.Add(
                            new SGF().AggregateFrom(
                                new List<MSGF>{a1, msgf}.OrderBy(l => l.GetContent())
                                .ToList()));
                    }
                    a1 = msgf;
                }
            }
            {//gen tripples
                MSGF a1 = null, a2 = null;
                foreach (var msgf in list)
                {
                    if (a1 != null && a2 != null)
                    {
                        ret.Add(
                            new SGF().AggregateFrom(
                                new List<MSGF> { a1, a2, msgf }.OrderBy(l => l.GetContent())
                                .ToList()));
                    }
                    a1 = a2;
                    a2 = msgf;
                }
            }
            var rnd = new Random();
            return
                ret.OrderBy(x => rnd.Next())
                .Take(Math.Max(3, (ret.Count + 9) / 10))
                .ToList();
        }
    }
}
