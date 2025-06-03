using System;
using Godot;

public partial class NoteTypeSelection : OptionButton
{
    private bool _isTypeArrow;

    public override void _Ready()
    {
        _isTypeArrow = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.TypeIsArrow).As<bool>();
        Connect("item_selected", new Callable(this, nameof(OnTypeChanged)));
        PresetDropdown(_isTypeArrow);
    }

    private void OnTypeChanged(int index)
    {
        switch (index)
        {
            case 0:
                SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.TypeIsArrow, false);
                InputHandler.UseArrows = false;
                break;
            case 1:
                SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.TypeIsArrow, true);
                InputHandler.UseArrows = true;
                break;
        }
    }

    private void PresetDropdown(bool isTypeArrow)
    {
        if (isTypeArrow)
            Selected = 1;
        else
            Selected = 0;
    }
}
