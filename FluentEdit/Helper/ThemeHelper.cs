using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FluentEdit.Helper
{
    internal class ThemeHelper
    {
        public static ElementTheme CurrentTheme
        {
            set
            {
                if (Window.Current.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            }
            get
            {
                if (Window.Current.Content is FrameworkElement rootElement)
                    return rootElement.RequestedTheme;
                return ElementTheme.Default;
            }
        }
    }
}
