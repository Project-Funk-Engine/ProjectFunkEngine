using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

public partial class NoteQueue : Node
{
    [Export]
    private Sprite2D _currentNote;

    [Export]
    private Sprite2D _nextNote;

    private Queue<Note> _noteQueue = new Queue<Note>();
    private Dictionary<string, Texture2D> _noteSprites = new Dictionary<string, Texture2D>();

    public override void _Ready()
    {
        _noteSprites["PlayerBase"] = GD.Load<Texture2D>(
            "res://scenes/CustomNotes/assets/single_note.png"
        );
        _noteSprites["PlayerDouble"] = GD.Load<Texture2D>(
            "res://scenes/CustomNotes/assets/double_note.png"
        );

        UpdateQueue();
    }

    public void LoadQueue(PlayerPuppet player)
    {
        for (int i = 0; i < player.Stats.CurNotes.Length; i++)
        {
            AddNoteToQueue(player.Stats.CurNotes[i]);
        }
    }

    public void AddNoteToQueue(Note noteType)
    {
        _noteQueue.Enqueue(noteType);
        UpdateQueue();
    }

    //returns current note, and removes it from the queue
    public Note GetCurrentNote()
    {
        if (_noteQueue.Count > 0)
        {
            Note currentNote = _noteQueue.Dequeue();
            UpdateQueue();
            return currentNote;
        }
        return null;
    }

    private void UpdateQueue()
    {
        if (_noteQueue.Count > 0 && _noteSprites.ContainsKey(_noteQueue.Peek().Name))
            _currentNote.Texture = _noteSprites[_noteQueue.Peek().Name];
        else
            _currentNote.Texture = null;

        if (_noteQueue.Count > 1)
        {
            Note[] notes = _noteQueue.ToArray();
            if (_noteSprites.ContainsKey(notes[1].Name))
                _nextNote.Texture = _noteSprites[notes[1].Name];
            else
                _nextNote.Texture = null;
        }
        else
        {
            _nextNote.Texture = null;
        }
    }

    //Fisher-Yates shuffle from: https://stackoverflow.com/a/1262619
    public void ScrambleQueue()
    {
        List<Note> tempList = new List<Note>(_noteQueue);
        Random rng = new Random();

        int n = tempList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (tempList[k], tempList[n]) = (tempList[n], tempList[k]);
        }

        _noteQueue = new Queue<Note>(tempList);
    }

    //TODO: should work, in order to run in game use
    // noteQueueInstance.AddNoteToQueue("single");
}
