using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace Roguelike
{   [DataContract]
    [KnownType(typeof(GameAction))]
    class Hero : Character
    {
        public Hero(Point coords, int hitPoints, int expPoints, int rangeOfVision, int speedPoints, string name)
        {
            Coords = coords;
            PrevCoords = new Point(coords.X, coords.Y);
            HitPoints = hitPoints; //should depend on class/hit dices
            ExpPoints = expPoints;
            RangeOfVision = rangeOfVision;
            SpeedPoints = speedPoints;
            MovePoints = 0;
            Name = name;
            Symbol = '@';
            IsActionDone = false;
            Program.GameEngine.SetObject(coords.X, coords.Y, this);
        }
        public Hero() { }
        public enum GameAction
        {
            OpenInventory = ConsoleKey.E,
            PickUpItem = ConsoleKey.G,
            Exit = ConsoleKey.Escape,
            InspectMap = ConsoleKey.M,
            Attack,
            DropItem
        }        [DataMember]        public int ExpPoints { get; set; }        [DataMember]        public GameAction CurrentGameAction { get; set; }        public void DoGameAction()
        {
            switch (CurrentGameAction)
            {
                case GameAction.OpenInventory:
                    //OpenInventory(Hero.Inventory)
                    //Hero.Inventory is a list, containing many lists of
                    //weapon, armor, potion and so on..
                    Program.GameEngine.InfoBorder.WriteNextLine("You open an inventory");
                    IsActionDone = false;
                    break;
                case GameAction.PickUpItem:
                    //Hero.AddItem(Item)
                    Program.GameEngine.InfoBorder.WriteNextLine("You pick up an item");
                    IsActionDone = true;
                    break;
                case GameAction.Exit:
                    Program.GameEngine.StartMenu();
                    IsActionDone = false;
                    break;
                case GameAction.Attack:
                    //make this as attack function
                    IsActionDone = true;
                    break;
                case GameAction.DropItem:
                    //drop item
                    Program.GameEngine.InfoBorder.WriteNextLine("You drop an item");
                    IsActionDone = false;
                    break;
                case GameAction.InspectMap:
                    Program.GameEngine.InfoBorder.Clear();
                    Program.GameEngine.InfoBorder.WriteNextLine("You are inspecting the map");
                    Program.GameEngine.InspectMap();
                    IsActionDone = false;
                    break;
                default:
                    IsActionDone = false;
                    break;
            }
        }        protected override void ResetGameAction()
        {
            CurrentGameAction = (GameAction)(1000);
        }        protected override bool HandleCollisions(TileFlyweight tile)
        {
            ResetGameAction();
            IsActionDone = true;

            if (tile.Object == null) return true;
            Target = tile.Object;
            
            if(Target is Monster)
            {
                CurrentGameAction = GameAction.Attack;
                Program.GameEngine.InfoBorder.WriteNextLine($"{Name} ran into {Target.Name}");
                return false;
            }

            return true;
            //if (Target is Treasure)...
        }    }}