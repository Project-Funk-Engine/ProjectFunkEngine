using Godot;

/**
 * <summary>BgAudioPlayer: Autoload to play scene persistant music.</summary>
 */
public partial class BgAudioPlayer : AudioStreamPlayer
{
    private readonly AudioStream _levelMusic = ResourceLoader.Load<AudioStream>(
        "res://Scenes/UI/TitleScreen/Assets/TitleSong.ogg"
    );

    private float _checkpoint;

    public static BgAudioPlayer LiveInstance { get; private set; }

    public override void _EnterTree()
    {
        LiveInstance = this;
    }

    private void PlayMusic(AudioStream music, float volume, bool resume = false)
    {
        ProcessMode = ProcessModeEnum.Always;

        if (!Playing || !music.Equals(Stream))
        {
            Stream = music;
            VolumeDb = volume;
            Play(resume ? _checkpoint : 0);
        }
    }

    /// <summary>
    /// Starts playing main menu music from the start if the track if it is not already playing.
    /// Does nothing if the track is already playing.
    /// </summary>
    /// <param name="volume">The volume to play the track at</param>
    public void PlayLevelMusic(float volume = -10f)
    {
        PlayMusic(_levelMusic, volume);
    }

    /// <summary>
    /// Starts playing main menu music from the last checkpoint if the track if it is not already playing.
    /// Does nothing if the track is already playing.
    /// </summary>
    /// <param name="volume"></param>
    public void ResumeLevelMusic(float volume = -10f)
    {
        PlayMusic(_levelMusic, volume, true);
    }

    public void StopMusic()
    {
        _checkpoint = GetPlaybackPosition();

        Stop();
        Stream = null;
    }
}
