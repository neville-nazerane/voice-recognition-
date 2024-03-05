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
await using var fileStream = File.OpenRead(wavFileName);

await foreach (var result in processor.ProcessAsync(fileStream))
{
    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
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
        writer.Write(args.Buffer, 0, args.BytesRecorded);
        if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 30) // Stop recording after 30 seconds
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
    Console.WriteLine("Recording started. It will automatically stop after 30 seconds.");
}