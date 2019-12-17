using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
    interface IMonsterIntelligence
    {
        void FindPath(Monster monster, BaseEntity target); //sets CurrentMoveAction
    }
}
