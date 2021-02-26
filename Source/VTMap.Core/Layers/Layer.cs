using System.ComponentModel;
using System.Runtime.CompilerServices;
using VTMap.Core.Interfaces;
using VTMap.Core.Projections;

namespace VTMap.Core.Layers
{
    public class Layer : INotifyPropertyChanged
    {
        IProjection _projection = new EPSG3857Projection();

        public IProjection Projection
        {
            get
            {
                return _projection;
            }
            set
            {
                if (_projection != value)
                {
                    _projection = value;
                    OnPropertyChanged();
                }
            }
        }

        bool _enabled = true;

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged();
                }
            }
        }

        protected IRenderer _renderer = null;

        public IRenderer Renderer { get => _renderer; }

        public bool HasRenderer => _renderer != null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
