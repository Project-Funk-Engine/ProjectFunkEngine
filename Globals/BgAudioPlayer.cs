using Godot;

public partial class BgAudioPlayer : AudioStreamPlayer
{
    private readonly AudioStream _levelMusic = (AudioStream)
        ResourceLoader.Load("res://scenes/SceneTransitions/assets/song18.mp3");

    private void PlayMusic(AudioStream music, float volume)
    {
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
