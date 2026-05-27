using System.Collections.Generic;
using System.Diagnostics;
using FluentEdit.Core.Settings;
using FluentEdit.Models;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.System.UserProfile;

namespace FluentEdit.Helper;

public class LocalizationManager
{
    private readonly ResourceManager resourceManager = new();
    public ResourceContext ResourceContext { get; }
    public ResourceMap ResourceMap { get; }

    public List<LanguageItem> languages = new();

    public LocalizationManager()
    {
        ResourceContext = resourceManager.CreateResourceContext();
        ResourceMap = resourceManager.MainResourceMap;
    }


    public void SetLanguage(LanguageItem languageItem)
    {
        if (languages.Contains(languageItem))
        {
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = languageItem.Tag;
            AppSettings.Language = languageItem.Tag;
        }
    }

    public void Initialize()
    {
        RegisterLanguageFromResource();

        var systemLanguages = GlobalizationPreferences.Languages;
        string systemLanguage = systemLanguages.Count > 0 ? systemLanguages[0] : DefaultValues.Language;

        var settingsLanguage = AppSettings.Language;
        var res = languages.Find(x => x.Tag == settingsLanguage);
        if (res == null)
        {
            SetLanguage(languages.Find(x => x.Tag == systemLanguage));
            return;
        }

        SetLanguage(res);
    }

    private void RegisterLanguageFromResource()
    {
        ResourceMap resourceMap = new ResourceManager().MainResourceMap.GetSubtree("LanguageList");
        for (uint i = 0; i < resourceMap.ResourceCount; i++)
        {
            var resource = resourceMap.GetValueByIndex(i);
            languages.Add(new LanguageItem(resource.Key, resource.Value.ValueAsString));
        }
    }
}
