using System.Linq;
using FunkEngine;
using Godot;
using Godot.Collections;
using Array = System.Array;

/**
 * <summary>Handles drawing a map from a MapGrid and scene transitions from player input. Currently, this also handles overarching win/lose conditions.</summary>
 */
public partial class Cartographer : Node2D
{
    public static readonly string LoadPath = "res://Scenes/Maps/Cartographer.tscn";

    [Export]
    public Sprite2D PlayerSprite;

    [Export]
    public Theme ButtonTheme;

    [Export]
    public Camera2D Camera;

    private Button[] _validButtons = Array.Empty<Button>();

    private Button _focusedButton;

    private static readonly Dictionary<Stages, Texture2D> StageIcons = new()
    {
        { Stages.Battle, GD.Load<Texture2D>("res://Scenes/Maps/Assets/BattleIcon.png") },
        { Stages.Elite, GD.Load<Texture2D>("res://Scenes/Maps/Assets/EliteIcon.png") },
        { Stages.Boss, GD.Load<Texture2D>("res://Scenes/Maps/Assets/BossIcon.png") },
        { Stages.Chest, GD.Load<Texture2D>("res://Scenes/Maps/Assets/ChestIcon.png") },
        { Stages.Shop, GD.Load<Texture2D>("res://Scenes/Maps/Assets/ShopIcon.png") },
        { Stages.Event, GD.Load<Texture2D>("res://Scenes/Maps/Assets/EventIcon.png") },
        { Stages.Map, GD.Load<Texture2D>("res://Scenes/Maps/Assets/FirstIcon.png") },
    };

    public override void _Ready()
    {
        DrawMap();
        SaveSystem.SaveGame();
        if (
            StageProducer.GetCurRoom().Type == Stages.Boss
            && StageProducer.GetCurRoom().Children.Length == 0
        )
        {
            WinArea();
        }
    }

    public override void _EnterTree()
    {
        BgAudioPlayer.LiveInstance.PlayLevelMusic();
    }

    private Vector2 GetPosition(int x, int y)
    {
        return new Vector2((float)(x + 1) * 640 / (StageProducer.Map.Width + 1), y * 48 + 16);
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

        _validButtons = _validButtons.OrderBy(x => x.Position.X).ToArray();
        AddFocusNeighbors();
    }

    private static readonly Vector2 MapIconSize = new(48, 48);

    private void DrawMapSprite(MapGrid.Room room)
    {
        var newButton = new Button();
        newButton.Theme = ButtonTheme;
        newButton.CustomMinimumSize = MapIconSize;
        newButton.IconAlignment = HorizontalAlignment.Center;
        AddChild(newButton);
        bool isChild = StageProducer.GetCurRoom().Children.Contains(room.Idx);

        // checks if the next room is one below current room, player has shortcuts
        bool isLaneChangeAllowed =
            room.Y == StageProducer.GetCurRoom().Y + 1 && StageProducer.PlayerStats.Shortcuts > 0;

        //button is disabled if it is not a child of current room.
        //unless player has charges of lane changing
        if (!isChild && !isLaneChangeAllowed)
        {
            newButton.Disabled = true;
            newButton.FocusMode = Control.FocusModeEnum.None;
        }
        else
        {
            //grab focus on children paths, to really make sure user wants to use a charge of maplanechanges
            if (isChild)
            {
                newButton.GrabFocus();
                _focusedButton = newButton;
            }
            newButton.Pressed += () =>
            {
                if (!isChild)
                    StageProducer.PlayerStats.Shortcuts--;

                EnterStage(room.Idx, newButton);
            };
            _validButtons = _validButtons.Append(newButton).ToArray();
        }

        newButton.Icon = StageIcons[room.Type];
        if (room.Y == 0)
            newButton.Icon = StageIcons[Stages.Map];

        newButton.ZIndex = 1;
        newButton.Position = GetPosition(room.X, room.Y) - newButton.Size / 2;
        if (room == StageProducer.GetCurRoom())
        {
            PlayerSprite.Position = newButton.Position + newButton.Size * .5f;
            Camera.Position -=
                (
                    (GetViewportRect().Size / 2) - (newButton.Position + newButton.Size * .5f)
                ).Normalized()
                * room.X
                * 48;
        }
    }

    private void AddFocusNeighbors()
    {
        for (int i = 0; i < _validButtons.Length; i++)
        {
            _validButtons[i].FocusNeighborRight = _validButtons[(i + 1) % (_validButtons.Length)]
                .GetPath();
            _validButtons[(i + 1) % (_validButtons.Length)].FocusNeighborLeft = _validButtons[i]
                .GetPath();
        }
    }

    private void EnterStage(int roomIdx, Button button)
    {
        StageProducer.LiveInstance.PreloadScene(roomIdx);
        foreach (Button btn in _validButtons)
        {
            btn.Disabled = true;
            if (btn == button)
                continue;
            btn.FocusMode = Control.FocusModeEnum.None;
        }

        var tween = CreateTween()
            .TweenProperty(PlayerSprite, "position", button.Position + button.Size * .5f, 1f);
        tween.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.InOut);
        tween.Finished += () =>
        {
            BgAudioPlayer.LiveInstance.StopMusic();
            StageProducer.LiveInstance.TransitionFromRoom(roomIdx);
        };
    }

    private void WinArea()
    {
        if (StageProducer.IsMoreLevels())
        {
            GetTree().Paused = false;
            //What the living fuck? Can't do this during ready?
            Callable
                .From(() => StageProducer.LiveInstance.TransitionStage(Stages.Continue))
                .CallDeferred();
            return;
        }

        EndScreen es = GD.Load<PackedScene>(EndScreen.LoadPath).Instantiate<EndScreen>();
        AddChild(es);
        es.TopLabel.Text = Tr("BATTLE_ROOM_WIN");
        ProcessMode = ProcessModeEnum.Disabled;
    }
}
