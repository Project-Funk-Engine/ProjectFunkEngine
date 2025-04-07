namespace FunkEngine.Classes.MidiMaestro;

/**
 * <summary>SongTemplate: Generic class to represent a rhythm battle.</summary>
 */
public struct SongTemplate
{
    public string Name;
    public readonly string AudioLocation;
    public string MIDILocation;
    public readonly string[] EnemyScenePath;
    public SongData SongData;

    public SongTemplate(
        SongData songData,
        string name = "",
        string audioLocation = "",
        string midiLocation = "",
        string[] enemyScenePath = null
    )
    {
        Name = name;
        AudioLocation = audioLocation;
        MIDILocation = midiLocation;
        SongData = songData;
        EnemyScenePath = enemyScenePath;
    }
}
