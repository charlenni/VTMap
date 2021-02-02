using System.ComponentModel;
using System.Runtime.CompilerServices;
using VTMap.Core.Interfaces;

namespace VTMap.Core.Layers
{
    public class Layer : INotifyPropertyChanged
    {
        bool enabled = true;

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

        public IRenderer renderer = null;

        public bool HasRenderer => renderer != null;

        public IRenderer Renderer => renderer;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
