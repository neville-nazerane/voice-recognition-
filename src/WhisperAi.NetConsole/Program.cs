using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;






var modelName = "ggml-MediumEn.bin";

if (!File.Exists(modelName))
    await GetModelAsync(modelName, GgmlType.MediumEn);

using var whisperFactory = WhisperFactory.FromPath(modelName);

await using var processor = whisperFactory.CreateBuilder()
                                    
                                            .WithLanguage("en")
                                            .Build();


Console.WriteLine("Listening...");
await using var stream = await RecordToFileAsync(10);
Console.WriteLine("Done listening");

Console.WriteLine("Processing started...");
var timeStamp = DateTime.UtcNow;
await foreach (var result in processor.ProcessAsync(stream))
{
    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
}
Console.WriteLine($"Processing done in {(DateTime.UtcNow - timeStamp).TotalSeconds}s");


async Task<Stream> RecordToStreamAsync(int seconds)
{
    var writer = new MemoryStream();

    var taskSource = new TaskCompletionSource<Stream>();

    var waveIn = new WaveInEvent
    {
        WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz sample rate, 16 bits per sample, 1 channel (mono)
    };

    waveIn.DataAvailable += (sender, args) =>
    {
        writer.WriteAsync(args.Buffer, 0, args.BytesRecorded);
        if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * seconds)
        {
            waveIn.StopRecording();
        }
    };

    waveIn.RecordingStopped += (sender, args) =>
    {
        waveIn.Dispose();
        taskSource.TrySetResult(writer);
    };

    waveIn.StartRecording();
    return await taskSource.Task;
}

async Task<Stream> RecordToFileAsync(int seconds)
{

    var fileName = Path.Combine(@"D:\temp\recordings", $"{Guid.NewGuid():N}.wav");


    var taskSource = new TaskCompletionSource<Stream>();

    var waveIn = new WaveInEvent
    {
        WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz sample rate, 16 bits per sample, 1 channel (mono)
    };

    using var writer = new WaveFileWriter(fileName, waveIn.WaveFormat);

    waveIn.DataAvailable += (sender, args) =>
    {
        writer.WriteAsync(args.Buffer, 0, args.BytesRecorded);
        if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * seconds)
        {
            waveIn.StopRecording();
        }
    };

    waveIn.RecordingStopped += async (sender, args) =>
    {
        await writer.DisposeAsync();
        waveIn.Dispose();
        var stream = File.OpenRead(fileName);

        taskSource.TrySetResult(stream);
    };

    waveIn.StartRecording();
    return await taskSource.Task;
}

static async Task GetModelAsync(string modelName, GgmlType ggmlType)
{
    Console.WriteLine("Downloading model...");
    await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    await using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
    Console.WriteLine("Done downloading...");
}