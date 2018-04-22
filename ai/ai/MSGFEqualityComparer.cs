using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai
{
    class MsgfEqualityComparer : IEqualityComparer<MSGF>
    {
        public bool Equals(MSGF x, MSGF y)
        {
            return x.GetContent() == y.GetContent();
        }

        public int GetHashCode(MSGF obj)
        {
            return obj.GetContent().GetHashCode();
        }
    }
}
