using Godot;

/**
 * <summary>BgAudioPlayer: Autoload to play scene persistant music.</summary>
 */
public partial class BgAudioPlayer : AudioStreamPlayer
{
    private readonly AudioStream _levelMusic = ResourceLoader.Load<AudioStream>(
        "res://Scenes/UI/TitleScreen/Assets/TitleSong.ogg"
    );

    public static BgAudioPlayer LiveInstance { get; private set; }

    public override void _EnterTree()
    {
        LiveInstance = this;
    }

    private void PlayMusic(AudioStream music, float volume)
    {
        ProcessMode = ProcessModeEnum.Always;
        if (Playing && music.Equals(Stream))
        {
            return;
        }

        Stream = music;
        VolumeDb = volume;
        Play();
    }

    public void PlayLevelMusic(float volume = -10f)
    {
        PlayMusic(_levelMusic, volume);
    }

    public void StopMusic()
    {
        Stop();
        Stream = null;
    }
}
