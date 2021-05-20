using System.ComponentModel;

public class ValueObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public static event PropertyChangedEventHandler ValueChanged;

    private double value = 0;
    public double Value
    {
        get => value;
        set
        {
            this.value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            ValueChanged?.Invoke(this, null);
        }
    }
}
