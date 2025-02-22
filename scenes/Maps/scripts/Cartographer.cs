using System;
using System.Linq;
using FunkEngine;
using Godot;

public partial class Cartographer : Node2D
{
    private Button[] validButtons = Array.Empty<Button>();

    public override void _Ready()
    {
        DrawMap();
    }

    private Vector2 GetPosition(int x, int y)
    {
        return new Vector2((float)x * 640 / StageProducer.MapSize.X - 1 + 64, y * 48 + 16);
    }

    private void DrawMap()
    {
        var rooms = StageProducer.Map.GetRooms();
        foreach (MapGrid.Room room in rooms)
        {
            DrawMapSprite(room);
            foreach (int roomIdx in room.Children)
            {
                Line2D newLine = new Line2D();
                newLine.AddPoint(GetPosition(room.X, room.Y));
                newLine.AddPoint(GetPosition(rooms[roomIdx].X, rooms[roomIdx].Y));
                AddChild(newLine);
            }
        }

        validButtons = validButtons.OrderBy(x => x.Position.X).ToArray();
        AddFocusNeighbors();
    }

    private void DrawMapSprite(MapGrid.Room room)
    {
        var newButton = new Button();
        AddChild(newButton);
        //button is disabled if it is not a child of current room.
        if (!StageProducer.CurRoom.Children.Contains(room.Idx))
        {
            newButton.Disabled = true;
            newButton.FocusMode = Control.FocusModeEnum.None;
        }
        else
        {
            newButton.GrabFocus();
            newButton.Pressed += () =>
            {
                EnterStage(room.Idx);
            };
            validButtons = validButtons.Append(newButton).ToArray();
        }

        switch (room.Type)
        {
            case Stages.Battle:
                newButton.Icon = (Texture2D)GD.Load("res://scenes/Maps/assets/BattleIcon.png");
                break;
            case Stages.Boss:
                newButton.Icon = (Texture2D)GD.Load("res://scenes/Maps/assets/BossIcon.png");
                break;
            case Stages.Chest:
                newButton.Icon = (Texture2D)GD.Load("res://scenes/Maps/assets/ChestIcon.png");
                break;
        }
        newButton.ZIndex = 1;
        newButton.Position = GetPosition(room.X, room.Y) - newButton.Size * 2;
    }

    private void AddFocusNeighbors()
    {
        GD.Print(validButtons);
        for (int i = 0; i < validButtons.Length; i++)
        {
            validButtons[i].FocusNeighborRight = validButtons[(i + 1) % (validButtons.Length)]
                .GetPath();
            validButtons[(i + 1) % (validButtons.Length)].FocusNeighborLeft = validButtons[i]
                .GetPath();
        }
    }

    private void EnterStage(int roomIdx)
    {
        GetNode<StageProducer>("/root/StageProducer").TransitionFromRoom(roomIdx);
    }
}
