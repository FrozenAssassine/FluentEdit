using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace FluentEdit.Helper;

internal class WindowStateHelper
{
    public static OverlappedPresenterState GetWindowState(Window window)
    {
        return (window.AppWindow.Presenter as OverlappedPresenter)?.State ?? OverlappedPresenterState.Restored;
    }

    public static OverlappedPresenterState SetWindowState(Window window, OverlappedPresenterState state)
    {
        var presenter = window.AppWindow.Presenter as OverlappedPresenter;

        if (presenter != null)
        {
            if (state == OverlappedPresenterState.Maximized)
                presenter.Maximize();
            else if (state == OverlappedPresenterState.Minimized)
                presenter.Minimize();
            else if (state == OverlappedPresenterState.Restored)
                presenter.Restore();
        }

        return state;
    }
}