using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Computer_Graphics;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Line), "line")]
[JsonDerivedType(typeof(Circle), "circle")]
[JsonDerivedType(typeof(Polygon), "polygon")]
[JsonDerivedType(typeof(Pacman), "pacman")]
[JsonDerivedType(typeof(Rectangle), "rectangle")]
internal abstract class Shape
{
    public System.Windows.Media.Color Color { get; set; }
    public int Thickness { get; set; }
    public abstract void Draw(WriteableBitmap bitmap, bool useAA);
    public abstract bool IsClose(System.Windows.Point p, double treshold = 5);
}
