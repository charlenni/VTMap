using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VTMap.Core.Events;
using VTMap.Core.Interfaces;
using VTMap.Core.Map;

namespace VTMap.Core.Layers
{
    /// <summary>
    /// Collection of layers
    /// </summary>
    public class LayerCollection
    {
        readonly MapData map;
        readonly SynchronizedCollection<Layer> layers;
        List<Layer> reverseLayers;
        List<IRenderer> renderers;
        bool changed = false;

        public LayerCollection(MapData owner)
        {
            map = owner;
            layers = new SynchronizedCollection<Layer>();
        }

        public List<IRenderer> Renderers
        {
            get
            {
                if (changed)
                    Update();

                return renderers;
            }
        }

        public void Add(Layer layer)
        {
            Add(layer, layers.Count - 1);
        }

        public void Add(Layer layer, int index = -1)
        {
            if (layers.Contains(layer))
                throw new ArgumentException("Layer added twice");

            if (layer is IUpdatedEventHandler)
                map.Updated += ((IUpdatedEventHandler)layer).OnUpdated;
            //if (layer is IGestureEventHandler)
            //    map.GestureDetected += ((IGestureEventHandler)layer).OnGestureDetected;

            layer.PropertyChanged += OnLayerChanged;

            if (index == -1)
                index = layers.Count;

            layers.Insert(index, layer);

            changed = true;
        }

        public void Remove(Layer layer)
        {
            if (!layers.Contains(layer))
                throw new ArgumentException("Unknown layer");

            changed = true;

            layer.PropertyChanged -= OnLayerChanged;

            if (layer is IUpdatedEventHandler)
                map.Updated -= ((IUpdatedEventHandler)layer).OnUpdated;

            layers.Remove(layer);
        }

        public void RemoveItem(int index)
        {
            var item = layers[index];

            if (item == null)
                throw new ArgumentException("Unknown index");

            Remove(item);
        }

        private void OnLayerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Layer.Enabled))
                changed = true;
        }

        private void Update()
        {
            try
            {
                reverseLayers = layers.Where((i) => i.Enabled && i.HasRenderer).Reverse().ToList();
                renderers = layers.Where((i) => i.Enabled && i.HasRenderer).Select((i) => i.Renderer).ToList();

                changed = false;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
        }
    }
}
