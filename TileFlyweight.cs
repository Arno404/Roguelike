using System;
namespace Roguelike
{
    class TileFlyweight
    {
        public string Name { get; }
        public string Description { get; }
        public char Symbol { get; }
        public int Price {get; set;}
        public Type TileType { get; }
        public ConsoleColor Color { get; }
        public BaseEntity Object { get; set; }

        public enum Type
        {
            Wall = 0,
            Ground = 1,
            Water = 2,
            Lava = 3
        }

        public TileFlyweight(string name, string description, char symbol, int price, Type type, ConsoleColor color = ConsoleColor.White, BaseEntity obj = null)
        {
            Name = name;    
            Description = description;
            Symbol = symbol;
            Price = price;
            TileType = type;
            Color = color;
            Object = obj;
        }
    }
}
