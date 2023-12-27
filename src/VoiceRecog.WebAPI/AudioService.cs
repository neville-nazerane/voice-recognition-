using System.Collections.Concurrent;

namespace VoiceRecog.WebAPI
{
    public class AudioService(DeepSpeechService deepSpeechService)
    {

        public static readonly ConcurrentDictionary<string, long> _fileSizes = new();

        public static readonly ConcurrentDictionary<string, Stream> _fileStreams = new();

        public static readonly ConcurrentDictionary<string, MemoryStream> _audioStreams = new();

        private readonly DeepSpeechService _deepSpeechService = deepSpeechService;

        public async Task StreamAudioToText(Stream stream, string key, CancellationToken cancellationToken = default)
        {
            bool streamJustCreated = false;
            var mainStream = _audioStreams.GetOrAdd(key, k =>
            {
                streamJustCreated = true;
                return new();
            });

            await stream.CopyToAsync(mainStream, cancellationToken);
            
            if (streamJustCreated)
                _ = _deepSpeechService.KeepReadingAsync(mainStream, CancellationToken.None);

        }

        public async Task ReadAudioStreamToFileAsync(Stream stream, string? fname = null, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            long totalAudioDataSize = 0; // To keep track of the total size of audio data written

            fname ??= Guid.NewGuid().ToString("N");
            string fileName = @$"downloadedAudioFiles\{fname}.wav";

            //if (!File.Exists(fileName))
            //    await File.WriteAllTextAsync(fileName, string.Empty, cancellationToken);

            var fileStream = _fileStreams.GetOrAdd(fname, k => File.OpenWrite(fileName));

            await WriteWavHeaderAsync(fileStream, 44100, 16, 4);

            await Console.Out.WriteLineAsync($"{DateTime.Now.ToLongTimeString()}: Starting to read... ");

            var ct = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            //ct.CancelAfter(TimeSpan.FromSeconds(10));

            while (!ct.Token.IsCancellationRequested)
            {
                bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                if (bytesRead > 0)
                {
                    //await Console.Out.WriteLineAsync("READ " + bytesRead);
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalAudioDataSize += bytesRead;
                }
                else break;
            }

            await Console.Out.WriteLineAsync($"{DateTime.Now.ToLongTimeString()}: Done reading");

            // Go back and update the header with the correct sizes
            //await UpdateWavHeaderAsync(fileStream, totalAudioDataSize);

            _fileSizes.AddOrUpdate(fname, totalAudioDataSize, (k, t) => t + totalAudioDataSize);

            await Console.Out.WriteLineAsync($"{DateTime.Now.ToLongTimeString()}: Wrapped up");
        }

        public async Task CompleteFileAsync(string fileName, CancellationToken _ = default)
        {

            var stream = _fileStreams[fileName];
            await UpdateWavHeaderAsync(stream, _fileSizes[fileName]);
            await stream.DisposeAsync();
            //long length = 0;
            //await using (var read = File.OpenRead(fileName))
            //    length = read.Length;

            //await using var stream = File.OpenWrite(fileName);
            //await UpdateWavHeaderAsync(stream, length, fileSizes[fileName]);
        }

        async Task WriteWavHeaderAsync(Stream stream, int sampleRate, int bitsPerSample, int channels)
        {
            await using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(0); // Placeholder for file size
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size (16 for PCM)
            writer.Write((short)1); // AudioFormat (1 for PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bitsPerSample / 8); // ByteRate
            writer.Write((short)(channels * bitsPerSample / 8)); // BlockAlign
            writer.Write((short)bitsPerSample);

            // Data subchunk
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(0); // Placeholder for data size
        }

        static async Task UpdateWavHeaderAsync(Stream fileStream, long totalAudioDataSize)
        {
            int fileLength = (int)fileStream.Length;
            int dataChunkSize = (int)totalAudioDataSize;
            int riffChunkSize = fileLength - 8; // Size of the whole file minus 8 bytes for the RIFF header

            fileStream.Position = 4; // Position to RIFF chunk size (after 'RIFF')
            await using var writer = new BinaryWriter(fileStream, System.Text.Encoding.UTF8, true);
            writer.Write(riffChunkSize);

            fileStream.Position = 40; // Position to data chunk size (after 'data')
            writer.Write(dataChunkSize);
        }



    }
}
