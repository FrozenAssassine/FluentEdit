using System;

namespace FluentEdit.Extensions;

public static class LocalizationExtensions
{
    public static string Localized(this string originalString, string localizeValue)
    {
        var res = MainWindow.localizationManager.ResourceMap.TryGetSubtree("Resources")?.TryGetValue(localizeValue, MainWindow.localizationManager.ResourceContext);
        if (res == null)
            return originalString;

        if (res.ValueAsString.Contains("\\n"))
            return res.ValueAsString.Replace("\\n", Environment.NewLine);
        return res.ValueAsString;
    }
}
