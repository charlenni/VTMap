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
        readonly MapData _map;
        readonly SynchronizedCollection<Layer> _layers;
        List<Layer> _reverseLayers;
        List<IRenderer> _renderers;
        bool changed = true;

        public LayerCollection(MapData owner)
        {
            _map = owner;
            _layers = new SynchronizedCollection<Layer>();
        }

        public List<IRenderer> Renderers
        {
            get
            {
                if (changed)
                    Update();

                return _renderers;
            }
        }

        public void Add(Layer layer)
        {
            Add(layer, -1);
        }

        public void Add(Layer layer, int index = -1)
        {
            if (_layers.Contains(layer))
                throw new ArgumentException("Layer added twice");

            if (layer is IUpdatedEventHandler)
                _map.Updated += ((IUpdatedEventHandler)layer).OnUpdated;
            //if (layer is IGestureEventHandler)
            //    map.GestureDetected += ((IGestureEventHandler)layer).OnGestureDetected;

            layer.PropertyChanged += OnLayerChanged;

            if (index == -1)
                index = _layers.Count;

            _layers.Insert(index, layer);

            changed = true;
        }

        public void Remove(Layer layer)
        {
            if (!_layers.Contains(layer))
                throw new ArgumentException("Unknown layer");

            changed = true;

            layer.PropertyChanged -= OnLayerChanged;

            if (layer is IUpdatedEventHandler)
                _map.Updated -= ((IUpdatedEventHandler)layer).OnUpdated;

            _layers.Remove(layer);
        }

        public void RemoveItem(int index)
        {
            var item = _layers[index];

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
                _reverseLayers = _layers.Where((i) => i.Enabled && i.HasRenderer).Reverse().ToList() ?? new List<Layer>();
                _renderers = _layers.Where((i) => i.Enabled && i.HasRenderer).Select((i) => i.Renderer).ToList() ?? new List<IRenderer>();

                changed = false;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
        }
    }
}
