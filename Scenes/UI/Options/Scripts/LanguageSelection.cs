using System;
using Godot;

public partial class LanguageSelection : OptionButton
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Connect("item_selected", new Callable(this, nameof(OnLanguageSelected)));
        PresetDropdown(TranslationServer.GetLocale());
    }

    public void OnLanguageSelected(int index)
    {
        switch (index)
        {
            case 0:
                ChangeLanguage("en");
                break;
            case 1:
                ChangeLanguage("cn");
                break;
        }
    }

    private void ChangeLanguage(string language)
    {
        TranslationServer.SetLocale(language);
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.LanguageKey, language);
    }

    private void PresetDropdown(string language)
    {
        switch (language)
        {
            case "cn":
                Selected = 1;
                break;
            default:
                Selected = 0;
                break;
        }
    }
}
