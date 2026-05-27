using System;
using Microsoft.UI.Xaml.Controls;
using FluentEdit.Extensions;

namespace FluentEdit.Dialogs
{
    public class NewVersionInfobar : InfoBar
    {
        public void Show(string version)
        {
            this.Title = "Updated".Localized("InfoBar_Update_Title/Text");
            var messageTemplate = "Welcome to version {0}".Localized("InfoBar_Update_Message/Text");
            this.Message = string.Format(messageTemplate, version);
            this.ActionButton = new HyperlinkButton { Content = "Release Notes".Localized("InfoBar_Update_Action/Text"), NavigateUri = new Uri("https://github.com/FrozenAssassine/FluentEdit/releases") };
            this.IsOpen = true;
            this.Width = 300;
            this.Severity = InfoBarSeverity.Success;
        }
    }
}
