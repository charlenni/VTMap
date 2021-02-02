using BruTile;

namespace VTMap.Core.Extensions
{
    public static class TileExtension
    {
        /// <summary>
        /// Default size of a tile
        /// </summary>
        public const int TileSize = 256;

        public static TileIndex Test(this TileIndex value)
        {
            return value;
        }

    }
}
