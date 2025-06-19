using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

public partial class EnemyDescriptions : Control
{
    [Export]
    private VBoxContainer DescriptionsContainer;

    private const string _loopIconPath = "res://Scenes/BattleDirector/Assets/LoopSymbol.png";
    private const string _damageInstanceIconPath =
        "res://Scenes/BattleDirector/Assets/DamageInstanceSymbol.png";
    private const string _battleStartIconPath =
        "res://Scenes/BattleDirector/Assets/BattleStartSymbol.png";
    private const string _battleEndIconPath =
        "res://Scenes/BattleDirector/Assets/BattleEndSymbol.png";

    private bool _isVisible = false;

    public void Setup(EnemyPuppet enemy)
    {
        if (enemy.InitialNote != (0, 0))
        {
            string desc = Scribe.NoteDictionary[enemy.InitialNote.NoteId].Description;
            AddDescriptionRow(Scribe.NoteDictionary[enemy.InitialNote.NoteId].Texture, desc);
            _isVisible = true;
        }

        foreach (var effect in enemy.GetBattleEvents())
        {
            if (effect.Description != null)
            {
                Texture2D icon = GetTriggerIcon(effect.GetTrigger());
                AddDescriptionRow(icon, effect.Description);
                _isVisible = true;
            }
        }

        Visible = _isVisible;
    }

    private void AddDescriptionRow(Texture2D iconTexture, string text)
    {
        HBoxContainer hbox = new HBoxContainer();

        TextureRect icon = new TextureRect();
        icon.Texture = iconTexture;

        Label desc = new Label();
        desc.Text = text;
        desc.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;

        hbox.AddChild(icon);
        hbox.AddChild(desc);
        DescriptionsContainer.AddChild(hbox);
    }

    private Texture2D GetTriggerIcon(BattleEffectTrigger trigger)
    {
        //TODO: add more as we get more enemy effect triggers
        return trigger switch
        {
            BattleEffectTrigger.OnLoop => GD.Load<Texture2D>(_loopIconPath),
            BattleEffectTrigger.OnDamageInstance => GD.Load<Texture2D>(_damageInstanceIconPath),
            BattleEffectTrigger.OnBattleStart => GD.Load<Texture2D>(_battleStartIconPath),
            BattleEffectTrigger.OnBattleEnd => GD.Load<Texture2D>(_battleEndIconPath),
            _ => null,
        };
    }
}
