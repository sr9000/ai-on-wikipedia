using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai
{
    class SGF
    {
        protected String Content = null;
        public List<MSGF> Decompose()
        {
            return
                Content
                    .Split(',')
                    .Select((word) => new MSGF().SetContent(word))
                    .ToList();
        }

        public SGF AggregateFrom(List<MSGF> msgfList)
        {
            if (!msgfList.Any())
            {
                Content = "";
            }
            else
            {
                Content =
                    msgfList
                        .Select(x => x.GetContent())
                        .Aggregate((a, b) => a + "," + b);
            }
            return this;
        }
    }
}
