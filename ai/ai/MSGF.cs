using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ai
{
    class MSGF
    {
        protected string Content = null;

        public MSGF() { }

        public MSGF SetContent(string content)
        {
            Content = content;
            return this;
        }

        public string GetContent()
        {
            return Content;
        }
    }
}
