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
                { "left", "res://Scenes/UI/Remapping/Assets/A_Key_Light.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/D_Key_Light.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/W_Key_Light.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/S_Key_Light.png" },
            }
        },
        {
            "ARROWS",
            new Dictionary<string, string>()
            {
                { "left", "res://Scenes/UI/Remapping/Assets/Arrow_Left_Key_Light.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/Arrow_Right_Key_Light.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/Arrow_Up_Key_Light.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/Arrow_Down_Key_Light.png" },
            }
        },
        {
            "QWERT",
            new Dictionary<string, string>()
            {
                { "left", "res://Scenes/UI/Remapping/Assets/Q_Key_Light.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/R_Key_Light.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/W_Key_Light.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/E_Key_Light.png" },
            }
        },
        {
            "CONTROLLER",
            new Dictionary<string, string>()
            {
                { "left", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Left.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Right.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Up.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Down.png" },
            }
        },
    };
}
