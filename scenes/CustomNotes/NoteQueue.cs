using System;
using System.Collections.Generic;
using Godot;

public partial class NoteQueue : Node
{
    [Export]
    private Sprite2D _currentNote;

    [Export]
    private Sprite2D _nextNote;

    private Queue<string> _noteQueue = new Queue<string>();
    private Dictionary<string, Texture2D> _noteSprites = new Dictionary<string, Texture2D>();

    public override void _Ready()
    {
        _noteSprites["single"] = GD.Load<Texture2D>(
            "res://scenes/CustomNotes/assets/single_note.png"
        );
        _noteSprites["double"] = GD.Load<Texture2D>(
            "res://scenes/CustomNotes/assets/double_note.png"
        );

        UpdateQueue();
    }

    public void AddNoteToQueue(string noteType)
    {
        _noteQueue.Enqueue(noteType);
        UpdateQueue();
    }

    //returns current note, and removes it from the queue
    public string GetCurrentNote()
    {
        if (_noteQueue.Count > 0)
        {
            string currentNoteName = _noteQueue.Dequeue();
            UpdateQueue();
            return currentNoteName;
        }
        return null;
    }

    private void UpdateQueue()
    {
        if (_noteQueue.Count > 0 && _noteSprites.ContainsKey(_noteQueue.Peek()))
            _currentNote.Texture = _noteSprites[_noteQueue.Peek()];
        else
            _currentNote.Texture = null;

        if (_noteQueue.Count > 1)
        {
            string[] notes = _noteQueue.ToArray();
            if (_noteSprites.ContainsKey(notes[1]))
                _nextNote.Texture = _noteSprites[notes[1]];
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
        List<string> tempList = new List<string>(_noteQueue);
        Random rng = new Random();

        int n = tempList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (tempList[k], tempList[n]) = (tempList[n], tempList[k]);
        }

        _noteQueue = new Queue<string>(tempList);
    }

    //TODO: should work, in order to run in game use
    // noteQueueInstance.AddNoteToQueue("single");
}
