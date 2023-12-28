using DeepSpeechClient.Interfaces;
using DeepSpeechClient.Models;
using DeepSpeechClient;
using System.Diagnostics;
using System.IO;

namespace VoiceRecog.WebAPI
{
    public class DeepSpeechService
    {
        private readonly DeepSpeech _deepSpeech;

        public DeepSpeechService()
        {
            _deepSpeech = new DeepSpeech(@"D:\Cloud\git\voice recognition\src\VoiceRecog.WebAPI\DeepspeechModels\deepspeech-0.9.3-models.pbmm");
        }

        public async Task KeepReadingAsync(MemoryStream stream, CancellationToken cancellationToken = default)
        {

            long lastLength = 0;

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), 
                                 cancellationToken);
                
                // if no changes in stream, to go next iteration
                if (lastLength == stream.Length) continue; 
                lastLength = stream.Length;


                //var bytes = new Memory<byte>();
                //stream.Position = 0;

                //await stream.ReadAsync(bytes, cancellationToken);

                byte[] byteArray = stream.ToArray();

                // Assuming the byte array is in 16-bit PCM format
                short[] shortArray = new short[byteArray.Length / 2];
                Buffer.BlockCopy(byteArray, 0, shortArray, 0, byteArray.Length);

                // Call the SpeechToText function

                var text = _deepSpeech.SpeechToText(shortArray, (uint)shortArray.Length);

                //await Console.Out.WriteLineAsync("DETECTED: " + text);
            }

        }

    }
}
