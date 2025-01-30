using System;
using Godot;

//Lets say this inits all the initial notes and manages the chart BG.

//What does this do?
//Input, visual looping, timing, battle stuff, combo, note creation

//Focus on the looping
/*

 This should manage creating sprites for notes???
 This should manage subview camera pos and zoom.

Movement should primarily be done from a parent node
!!Backgrounds need to be big enough/notes and beats spaced enough that when a note goes off screen, it can be teleported in position offscreen
BackGround probably needs 2 sprites or parallax:
    Get a set length, based on viewport and loop/song length (Const PLAYWIDTH)
    Once one BG hits a certain left pos return it to the right pos

Notes are similar, but only need 1 representation.
    Once hits left bounds return to right bounds
    (Something else should probably manage refreshing, input, etc)
    Can probably use an object pool

If timing based input checking:
    This is enough, notes are visually just sprites
    Collision based - This might need to manage that, or have a sister manager that does, notes need more stuff on their own
 */

public partial class ChartManager : SubViewportContainer
{
    //Simulated variables, remove later
    private const int Bpm = 120;
    private const double SongLength = 160; //secs

    //Arbitrary vars, play with these
    private const double ChartLength = 1400;
    private const int NumLoops = 5; //TODO: Loops should be based on measures of a song?

    //Nodes from scene
    [Export]
    public CanvasGroup ChartLoopables;

    private double _loopLen; //secs
    private int _beatsPerLoop;

    public override void _Ready()
    {
        _loopLen = SongLength / NumLoops;
        _beatsPerLoop = (int)(_loopLen / (60f / Bpm));

        ChartLoopables = GetNode<CanvasGroup>("%ChartLoopables");

        speedL = 700 / _loopLen; // px/s
        GD.Print(speedL);

        foreach (Node child in ChartLoopables.GetChildren())
        {
            if (child is ChartBg)
            {
                ChartBg chartBg = (ChartBg)child;
                chartBg.Speed = (float)speedL;
            }
        }

        InitTestNote();
    }

    public override void _Process(double delta)
    {
        //ChartLoopables.Position += 10 * Vector2.Left;
        ProcessTestNote(delta);
    }

    //Test code TODO: REMOVE LATER
    private Sprite2D[] _testNotes = new Sprite2D[4];
    private float[] _testBeats = { 60, 30, 32, 10 };
    private double speedL;

    /*The logic:
     *Spawn in pos is a proportion (intended beat/beats per loop) = (intended pos/track length in px) ->
     *		intended pos = intended beat / bpl * track length
     *
     *		Respawn (probably obj pool, or for now new instantiation)
     *			When a note's pos is at its intended initial pos, queue up and spawn the next note of its beat at intended pos + track length
     */
    public void InitTestNote()
    {
        _testNotes[0] = GetNode<Sprite2D>("%TestNote");
        _testNotes[1] = GetNode<Sprite2D>("%TestNote2");
        _testNotes[2] = GetNode<Sprite2D>("%TestNote3");
        _testNotes[3] = GetNode<Sprite2D>("%TestNote4");
        int i = 0;
        foreach (Sprite2D note in _testNotes)
        {
            note.Position = new Vector2(_testBeats[i] / _beatsPerLoop * 700, 150);
            i++;
        }
    }

    public void ProcessTestNote(double delta)
    {
        int i = 0;
        foreach (Sprite2D note in _testNotes)
        {
            if (note.Position.X <= (_testBeats[i] / _beatsPerLoop * 700) - 700)
            {
                note.Position = new Vector2(_testBeats[i] / _beatsPerLoop * 700, 150);
            }
            note.Position += Vector2.Left * (float)speedL * (float)delta;
            i++;
        }
    }
}
