using System;
using System.Linq;
using FunkEngine;
using Godot;

public partial class Cartographer : Node2D
{
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
        foreach (StageProducer.MapGrid.Room room in rooms)
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
    }

    private void DrawMapSprite(StageProducer.MapGrid.Room room)
    {
        var newSprite = new Button();
        AddChild(newSprite);
        //button is disabled if it is not a child of current room.
        if (!StageProducer.CurRoom.Children.Contains(room.Idx))
        {
            newSprite.Disabled = true;
            newSprite.FocusMode = Control.FocusModeEnum.None;
        }
        else
        {
            newSprite.GrabFocus();
            newSprite.Pressed += () =>
            {
                EnterStage(room.Idx);
            };
        }
        newSprite.Icon = (Texture2D)GD.Load("res://icon.svg"); //TODO: Room types icons
        newSprite.Scale *= .25f;
        newSprite.ZIndex = 1;
        newSprite.Position = GetPosition(room.X, room.Y) - newSprite.Size * 2;
    }

    private void EnterStage(int roomIdx)
    {
        GetNode<StageProducer>("/root/StageProducer").TransitionFromRoom(roomIdx);
    }
}
