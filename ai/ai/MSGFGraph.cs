using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai
{
    class MsgfGraph
    {
        protected class Edge
        {
            public MSGF NodeFrom;
            public MSGF NodeTo;

            public Edge(MSGF nodeFrom, MSGF nodeTo)
            {
                NodeFrom = nodeFrom;
                NodeTo = nodeTo;
            }
        }

        protected class EdgeEqualityComparer : IEqualityComparer<Edge>
        {
            protected MsgfEqualityComparer MsgfComparer = new MsgfEqualityComparer();
            public bool Equals(Edge x, Edge y)
            {
                return MsgfComparer.Equals(x.NodeFrom, y.NodeFrom)
                    && MsgfComparer.Equals(x.NodeTo, y.NodeTo);
            }

            public int GetHashCode(Edge obj)
            {
                return MsgfComparer.GetHashCode(obj.NodeFrom) ^ MsgfComparer.GetHashCode(obj.NodeTo);
            }
        }

        protected class NodeEdges
        {
            public HashSet<Edge> IncomingEdges = new HashSet<Edge>(new EdgeEqualityComparer());
            public HashSet<Edge> OutgoingEdges = new HashSet<Edge>(new EdgeEqualityComparer());
        }

        protected Dictionary<MSGF,NodeEdges> NodeDictionary = new Dictionary<MSGF, NodeEdges>(new MsgfEqualityComparer());
        protected Dictionary<Edge, uint> EdgeDictionary = new Dictionary<Edge, uint>(new EdgeEqualityComparer());

        protected MsgfGraph TouchNode(MSGF node)
        {
            if (!HasNode(node))
            {
                NodeDictionary.Add(node, new NodeEdges());
            }
            return this;
        }

        protected MsgfGraph TouchEdge(MSGF nodeFrom, MSGF nodeTo)
        {
            TouchNode(nodeFrom);
            TouchNode(nodeTo);

            if (!HasEdge(nodeFrom, nodeTo))
            {
                EdgeDictionary.Add(new Edge(nodeFrom, nodeTo), 0);
                NodeDictionary[nodeFrom].OutgoingEdges.Add(new Edge(nodeFrom, nodeTo));
                NodeDictionary[nodeTo].IncomingEdges.Add(new Edge(nodeFrom, nodeTo));
            }
            return this;
        }

        public MsgfGraph SetEdge(MSGF nodeFrom, MSGF nodeTo, uint freq = 1)
        {
            if (freq == 0)
            {
                return ClearEdge(nodeFrom, nodeTo);
            }
            TouchEdge(nodeFrom, nodeTo);
            EdgeDictionary[new Edge(nodeFrom, nodeTo)] = freq;
            return this;
        }

        public MsgfGraph AddEdge(MSGF nodeFrom, MSGF nodeTo, uint count = 1)
        {
            if (count == 0)
            {
                return this;
            }
            TouchEdge(nodeFrom, nodeTo);
            EdgeDictionary[new Edge(nodeFrom, nodeTo)] += count;
            return this;
        }

        public MsgfGraph PopEdge(MSGF nodeFrom, MSGF nodeTo, uint count = 1)
        {
            if (count == 0)
            {
                return this;
            }
            TouchEdge(nodeFrom, nodeTo);
            if (EdgeDictionary[new Edge(nodeFrom, nodeTo)] <= count)
            {
                return ClearEdge(nodeFrom, nodeTo);
            }
            EdgeDictionary[new Edge(nodeFrom, nodeTo)] -= count;
            return this;
        }

        public MsgfGraph ClearEdge(MSGF nodeFrom, MSGF nodeTo)
        {
            if (HasEdge(nodeFrom, nodeTo))
            {
                EdgeDictionary.Remove(new Edge(nodeFrom, nodeTo));
                NodeDictionary[nodeFrom].IncomingEdges.Remove(new Edge(nodeFrom, nodeTo));
                NodeDictionary[nodeFrom].OutgoingEdges.Remove(new Edge(nodeFrom, nodeTo));
            }
            return this;
        }

        public bool HasEdge(MSGF nodeFrom, MSGF nodeTo)
        {
            return EdgeDictionary.ContainsKey(new Edge(nodeFrom, nodeTo));
        }

        public uint GetEdgeFreq(MSGF nodeFrom, MSGF nodeTo)
        {
            if (HasEdge(nodeFrom, nodeTo))
            {
                return EdgeDictionary[new Edge(nodeFrom, nodeTo)];
            }
            else
            {
                return 0;
            }
        }

        public uint GetEdgeCount()
        {
            return (uint)EdgeDictionary.Count;
        }

        public uint GetNodeCount()
        {
            return (uint)NodeDictionary.Count;
        }

        public List<MSGF> GetAllNodes()
        {
            return
                NodeDictionary.Keys
                .ToList();
        }

        public List<Tuple<MSGF, MSGF, uint>> GetAllEdges()
        {
            return
                EdgeDictionary.Select(kvpair =>
                        new Tuple<MSGF, MSGF, uint>(kvpair.Key.NodeFrom, kvpair.Key.NodeTo, kvpair.Value))
                    .ToList();
        }

        public List<MSGF> GetOutgoingNodes(MSGF node)
        {
            if (!HasNode(node))
            {
                return new List<MSGF>();
            }
            return
                NodeDictionary[node].OutgoingEdges
                    .Select(edge => edge.NodeTo)
                    .ToList();
        }

        public List<MSGF> GetIncomingNodes(MSGF node)
        {
            if (!HasNode(node))
            {
                return new List<MSGF>();
            }
            return
                NodeDictionary[node].IncomingEdges
                    .Select(edge => edge.NodeFrom)
                    .ToList();
        }

        public MsgfGraph GetDeepCopy()
        {
            MsgfGraph ret = new MsgfGraph();
            foreach (var nodeEdgese in NodeDictionary)
            {
                ret.AddNode(nodeEdgese.Key);
            }
            foreach (var u in EdgeDictionary)
            {
                ret.SetEdge(u.Key.NodeFrom, u.Key.NodeTo, u.Value);
            }
            return ret;
        }

        public MsgfGraph AddNode(MSGF node)
        {
            return TouchNode(node);
        }

        public MsgfGraph PopNode(MSGF node)
        {
            foreach (var edge in NodeDictionary[node].IncomingEdges.ToList())
            {
                ClearEdge(edge.NodeFrom, edge.NodeTo);
            }
            NodeDictionary.Remove(node);
            return this;
        }

        public bool HasNode(MSGF node)
        {
            return NodeDictionary.ContainsKey(node);
        }
    }
}
