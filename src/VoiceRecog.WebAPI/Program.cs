using VoiceRecog.WebAPI;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddTransient<AudioService>();

var app = builder.Build();


app.MapGet("/", () => "Hello audio world!");
app.MapPost("/audioToFile", StreamAudioToFileAsync);

app.MapPost("/audioToSpecificFile/{fileName}", StreamAudioAFileAsync);

app.MapPost("/completeFile/{fileName}", CompleteFileAsync);

await app.RunAsync();

static async Task StreamAudioToFileAsync(HttpRequest req,
                                   AudioService service,
                                   CancellationToken cancellationToken = default)
{
    await using var stream = req.Body;
    await service.ReadAudioStreamToFileAsync(stream, cancellationToken: cancellationToken);
}

static async Task StreamAudioAFileAsync(HttpRequest req,
                                        AudioService service,
                                        string fileName,
                                        CancellationToken cancellationToken = default)
{
    await using var stream = req.Body;
    await service.ReadAudioStreamToFileAsync(stream, fileName, cancellationToken);
}

static Task CompleteFileAsync(AudioService service,
                                    string fileName,
                                    CancellationToken cancellationToken = default)
    => service.CompleteFileAsync(fileName, cancellationToken);