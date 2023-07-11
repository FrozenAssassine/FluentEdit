using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace FluentEdit.Dialogs
{
    public class NewVersionInfobar : muxc.InfoBar
    {
        public void Show(string version)
        {
            this.Title = "Updated";
            this.Message = "Welcome to version " + version + "";
            this.ActionButton = new HyperlinkButton { Content="Release Notes", NavigateUri = new Uri("https://github.com/FrozenAssassine/FluentEdit/releases") };
            this.IsOpen = true;
            this.Width = 300;
            this.Severity = muxc.InfoBarSeverity.Success;
        }
    }
}
