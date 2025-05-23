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
}
