using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
namespace TPulseAPI
{
    public class MapTools
    {

        private static Random Random = new Random();

        public static void GetRandomClearTileWithInRange(int startTileX, int startTileY, int tileXRange, int tileYRange,
                                                  out int tileX, out int tileY)
        {
            int j = 0;
            do
            {
                if (j == 100)
                {
                    tileX = startTileX;
                    tileY = startTileY;
                    break;
                }

                tileX = startTileX + Random.Next(tileXRange * -1, tileXRange);
                tileY = startTileY + Random.Next(tileYRange * -1, tileYRange);
                j++;
            } while (TileValid(tileX, tileY) && !TileClear(tileX, tileY));
        }

        /// <summary>
        /// Determines if a tile is valid
        /// </summary>
        /// <param name="tileX">Location X</param>
        /// <param name="tileY">Location Y</param>
        /// <returns>If the tile is valid</returns>
        public static bool TileValid(int tileX, int tileY)
        {
            return tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY;
        }

        /// <summary>
        /// Checks to see if the tile is clear.
        /// </summary>
        /// <param name="tileX">Location X</param>
        /// <param name="tileY">Location Y</param>
        /// <returns>The state of the tile</returns>
        private static bool TileClear(int tileX, int tileY)
        {
            return !Main.tile[tileX, tileY].active;
        }
    }
}
