using Godot;

/**
 * <summary>BgAudioPlayer: Autoload to play scene persistant music..</summary>
 */
public partial class BgAudioPlayer : AudioStreamPlayer
{
    private readonly AudioStream _levelMusic = (AudioStream)
        ResourceLoader.Load("res://Scenes/UI/TitleScreen/Assets/TitleSong.ogg");

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
