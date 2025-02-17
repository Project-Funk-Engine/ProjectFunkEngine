using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

// TODO: implement saving

public static class SaveSystem
{
    private static string SavePath => "res://SaveData/SaveData.json"; // Update if needed

    // Loads only the notes section
    public static Dictionary<string, int> LoadNotes()
    {
        var saveData = LoadSaveData();
        if (saveData != null && saveData.Notes != null)
        {
            return saveData.Notes;
        }
        else
        {
            return new Dictionary<string, int>();
        }
    }

    // This method loads the entire save data
    public static SaveData LoadSaveData()
    {
        string path = ProjectSettings.GlobalizePath(SavePath);
        if (!File.Exists(path))
        {
            GD.PrintErr("Can't load save game");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonSerializer.Deserialize<SaveData>(json);
        return data;
    }
}

public class SaveData
{
    public string AccountName { get; set; }
    public Dictionary<string, int> Notes { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, object> Relics { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, float> Settings { get; set; } = new Dictionary<string, float>();
}
