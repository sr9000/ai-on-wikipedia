using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai
{
    class Associations
    {
        protected MsgfGraph Graph = new MsgfGraph();
        protected Limit UnitsLimit;
        protected HashSet<MSGF> StartNodes = new HashSet<MSGF>(new MsgfEqualityComparer());
        protected HashSet<SGF> Queries = new HashSet<SGF>(new SgfEqualityComparer());
        protected List<SGF> QueriesList = new List<SGF>();
        protected Random Rnd = new Random();

        protected Associations AddQueriesFromResultsSgf(List<SGF> queries)
        {
            var peparedQueries =
                queries.Select(QueryGenerator.GenQueries)
                    .Aggregate((a, b) =>
                    {
                        a.AddRange(b);
                        return a;
                    })
                    .OrderBy(x => Rnd.Next());
            foreach (var query in peparedQueries)
            {
                if (!Queries.Contains(query))
                {
                    Queries.Add(query);
                    QueriesList.Add(query);
                }
            }
            return this;
        }

        public Associations(Limit limit)
        {
            UnitsLimit = limit;
        }

        public AssociationsResult GetResult()
        {
            return new AssociationsResult(StartNodes, Graph.GetDeepCopy());
        }

        public Associations InitAssociations(SGF sgf)
        {
            foreach (var msgf in sgf.Decompose())
            {
                StartNodes.Add(msgf);
            }
            return AddQueriesFromResultsSgf(new List<SGF>{sgf});
        }

        public bool HaveQueries()
        {
            return QueriesList.Any();
        }

        public SGF GetQuery()
        {
            var ret = QueriesList.First();
            QueriesList.RemoveAt(0);
            return ret;
        }

        public Associations Associate(SGF query, List<SGF> queryResult)
        {
            //add new queries
            AddQueriesFromResultsSgf(queryResult);

            //remove old query
            Queries.Remove(query);

            //add graph edges
            var queryDecomposed = query.Decompose();
            foreach (var sgf in queryResult)
            {
                foreach (var msgfTo in sgf.Decompose())
                {
                    foreach (var msgfFrom in queryDecomposed)
                    {
                        Graph.AddEdge(msgfFrom, msgfTo);
                    }
                }
            }

            //update limits
            UnitsLimit.UpdateCurrentValues(
                (uint)Math.Max(QueriesList.Count(), Queries.Count)
                , Graph.GetNodeCount()
                , Graph.GetEdgeCount());

            return this;
        }

        public bool IsLimitReached()
        {
            return UnitsLimit.IsLimitReached();
        }

        public Associations UpdateLimit(Limit limit)
        {
            UnitsLimit.UpdateLimit(limit.GetTotalUnitsLimit());
            return this;
        }
    }
}
