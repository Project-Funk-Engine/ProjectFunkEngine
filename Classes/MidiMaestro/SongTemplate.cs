namespace FunkEngine.Classes.MidiMaestro;

public partial class SongTemplate
{
    public string Name;
    public string AudioLocation;
    public string MIDILocation;
    public string EnemyScenePath;
    public SongData SongData;

    public SongTemplate(
        SongData songData,
        string name = "",
        string audioLocation = "",
        string midiLocation = "",
        string enemyScenePath = ""
    )
    {
        Name = name;
        AudioLocation = audioLocation;
        MIDILocation = midiLocation;
        SongData = songData;
        EnemyScenePath = enemyScenePath;
    }
}
