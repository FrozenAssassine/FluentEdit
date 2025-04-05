using Microsoft.UI.Xaml;

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
        m_window.Activate();
    }
}
