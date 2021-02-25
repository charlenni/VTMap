using System;
using VTMap.Core.Events;
using VTMap.Core.Layers;

namespace VTMap.Core.Map
{
    /// <summary>
    /// Holds all data for map like layers and so on
    /// </summary>
    public class MapData
    {
        /// <summary>
        /// Layers for this map
        /// </summary>
        public LayerCollection Layers { get; set; }
        public LayerCollection Overlays { get; set; }

        public EventHandler<UpdateEventArgs> Updated;

        public MapData()
        {
            Layers = new LayerCollection(this);
            Overlays = new LayerCollection(this);
        }

        public void MapDataUpdate(UpdateEventArgs args)
        {
            Updated?.Invoke(this, args);
        }
    }
}
