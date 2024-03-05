using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;





var fileName = Path.Combine(@"D:\temp\recordings", $"{Guid.NewGuid():N}.wav");

await RecordingAsync(fileName);



var modelName = "ggml-base.bin";

if (!File.Exists(modelName))
{
    Console.WriteLine("Downloading model...");
    await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
    await using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
    Console.WriteLine("Done downloading...");
}


using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

await using var processor = whisperFactory.CreateBuilder()
                                            .WithLanguage("en")
                                            .Build();


var wavFileName = fileName;
await using var stream = File.OpenRead(wavFileName);

//var stream = new MemoryStream();
//await KeepRecordingAsync(stream);

Console.WriteLine("Recog started...");
var timeStamp = DateTime.UtcNow;
await foreach (var result in processor.ProcessAsync(stream))
{
    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
}
Console.WriteLine($"Recog done in {(DateTime.UtcNow - timeStamp).TotalMilliseconds}ms");




async Task KeepRecordingAsync(Stream stream)
{

    var taskSource = new TaskCompletionSource();

    var waveIn = new WaveInEvent
    {
        WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz sample rate, 16 bits per sample, 1 channel (mono)
    };

    var writer = stream;

    waveIn.DataAvailable += (sender, args) =>
    {
        writer.WriteAsync(args.Buffer, 0, args.BytesRecorded);
        if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 10)
        {
            waveIn.StopRecording();
        }
    };

    waveIn.RecordingStopped += async (sender, args) =>
    {
        waveIn.Dispose();
        taskSource.TrySetResult();
    };

    waveIn.StartRecording();
    await taskSource.Task;
    Console.WriteLine("Recording started...");
}

async Task RecordingAsync(string outputFilePath)
{

    var taskSource = new TaskCompletionSource();

    var waveIn = new WaveInEvent
    {
        WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz sample rate, 16 bits per sample, 1 channel (mono)
    };

    using var writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);

    waveIn.DataAvailable += (sender, args) =>
    {
        writer.WriteAsync(args.Buffer, 0, args.BytesRecorded);
        if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 10)
        {
            waveIn.StopRecording();
        }
    };

    waveIn.RecordingStopped += async (sender, args) =>
    {
        await writer.DisposeAsync();
        waveIn.Dispose();
        taskSource.TrySetResult();
    };

    waveIn.StartRecording();
    await taskSource.Task;
}