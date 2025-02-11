using System;
using System.Collections.Generic;
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

        LoadQueueFromSave();
        ScrambleQueue();
        UpdateQueue();
    }

    // Loads the notes from SaveData.json, and adds them to the queue
    public void LoadQueueFromSave()
    {
        Dictionary<string, int> savedNotes = SaveSystem.LoadNotes();
        foreach (var noteEntry in savedNotes)
        {
            string noteName = noteEntry.Key;
            int numNotes = noteEntry.Value;

            for (int i = 0; i < numNotes; i++)
            {
                AddNoteToQueue(CreateNoteFromName(noteName));
            }
        }
    }

    // Creates a note from a string of the note's name.
    private Note CreateNoteFromName(string noteName)
    {
        if (noteName == "PlayerBase")
        {
            return new Note(
                "PlayerBase",
                null,
                1,
                (director, note, timing) =>
                {
                    director.Enemy.TakeDamage((int)timing);
                }
            );
        }
        if (noteName == "PlayerDouble")
        {
            return new Note(
                "PlayerDouble",
                null,
                1,
                (director, note, timing) =>
                {
                    director.Enemy.TakeDamage(2 * (int)timing);
                }
            );
        }

        return null;
    }

    public void AddNoteToQueue(Note noteType)
    {
        _noteQueue.Enqueue(noteType);
        UpdateQueue();
    }

    // Returns current note, and removes it from the queue
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

    // Updates the queue's graphics
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

    // Fisher-Yates shuffle from: https://stackoverflow.com/a/1262619
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
