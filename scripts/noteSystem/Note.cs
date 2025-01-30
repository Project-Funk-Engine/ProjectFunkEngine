using Godot;

/**
 * @class Note
 * @brief This class represents a note that scrolls across the screen to be played by the player. WIP
 */
public partial class Note : Sprite2D
{
    private float _defaultNoteSpeed = 100;

    //TODO: This is for a time based implementation. We need to find a way to make this exact.
    private float _expectedHitTime = 0;

    public void Init(NoteManager.ArrowData parentArrowData, float speed, float hitTime)
    {
        _defaultNoteSpeed = speed;
        _expectedHitTime = hitTime;
        SelfModulate = parentArrowData.color;
        SetPosition(new Vector2(700, parentArrowData.node.Position.Y));
        RotationDegrees = parentArrowData.node.RotationDegrees;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        SetPosition(new Vector2(Position.X - _defaultNoteSpeed * ((float)delta * 100), Position.Y));

        if (Position.X < -100)
            QueueFree();
    }
}
