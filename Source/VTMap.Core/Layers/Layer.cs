using System.ComponentModel;
using System.Runtime.CompilerServices;
using VTMap.Core.Interfaces;

namespace VTMap.Core.Layers
{
    public class Layer : INotifyPropertyChanged
    {
        bool enabled = true;
        protected IRenderer _renderer = null;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasRenderer => _renderer != null;

        public IRenderer Renderer { get => _renderer; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
