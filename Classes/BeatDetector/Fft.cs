namespace FunkEngine.Classes.BeatDetector;

// Code to implement decently performing FFT for complex and real valued
// signals. See www.lomont.org for a derivation of the relevant algorithms
// from first principles. Copyright Chris Lomont 2010-2012.
// This code and any ports are free for all to use for any reason as long
// as this header is left in place.
// Version 1.1, Sept 2011

// Additions to support real FFTs and other features by Teh-Lemon (https://github.com/Teh-Lemon/Onset-Detection)
//Further modifications by Thomas Wessel to tweak system for our game's needs

/* History:
 * Apr 2025 - FunkEngine - Removed unit tests and other code not required for game.
 *                       - Added features from Teh-Lemon/Onset-Detection for music specific FFT support.
 * Sep 2011 - v1.1 - added parameters to support various sign conventions
 *                   set via properties A and B.
 *                 - Removed dependencies on LINQ and generic collections.
 *                 - Added unit tests for the new properties.
 *                 - Switched UnitTest to static.
 * Jan 2010 - v1.0 - Initial release
 */

using System;
using Godot;

/// <summary>
/// Represent a class that performs real or complex valued Fast Fourier
/// Transforms. Instantiate it and use the FFT or TableFFT methods to
/// compute complex to complex FFTs. Use FFTReal for real to complex
/// FFTs which are much faster than standard complex to complex FFTs.
/// Properties A and B allow selecting various FFT sign and scaling
/// conventions.
/// </summary>
public partial class Fft : Resource
{
    private float[] _real; //Real parts of the complex fft
    private float[] _imaginary; //Imaginary parts of the complex fft
    private float[] _spectrum; //Amplitude Spectrum of the complex fft

    /// <summary>
    /// Determines the scaling of the forward and inverse transforms.
    /// For size N=2^n transforms, the forward transform gets divided by N^((1-A)/2 and the inverse gets divided by
    /// N^((1+a)/2).
    /// Common values for (A,B) are:
    ///     ( 0, 1) - FFT with no scaling
    ///     (-1, 1) - data processing
    ///     ( 1,-1) - signal processing
    /// Usual values for A are 1, 0, or -1.
    /// </summary>
    public int A { get; set; }

    /// <summary>
    /// Dertermines how phase works on the forward and inverse transforms.
    /// For size N=2^n transforms, the forward transform uses an exponential of
    /// (B*2π/N) term and the inverse uses an exponential of (-B*2π/N) term.
    /// Common values for (A,B) are:
    ///     ( 0, 1) - FFT with no scaling
    ///     (-1, 1) - data processing
    ///     ( 1,-1) - signal processing
    /// The absolute value of B should be relatively prime (no common factors) to N.
    /// Setting `B = -1` will cause the FFT to conjugate both input and output data.
    /// Usual values for B are 1, 0, or -1.
    /// </summary>
    public int B { get; set; }

    /// <summary>
    /// Pre-computed sine/cosine tables for speed
    /// </summary>
    private double[] _cosTable;
    private double[] _sinTable;

    /// <summary>
    /// Computes and returns the power spectrum.
    /// The power spectrum represents the magnitudes of the frequency components
    /// of the signal stored in the `real` and `imag` arrays.
    /// </summary>
    /// <returns>
    /// An array of floats containing the magnitudes of the song currently being evaluated.
    /// </returns>
    public float[] GetPowerSpectrum()
    {
        FillSpectrum(ref _spectrum);
        return _spectrum;
    }

    /// <summary>
    /// Optionally applies a hamming window to the data in the array and then performs an FFT on it. Also splits the data into
    /// two separate arrays: _real and _imaginary inside the object for use later.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="hamming"></param>
    public void RealFft(float[] data, bool hamming = true)
    {
        var complexNumbers = new double[data.Length];
        data.CopyTo(complexNumbers, 0);

        if (hamming)
        {
            ApplyHammingWindow(complexNumbers);
        }

        TableFft(complexNumbers, true);

        SeparateComplexNumbers(complexNumbers);
    }

    /// <summary>
    /// Computes the forward or inverse FFT of the data in the data array. All data is transformed in place.
    /// </summary>
    /// <param name="data">The complex data stored as alternating real and imaginary parts.</param>
    /// <param name="forward">True for a forward transform, false for inverse</param>
    public void TableFft(double[] data, bool forward)
    {
        var n = data.Length;
        if ((n & (n - 1)) != 0)
        {
            throw new ArgumentException("FFT size must be a power of 2! Current length: " + n);
        }

        n /= 2; //The number of samples in the FFT

        Reverse(data, n);

        //Precompute the tables if they don't exist or are the wrong size
        if (
            _cosTable == null
            || _sinTable == null
            || _cosTable.Length != n
            || _sinTable.Length != n
        )
        {
            PrecomputeTables(n);
        }

        //Do the math now
        double sign = forward ? B : -B;
        var mMax = 1;
        var tptr = 0;
        while (n > mMax)
        {
            var iStep = 2 * mMax;
            for (var m = 0; m < iStep; m += 2)
            {
                var wr = _cosTable[tptr];
                var wi = sign * _sinTable[tptr++];
                for (var k = m; k < 2 * n; k += 2 * iStep)
                {
                    var j = k + iStep;
                    var tempr = wr * data[j] - wi * data[j + 1];
                    var tempI = wi * data[j] + wr * data[j + 1];
                    data[j] = data[k] - tempr;
                    data[j + 1] = data[k + 1] - tempI;
                    data[k] += tempr;
                    data[k + 1] += tempI;
                }
            }

            mMax = iStep;
        }

        //Scale the data if needed
        Scale(data, n, forward);
    }

