using Godot;

/**
 * @class NoteArrow
 * @brief This class represents a visual note that scrolls across the screen to be played by the player. WIP
 */
public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public enum ArrowType
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public int NoteIdx;
    public float Bounds;

    public void Init(InputHandler.ArrowData parentArrowData)
    {
        ZIndex = 1;

        SelfModulate = parentArrowData.Color;
        Position += Vector2.Down * (parentArrowData.Node.GlobalPosition.Y);
        RotationDegrees = parentArrowData.Node.RotationDegrees;

        //This could be good as a function to call on something, to have many things animated to the beat.
        var tween = CreateTween();
        tween.TweenProperty(this, "scale", Scale, 60f / TimeKeeper.Bpm / 2);
        tween.SetEase(Tween.EaseType.In);
        tween.SetTrans(Tween.TransitionType.Elastic);
        tween.TweenProperty(this, "scale", Scale * 1.25f, 60f / TimeKeeper.Bpm / 2);
        tween.SetLoops();
        tween.Play();
    }

    public override void _Process(double delta)
    {
        Vector2 newPos = Position;
        newPos.X =
            (float)(
                (-TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength)
                % TimeKeeper.ChartLength
                / 2
            ) + Bounds;
        if (newPos.X > Position.X)
        {
            OnLoop();
        }
        Position = newPos;
    }

    public void OnLoop()
    {
        Visible = true;
    }

    public void NoteHit()
    {
        Visible = false;
    }
}
