using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Computer_Graphics;
public class EditablePoint : INotifyPropertyChanged
{
    private byte x;
    private byte y;

    public byte X
    {
        get => x;
        set
        {
            if (x != value)
            {
                x = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayX));
            }
        }
    }

    public byte Y
    {
        get => y;
        set
        {
            if (y != value)
            {
                y = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayY));
            }
        }
    }
    public byte DisplayX
    {
        get => X;
        set
        {
            X = value;
        }
    }

    public byte DisplayY
    {
        get => (byte)(255 - Y);
        set
        {
            Y = (byte)(255 - value);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
