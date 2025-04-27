namespace FunkEngine.Classes.MidiMaestro;

/**
 * <summary>SongTemplate: Generic class to represent a rhythm battle.</summary>
 */
public struct SongTemplate
{
    public string Name;
    public readonly string AudioLocation;
    public string SongMapLocation;
    public readonly string[] EnemyScenePath;
    public SongData SongData;

    public SongTemplate(
        SongData songData,
        string name = "",
        string audioLocation = "",
        string songMapLocation = "",
        string[] enemyScenePath = null
    )
    {
        Name = name;
        AudioLocation = audioLocation;
        SongMapLocation = songMapLocation;
        SongData = songData;
        EnemyScenePath = enemyScenePath;
    }
}
