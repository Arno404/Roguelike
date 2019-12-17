using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Roguelike
{   [DataContract]
    class Monster : Character
    {
        public Monster(Point coords, int hitPoints, int rangeOfVision, int speedPoints, string name, char symbol)
        {
            Coords = coords;
            PrevCoords = new Point(coords.X, coords.Y);
            HitPoints = hitPoints; //should depend on class/hit dices
            RangeOfVision = rangeOfVision;
            SpeedPoints = speedPoints;
            MovePoints = 0;
            Name = name;
            Symbol = symbol;
            IsActionDone = false;
            Program.GameEngine.SetObject(coords.X, coords.Y, this);
            ChanceToGoUp = 0.25;
            ChanceToGoDown = 0.25;
            ChanceToGoRight = 0.25;
            ChanceToGoLeft = 0.25;
            StandardDecreasingChance = 0.025;
            PrevDistance = 0;
        }
        //Tile MonsterTile (color and shape of 
        // symbol that represents monster)

        [DataMember]
        private double ChanceToGoUp;
        [DataMember]
        private double ChanceToGoDown;
        [DataMember]
        private double ChanceToGoRight;
        [DataMember]
        private double ChanceToGoLeft;
        [DataMember]
        private double StandardDecreasingChance;
        [DataMember]
        private double PrevDistance;
        public enum GameAction 
        {
            Attack
        }
        [DataMember]
        public GameAction CurrentGameAction { get; set; }

        #region AI
        private void DecreaseChances()
        {
            ChanceToGoUp -= StandardDecreasingChance;
            ChanceToGoDown -= StandardDecreasingChance;
            ChanceToGoRight -= StandardDecreasingChance;
            ChanceToGoLeft -= StandardDecreasingChance;
        }
        private void IncreaseChances()
        {
            ChanceToGoUp += StandardDecreasingChance;
            ChanceToGoDown += StandardDecreasingChance;
            ChanceToGoRight += StandardDecreasingChance;
            ChanceToGoLeft += StandardDecreasingChance;
        }
        private void SetChances(double distance, BaseCharacter.MoveAction direction)
        {
            if (distance < PrevDistance)
            {
                DecreaseChances();
                switch (direction)
                {
                    case MoveAction.Up:
                        ChanceToGoUp += StandardDecreasingChance * 4;
                        break;
                    case MoveAction.Down:
                        ChanceToGoDown += StandardDecreasingChance * 4;
                        break;
                    case MoveAction.Right:
                        ChanceToGoRight += StandardDecreasingChance * 4;
                        break;
                    case MoveAction.Left:
                        ChanceToGoLeft += StandardDecreasingChance * 4;
                        break;
                }
            }
            else
            {
                IncreaseChances();
                switch (direction)
                {
                    case MoveAction.Up:
                        ChanceToGoUp -= StandardDecreasingChance * 4;
                        break;
                    case MoveAction.Down:
                        ChanceToGoDown -= StandardDecreasingChance * 4;
                        break;
                    case MoveAction.Right:
                        ChanceToGoRight -= StandardDecreasingChance * 4;
                        break;
                    case MoveAction.Left:
                        ChanceToGoLeft -= StandardDecreasingChance * 4;
                        break;
                }
            }
        }
        private void SetStandardChances()
        {
            ChanceToGoUp = 0.25;
            ChanceToGoDown = 0.25;
            ChanceToGoRight = 0.25;
            ChanceToGoLeft = 0.25;
        }
        private void SetMoveAction()
        {
            double randnumber = Program.GameEngine.GameRandom.NextDouble();
            if (randnumber < ChanceToGoUp)
            {
                CurrentMoveAction = BaseCharacter.MoveAction.Up;
                return;
            }
            if (randnumber < ChanceToGoUp + ChanceToGoDown)
            {
                CurrentMoveAction = BaseCharacter.MoveAction.Down;
                return;
            }
            if (randnumber < ChanceToGoUp + ChanceToGoDown + ChanceToGoRight)
            {
                CurrentMoveAction = BaseCharacter.MoveAction.Right;
                return;
            }
            CurrentMoveAction = BaseCharacter.MoveAction.Left;
        }
        #endregion

        public void MoveTo(BaseCharacter enemy)
        {
            double distance;

            if (Coords.GetDistance(enemy.Coords) > RangeOfVision)
            {
                IsActionDone = false;
                return;
            }
            SetMoveAction();
            Move();
            distance = Coords.GetDistance(enemy.Coords);
            SetChances(distance, CurrentMoveAction);
            if (IsActionDone) PrevDistance = distance;
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

            if (Target is Hero)
            {
                CurrentGameAction = GameAction.Attack;
                Program.GameEngine.InfoBorder.WriteNextLine($"{Name} ran into {Target.Name}");
                return false;
            }

            return true;
        }
    }
}
