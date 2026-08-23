using System;
using UnityEngine;

namespace Takenoko
{
    public static class RGBToSpectrum
    {
        public const int SampleCount = 41;
        public const float MinWavelength = 380.0f;
        public const float MaxWavelength = 780.0f;
        public const float WavelengthStep = 10.0f;

        private static readonly double[] D65 =
        {
            49.9755, 54.6482, 82.7549, 91.4860, 93.4318, 86.6823, 104.865, 117.008,
            117.812, 114.861, 115.923, 108.811, 109.354, 107.802, 104.790, 107.689,
            104.405, 104.046, 100.000, 96.3342, 95.7880, 88.6856, 90.0062, 89.5991,
            87.6987, 83.2886, 83.6992, 80.0268, 80.2146, 82.2778, 78.2842, 69.7213,
            71.6091, 74.3490, 61.6040, 69.8856, 75.0870, 63.5927, 46.4182, 66.8054,
            63.3828
        };

        private static readonly double[,] SpectrumToXYZ = BuildSpectrumToXYZ();

        public static float GetWavelength(int index)
        {
            if (index < 0 || index >= SampleCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return MinWavelength + index * WavelengthStep;
        }

        public static float[] FromLinearSRGB(
            Color color,
            int iterations = 20000,
            double colorWeight = 1000000.0,
            double smoothness = 1.0,
            float maxReflectance = 1.0f)
        {
            double r = Math.Max(0.0, color.r);
            double g = Math.Max(0.0, color.g);
            double b = Math.Max(0.0, color.b);

            double targetX = 0.4123907993 * r + 0.3575843394 * g + 0.1804807884 * b;
            double targetY = 0.2126390059 * r + 0.7151686788 * g + 0.0721923154 * b;
            double targetZ = 0.0193308187 * r + 0.1191947798 * g + 0.9505321522 * b;

            double[] spectrum = new double[SampleCount];
            double[] gradient = new double[SampleCount];

            double upper = maxReflectance > 0.0f ? maxReflectance : double.PositiveInfinity;
            double initial = Math.Min(targetY, upper);

            for (int i = 0; i < SampleCount; i++)
            {
                spectrum[i] = initial;
            }

            double frobeniusSquared = 0.0;

            for (int c = 0; c < 3; c++)
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    frobeniusSquared += SpectrumToXYZ[c, i] * SpectrumToXYZ[c, i];
                }
            }

            double stepSize = 0.95 / (colorWeight * frobeniusSquared + 16.0 * smoothness);

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                Array.Clear(gradient, 0, gradient.Length);

                double x = 0.0;
                double y = 0.0;
                double z = 0.0;

                for (int i = 0; i < SampleCount; i++)
                {
                    x += SpectrumToXYZ[0, i] * spectrum[i];
                    y += SpectrumToXYZ[1, i] * spectrum[i];
                    z += SpectrumToXYZ[2, i] * spectrum[i];
                }

                double errorX = x - targetX;
                double errorY = y - targetY;
                double errorZ = z - targetZ;

                for (int i = 0; i < SampleCount; i++)
                {
                    gradient[i] += colorWeight * (
                        SpectrumToXYZ[0, i] * errorX +
                        SpectrumToXYZ[1, i] * errorY +
                        SpectrumToXYZ[2, i] * errorZ);
                }

                for (int i = 1; i < SampleCount - 1; i++)
                {
                    double secondDerivative = spectrum[i - 1] - 2.0 * spectrum[i] + spectrum[i + 1];

                    gradient[i - 1] += smoothness * secondDerivative;
                    gradient[i] -= 2.0 * smoothness * secondDerivative;
                    gradient[i + 1] += smoothness * secondDerivative;
                }

                double maxDelta = 0.0;

                for (int i = 0; i < SampleCount; i++)
                {
                    double previous = spectrum[i];
                    spectrum[i] = Clamp(previous - stepSize * gradient[i], 0.0, upper);
                    maxDelta = Math.Max(maxDelta, Math.Abs(spectrum[i] - previous));
                }

                if (maxDelta < 1e-10)
                {
                    break;
                }
            }

            float[] result = new float[SampleCount];

            for (int i = 0; i < SampleCount; i++)
            {
                result[i] = (float)spectrum[i];
            }