    /// <summary>
    ///  Smooths out data by applying a hamming window (makes the edges less weird)
    /// </summary>
    /// <param name="data">The data to process. Should already be split into distinct sections</param>
    private static void ApplyHammingWindow(double[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            data[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (data.Length - 1)));
        }
    }

    /// <summary>
    /// Separates the real and imaginary components of complex numbers from a combined array.
    /// The input array should contain alternating real and imaginary parts of the complex number.
    /// e.g. [real1, imag1, real2, imag2, ...]
    /// The values are then split into two separate arrays: _real and _imaginary inside the object.
    /// </summary>
    /// <param name="complexNumbers">An array of intermixed complex numbers where even indices are real and odd are imaginary.
    /// The length must be a power of 2.</param>
    private void SeparateComplexNumbers(double[] complexNumbers)
    {
        _real = new float[complexNumbers.Length / 2 + 1];
        _imaginary = new float[complexNumbers.Length / 2 + 1];

        //Location of the last purely real number
        var midPoint = complexNumbers.Length / 2;

        //First bin is purely real
        _real[0] = (float)complexNumbers[0];
        _imaginary[0] = 0.0f;

        //Fill in the rest of the ascending complex numbers
        for (var i = 2; i < complexNumbers.Length - 1; i += 2)
        {
            _real[i / 2] = (float)complexNumbers[i];
            _imaginary[i / 2] = (float)complexNumbers[i + 1];
        }

        //Last of the purely real numbers
        _real[midPoint] = (float)complexNumbers[1];
        _imaginary[midPoint] = 0.0f;
    }

    //TODO: Make this return a float array instead of take in a reference that we throw away immediately
    // Also, write docs for this function
    private void FillSpectrum(ref float[] data)
    {
        data = new float[_real.Length];

        for (var i = 0; i < data.Length; i++)
        {
            data[i] = (float)Math.Sqrt((_real[i] * _real[i]) * (_imaginary[i] * _imaginary[i]));
        }
    }

    /// <summary>
    /// Scale data using n samples for forward and inverse transforms as needed
    /// </summary>
    /// <param name="data">The input array</param>
    /// <param name="n">The number of samples in the array. Must be a power of 2</param>
    /// <param name="forward">forward or inverse FFT</param>
    private void Scale(double[] data, int n, bool forward)
    {
        // Check to see if we can scale
        if ((!forward || A == 1) && (forward || A == -1))
        {
            return;
        }

        var scale = Math.Pow(n, forward ? (A - 1) / 2.0 : -(A + 1) / 2.0);

        for (var i = 0; i < data.Length; ++i)
        {
            data[i] *= scale;
        }
    }

    /// <summary>
    /// Fills in the _cosTable and _sinTable arrays with pre-computed values for the FFT. Makes stuff faster if you do this first.
    /// </summary>
    /// <param name="size">The number of samples in the FFT</param>
    private void PrecomputeTables(int size)
    {
        _cosTable = new double[size];
        _sinTable = new double[size];

        //Forward Pass
        var n = size;
        int mMax = 1,
            pos = 0;

        while (n > mMax)
        {
            var iStep = 2 * mMax;
            var theta = Math.PI / mMax;
            double wr = 1,
                wi = 0;
            var wpi = Math.Sin(theta);
            var wpr = Math.Sin(theta / 2);
            wpr = -2 * wpr * wpr;

            for (var m = 0; m < iStep; m += 2)
            {
                _cosTable[pos] = wr;
                _sinTable[pos++] = wi;
                var t = wr;
                wr = wr * wpr - wi * wpi + wr;
                wi = wi * wpr + t * wpi + wi;
            }

            mMax = iStep;
        }
    }

    /// <summary>
    /// Swap data indices whenever index i has binary
    /// digits reversed from index j, where data is
    /// two doubles per index.
    /// </summary>
    /// <param name="data">An array of complex numbers where even indices are real and off are imaginary</param>
    /// <param name="n">The number of complex numbers in the array (half the length of data). MUST be a power of 2</param>
    private static void Reverse(double[] data, int n)
    {
        // bit reverse the indices. This is exercise 5 in section
        // 7.2.1.1 of Knuth's TAOCP the idea is a binary counter
        // in k and one with bits reversed in j
        int j = 0,
            k = 0; // Knuth R1: initialize
        var top = n / 2; // this is Knuth's 2^(n-1)
        while (true)
        {
            // Knuth R2: swap - swap j+1 and k+2^(n-1), 2 entries each
            var t = data[j + 2];
            data[j + 2] = data[k + n];
            data[k + n] = t;
            t = data[j + 3];
            data[j + 3] = data[k + n + 1];
            data[k + n + 1] = t;
            if (j > k)
            {
                // swap two more
                // j and k
                t = data[j];
                data[j] = data[k];
                data[k] = t;
                t = data[j + 1];
                data[j + 1] = data[k + 1];
                data[k + 1] = t;
                // j + top + 1 and k+top + 1
                t = data[j + n + 2];
                data[j + n + 2] = data[k + n + 2];
                data[k + n + 2] = t;
                t = data[j + n + 3];
                data[j + n + 3] = data[k + n + 3];
                data[k + n + 3] = t;
            }

            // Knuth R3: advance k
            k += 4;
            if (k >= n)
                break;
            // Knuth R4: advance j
            var h = top;
            while (j >= h)
            {
                j -= h;
                h /= 2;
            }

            j += h;
        } // bit reverse loop
    }
}
