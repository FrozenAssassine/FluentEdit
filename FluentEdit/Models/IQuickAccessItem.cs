using Microsoft.UI.Xaml.Media;

namespace FluentEdit.Models
{
    public interface IQuickAccessItem
    {
        string Command { get; set; }
        string Shortcut { get; set; }
        string InfoText { get; set; }
        object Tag { get; set; }
        Brush TextColor { get; set; }
    }
}
