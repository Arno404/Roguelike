using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Roguelike
{
    class Location
    {
        //private string LastVisitedLocation = "location1";
        public readonly string[] AsciiView = File.ReadAllLines($"Locations/location1.txt");
        private Point HeroPrevCoords;
        public void Draw(Hero hero)
        {
            Console.CursorVisible = false;
            //Console.SetWindowSize();
            //string[] locationArr = File.ReadAllLines($"Locations/{LastVisitedLocation}.txt");
            for (int i = 0; i < AsciiView.Length; i++)
                Console.WriteLine(AsciiView[i]);
            Console.SetCursorPosition(hero.Coords.X, hero.Coords.Y);
            Console.Write("@");
            HeroPrevCoords = new Point(hero.Coords.X, hero.Coords.Y);
        }

        public void Redraw(Hero hero)
        {
            Console.SetCursorPosition(hero.Coords.X, hero.Coords.Y);
            Console.Write("@");
            Console.SetCursorPosition(HeroPrevCoords.X, HeroPrevCoords.Y);
            Console.Write(GetSymbolByCoords(HeroPrevCoords));
            HeroPrevCoords = new Point(hero.Coords.X, hero.Coords.Y);
        }
        public char GetSymbolByCoords(Point point)
        {
            return AsciiView[point.Y][point.X];
        }
    }
}