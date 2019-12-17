using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Roguelike
{
    class DijkstraAI : IMonsterIntelligence
    {
        private void PutRegionToFile(Tile[,] region)
        {
            TileFactory fact = new TileFactory();

            using (StreamWriter sw = new StreamWriter("Dijkstra.txt"))
            {
                for (int i = 0; i < region.GetLength(0); i++)
                {
                    for (int j = 0; j < region.GetLength(1); j++)
                    {
                        sw.Write(fact.GetTile(region[i, j]).Symbol.ToString());
                    }
                    sw.Write("\n");
                }
            }
        }
        public void FindPath(Monster monster, BaseEntity target)
        {
            Tile[,] region = Program.GameEngine.GetMapRegion(monster.Coords.Invert(), target.Coords.Invert(), 0);

            //PutRegionToFile(region);
        }
    }
}
