using System;
using System.Collections.Generic;
using Godot;

public partial class EnemyDescriptions : Node
{
    public List<string> Descriptions = new List<string>();

    [Export]
    private VBoxContainer DescriptionsContainer;

    public void Setup(EnemyPuppet enemy)
    {
        if (enemy.InitialNote != (0, 0))
            Descriptions.Add(Scribe.NoteDictionary[enemy.InitialNote.NoteId].Description);

        foreach (var effect in enemy.GetBattleEvents())
        {
            if (effect.Description != null)
                Descriptions.Add(effect.Description);
        }

        foreach (var description in Descriptions)
        {
            HBoxContainer hbox = new HBoxContainer();

            Sprite2D icon = new Sprite2D();
            icon.Texture = Scribe.NoteDictionary[enemy.InitialNote.NoteId].Texture;

            Label desc = new Label();
            desc.Text = description;
            desc.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;

            hbox.AddChild(icon);
            hbox.AddChild(desc);
            DescriptionsContainer.AddChild(hbox);
        }
    }
}
