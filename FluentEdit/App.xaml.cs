using Microsoft.UI.Xaml;
using System;

namespace FluentEdit;

public partial class App : Application
{
    public static MainWindow m_window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();

        var arguments = Environment.GetCommandLineArgs();
        m_window.SendLaunchArguments(arguments);

        m_window.Activate();
    }
}
