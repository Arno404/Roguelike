using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CaveGenerator;

namespace Roguelike
{
    class GameEngine
    {
        private Hero CurrentHero;
        private MapInspector Inspector;
        private bool GameOver = false;
        public bool GameStarted { get; private set; }
        private Cave Map;
        public Rectangle HeroInfoBorder { get; set; }
        public Rectangle MapBorder { get; set; }
        public InfoBorder InfoBorder { get; set; }
        public Random GameRandom = new Random();
        private int ConsoleHeight = 0;
        private int ConsoleWidth = 0;
        private Monster TmpMonster; //make this as list
        public void Init()
        {
            Map = new Cave();
            Map.Build();
            Map.ConnectCaves();
            Map.BuildTileMap();
            Map.Offset = new Point(0, 0);
            CurrentHero = new Hero(new Point(12, 10), 15, 0, 8, 10, "Chiks-Chiriks");
            TmpMonster = new Monster(new Point(15, 15), 10, 10, 10, "Snake", 'S', new DijkstraAI());
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
            if (!CurrentHero.IsActionDone)
            {
                //"if" is not correct, we mustn't go there if we hit a wall, for example
                CurrentHero.DoGameAction();
                if(!CurrentHero.IsActionDone) return;
            }

            TmpMonster.MoveTo(CurrentHero);
            TmpMonster.DoGameAction();
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
            char symbol = CurrentHero.Symbol;
            TileFlyweight tile;
            ConsoleColor color;
            string description;
            while (Inspector.IsInspect)
            {
                tile = GetTile(Inspector.Coords.X, Inspector.Coords.Y);
                if (tile.Object != null) //TODO: draw this with right color
                {
                    description = tile.Object.Name; // must be description
                    symbol = tile.Object.Symbol;
                }
                else
                {
                    description = tile.Description;
                    symbol = tile.Symbol;
                }
                color = tile.Color;

                InfoBorder.ClearLineAndWrite($"{description}: {symbol}", color, tile.Description.Length + 2, 1);

                RedrawInspector(symbol, color);
                Input(Inspector);
                HandleConsoleResize();
                if (!Inspector.IsInspect)
                {
                    SetMapOffset();
                    DrawAfterMapMoving();
                    Draw();
                    Program.GameEngine.InfoBorder.Clear();
                    break;
                }
                Inspector.Move();
                if (!Map.WorldTile[Inspector.Coords.Y, Inspector.Coords.X].Visible) Inspector.RestoreCoords();
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
            int vision = CurrentHero.RangeOfVision;
            if (distToTop <= vision)
                MoveMapToDir(MapMoveDirection.Bot);
            else if (distToBot <= vision)
                MoveMapToDir(MapMoveDirection.Top);
            else if (distToRight <= vision)
                MoveMapToDir(MapMoveDirection.Left);
            else if (distToLeft <= vision)
                MoveMapToDir(MapMoveDirection.Right);
        }

        private void MoveMapToDir(MapMoveDirection direction)
        {
            int offset, length;
            switch (direction)
            {
                case MapMoveDirection.Top:
                    offset = Map.Offset.Y;
                    length = Map.Map.GetLength(0);
                    Map.Offset.Y = offset + 1 < length ? offset + 1 : offset;
                    break;
                case MapMoveDirection.Bot:
                    offset = Map.Offset.Y;
                    Map.Offset.Y = offset - 1 >= 0 ? offset - 1 : offset;
                    break;
                case MapMoveDirection.Left:
                    offset = Map.Offset.X;
                    length = Map.Map.GetLength(1);
                    Map.Offset.X = offset + 1 < length ? offset + 1 : offset;
                    break;
                case MapMoveDirection.Right:
                    offset = Map.Offset.X;
                    Map.Offset.X = offset - 1 >= 0 ? offset - 1 : offset;
                    break;
            }
            DrawAfterMapMoving();
        }

        private void SetMapOffset()
        {
            Map.Offset.X = CurrentHero.Coords.X - MapBorder.Width / 2 >= 0 ?
                                CurrentHero.Coords.X - MapBorder.Width / 2 : 0;
            Map.Offset.Y = CurrentHero.Coords.Y - MapBorder.Height / 2 >= 0 ?
                            CurrentHero.Coords.Y - MapBorder.Height / 2 : 0;
        }

        #region drawstuff

        private void RedrawInspector(char symbol, ConsoleColor color)
        {
            int left = Inspector.Coords.X;
            int top = Inspector.Coords.Y;
            HandleConsoleResize();

            Console.BackgroundColor = ConsoleColor.DarkYellow;

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
                Console.ForegroundColor = color;
                Console.Write(symbol);
                Console.ForegroundColor = ConsoleColor.White;
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
                top >= MapBorder.Offset.Y && top <= MapBorder.Offset.Y + MapBorder.Height - 3 &&
                Map.WorldTile[character.Coords.Y, character.Coords.X].Visible)
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
                top >= MapBorder.Offset.Y && top <= MapBorder.Offset.Y + MapBorder.Height - 3 &&
                Map.WorldTile[character.PrevCoords.Y, character.PrevCoords.X].Visible)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(GetTile(character.PrevCoords.X, character.PrevCoords.Y).Symbol);
            }
            DrawCharacter(character);
        }
        private void Redraw()
        {
            Draw();
            //RedrawCharacter(CurrentHero);
            //loop over list of monsters
            RedrawCharacter(TmpMonster);
        }

        private void Draw()
        {
            FOV();
            TileFlyweight tileFlyweight;
            for (int i = Map.Offset.Y, y = 0; y < MapBorder.Height - 2 && i < Map.WorldTile.GetLength(0); i++, y++)
            {
                for (int j = Map.Offset.X, x = 0; x < MapBorder.Width - 2 && j < Map.WorldTile.GetLength(1); j++, x++)
                {
                    if (Map.WorldTile[i, j].Visible)
                    {
                        Console.SetCursorPosition(MapBorder.Offset.X + x, MapBorder.Offset.Y + y);
                        tileFlyweight = GetTile(j, i);
                        Console.ForegroundColor = tileFlyweight.Color;
                        Console.Write(tileFlyweight.Symbol);
                        Console.ResetColor();
                    }
                }
            }
            DrawCharacter(CurrentHero);
            //loop over list of monsters
            DrawCharacter(TmpMonster);
        }

        private void DrawAfterMapMoving()
        {
            FOV();
            for (int i = Map.Offset.Y, y = 0; y < MapBorder.Height - 2 && i < Map.WorldTile.GetLength(0); i++, y++)
            {
                Console.SetCursorPosition(MapBorder.Offset.X, y + 1);
                Console.Write(new string(' ', MapBorder.Width - 2));
                for (int j = Map.Offset.X, x = 0; x < MapBorder.Width - 2 && j < Map.WorldTile.GetLength(1); j++, x++)
                {
                    if (Map.WorldTile[i, j].Visible)
                    {
                        Console.SetCursorPosition(MapBorder.Offset.X + x, MapBorder.Offset.Y + y);
                        Console.Write(GetTile(j, i).Symbol);
                    }
                }
            }
            DrawCharacter(CurrentHero);
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

            InfoBorder = new InfoBorder
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

        private void FOV()
        {
            double x, y;
            /*for (int i = Map.Offset.Y; i < Map.Offset.Y + MapBorder.Height - 2 && i < Map.WorldTile.GetLength(0); i++)
                for (int j = Map.Offset.X; j < Map.Offset.X + MapBorder.Width - 2 && j < Map.WorldTile.GetLength(1); j++)
                    Map.WorldTile[i, j].Visible = false;*/
            for (int i = 0; i < 360; i++)
            {
                x = Math.Cos(i * 0.01745);
                y = Math.Sin(i * 0.01745);
                ComputeFOV(x, y);
            }
        }

        private void ComputeFOV(double x, double y)
        {
            double ox, oy;
            ox = CurrentHero.Coords.X + 0.5;
            oy = CurrentHero.Coords.Y + 0.5;
            for (int i = 0; i < CurrentHero.RangeOfVision; i++)
            {
                Map.WorldTile[(int)oy, (int)ox].Visible = true;
                if (Map.WorldTile[(int)oy, (int)ox].Type == TileFlyweight.Type.Wall)
                    return;
                ox += x;
                oy += y;
            }
        }

        public int GetMapHeight()
        {
            return Map.MapSize.Height;
        }

        public int GetMapWidth()
        {
            return Map.MapSize.Width;
        }

        /// <summary>
        /// Gets the tile.
        /// </summary>
        /// <returns>The tile.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public TileFlyweight GetTile(int x, int y)
        {
            Tile tile = Map.WorldTile[y, x];
            TileFactory factory = new TileFactory();
            return factory.GetTile(tile);
        }

        /// <summary>
        /// Sets the object to a tile.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="obj">Object.</param>
        public void SetObject(int x, int y, BaseEntity obj)
        {
            Map.WorldTile[y, x].Object = obj;
        }

        /// <summary>
        /// Removes the object from tile.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void RemoveObject(int x, int y)
        {
            Map.WorldTile[y, x].Object = null;
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
                if (CurrentHero.IsActionDone)
                    Redraw();
                //need to Redraw() not only when CurrentHero doesn't move
                //For example, if we just killed a monster
            }
        }
    }
}
