using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CaveGenerator;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Roguelike
{   [Serializable]
    [KnownType(typeof(Hero))]
    [KnownType(typeof(Cave))]
    [KnownType(typeof(Monster))]
    [KnownType(typeof(Rectangle))]
    class GameEngine : ISerializable
    {   [DataMember]
        private Hero CurrentHero;
        [NonSerialized]
        private MapInspector Inspector;
        [NonSerialized]
        private bool GameOver = false;
        [JsonIgnore]
        public bool GameStarted { get; private set; }
        [DataMember]
        private Cave Map;
        [DataMember]
        public Rectangle HeroInfoBorder { get; set; }
        [DataMember]
        public Rectangle MapBorder { get; set; }
        [DataMember]
        public Rectangle InfoBorder { get; set; }
        [DataMember]
        private int ConsoleHeight = 0;
        [DataMember]
        private int ConsoleWidth = 0;
        [DataMember]
        private Monster TmpMonster; //make this as list
        [DataMember]
        public Random GameRandom = new Random();
        public GameEngine(){}
        public GameEngine(SerializationInfo info, StreamingContext context)
        {
            this.CurrentHero = (Hero)info.GetValue("CurrentHero", typeof(Hero));
            this.HeroInfoBorder = (Rectangle)info.GetValue("HeroInfoBorder", typeof(Rectangle));
            this.Map = (Cave)info.GetValue("Map", typeof(Cave));
            this.MapBorder = (Rectangle)info.GetValue("MapBorder", typeof(Rectangle));
            this.InfoBorder = (Rectangle)info.GetValue("InfoBorder", typeof(Rectangle));
            this.ConsoleHeight = (int)info.GetValue("ConsoleH", typeof(int));
            this.ConsoleWidth = (int)info.GetValue("ConsoleW", typeof(int));
            this.TmpMonster = (Monster)info.GetValue("TmpMonster", typeof(Monster));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CurrentHero", this.CurrentHero);
            info.AddValue("Map", this.Map);
            info.AddValue("HeroInfoBorder", this.HeroInfoBorder);
            info.AddValue("MapBorder", this.MapBorder);
            info.AddValue("InfoBorder", this.InfoBorder);
            info.AddValue("ConsoleH", this.ConsoleHeight);
            info.AddValue("ConsoleW", this.ConsoleWidth);
            info.AddValue("TmpMonster", this.TmpMonster);
        }
        public void InitFromSave(Int16 numberofsave)
        {
            string path = MakeCorrectPath();
            FileStream buffer = File.OpenRead(path+$"{numberofsave}.txt");
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GameEngine));
            GameEngine tmpengine = jsonSerializer.ReadObject(buffer) as GameEngine;
            CurrentHero = tmpengine.CurrentHero;
            Map = tmpengine.Map;
            buffer.Close();
            TmpMonster = tmpengine.TmpMonster;
            Inspector = new MapInspector("Inspector", new Point(CurrentHero.Coords.X, CurrentHero.Coords.Y));
            GameStarted = true;
            ConsoleHeight = tmpengine.ConsoleHeight;
            ConsoleWidth = tmpengine.ConsoleHeight;
        }
        public string MakeCorrectPath()
        {
            string path = "";
            if (Environment.OSVersion.ToString()[0] == 'M')
            {
                path = @"..\Saves\Save";
            }
            if (Environment.OSVersion.ToString()[0] == 'U')
            {
                path = @"../Saves/Save";
            }
            return path;
        }
        public void Save(Int16 numberofsavefile)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GameEngine));
            string path = MakeCorrectPath();
            DirectoryInfo dirInfo = new DirectoryInfo(path.Substring(0,8));
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            FileStream buffer = File.Create(path+$"{numberofsavefile}.txt");
            jsonSerializer.WriteObject(buffer,this);
            buffer.Close();
        }
        public void Init()
        {
            Map = new Cave();
            Map.Build();
            Map.ConnectCaves();
            Map.WriteMapIntoFile();
            Map.Offset = new Point(0, 0);
            CurrentHero = new Hero(new Point(12, 10), 15, 0, 20, Character.Speed.Normal, "Chiks-Chiriks");
            TmpMonster = new Monster(new Point(15, 15), 10, 10, Character.Speed.Normal, "Snake", 'S');
            Inspector = new MapInspector("Inspector", new Point(CurrentHero.Coords.X, CurrentHero.Coords.Y));
            GameStarted = true;
            ConsoleHeight = Console.WindowHeight;
            ConsoleWidth = Console.WindowWidth;
        }

        private void Input()
        {
            var key = Console.ReadKey(true).Key;
            CurrentHero.CurrentMoveAction = (BaseCharacter.MoveAction)key;
            CurrentHero.CurrentGameAction = (Hero.GameAction)key;
        }

        private void Input(MapInspector inspector)
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.M) inspector.IsInspect = false;
            inspector.CurrentMoveAction = (BaseCharacter.MoveAction)key;
        }

        private void Logic()
        {
            CurrentHero.Move();
            if (!CurrentHero.IsMoved)
            {
                //"if" is not correct, we mustn't go there if we hit a wall, for example
                CurrentHero.DoGameAction();
                return;
            }
            TmpMonster.MoveTo(CurrentHero);
            MoveMap(CurrentHero);
        }
        public void StartMenu()
        {
            StartingMenu startingMenu = new StartingMenu();
            startingMenu.Process();
        }

        public void InspectMap()
        {
            Inspector.IsInspect = true;
            Inspector.Coords.SetValue(CurrentHero.Coords);
            TileFactory factory = new TileFactory();
            TileFlyweight tile;
            while (Inspector.IsInspect)
            {
                tile = factory.GetTile((TileFlyweight.Type)Map.Map[Inspector.Coords.Y] [Inspector.Coords.X]);
                Console.SetCursorPosition(InfoBorder.Offset.X, InfoBorder.Offset.Y);
                Console.WriteLine(new string(' ', 20));
                Console.SetCursorPosition(InfoBorder.Offset.X, InfoBorder.Offset.Y);
                Console.WriteLine($"{tile.Description}: {tile.Symbol}");
                RedrawInspector(tile.Symbol);
                Input(Inspector);
                HandleConsoleResize();
                if (!Inspector.IsInspect)
                {
                    SetMapOffset();
                    Draw();
                    break;
                }
                Inspector.Move();
                MoveMap(Inspector);
            }
        }

        enum MapMoveDirection
        {
            Top,
            Bot,
            Left,
            Right
        }

        private void MoveMap(BaseCharacter baseCharacter)
        {
            int distToTop = baseCharacter.Coords.Y - Map.Offset.Y + 1;
            int distToLeft = baseCharacter.Coords.X - Map.Offset.X + 1;
            int distToRight = MapBorder.Width - baseCharacter.Coords.X + Map.Offset.X - 2;
            int distToBot = MapBorder.Height - 2 - baseCharacter.Coords.Y + Map.Offset.Y;
            int critDistHor = MapBorder.Width / 8;
            int critDistVert = MapBorder.Height / 8;
            if (distToTop <= critDistVert)
                MoveMapToDir(MapMoveDirection.Bot);
            else if (distToBot <= critDistVert)
                MoveMapToDir(MapMoveDirection.Top);
            else if (distToRight <= critDistHor)
                MoveMapToDir(MapMoveDirection.Left);
            else if (distToLeft <= critDistHor)
                MoveMapToDir(MapMoveDirection.Right);
        }

        private void MoveMapToDir(MapMoveDirection direction)
        {
            int offset, length;
            switch (direction)
            {
                case MapMoveDirection.Top:
                    offset = Map.Offset.Y;
                    length = Map.WorldAscii.Length;
                    Map.Offset.Y = offset + 1 < length ? offset + 1 : offset;
                    break;
                case MapMoveDirection.Bot:
                    offset = Map.Offset.Y;
                    Map.Offset.Y = offset - 1 >= 0 ? offset - 1 : offset;
                    break;
                case MapMoveDirection.Left:
                    offset = Map.Offset.X;
                    length = Map.WorldAscii[0].Length;
                    Map.Offset.X = offset + 1 < length ? offset + 1 : offset;
                    break;
                case MapMoveDirection.Right:
                    offset = Map.Offset.X;
                    Map.Offset.X = offset - 1 >= 0 ? offset - 1 : offset;
                    break;
            }
            Draw();
        }

        private void SetMapOffset()
        {
            Map.Offset.X = CurrentHero.Coords.X - MapBorder.Width / 2 >= 0 ?
                                CurrentHero.Coords.X - MapBorder.Width / 2 : 0;
            Map.Offset.Y = CurrentHero.Coords.Y - MapBorder.Height / 2 >= 0 ?
                            CurrentHero.Coords.Y - MapBorder.Height / 2 : 0;
        }

        #region drawstuff

        private void RedrawInspector(char symbol)
        {
            int left = Inspector.Coords.X;
            int top = Inspector.Coords.Y;
            HandleConsoleResize();
            while (!Console.KeyAvailable && Inspector.IsInspect)
            {
                HandleConsoleResize();
                if (!Inspector.IsInspect) break;

                Console.SetCursorPosition(left - Map.Offset.X + MapBorder.Offset.X, top - Map.Offset. Y + MapBorder.Offset.Y);
                Console.Write(' ');
                Thread.Sleep(100);

                HandleConsoleResize();
                if (!Inspector.IsInspect) break;

                Console.SetCursorPosition(left - Map.Offset.X + MapBorder.Offset.X, top - Map.Offset.Y + MapBorder.Offset.Y);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write(symbol);
                Console.ResetColor();
                Thread.Sleep(100);
            }
            Console.ResetColor();
            HandleConsoleResize();
            if (!Inspector.IsInspect) return;
            Console.SetCursorPosition(left - Map.Offset.X + MapBorder.Offset.X, top - Map.Offset.Y + MapBorder.Offset.Y);
            Console.Write(symbol);
        }

        private void DrawCharacter(Character character)
        {
            int left = character.Coords.X - Map.Offset.X + MapBorder.Offset.X;
            int top = character.Coords.Y - Map.Offset.Y + MapBorder.Offset.Y;
            if (left >= MapBorder.Offset.X && left <= MapBorder.Offset.X + MapBorder.Width - 3 &&
                top >= MapBorder.Offset.Y && top <= MapBorder.Offset.Y + MapBorder.Height - 3)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(character.Symbol);
            }
        }
        private void RedrawCharacter(Character character)
        {
            //must not redraw when character coords is out of current console size 
            DrawCharacter(character);
            int left = character.PrevCoords.X - Map.Offset.X + MapBorder.Offset.X;
            int top = character.PrevCoords.Y - Map.Offset.Y + MapBorder.Offset.Y;
            if (left >= MapBorder.Offset.X && left <= MapBorder.Offset.X + MapBorder.Width - 3 &&
                top >= MapBorder.Offset.Y && top <= MapBorder.Offset.Y + MapBorder.Height - 3)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(Map.WorldAscii[character.PrevCoords.Y][character.PrevCoords.X]);
            }
        }
        private void Redraw()
        {
            RedrawCharacter(CurrentHero);
            //loop over list of monsters
            RedrawCharacter(TmpMonster);
        }

        private void Draw()
        {
            //Console.Clear();
            //DrawAllBorders();
            for (int i = Map.Offset.Y, j = 0; j < MapBorder.Height - 2 && i < Map.WorldAscii.Length; i++, j++)
            {
                Console.SetCursorPosition(MapBorder.Offset.X, MapBorder.Offset.Y + j);
                string mapstr = Map.WorldAscii[i].Length > Map.Offset.X + MapBorder.Width - 2 ?
                 Map.WorldAscii[i].Substring(Map.Offset.X, MapBorder.Width - 2) :
                 Map.WorldAscii[i].Substring(Map.Offset.X);
                Console.WriteLine(mapstr);
            }
            DrawCharacter(CurrentHero);
            //loop over list of monsters
            DrawCharacter(TmpMonster);
        }

        private void DrawAllBorders()
        {
            MapBorder = new Rectangle
            {
                Width = Console.WindowWidth * 3 / 4,
                Height = Console.WindowHeight * 4 / 5
            };
            MapBorder.Location = new Point(Console.WindowWidth - MapBorder.Width, 0);
            MapBorder.Offset = new Point(MapBorder.Location.X + 1, MapBorder.Location.Y + 1);
            DrawBorder(MapBorder);

            InfoBorder = new Rectangle
            {
                Height = Console.WindowHeight - MapBorder.Height,
                Width = Console.WindowWidth,
                Location = new Point(0, MapBorder.Height),
                Offset = new Point(1, MapBorder.Height + 1)
            };
            DrawBorder(InfoBorder);

            HeroInfoBorder = new Rectangle
            {
                Width = Console.WindowWidth - MapBorder.Width,
                Height = MapBorder.Height,
                Location = new Point(0, 0),
                Offset = new Point(1, 1)
            };
            DrawBorder(HeroInfoBorder);
        }

        private void DrawBorder(Rectangle border)
        {
            int width = border.Width;
            int height = border.Height;
            Point location = border.Location;
            Console.SetCursorPosition(location.X, location.Y);
            Console.Write("╔");
            Console.SetCursorPosition(location.X + 1, location.Y);
            Console.Write(new string('═', width - 2));
            Console.SetCursorPosition(location.X + width - 1, location.Y);
            Console.WriteLine("╗");
            for (int i = 1; i < height - 1; i++)
            {
                Console.SetCursorPosition(location.X, location.Y + i);
                Console.Write("║");
                Console.SetCursorPosition(location.X + 1, location.Y + i);
                Console.Write(new string(' ', width - 2));
                Console.SetCursorPosition(location.X + width - 1, location.Y + i);
                Console.WriteLine("║");
            }
            Console.SetCursorPosition(location.X, location.Y + height - 1);
            Console.Write("╚");
            Console.SetCursorPosition(location.X + 1, location.Y + height - 1);
            Console.Write(new string('═', width - 2));
            Console.SetCursorPosition(location.X + width - 1, location.Y + height - 1);
            Console.Write("╝");
        }

        #endregion

        public int GetMapHeight()
        {
            return Map.MapSize.Height;
        }

        public int GetMapWidth()
        {
            return Map.MapSize.Width;
        }

        public char GetMapSymbol(Point point)
        {
            char symbol = Map.WorldAscii[point.Y][point.X];
            return symbol;
        }
        public char GetEntitySymbol(Point point)
        {
            //list over all entities
            char tmpsymbol = 'S';
            return tmpsymbol;
        }

        private void HandleConsoleResize()
        {
            if (ConsoleWidth != Console.WindowWidth || ConsoleHeight != Console.WindowHeight)
            {
                Inspector.IsInspect = false;
                Console.CursorVisible = false;
                Console.Clear();
                DrawAllBorders();
                SetMapOffset();
                Draw();
            }
            ConsoleWidth = Console.WindowWidth;
            ConsoleHeight = Console.WindowHeight;
        }

        public void PlayGame()
        {
            Console.Clear();
            DrawAllBorders();
            Draw();
            while (!GameOver)
            {
                Input();
                HandleConsoleResize();
                Logic();
                if (CurrentHero.IsMoved)
                    Redraw();
                //need to Redraw() not only when CurrentHero doesn't move
                //For example, if we just killed a monster
            }
        }
    }
}
