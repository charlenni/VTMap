using BruTile;
using SkiaSharp;
using System;
using System.Linq;
using VTMap.Common.Interfaces;
using VTMap.Common.Primitives;
using VTMap.MapboxGL.Extensions;

namespace VTMap.MapboxGL
{
    public class MGLRasterTileSource : IDrawableTileSource
    {
        public string Name { get; }

        public double MinVisible {get; set;}

        public double MaxVisible { get; set; }

        public int TileSize { get; set; }

        /// <summary>
        /// Tile source that provides the content of the tiles. In this case, it provides byte[] with images.
        /// </summary>
        public ITileSource Source { get; }

        /// <summary>
        /// Style to use for drawing images
        /// </summary>
        public IVectorStyleLayer Style { get; set; }

        public ITileSchema Schema => Source.Schema;

        public Attribution Attribution => Source.Attribution;

        public MGLRasterTileSource(string name, ITileSource source)
        {
            Name = name;
            Source = source;
        }

        Drawable IDrawableTileSource.GetDrawableTile(TileInfo ti)
        {
            // Check Schema for TileInfo
            var tileInfo = Schema.YAxis == YAxis.OSM ? ti.ToTMS() : ti;

            try
            {
                var bytes = Source.GetTile(tileInfo);
                var image = SKImage.FromEncodedData(bytes);

                var result = new RasterTile(TileSize, image, Style.Paints.FirstOrDefault<IVectorPaint>());

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Provides the image as byte array
        /// </summary>
        /// <param name="tileInfo">TileInfo for tile to get</param>
        /// <returns>Image as byte array</returns>
        public byte[] GetTile(TileInfo tileInfo)
        {
            return Source.GetTile(tileInfo);
        }
    }
}
