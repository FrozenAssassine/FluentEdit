namespace FluentEdit.Models;

public class LanguageItem
{
    public string Tag { get; }
    public string DisplayName { get; }

    public LanguageItem(string tag, string displayName)
    {
        Tag = tag;
        DisplayName = displayName;
    }
}
