using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
    class Monster : Character
    {
        public Monster(Point coords, int hitPoints, int rangeOfVision, int speedPoints, string name, char symbol, IMonsterIntelligence ai)
        {
            Coords = coords;
            PrevCoords = new Point(coords.X, coords.Y);
            HitPoints = hitPoints; //should depend on class/hit dices
            RangeOfVision = rangeOfVision;
            SpeedPoints = speedPoints;
            MovePoints = 0;
            Name = name;
            Symbol = symbol;
            AI = ai;
            IsActionDone = false;
            Program.GameEngine.SetObject(coords.X, coords.Y, this);
        }
        public enum GameAction 
        {
            Attack
        }
        public GameAction CurrentGameAction { get; set; }
        public IMonsterIntelligence AI { private get; set; }
        public void MoveTo(BaseEntity enemy)
        {
            if (Coords.GetDistance(enemy.Coords) > RangeOfVision)
            {
                IsActionDone = false;
                return;
            }

            AI.FindPath(this, enemy);
            Move();
        }
        public void DoGameAction()
        {
            switch (CurrentGameAction)
            {
                case GameAction.Attack:
                    //make this as attack function
                    IsActionDone = true;
                    break;
                default:
                    IsActionDone = false;
                    break;
            }
        }
        protected override void ResetGameAction()
        {
            CurrentGameAction = (GameAction)(1000);
        }
        protected override bool HandleCollisions(TileFlyweight tile)
        {
            ResetGameAction();
            if (tile.Object == null) return true;
            Target = tile.Object;

            if (Target.Symbol == '@') //simple check, <=> (this is Hero)
            {
                CurrentGameAction = GameAction.Attack;
                Program.GameEngine.InfoBorder.WriteNextLine($"{Name} ran into {Target.Name}");
                return false;
            }

            return true;
        }
    }
}
