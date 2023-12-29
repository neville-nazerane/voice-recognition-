using NAudio.Wave;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;
using NWaves.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Samples.Utils
{
    public static class AudioParseHelper
    {


        public static async Task WriteFilesToCsvAsync(string filesSrcPath,
                                                      string destCsvPath)
        {
            var columnNames = new List<string> { "Utterance" };
            columnNames.AddRange(Enumerable.Range(1, 13).Select(r => $"Mean_MFCC{r}").ToArray());
            columnNames.AddRange(Enumerable.Range(1, 13).Select(r => $"StdDev_MFCC{r}").ToArray());

            await FileUtils.AddCsvLineAsync(destCsvPath, columnNames);
            int count = 0;
            foreach (var file in Directory.GetFiles(filesSrcPath))
            {
                if (!file.EndsWith(".wav")) continue;

                var label = file[..^4].Split("__").ElementAtOrDefault(1);
                if (label is null) continue;
                label = label.Replace("_", " ");

                await Console.Out.WriteLineAsync($"Writing file {++count} named '{label}'");

                await using var stream = File.OpenRead(file);

                var features = ExtractMfcc(stream, 44100, 1103, 441, 13);
                var aggrMfccs = AggregateMfccs(features);

                var lineItems = new List<object> { label };
                foreach (var f in aggrMfccs)
                    lineItems.Add(f);

                await FileUtils.AddCsvLineAsync(destCsvPath, lineItems);
            }

        }


        public static float[][] ExtractMfcc(Stream audioStream,
                                            int sampleRate,
                                            int mfccSize,
                                            int hopSize,
                                            int featureCount)
        {

            var waveFile = new WaveFile(audioStream);

            // Assuming the wave file is mono channel
            var signal = waveFile[Channels.Left];

            var options = new MfccOptions
            {
                SamplingRate = sampleRate,
                FeatureCount = featureCount,
                FrameDuration = mfccSize / (float)sampleRate,
                HopDuration = hopSize / (float)sampleRate,
                PreEmphasis = 0.97,
                Window = WindowType.Hamming
            };

            var mfccExtractor = new MfccExtractor(options);
            var mfccFeatures = mfccExtractor.ComputeFrom(signal);

            return [.. mfccFeatures];
        }

        public static double[] AggregateMfccs(float[][] mfccs)
        {
            int numCoefficients = mfccs[0].Length;
            double[] aggregatedMfccs = new double[numCoefficients * 2]; // Mean and Std for each coefficient

            for (int i = 0; i < numCoefficients; i++)
            {
                // Calculate mean
                double mean = mfccs.Average(frame => frame[i]);
                aggregatedMfccs[i] = mean;

                // Calculate standard deviation
                double sumOfSquares = mfccs.Sum(frame => Math.Pow(frame[i] - mean, 2));
                double stdDeviation = Math.Sqrt(sumOfSquares / mfccs.Length);
                aggregatedMfccs[i + numCoefficients] = stdDeviation;
            }

            return aggregatedMfccs;
        }


        //static void ExtractFeatures(Stream audioStream)
        //{

        //    var framed = NAudio.Wave.Mp3Frame.LoadFromStream(audioStream);

        //    var e = new MfccExtractor(new()
        //    {

        //    });

        //    e.

        //}

        //public double[][] ExtractMFCCsFromStream(Stream audioStream, int sampleRate, int mfccCount)
        //{
        //    using (var audioFileReader = new WaveFileReader(audioStream))
        //    {
        //        var samples = new List<float>();
        //        var buffer = new float[audioFileReader.WaveFormat.SampleRate];
        //        int read;
        //        while ((read = audioFileReader.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            samples.AddRange(buffer.Take(read));
        //        }

        //        double[] doubleSamples = samples.Select(s => (double)s).ToArray();

        //        var mfcc = new MelFrequencyCepstrumCoefficient(sampleRate, mfccCount);
        //        return mfcc.Transform(doubleSamples);
        //    }
        //}

    }

}
