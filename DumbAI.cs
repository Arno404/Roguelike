using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
    class DumbAI : IMonsterIntelligence
    {
        public void FindPath(Monster monster, BaseEntity target)
        {
            int distanceY = monster.Coords.X - target.Coords.X;
            int distanceX = monster.Coords.Y - target.Coords.Y;
            int absX = Math.Abs(distanceX);
            int absY = Math.Abs(distanceY);

            if (distanceX > 0 && distanceY > 0)
            {
                if (absX < absY)
                    monster.CurrentMoveAction = BaseCharacter.MoveAction.Left;
                else monster.CurrentMoveAction = BaseCharacter.MoveAction.Up;
            }
            if (distanceX < 0 && distanceY > 0)
            {
                if (absX < absY)
                    monster.CurrentMoveAction = BaseCharacter.MoveAction.Left;
                else monster.CurrentMoveAction = BaseCharacter.MoveAction.Down;
            }
            if (distanceX > 0 && distanceY < 0)
            {
                if (absX < absY)
                    monster.CurrentMoveAction = BaseCharacter.MoveAction.Right;
                else monster.CurrentMoveAction = BaseCharacter.MoveAction.Up;
            }
            if (distanceX < 0 && distanceY < 0)
            {
                if (absX < absY)
                    monster.CurrentMoveAction = BaseCharacter.MoveAction.Right;
                else monster.CurrentMoveAction = BaseCharacter.MoveAction.Down;
            }
            if (distanceX == 0)
            {
                if(distanceY > 0) monster.CurrentMoveAction = BaseCharacter.MoveAction.Left;
                else monster.CurrentMoveAction = BaseCharacter.MoveAction.Right;
            }
            if (distanceY == 0)
            {
                if (distanceX > 0) monster.CurrentMoveAction = BaseCharacter.MoveAction.Up;
                else monster.CurrentMoveAction = BaseCharacter.MoveAction.Down;
            }
        }
    }
}
