namespace FunkEngine.Classes.MidiMaestro;

public partial class SongTemplate
{
    public string Name;
    public string AudioLocation;
    public string MIDILocation;
    public SongData SongData;

    public SongTemplate(
        SongData songData,
        string name = "",
        string audioLocation = "",
        string midiLocation = ""
    )
    {
        Name = name;
        AudioLocation = audioLocation;
        MIDILocation = midiLocation;
        SongData = songData;
    }
}