            return result;
        }

        public static float[] FromSRGB(
            Color color,
            int iterations = 20000,
            double colorWeight = 1000000.0,
            double smoothness = 1.0,
            float maxReflectance = 1.0f)
        {
            return FromLinearSRGB(color.linear, iterations, colorWeight, smoothness, maxReflectance);
        }

        public static Color ToLinearSRGB(float[] spectrum)
        {
            ValidateSpectrum(spectrum);

            double x = 0.0;
            double y = 0.0;
            double z = 0.0;

            for (int i = 0; i < SampleCount; i++)
            {
                x += SpectrumToXYZ[0, i] * spectrum[i];
                y += SpectrumToXYZ[1, i] * spectrum[i];
                z += SpectrumToXYZ[2, i] * spectrum[i];
            }

            float r = (float)(3.2409699419 * x - 1.5373831776 * y - 0.4986107603 * z);
            float g = (float)(-0.9692436363 * x + 1.8759675015 * y + 0.0415550574 * z);
            float b = (float)(0.0556300797 * x - 0.2039769589 * y + 1.0569715142 * z);

            return new Color(r, g, b, 1.0f);
        }

        public static Color ToSRGB(float[] spectrum)
        {
            return ToLinearSRGB(spectrum).gamma;
        }

        public static float[] SpectralLerp(float[] a, float[] b, float t, bool preserveLuminance = true)
        {
            ValidateSpectrum(a);
            ValidateSpectrum(b);

            t = Mathf.Clamp01(t);

            if (t <= 0.0f)
            {
                return (float[])a.Clone();
            }

            if (t >= 1.0f)
            {
                return (float[])b.Clone();
            }

            double baselineA = double.PositiveInfinity;
            double baselineB = double.PositiveInfinity;

            for (int i = 0; i < SampleCount; i++)
            {
                baselineA = Math.Min(baselineA, Math.Max(0.0, a[i]));
                baselineB = Math.Min(baselineB, Math.Max(0.0, b[i]));
            }

            double baseline = baselineA + (baselineB - baselineA) * t;
            double[] residualA = new double[SampleCount];
            double[] residualB = new double[SampleCount];

            double sumA = 0.0;
            double sumB = 0.0;

            for (int i = 0; i < SampleCount; i++)
            {
                residualA[i] = Math.Max(0.0, a[i] - baselineA);
                residualB[i] = Math.Max(0.0, b[i] - baselineB);

                sumA += residualA[i];
                sumB += residualB[i];
            }

            double[] transported = new double[SampleCount];

            if (sumA > 1e-12 && sumB > 1e-12)
            {
                TransportSpectrum(residualA, sumA, residualB, sumB, t, transported);
            }
            else if (sumA > 1e-12)
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    transported[i] = residualA[i] / sumA;
                }
            }
            else if (sumB > 1e-12)
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    transported[i] = residualB[i] / sumB;
                }
            }

            double residualEnergy = sumA + (sumB - sumA) * t;
            float[] result = new float[SampleCount];

            for (int i = 0; i < SampleCount; i++)
            {
                result[i] = (float)(baseline + transported[i] * residualEnergy);
            }

            if (preserveLuminance)
            {
                MatchInterpolatedLuminance(result, a, b, t);
            }

            return result;
        }

        public static float[] LinearSpectralLerp(float[] a, float[] b, float t)
        {
            ValidateSpectrum(a);
            ValidateSpectrum(b);

            t = Mathf.Clamp01(t);
            float[] result = new float[SampleCount];

            for (int i = 0; i < SampleCount; i++)
            {
                result[i] = Mathf.Lerp(a[i], b[i], t);
            }

            return result;
        }

        private static void TransportSpectrum(double[] a, double sumA, double[] b, double sumB, float t, double[] result)
        {
            int indexA = SampleCount - 1;
            int indexB = SampleCount - 1;

            double remainingA = 0.0;
            double remainingB = 0.0;

            const double epsilon = 1e-14;

            while ((indexA >= 0 || remainingA > epsilon) && (indexB >= 0 || remainingB > epsilon))
            {
                if (remainingA <= epsilon)
                {
                    while (indexA >= 0 && a[indexA] <= epsilon)
                    {
                        indexA--;
                    }

                    if (indexA < 0)
                    {
                        break;
                    }

                    remainingA = a[indexA] / sumA;
                }

                if (remainingB <= epsilon)
                {
                    while (indexB >= 0 && b[indexB] <= epsilon)
                    {
                        indexB--;
                    }

                    if (indexB < 0)
                    {
                        break;
                    }

                    remainingB = b[indexB] / sumB;
                }

                double amount = Math.Min(remainingA, remainingB);

                double wavelengthA = GetWavelength(indexA);
                double wavelengthB = GetWavelength(indexB);

                double waveNumberA = 1.0 / wavelengthA;
                double waveNumberB = 1.0 / wavelengthB;
                double waveNumber = waveNumberA + (waveNumberB - waveNumberA) * t;
                double wavelength = 1.0 / waveNumber;

                SplatSpectrum(result, wavelength, amount);

                remainingA -= amount;
                remainingB -= amount;

                if (remainingA <= epsilon)
                {
                    remainingA = 0.0;
                    indexA--;
                }

                if (remainingB <= epsilon)
                {
                    remainingB = 0.0;
                    indexB--;
                }
            }

            double sum = 0.0;

            for (int i = 0; i < SampleCount; i++)
            {
                sum += result[i];
            }

            if (sum > epsilon)
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    result[i] /= sum;
                }
            }
        }

        private static void SplatSpectrum(double[] spectrum, double wavelength, double value)
        {
            double position = (wavelength - MinWavelength) / WavelengthStep;
            position = Clamp(position, 0.0, SampleCount - 1.0);

            int lower = Mathf.Clamp((int)Math.Floor(position), 0, SampleCount - 1);
            int upper = Math.Min(lower + 1, SampleCount - 1);
            double alpha = Clamp(position - lower, 0.0, 1.0);

            spectrum[lower] += value * (1.0 - alpha);
            spectrum[upper] += value * alpha;
        }

        private static void MatchInterpolatedLuminance(float[] result, float[] a, float[] b, float t)
        {
            Color colorA = ToLinearSRGB(a);
            Color colorB = ToLinearSRGB(b);
            Color resultColor = ToLinearSRGB(result);

            double luminanceA = GetLuminance(colorA);
            double luminanceB = GetLuminance(colorB);
            double resultLuminance = GetLuminance(resultColor);
            double targetLuminance = luminanceA + (luminanceB - luminanceA) * t;

            if (resultLuminance <= 1e-12)
            {
                return;
            }

            double scale = targetLuminance / resultLuminance;

            for (int i = 0; i < SampleCount; i++)
            {
                result[i] = (float)Math.Max(0.0, result[i] * scale);
            }
        }

        private static double GetLuminance(Color color)
        {
            return 0.2126390059 * color.r + 0.7151686788 * color.g + 0.0721923154 * color.b;
        }

        private static double[,] BuildSpectrumToXYZ()
        {
            double[,] result = new double[3, SampleCount];

            double whiteX = 0.0;
            double whiteY = 0.0;
            double whiteZ = 0.0;

            for (int i = 0; i < SampleCount; i++)
            {
                double wavelength = MinWavelength + i * WavelengthStep;
                Vector3 cmf = EvaluateCIE1931((float)wavelength);
                double integrationWeight = i == 0 || i == SampleCount - 1 ? 0.5 : 1.0;
                double weight = D65[i] * WavelengthStep * integrationWeight;

                whiteX += weight * cmf.x;
                whiteY += weight * cmf.y;
                whiteZ += weight * cmf.z;
            }

            const double targetWhiteX = 0.9504559271;
            const double targetWhiteY = 1.0;
            const double targetWhiteZ = 1.0890577508;

            double scaleX = targetWhiteX / whiteX;
            double scaleY = targetWhiteY / whiteY;
            double scaleZ = targetWhiteZ / whiteZ;

            for (int i = 0; i < SampleCount; i++)
            {
                double wavelength = MinWavelength + i * WavelengthStep;
                Vector3 cmf = EvaluateCIE1931((float)wavelength);
                double integrationWeight = i == 0 || i == SampleCount - 1 ? 0.5 : 1.0;
                double weight = D65[i] * WavelengthStep * integrationWeight;

                result[0, i] = weight * cmf.x * scaleX;
                result[1, i] = weight * cmf.y * scaleY;
                result[2, i] = weight * cmf.z * scaleZ;
            }

            return result;
        }

        private static Vector3 EvaluateCIE1931(float wavelength)
        {
            double t1 = (wavelength - 442.0) * (wavelength < 442.0 ? 0.0624 : 0.0374);
            double t2 = (wavelength - 599.8) * (wavelength < 599.8 ? 0.0264 : 0.0323);
            double t3 = (wavelength - 501.1) * (wavelength < 501.1 ? 0.0490 : 0.0382);

            double x =
                0.362 * Math.Exp(-0.5 * t1 * t1) +
                1.056 * Math.Exp(-0.5 * t2 * t2) -
                0.065 * Math.Exp(-0.5 * t3 * t3);

            t1 = (wavelength - 568.8) * (wavelength < 568.8 ? 0.0213 : 0.0247);
            t2 = (wavelength - 530.9) * (wavelength < 530.9 ? 0.0613 : 0.0322);

            double y =
                0.821 * Math.Exp(-0.5 * t1 * t1) +
                0.286 * Math.Exp(-0.5 * t2 * t2);

            t1 = (wavelength - 437.0) * (wavelength < 437.0 ? 0.0845 : 0.0278);
            t2 = (wavelength - 459.0) * (wavelength < 459.0 ? 0.0385 : 0.0725);

            double z =
                1.217 * Math.Exp(-0.5 * t1 * t1) +
                0.681 * Math.Exp(-0.5 * t2 * t2);

            return new Vector3((float)x, (float)y, (float)z);
        }

        private static void ValidateSpectrum(float[] spectrum)
        {
            if (spectrum == null)
            {
                throw new ArgumentNullException(nameof(spectrum));
            }

            if (spectrum.Length != SampleCount)
            {
                throw new ArgumentException($"Spectrum must contain {SampleCount} samples.");
            }
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }
    }
}
