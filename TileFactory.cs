﻿using System;
using System.Collections.Generic;

namespace Roguelike
{
    class TileFactory
    {
        static public Dictionary<TileFlyweight.Type, TileFlyweight> tiles = new Dictionary<TileFlyweight.Type, TileFlyweight>();
       
        public TileFactory()
        {
            if (!tiles.ContainsKey(TileFlyweight.Type.Ground))
                tiles.Add(TileFlyweight.Type.Ground, new TileFlyweight("Ground", "Ground", '.', TileFlyweight.Type.Ground));
            if (!tiles.ContainsKey(TileFlyweight.Type.Wall))
                tiles.Add(TileFlyweight.Type.Wall, new TileFlyweight("Wall", "Common wall", '▒', TileFlyweight.Type.Wall));
            if (!tiles.ContainsKey(TileFlyweight.Type.Water))
                tiles.Add(TileFlyweight.Type.Water, new TileFlyweight("Treasure", "Unusual treasure", 'T', TileFlyweight.Type.Water));
            if (!tiles.ContainsKey(TileFlyweight.Type.Lava))
                tiles.Add(TileFlyweight.Type.Lava, new TileFlyweight("Treasure", "Unusual treasure", 'T', TileFlyweight.Type.Lava));
        }

        public TileFlyweight GetTile(Tile tile)
        {
            TileFlyweight.Type key = tile.Type;
            if (tiles.ContainsKey(key))
            {
                tiles[key].Object = tile.Object;
                return tiles[key];
            }
            return null;
        }
    }
}
