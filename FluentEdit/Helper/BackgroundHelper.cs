using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FluentEdit.Helper
{
    internal class BackgroundHelper
    {
        public static void SetBackground(Control element, Color? color, int type)
        {
            if (element == null)
                return;

            //remove mica
            if (BackdropMaterial.GetApplyToRootOrPageBackground(element) && type != 0)
                BackdropMaterial.SetApplyToRootOrPageBackground(element, false);

            if (type == 1 && color.HasValue) //acrylic
            {
                var clr = color.Value;
                int transparency = clr.A;
                element.Background = new AcrylicBrush
                {
                    TintColor = clr,
                    TintOpacity = transparency / 255.0,
                    FallbackColor = clr,
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                };
            }
            else if (type == 2 && color.HasValue) //static
            {
                element.Background = new SolidColorBrush { Color = color.Value };
            }
            else if (type == 0) //mica
            {
                BackdropMaterial.SetApplyToRootOrPageBackground(element, true);
            }
        }
    }
}
