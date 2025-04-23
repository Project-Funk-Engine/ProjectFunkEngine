using Godot;

namespace FunkEngine.Classes.BeatDetector;

using System;
using System.Collections.Generic;
using NAudio.Wave;

public partial class OnsetDetector : Resource
{
    private Fft _fft = new Fft();
    private AudioFileReader _pcm;
    private int _sampleSize;

    public float[] Onsets { get; private set; }
    private float[] LowOnsets { get; set; }
    private float[] MidOnsets { get; set; }
    private float[] HighOnsets { get; set; }

    private float[] _previousSpectrum;
    private float[] _spectrum;

    private bool _rectify;

    private List<float> _fluxes;

    public OnsetDetector(AudioFileReader pcm, int sampleWindow)
    {
        _pcm = pcm;
        _sampleSize = sampleWindow;

        _spectrum = new float[sampleWindow / 2 + 1];
        _previousSpectrum = new float[_spectrum.Length];
        _rectify = true;
        _fluxes = [];
    }

    public bool AddFlux(float[] samples, bool hamming = true)
    {
        if (samples != null)
        {
            _fft.RealFft(samples, hamming);

            //update the spectrums
            Array.Copy(_spectrum, _previousSpectrum, _spectrum.Length);
            Array.Copy(_fft.GetPowerSpectrum(), _spectrum, _spectrum.Length);

            _fluxes.Add(CompareSpectra(_spectrum, _previousSpectrum, _rectify));
            return false;
        }
        return true;
    }

    //TODO: Check thresholdTimeSpan. It was 0.0 but that broke stuff.
    public void FindOnsets(float sensitivity = 1.5f, float thresholdTimeSpan = 0.1f)
    {
        //TODO: THIS MIGHT BE THE CAUSE: thresholdAverage is all NaN - is that ok?
        //Spectrum is also very small. like E-12 small
        //Kinda fixed by above to do but still not great
        var thresholdAverage = GetThresholdAverage(
            _fluxes,
            _sampleSize,
            thresholdTimeSpan,
            sensitivity
        );
        Onsets = GetPeaks(_fluxes, thresholdAverage, _sampleSize);
    }

    public void NormalizeOnsets(int type)
    {
        if (Onsets == null)
        {
            return;
        }

        var max = 0.0f;
        var min = 0.0f;
        var range = 0.0f;

        //Find the strongest/weakest onset
        for (int i = 0; i < Onsets.Length; i++)
        {
            max = Math.Max(max, Onsets[i]);
            min = Math.Min(min, Onsets[i]);
        }
        range = max - min;

        //Normalize the onsets
        switch (type)
        {
            case 0:
                for (int i = 0; i < Onsets.Length; i++)
                {
                    Onsets[i] /= max;
                }

                break;

            case 1:
                for (int i = 0; i < Onsets.Length; i++)
                {
                    if (Onsets[i] == min)
                    {
                        Onsets[i] = 0.01f;
                    }
                    else
                    {
                        Onsets[i] -= min;
                        Onsets[i] /= range;
                    }
                }
                break;
        }
    }

    private static float CompareSpectra(float[] spectrum, float[] previousSpectrum, bool rectify)
    {
        //Find the difference between the two spectra and sum them up
        var flux = 0.0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            var value = (spectrum[i] - previousSpectrum[i]);
            if (!rectify || value > 0)
            {
                flux += value;
            }
        }
        return flux;
    }

    private float[] GetPeaks(List<float> data, float[] dataAverage, int sampleCount)
    {
        //Minimum time between peaks in seconds. Effectively the resolution of the beat detection. 10ms is about as good as humans can detect.
        //Our game is not anywhere close to that precise, so we should work on finding a good value for this. (or just use the default idk)
        //TODO: That^^^ (or make this a variable that the user can set)
        const float minTimeBetweenPeaks = 0.01f;

        //Number of samples to ignore after each onset
        var immunityPeriod = (int)(
            (float)sampleCount / (float)_pcm.WaveFormat.SampleRate / minTimeBetweenPeaks
        );

        //Where we store the results
        var peaks = new float[data.Count];

        //find the peaks
        for (int i = 0; i < data.Count; i++)
        {
            //If the flux is above the average
            if (data[i] >= dataAverage[i])
            {
                peaks[i] = data[i] - dataAverage[i];
            }
            else
            {
                peaks[i] = 0.0f;
            }
        }

        //Do all peak cleanup here:
        peaks[0] = 0.0f;
        for (int i = 1; i < peaks.Length - 1; i++)
        {
            //If the next value is lower than the current value, that means that it is the end of the peak
            if (peaks[i] < peaks[i + 1])
            {
                peaks[i] = 0.0f;
                continue;
            }
            //If the peak is too close to the last peak, ignore it
            if (peaks[i] > 0.0f)
            {
                for (int j = i + 1; j < i + immunityPeriod; j++)
                {
                    if (peaks[j] > 0)
                    {
                        peaks[j] = 0.0f;
                    }
                }
            }
        }

        GD.Print("Peaks: ", peaks);

        return peaks;
    }

    private float[] GetThresholdAverage(
        List<float> data,
        int sampleWindow,
        float thresholdTimeSpan,
        float thresholdMultiplier
    )
    {
        var thresholdAverage = new List<float>();

        //How many spectral fluxes to inspect at a time, approximation is fine
        var sourceTimeSpan = (float)(sampleWindow) / (float)(_pcm.WaveFormat.SampleRate);
        var windowSize = (int)(thresholdTimeSpan / sourceTimeSpan / 2);

        for (int i = 0; i < data.Count; i++)
        {
            //Max/Min to prevent out of bounds
            //look at values to the left and right of the current spectral flux
            var start = Math.Max(i - windowSize, 0);
            var end = Math.Min(data.Count, i + windowSize);

            //current Average
            var mean = 0.0f;

            //Sum up the surrounding values
            for (int j = start; j < end; j++)
            {
                mean += data[j];
            }

            //Find the average
            mean /= (end - start);

            //Multiply the average by the threshold multiplier to increase sensitivity
            thresholdAverage.Add(mean * thresholdMultiplier);
        }

        return thresholdAverage.ToArray();
    }

    public float TimePerSample()
    {
        return (float)_sampleSize / (float)_pcm.WaveFormat.SampleRate;
    }
}
