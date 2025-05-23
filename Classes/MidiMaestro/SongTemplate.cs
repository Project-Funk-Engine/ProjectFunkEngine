using System.Text.Json;
using Godot;

namespace FunkEngine.Classes.MidiMaestro;

/**
 * <summary>SongTemplate: Generic class to represent a rhythm battle.</summary>
 */
public struct SongTemplate
{
    public string Name;
    public readonly NoteChart Chart;
    public readonly string[] EnemyScenePath;

    public SongTemplate(string name = "", string[] enemyScenePath = null, NoteChart chart = null)
    {
        Name = name;
        Chart = chart;
        EnemyScenePath = enemyScenePath;
    }

    private struct NoteChartMemento(
        string name = "",
        string[] enemyScenePath = null,
        string chartPath = ""
    )
    {
        public string Name = name;
        public string ChartPath = chartPath;
        public string[] EnemyScenePath = enemyScenePath;
    }

    public static string ToJSONString(SongTemplate template, string chartPath)
    {
        NoteChartMemento inbetween = new NoteChartMemento(
            template.Name,
            template.EnemyScenePath,
            chartPath
        );
        string result = JsonSerializer.Serialize(inbetween);
        return result;
    }

    public static SongTemplate CreateFromPath(string path, bool fromUserPath = true)
    {
        if (!FileAccess.FileExists(path))
            return new SongTemplate();

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();

        file.Close();
        try
        {
            NoteChartMemento result = JsonSerializer.Deserialize<NoteChartMemento>(json);
            if (string.IsNullOrEmpty(result.ChartPath))
                return new SongTemplate();
            if (!FileAccess.FileExists(result.ChartPath))
                return new SongTemplate();

            return new SongTemplate(
                result.Name,
                result.EnemyScenePath,
                ResourceLoader.Load<NoteChart>(result.ChartPath)
            );
        }
        catch (JsonException)
        {
            GD.PushWarning("Cannot deserialize SongTemplate, returning null.");
            return new SongTemplate();
        }
    }
}
