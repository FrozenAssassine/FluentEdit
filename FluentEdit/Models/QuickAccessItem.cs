using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace FluentEdit.Models
{
    internal class QuickAccessItem : IQuickAccessItem
    {
        public delegate void RunCommandWindowItemClickedEvent(object sender, RoutedEventArgs e);
        public event RunCommandWindowItemClickedEvent RunCommandWindowItemClicked;
        public void InvokeEvent()
        {
            RunCommandWindowItemClicked?.Invoke(this, null);
        }

        public object Tag { get; set; }
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public Brush TextColor { get; set; }
        public string InfoText { get; set; } = null;
    }
}
