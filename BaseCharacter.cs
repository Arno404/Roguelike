using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace Roguelike
{   [DataContract]
    class BaseCharacter : BaseEntity
    {
        public enum MoveAction
        {
            Up = ConsoleKey.UpArrow,
            Down = ConsoleKey.DownArrow,
            Right = ConsoleKey.RightArrow,
            Left = ConsoleKey.LeftArrow
        }
        [DataMember]
        public MoveAction CurrentMoveAction { get; set; }

        public void MoveUp()
        {
            --Coords.Y;
        }

        public void MoveDown()
        {
            ++Coords.Y;
        }

        public void MoveLeft()
        {
            --Coords.X;
        }

        public void MoveRight()
        {
            ++Coords.X;
        }

        public void RestoreCoords()
        {
            Coords.X = PrevCoords.X;
            Coords.Y = PrevCoords.Y;
        }

        protected void SetPrevCoords()
        {
            PrevCoords.X = Coords.X;
            PrevCoords.Y = Coords.Y;
        }

        protected void SetPrevPlusMove(MoveAction action)
        {
            SetPrevCoords();
            switch (action)
            {
                case MoveAction.Up:
                    MoveUp();
                    break;
                case MoveAction.Down:
                    MoveDown();
                    break;
                case MoveAction.Left:
                    MoveLeft();
                    break;
                case MoveAction.Right:
                    MoveRight();
                    break;
            }
        }

        public void MoveEntity()
        {
            SetPrevPlusMove(CurrentMoveAction);
            Program.GameEngine.RemoveObject(PrevCoords.X, PrevCoords.Y);
            Program.GameEngine.SetObject(Coords.X, Coords.Y, this);
        }
        public virtual void Move() //sets IsMoved
        {
            SetPrevPlusMove(CurrentMoveAction);
        }
    }
}