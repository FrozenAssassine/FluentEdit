using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace FluentEdit.Helper;

public class ThemeHelper
{
    public static ElementTheme _RequestedTheme = ElementTheme.Default;
    public static ElementTheme CurrentTheme
    {
        get { return GetWindowTheme(App.m_window); }
        set { SetWindowTheme(App.m_window, value); }
    }

    public static void SetWindowTheme(Window window, ElementTheme theme)
    {
        if (window.Content is FrameworkElement frame)
            frame.RequestedTheme = theme;
    }
    public static ElementTheme GetWindowTheme(Window window)
    {
        if (window.Content is FrameworkElement frame)
            return frame.RequestedTheme;
        return ElementTheme.Default;
    }
}
