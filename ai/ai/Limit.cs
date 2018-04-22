using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai
{
    class Limit
    {
        protected uint TotalUnits;
        protected uint CurrentQueries, CurrentNodes, CurrentEdges;

        public Limit(uint totalUnits = 100, uint currentQueries = 0, uint currentNodes = 0, uint currentEdges = 0)
        {
            TotalUnits = totalUnits;
            CurrentEdges = currentEdges;
            CurrentNodes = currentNodes;
            CurrentQueries = currentQueries;
        }

        public Limit UpdateLimit(uint totalUnits)
        {
            TotalUnits = totalUnits;
            return this;
        }

        public Limit UpdateCurrentValues(uint currentQueries, uint currentNodes, uint currentEdges)
        {
            CurrentEdges = currentEdges;
            CurrentNodes = currentNodes;
            CurrentQueries = currentQueries;
            return this;
        }

        public uint GetTotalUnitsLimit()
        {
            return TotalUnits;
        }

        /// <summary>
        /// T1 Queries
        /// T2 Nodes
        /// T3 Edges
        /// </summary>
        /// <returns></returns>
        public Tuple<uint, uint, uint> GetCurrentValues()
        {
            return new Tuple<uint, uint, uint>(CurrentQueries, CurrentNodes, CurrentEdges);
        }

        public bool IsLimitReached()
        {
            return (TotalUnits <= (CurrentNodes + CurrentEdges + CurrentQueries));
        }
    }
}
