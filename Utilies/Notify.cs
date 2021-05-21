using System.ComponentModel;

namespace Extensions
{
    public class Notify : INotifyPropertyChanged  
    {
        public Notify() {}

        public Notify(object source) => this.source = source;

        public event PropertyChangedEventHandler PropertyChanged;

        private object source;

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private string _ { get; set; } = null;

        public void Update<TVal, TProp>(ref TVal reference, TVal value, ItemPropertyInfo property)
        {
            reference = value;
            Raise(property.Name);
        }

        public void Update<TVal>(ref TVal reference, TVal value, string info)
        {
            reference = value;
            Raise(info);
        }
    }
}
