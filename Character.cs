using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Roguelike
{
    [DataContract]
    [KnownType(typeof(BaseEntity))]
    abstract class Character : BaseCharacter
    {
        [DataMember]
        public int HitPoints { get; set; }
        [DataMember]
        public int MovePoints { get; set; }
        [DataMember]
        public int SpeedPoints { get; set; }
        [DataMember]
        public int RangeOfVision { get; set; }
        [DataMember]
        public bool IsActionDone { get; set; }
        [DataMember]
        public BaseEntity Target { get; set; }
        protected abstract void ResetGameAction();
        protected abstract bool HandleCollisions(TileFlyweight tile);
        //sets IsActionDone, sets CurrentGameAction, returns true if we can move
        public override void Move()
        {
            TileFlyweight tile;
            bool isMoved;

            SetPrevCoords();
            Target = null;

            switch (CurrentMoveAction)
            {
                case MoveAction.Up:
                    tile = Program.GameEngine.GetTile(Coords.X, Coords.Y - 1);
                    break;
                case MoveAction.Down:
                    tile = Program.GameEngine.GetTile(Coords.X, Coords.Y + 1);
                    break;
                case MoveAction.Left:
                    tile = Program.GameEngine.GetTile(Coords.X - 1, Coords.Y);;
                    break;
                case MoveAction.Right:
                    tile = Program.GameEngine.GetTile(Coords.X + 1, Coords.Y);
                    break;
                default:
                    IsActionDone = false;
                    return;
            }

            if(tile.Price == -1)
            {
                ResetGameAction();
                IsActionDone = true;

                if (this.Symbol == '@') //simple check, <=> (this is Hero)
                    Program.GameEngine.InfoBorder.WriteNextLine($"{Name} can't go there"); //this code is not nessesary
                return;
            }
            MovePoints += SpeedPoints;
            if(MovePoints < tile.Price)
            {
                ResetGameAction();
                IsActionDone = true;
                return;
            }
            MovePoints -= tile.Price;

            isMoved = HandleCollisions(tile);
            if (isMoved) MoveEntity();
        }
    }
}
