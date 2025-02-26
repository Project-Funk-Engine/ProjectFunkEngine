namespace FunkEngine.Classes.MidiMaestro;

public partial class SongTemplate
{
    public string Name;
    public string AudioLocation;
    public string MIDILocation;

    public SongTemplate(string name = "", string audioLocation = "", string midiLocation = "")
    {
        Name = name;
        AudioLocation = audioLocation;
        MIDILocation = midiLocation;
    }
}
