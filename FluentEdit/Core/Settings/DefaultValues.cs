using Windows.UI;

namespace FluentEdit.Core.Settings;

public class DefaultValues
{
    public const string FontFamily = "Consolas";
    public const int FontSize = 18;
    public const int Theme = 0; //ElemenTheme.Default
    public const int BackgroundType = 0; //BackroundType.Mica
    public static readonly Color DefaultStaticBackground = Color.FromArgb(255, 25, 25, 25);
    public static readonly Color DefaultAcrylicBackground = Color.FromArgb(150, 25, 25, 25);
}
