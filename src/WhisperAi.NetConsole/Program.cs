using NAudio.Wave;
using System.Collections.Concurrent;
using Whisper.net;
using Whisper.net.Ggml;




CleanUp();

SemaphoreSlim writeLock = new(1, 1);

var modelName = "ggml-MediumEn.bin";

if (!File.Exists(modelName))
    await GetModelAsync(modelName, GgmlType.MediumEn);

using var whisperFactory = WhisperFactory.FromPath(modelName);

await using var processor = whisperFactory.CreateBuilder()
                                    
                                            .WithLanguage("en")
                                            .Build();


var results = new ConcurrentDictionary<string, string>();

var tasks = Enumerable.Range(0, 50)
                      .Select(RunAsync);

await Task.WhenAll(tasks);



//await RunAsync(0);



async Task RunAsync(int t)
{
    await Task.Delay(TimeSpan.FromSeconds(t));
    Console.WriteLine("Listening...");
    await using var stream = await RecordToFileAsync(5);
    Console.WriteLine("Done listening");

    Console.WriteLine("Processing started...");
    var timeStamp = DateTime.UtcNow;
    
    int counter = 0;

    await foreach (var result in processor.ProcessAsync(stream))
    {
        results.TryAdd($"{t}:{++counter}", result.Text);
        Console.WriteLine($"{t}: {result.Text}");
    }

    Console.WriteLine($"Processing done in {(DateTime.UtcNow - timeStamp).TotalSeconds}s");

}

Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");

var res = results.OrderBy(r => r.Key)
                    .Select(r => r.Value)
                    .ToArray();

Console.WriteLine(string.Join("\n", res));



















async Task<Stream> RecordToStreamAsync(int seconds)
{
    var ms = new MemoryStream();

    var taskSource = new TaskCompletionSource<Stream>();

    var waveIn = new WaveInEvent
    {
        WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz sample rate, 16 bits per sample, 1 channel (mono)
    };

    waveIn.DataAvailable += (sender, args) =>
    {
        ms.WriteAsync(args.Buffer, 0, args.BytesRecorded);
        if (ms.Position > waveIn.WaveFormat.AverageBytesPerSecond * seconds)
        {
            waveIn.StopRecording();
        }
    };

    waveIn.RecordingStopped += (sender, args) =>
    {
        waveIn.Dispose();
        ms.Position = 0;
        taskSource.TrySetResult(ms);
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

    waveIn.DataAvailable += async (sender, args) =>
    {
        await writeLock.WaitAsync();
        try
        {
            await writer.WriteAsync(args.Buffer, 0, args.BytesRecorded);
        }
        finally
        {
            writeLock.Release();
        }
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

void CleanUp()
{
    foreach (var file in Directory.EnumerateFiles(@"D:\temp\recordings"))
        File.Delete(file);
}

static async Task GetModelAsync(string modelName, GgmlType ggmlType)
{
    Console.WriteLine("Downloading model...");
    await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    await using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
    Console.WriteLine("Done downloading...");
}