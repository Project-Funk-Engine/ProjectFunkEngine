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

    public struct NoteChartMemento(
        string name = "",
        string[] enemyScenePath = null,
        string chartPath = ""
    )
    {
        public string Name { get; init; } = name;
        public string ChartPath { get; init; } = chartPath;
        public string[] EnemyScenePath { get; init; } = enemyScenePath;
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
            return new SongTemplate("CUSTOM_SONTEM_NOT_FOUND", [path]);

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();

        file.Close();
        try
        {
            NoteChartMemento result = JsonSerializer.Deserialize<NoteChartMemento>(json);
            if (string.IsNullOrEmpty(result.ChartPath))
                return new SongTemplate("CUSTOM_NO_CHART_PATH", [json]);
            if (!FileAccess.FileExists(path.GetBaseDir() + "/" + result.ChartPath))
                return new SongTemplate(
                    "CUSTOM_CHART_NOT_FOUND",
                    [path.GetBaseDir() + "/" + result.ChartPath]
                );

            NoteChart nc = ResourceLoader.Load<NoteChart>(
                path.GetBaseDir() + "/" + result.ChartPath
            );
            return new SongTemplate(result.Name, result.EnemyScenePath, nc);
        }
        catch (JsonException)
        {
            GD.PushWarning("Cannot deserialize SongTemplate, returning error object.");
            return new SongTemplate("CUSTOM_COULD_NOT_READ", [json]);
        }
    }
}
