using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai
{
    class SgfEqualityComparer : IEqualityComparer<SGF>
    {
        protected MsgfEqualityComparer MsgfComparer = new MsgfEqualityComparer();
        public bool Equals(SGF x, SGF y)
        {
            var l1 = x.Decompose();
            var l2 = y.Decompose();
            if (l1.Count() != l2.Count())
            {
                return false;
            }
            return
                l1.Zip(l2, (a, b) => MsgfComparer.Equals(a, b))
                    .All(b => b);
        }

        public int GetHashCode(SGF obj)
        {
            return obj.Decompose().Select(MsgfComparer.GetHashCode).Aggregate((a, b) => a ^ b);
        }
    }
}
