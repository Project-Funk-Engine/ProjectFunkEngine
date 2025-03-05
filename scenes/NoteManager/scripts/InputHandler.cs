using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

/**
 * @class InputHandler
 * @brief InputHandler to handle input, and manage note checkers. WIP
 */
public partial class InputHandler : Node2D
{
    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    public ArrowData[] Arrows = new ArrowData[]
    {
        new ArrowData()
        {
            Color = Colors.Green,
            Key = "arrowUp",
            Type = ArrowType.Up,
        },
        new ArrowData()
        {
            Color = Colors.Aqua,
            Key = "arrowDown",
            Type = ArrowType.Down,
        },
        new ArrowData()
        {
            Color = Colors.HotPink,
            Key = "arrowLeft",
            Type = ArrowType.Left,
        },
        new ArrowData()
        {
            Color = Colors.Red,
            Key = "arrowRight",
            Type = ArrowType.Right,
        },
    };

    private void InitializeArrowCheckers()
    {
        //Set the color of the arrows
        for (int i = 0; i < Arrows.Length; i++)
        {
            Arrows[i].Node = GetNode<NoteChecker>("noteCheckers/" + Arrows[i].Key);
            Arrows[i].Node.SetColor(Arrows[i].Color);
        }
    }

    public void FeedbackEffect(ArrowType arrow, string text)
    {
        // Get the particle node for this arrow
        var particles = Arrows[(int)arrow].Node.Particles;

        // Set the particle amount based on timing
        int particleAmount;
        switch (text)
        {
            case "Perfect":
                particleAmount = 10; // A lot of particles for Perfect
                break;
            case "Great":
                particleAmount = 7; // Moderate amount for Great
                break;
            case "Good":
                particleAmount = 4; // Few particles for Good
                break;
            default:
                return; // No particles for a miss
        }

        particles.Emit(particleAmount);
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();
        LoadControlScheme();
    }

    private void LoadControlScheme()
    {
        string scheme = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputKey).As<string>();
        foreach (var arrow in Arrows)
        {
            var events = InputMap.ActionGetEvents(arrow.Key);
            foreach (var inputEvent in events)
            {
                InputMap.ActionEraseEvent(arrow.Key, inputEvent);
            }
        }

        var selectedScheme = ControlSchemes.Schemes[scheme];

        foreach (var arrow in Arrows)
        {
            if (selectedScheme.ContainsKey(arrow.Key))
            {
                string inputName = selectedScheme[arrow.Key];

                if (inputName.StartsWith("Joypad")) // Controller input (D-pad or Stick)
                {
                    if (inputName.Contains("Stick")) // Left Stick Motion
                    {
                        InputEventJoypadMotion eventJoypadMotion = new InputEventJoypadMotion();
                        eventJoypadMotion.Axis = GetJoypadAxis(inputName);
                        InputMap.ActionAddEvent(arrow.Key, eventJoypadMotion);
                    }

                    // Add D-pad button mapping
                    InputEventJoypadButton eventJoypadButton = new InputEventJoypadButton();
                    eventJoypadButton.ButtonIndex = GetJoypadButton(inputName);

                    if (eventJoypadButton.ButtonIndex != JoyButton.Invalid)
                    {
                        InputMap.ActionAddEvent(arrow.Key, eventJoypadButton);
                    }
                    else
                    {
                        GD.PrintErr($"Invalid joypad button mapping: {inputName}");
                    }
                }
                else // Keyboard input
                {
                    if (Enum.TryParse(inputName, out Key keycode))
                    {
                        InputEventKey eventKey = new InputEventKey();
                        eventKey.Keycode = keycode;
                        InputMap.ActionAddEvent(arrow.Key, eventKey);
                    }
                    else
                    {
                        GD.PrintErr($"Invalid key mapping: {inputName}");
                    }
                }
            }
        }
    }

    private JoyAxis GetJoypadAxis(string action)
    {
        return action switch
        {
            "Joypad_Left_Stick_Up" => JoyAxis.LeftY,
            "Joypad_Left_Stick_Down" => JoyAxis.LeftY,
            "Joypad_Left_Stick_Left" => JoyAxis.LeftX,
            "Joypad_Left_Stick_Right" => JoyAxis.LeftX,
            _ => JoyAxis.Invalid, // Return an invalid axis if unknown
        };
    }

    private JoyButton GetJoypadButton(string action)
    {
        return action switch
        {
            "Joypad_Dpad_Up" => JoyButton.DpadUp,
            "Joypad_Dpad_Down" => JoyButton.DpadDown,
            "Joypad_Dpad_Left" => JoyButton.DpadLeft,
            "Joypad_Dpad_Right" => JoyButton.DpadRight,
            _ => JoyButton.Invalid, // Return an invalid button if unknown
        };
    }

    public override void _Process(double delta)
    {
        foreach (var arrow in Arrows)
        {
            if (Input.IsActionJustPressed(arrow.Key))
            {
                EmitSignal(nameof(NotePressed), (int)arrow.Type);
                arrow.Node.SetPressed(true);
            }
            else if (Input.IsActionJustReleased(arrow.Key))
            {
                EmitSignal(nameof(NoteReleased), (int)arrow.Type);
                arrow.Node.SetPressed(false);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Switch to CONTROLLER scheme if gamepad is used
        if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
        {
            string currentScheme = (string)ProjectSettings.GetSetting("game/input_scheme");
            if (currentScheme != "CONTROLLER")
            {
                GD.Print("Gamepad detected, switching to CONTROLLER scheme.");
                ProjectSettings.SetSetting("game/input_scheme", "CONTROLLER");
                ProjectSettings.Save();
                LoadControlScheme();
            }
        }

        // Detect left stick movement and trigger button presses
        if (@event is InputEventJoypadMotion motionEvent)
        {
            float deadzone = 0.5f; // Stick must be moved past this threshold

            if (motionEvent.Axis == JoyAxis.LeftX)
            {
                if (motionEvent.AxisValue < -deadzone)
                {
                    Input.ActionPress("arrowLeft");
                }
                else if (motionEvent.AxisValue > deadzone)
                {
                    Input.ActionPress("arrowRight");
                }
                else
                {
                    Input.ActionRelease("arrowLeft");
                    Input.ActionRelease("arrowRight");
                }
            }
            else if (motionEvent.Axis == JoyAxis.LeftY)
            {
                if (motionEvent.AxisValue < -deadzone)
                {
                    Input.ActionPress("arrowUp");
                }
                else if (motionEvent.AxisValue > deadzone)
                {
                    Input.ActionPress("arrowDown");
                }
                else
                {
                    Input.ActionRelease("arrowUp");
                    Input.ActionRelease("arrowDown");
                }
            }
        }
    }
}
