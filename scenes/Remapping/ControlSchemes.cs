using System.Collections.Generic;
using Godot;

public static class ControlSchemes
{
    public static Dictionary<string, Dictionary<string, string>> SpriteMappings = new Dictionary<
        string,
        Dictionary<string, string>
    >()
    {
        {
            "WASD",
            new Dictionary<string, string>()
            {
                { "left", "res://scenes/Remapping/assets/A_Key_Light.png" },
                { "right", "res://scenes/Remapping/assets/D_Key_Light.png" },
                { "up", "res://scenes/Remapping/assets/W_Key_Light.png" },
                { "down", "res://scenes/Remapping/assets/S_Key_Light.png" },
            }
        },
        {
            "ARROWS",
            new Dictionary<string, string>()
            {
                { "left", "res://scenes/Remapping/assets/Arrow_Left_Key_Light.png" },
                { "right", "res://scenes/Remapping/assets/Arrow_Right_Key_Light.png" },
                { "up", "res://scenes/Remapping/assets/Arrow_Up_Key_Light.png" },
                { "down", "res://scenes/Remapping/assets/Arrow_Down_Key_Light.png" },
            }
        },
        {
            "QWERT",
            new Dictionary<string, string>()
            {
                { "left", "res://scenes/Remapping/assets/Q_Key_Light.png" },
                { "right", "res://scenes/Remapping/assets/R_Key_Light.png" },
                { "up", "res://scenes/Remapping/assets/W_Key_Light.png" },
                { "down", "res://scenes/Remapping/assets/E_Key_Light.png" },
            }
        },
        {
            "CONTROLLER",
            new Dictionary<string, string>()
            {
                { "left", "res://scenes/Remapping/assets/Positional_Prompts_Left.png" },
                { "right", "res://scenes/Remapping/assets/Positional_Prompts_Right.png" },
                { "up", "res://scenes/Remapping/assets/Positional_Prompts_Up.png" },
                { "down", "res://scenes/Remapping/assets/Positional_Prompts_Down.png" },
            }
        },
    };
}
