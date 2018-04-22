using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ai
{
    class AssociationsResult
    {
        protected HashSet<MSGF> StartNodes;
        protected MsgfGraph MsgfGraph;

        protected MsgfGraph ProbabilityGraph = new MsgfGraph();
        protected Dictionary<MSGF, uint> NodesProbability = new Dictionary<MSGF, uint>(new MsgfEqualityComparer());

        protected Thread Worker;
        protected Object WorkerSynchronizator = new Object();

        protected Random Rnd = new Random();

        public uint TargetDistance { get; set; }
        public uint TotalUpdates { get; protected set; }

        public AssociationsResult(HashSet<MSGF> startNodes, MsgfGraph msgfGraph, uint targetDistance = 3)
        {
            StartNodes = startNodes;
            MsgfGraph = msgfGraph;
            TargetDistance = targetDistance;
            TotalUpdates = 0;
            foreach (var node in MsgfGraph.GetAllNodes())
            {
                NodesProbability.Add(node, 0);
            }
        }

        public Dictionary<string, double> FingUsecases(uint count = 10)
        {
            Dictionary<string, double> ret = new Dictionary<string, double>();
            foreach (var node in ProbabilityGraph.GetAllNodes())
            {
                double x = (double) NodesProbability[node];
                if (ret.Any())
                {
                    double min = ret.Min(pair => pair.Value);
                    if (min < x)
                    {
                        if (ret.Count() >= count)
                        {
                            ret.Remove(ret.First(pair => Math.Abs(pair.Value - min) < 2*Double.Epsilon).Key);
                        }
                        ret.Add(node.GetContent(), x);
                    }
                }
                else
                {
                    ret.Add(node.GetContent(), x);
                }
                foreach (var outgoingNode in ProbabilityGraph.GetOutgoingNodes(node))
                {
                    var tempOutgoingX = x*ProbabilityGraph.GetEdgeFreq(node, outgoingNode)*NodesProbability[outgoingNode];
                    double outgoingX = Math.Pow(tempOutgoingX, 1.0/3.0);
                    if (ret.Any())
                    {
                        double min = ret.Min(pair => pair.Value);
                        if (min < outgoingX)
                        {
                            if (ret.Count() >= count)
                            {
                                ret.Remove(ret.First(pair => Math.Abs(pair.Value - min) < 2 * Double.Epsilon).Key);
                            }
                            ret.Add(node.GetContent() + "," + outgoingNode.GetContent(), outgoingX);
                        }
                    }
                    else
                    {
                        ret.Add(node.GetContent() + "," + outgoingNode.GetContent(), outgoingX);
                    }
                    foreach (var outgoingNode2 in ProbabilityGraph.GetOutgoingNodes(outgoingNode))
                    {
                        var tempOutgoingX2 = tempOutgoingX*ProbabilityGraph.GetEdgeFreq(outgoingNode, outgoingNode2)*
                                             NodesProbability[outgoingNode2];
                        double outgoingX2 = Math.Pow(tempOutgoingX2, 1.0/5.0);
                        if (ret.Any())
                        {
                            double min = ret.Min(pair => pair.Value);
                            if (min < outgoingX2)
                            {
                                if (ret.Count() >= count)
                                {
                                    ret.Remove(ret.First(pair => Math.Abs(pair.Value - min) < 2 * Double.Epsilon).Key);
                                }
                                ret.Add(node.GetContent() + "," + outgoingNode.GetContent() + "," + outgoingNode2.GetContent(), outgoingX2);
                            }
                        }
                        else
                        {
                            ret.Add(node.GetContent() + "," + outgoingNode.GetContent() + "," + outgoingNode2.GetContent(), outgoingX2);
                        }
                    }
                }
            }
            return ret;
        }

        public AssociationsResult ExportToTextFile(string fileName)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(fileName))
            {
                file.WriteLine(ProbabilityGraph.GetNodeCount());
                Dictionary<int, MSGF> orderByInt = new Dictionary<int, MSGF>();
                Dictionary<MSGF, int> orderByVal = new Dictionary<MSGF, int>(new MsgfEqualityComparer());
                foreach (var node in ProbabilityGraph.GetAllNodes())
                {
                    orderByInt.Add(orderByInt.Count(), node);
                    orderByVal.Add(node, orderByVal.Count());
                    file.Write(" " + node.GetContent());
                }
                file.WriteLine();

                for (int i = 0; i < orderByInt.Count(); ++i)
                {
                    file.Write(" " + NodesProbability[orderByInt[i]]);
                }
                file.WriteLine();

                for (int i = 0; i < orderByInt.Count(); ++i)
                {
                    var nodeFrom = orderByInt[i];
                    var nodesTo = ProbabilityGraph.GetOutgoingNodes(nodeFrom);
                    file.Write(" " + nodesTo.Count());
                    foreach (var nodeTo in nodesTo)
                    {
                        file.Write(" " + orderByVal[nodeTo] + " " + ProbabilityGraph.GetEdgeFreq(nodeFrom, nodeTo));
                    }
                    file.WriteLine();
                }
            }
            return this;
        }

        protected void DoWave()
        {
            TotalUpdates += 1;

            MSGF currentNode = StartNodes.ToList()[Rnd.Next(StartNodes.Count)];
            HashSet<MSGF> uniquePathsNodes = new HashSet<MSGF>(new MsgfEqualityComparer());

            double rnd = Math.Min(0.999999, Rnd.NextDouble());
            uint maxSteps = (uint)Math.Floor((TargetDistance + 1)*((Math.Log(1 - rnd))/(Math.Log(0.05))));
            maxSteps = Math.Max(0, Math.Min(2*TargetDistance, maxSteps));

            for (uint i = 0; i < maxSteps; ++i)
            {
                uniquePathsNodes.Add(currentNode);
                var nodesTo = MsgfGraph.GetOutgoingNodes(currentNode).FindAll(node => !uniquePathsNodes.Contains(node));
                if (!nodesTo.Any())
                {
                    break;
                }
                uint outputSum =
                    nodesTo.Select(node => MsgfGraph.GetEdgeFreq(currentNode, node))
                    .Aggregate((a, b) => a+b);
                int pathNumber = Rnd.Next((int)outputSum);
                foreach (var nodeTo in nodesTo)
                {
                    pathNumber -= (int)MsgfGraph.GetEdgeFreq(currentNode, nodeTo);
                    if (pathNumber <= 0)
                    {
                        ProbabilityGraph.AddEdge(currentNode, nodeTo);
                        if (!StartNodes.Contains(nodeTo))
                        {
                            NodesProbability[nodeTo] += 1;
                        }
                        currentNode = nodeTo;
                        break;
                    }
                }
            }
        }

        protected void DoWork(uint iterationsCount)
        {
            for (uint i = 0; i < iterationsCount; i++)
            {
                DoWave();

                try
                {
                    Thread.Sleep(0); //check Interrupted
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
            }
        }

        public AssociationsResult Run(uint iterationsCount = 100)
        {
            lock (WorkerSynchronizator)
            {
                if (Worker == null)
                {
                    Worker = new Thread(() => DoWork(iterationsCount));
                    Worker.Start();
                }
            }
            return this;
        }

        public AssociationsResult Pause()
        {
            lock (WorkerSynchronizator)
            {
                if (Worker != null)
                {
                    Worker.Interrupt();
                    Worker.Join();
                    Worker = null;
                }
            }
            return this;
        }
    }
}
