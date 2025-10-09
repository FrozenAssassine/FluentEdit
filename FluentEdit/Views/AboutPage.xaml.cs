using System;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace FluentEdit.Views;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
    }

    public string AppVersion => Package.Current.Id.Version.Major + "." +
            Package.Current.Id.Version.Minor + "." +
            Package.Current.Id.Version.Build;

    private async void NavigateToLink_Click(Controls.SettingsControl sender)
    {
        if (sender.Tag == null)
            return;

        await Windows.System.Launcher.LaunchUriAsync(new Uri(sender.Tag.ToString()));
    }
}
