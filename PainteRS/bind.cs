using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PainteRS
{
    internal class bind : INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> _LIST;
        public List<string> LIST
        {
            get { return _LIST; }
            set
            {
                _LIST = value;
                OnPropertyChanged("ListBox");
            }

        }
    }
}
