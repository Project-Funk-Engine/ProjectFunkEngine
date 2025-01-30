using System;
using Godot;

/**
 * @class BattleDirector
 * @brief Higher priority director to manage battle effects. Can directly access managers, which should signal up to Director WIP
 */
public partial class BattleDirector : Node2D
{
    [Export]
    public ChartManager CM;

    [Export]
    public NoteManager NM;

    //TODO: Slowly add data based on what it needs.
    public struct SongData
    {
        public int Bpm;
        public double SongLength;
        public int NumLoops;
    }

    private SongData _curSong;
    private Note[] _notes;

    public override void _Ready()
    {
        AddExampleNote();

        CM.PrepChart(_curSong, _notes);
        //TODO: Hook up signals
    }

    private void AddExampleNote()
    {
        //Create Dummy Song Data
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = 160,
            NumLoops = 5,
        };
        //Add note
        _notes = new Note[1];
        _notes[0] = new Note(NoteArrow.ArrowType.Left, 32);
    }
}
