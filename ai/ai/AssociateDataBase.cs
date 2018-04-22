using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ai
{
    class AssociateDataBase
    {
        protected Object RequestSunchronizer = new Object();
        protected Thread Sleeper = new Thread(ThreadBody);

        protected static void ThreadBody()
        {
            Thread.Sleep(2000);
        }
        
        public List<string> GetLog()
        {
            return new List<string> {"Log not implemented"};
        }

        public QueryAsyncRequest QueryAsyncRequest(SGF query)
        {
            lock (RequestSunchronizer)
            {
                try
                {
                    Sleeper.Join();
                }
                catch (ThreadStateException) { }
                Sleeper = new Thread(ThreadBody);
                Sleeper.Start();
            }
            return new QueryAsyncRequest(query);
        }
    }
}
