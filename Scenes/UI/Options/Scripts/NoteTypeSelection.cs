using System;
using Godot;

public partial class NoteTypeSelection : CheckBox
{
    private bool _isTypeArrow;

    public override void _Ready()
    {
        _isTypeArrow = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.TypeIsArrow).As<bool>();
        ButtonPressed = _isTypeArrow;
        Toggled += OnTypeChanged;
    }

    private void OnTypeChanged(bool pressed)
    {
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.TypeIsArrow, pressed);
        InputHandler.UseArrows = pressed;
    }
}
