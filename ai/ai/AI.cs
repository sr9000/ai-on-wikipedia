using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ai
{
    //SGF is acronym for "Symbolic-graphic form"
    class AI
    {
        public static AssociationsFinder FindAssociationsAsync(SGF sgf)
        {
            var associationsFinder = 
                new AssociationsFinder(
                    sgf
                    , new AssociateDataBase()
                    , new Decomposer(true)
                    , new Associations(new Limit(30000)));
            return associationsFinder.Run();
        }
    }
}
