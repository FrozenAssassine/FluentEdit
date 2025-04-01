using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;

namespace FluentEdit.Models
{
    internal class QuickAccessSubItem : IQuickAccessItem
    {
        public List<IQuickAccessItem> Items { get; set; } = new List<IQuickAccessItem>();
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public object Tag { get; set; }
        public Brush TextColor { get; set; }
        public string InfoText { get; set; } = null;
    }
}
