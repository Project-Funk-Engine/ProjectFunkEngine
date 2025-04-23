using System.IO;

namespace FunkEngine.Classes.BeatDetector;

using System;
using Godot;
using NAudio.Wave;

/// <summary>
/// Loads an audio file and performs spectral flux onset detection.
///
/// Pretty cooooooooollllllllllllllllll
/// </summary>
//TODO: Finish documentation
public partial class AudioFileAnalyzer : Resource
{
    const int SAMPLE_SIZE = 1024; // Size of the sample window. Can change if needed.

    Fft _fft;

    public AudioFileReader PCMStream { get; set; } //The audio steam from our MP3/WAV file

    private OnsetDetector _onsetDetector;

    public float[] OnsetsFound { get; set; }
    public float TimePerSample { get; set; }

    public AudioFileAnalyzer()
    {
        SetUpFFT();
    }

    /// <summary>
    /// This project supports both WAV and MP3 files. We might be able to do more? IDK It's limited to that for now.
    /// </summary>
    /// <param name="filePath">The path to your audio file</param>
    public void LoadAudioFromFile(string filePath)
    {
        //TODO: Only wav supported on platforms other than windows - need to find fix
        if (!File.Exists(filePath) || (!filePath.EndsWith(".mp3") && !filePath.EndsWith(".wav")))
        {
            throw new FileNotFoundException(
                "File not found or unsupported file type. Please provide a valid WAV or MP3 file."
            );
        }

        PCMStream = new AudioFileReader(filePath);

        if (PCMStream == null)
        {
            throw new FormatException("Failed to load audio file.");
        }

        if (PCMStream.WaveFormat.Channels > 2)
        {
            throw new FormatException("Audio File cannot have more than 2 channels!");
        }

        OnsetsFound = null;
    }

    public void DetectOnsets(float sensitivity = 1.5f)
    {
        _onsetDetector = new OnsetDetector(PCMStream, 1024);

        bool hasFinished = false;

        PCMStream.Position = 0;

        do
        {
            hasFinished = _onsetDetector.AddFlux(ReadMonoPCM());
        } while (!hasFinished);

        // Find the peaks (onsets)
        _onsetDetector.FindOnsets(sensitivity);
        OnsetsFound = _onsetDetector.Onsets;
    }

    public void NormalizeOnsets(int type)
    {
        _onsetDetector.NormalizeOnsets(type);
    }

    public float[] getOnsets()
    {
        return _onsetDetector.Onsets;
    }

    public float GetTimePerSample()
    {
        return _onsetDetector.TimePerSample();
    }

    #region Internals

    float[] ReadMonoPCM()
    {
        int size = SAMPLE_SIZE;

        if (PCMStream.WaveFormat.Channels == 2)
        {
            size *= 2;
        }

        float[] output = new float[size];

        // Read in a sample
        if (PCMStream.Read(output, 0, size) == 0)
        {
            // If end of audio file
            return null;
        }

        // If stereo, convert to mono
        if (PCMStream.WaveFormat.Channels == 2)
        {
            return ConvertStereoToMono(output);
        }
        else
        {
            return output;
        }
    }

    float[] ConvertStereoToMono(float[] input)
    {
        float[] output = new float[input.Length / 2];
        int outputIndex = 0;

        float leftChannel = 0.0f;
        float rightChannel = 0.0f;

        // Go through each pair of samples
        // Average out the pair
        // Save to output
        for (int i = 0; i < input.Length; i += 2)
        {
            leftChannel = input[i];
            rightChannel = input[i + 1];

            // Average the two channels
            output[outputIndex] = (leftChannel + rightChannel) / 2;
            outputIndex++;
        }

        return output;
    }

    void SetUpFFT()
    {
        // Set up the FFT
        _fft = new Fft();

        _fft.A = 0;
        _fft.B = 1;
    }

    #endregion
}
