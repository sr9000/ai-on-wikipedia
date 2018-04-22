using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ai
{
    class AssociationsFinder
    {
        protected SGF Sgf;
        protected AssociateDataBase MyAssociateDataBase;
        protected Decomposer MyDecomposer;
        protected Associations MyAssociations;
        protected Thread Worker;
        protected Object WorkerSynchronizator = new Object();

        public delegate void AllWorkIsCompleteHandler();

        public event AllWorkIsCompleteHandler EAllWorkIsComplete;

        public AssociationsFinder(
            SGF sgf
            , AssociateDataBase associateDataBase
            , Decomposer decomposer
            , Associations associations)
        {
            Sgf = sgf;
            MyAssociateDataBase = associateDataBase;
            MyDecomposer = decomposer;
            MyAssociations = associations;

            MyAssociations.InitAssociations(sgf);
        }

        protected void DoWork()
        {
            while (MyAssociations.HaveQueries() && !MyAssociations.IsLimitReached())
            {
                SGF query = MyAssociations.GetQuery();
                QueryAsyncRequest queryWaiter = MyAssociateDataBase.QueryAsyncRequest(query);

                try
                {
                    queryWaiter.Wait();
                }
                catch (ThreadInterruptedException)
                {
                    queryWaiter.Stop();
                    return;
                }

                List<SGF> queryResult = MyDecomposer.Proceed(queryWaiter.GetResult());
                MyAssociations.Associate(query, queryResult);

                try
                {
                    Thread.Sleep(0); //check Interrupted
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
            }
            if (EAllWorkIsComplete != null)
            {
                EAllWorkIsComplete(); //Generally impossible, but...
            }
        }

        public AssociationsFinder Run()
        {
            lock (WorkerSynchronizator)
            {
                if (Worker == null)
                {
                    Worker = new Thread(DoWork);
                    Worker.Start();
                }
            }
            return this;
        }

        public AssociationsFinder Pause()
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

        public AssociationsResult GetResult()
        {
            return MyAssociations.GetResult();
        }

        public List<string> GetLog()
        {
            return MyAssociateDataBase.GetLog();
        }

        public AssociationsFinder UpdateLimit(Limit limit)
        {
            MyAssociations.UpdateLimit(limit);
            return this;
        }
    }
}